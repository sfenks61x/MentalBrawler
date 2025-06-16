using UnityEngine;
using Fusion;

public class ObjectPullerWithJoint : NetworkBehaviour
{
    public Transform pullPoint;
    public PlayerRef myOwner;

    [Header("Yumruk Ayarları")]
    public float punchRadius = 1f;
    public float punchForce = 5f;
    public float upwardForceMultiplier = 1f;
    public float pullLockDurationAfterPunch = 0.5f;

    private GameObject heldObject;
    private Rigidbody heldRigidbody;
    private Collider heldCollider;
    private Vector3 heldOffset = Vector3.zero;
    private bool pullHeldPrevious = false;

    [Networked] private TickTimer pullLockTimer { get; set; }
    [Networked] private TickTimer punchDelayTimer { get; set; }
    [Networked] private bool punchTriggered { get; set; }
    [Networked] private bool temporarilyDisablePullInput { get; set; }

    [Header("Animasyon")]
    public Animator playerAnimator;

    private NetworkObject netObj;

    public override void Spawned()
    {
        netObj = GetComponent<NetworkObject>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!netObj.HasInputAuthority) return;
        if (!GetInput(out NetworkInputData inputData)) return;

        if (pullLockTimer.ExpiredOrNotRunning(Runner) == false)
        {
            ForceStopPull();
            pullHeldPrevious = false;

            if (playerAnimator != null)
                playerAnimator.SetFloat("pullLockTimer", (float)pullLockTimer.RemainingTime(Runner));

            return;
        }

        bool rawPullInput = inputData.pullHeld;
        bool pullStarted = rawPullInput && !pullHeldPrevious && !temporarilyDisablePullInput;
        bool pullOngoing = rawPullInput && !temporarilyDisablePullInput;

        if (pullStarted)
        {
            if (heldObject == null)
            {
                TryGrabObject();
            }

            if (playerAnimator != null)
                playerAnimator.SetBool("IsPulling", true);
        }
        else if (pullOngoing && heldObject != null)
        {
            PullHeldObject();

            if (playerAnimator != null)
                playerAnimator.SetBool("IsPulling", true);
        }
        else
        {
            if (heldObject != null)
                ReleaseObject();

            if (playerAnimator != null)
                playerAnimator.SetBool("IsPulling", false);
        }

        pullHeldPrevious = rawPullInput;
    }

    void TryGrabObject()
    {
        if (!HasStateAuthority) return;

        float maxGrabDistance = 1.5f;
        Collider[] hits = Physics.OverlapSphere(pullPoint.position, 0.5f);
        foreach (var hit in hits)
        {
            if (hit.attachedRigidbody != null &&
                LayerMask.LayerToName(hit.gameObject.layer) == "Pullable" &&
                hit.gameObject != this.gameObject &&
                !hit.transform.IsChildOf(transform))
            {
                float dist = Vector3.Distance(pullPoint.position, hit.transform.position);
                if (dist > maxGrabDistance) continue;

                heldObject = hit.gameObject;
                heldRigidbody = heldObject.GetComponent<Rigidbody>();
                heldCollider = heldObject.GetComponent<Collider>();

                if (heldCollider != null) heldCollider.enabled = false;
                if (heldRigidbody != null)
                {
                    heldRigidbody.useGravity = false;
                    heldRigidbody.linearVelocity = Vector3.zero;
                }

                heldOffset = Quaternion.Inverse(transform.rotation) * (heldRigidbody.position - pullPoint.position);
                break;
            }
        }
    }

    void PullHeldObject()
    {
        if (!HasStateAuthority || heldRigidbody == null) return;

        Vector3 rotatedOffset = transform.rotation * heldOffset;
        Vector3 targetPos = pullPoint.position + rotatedOffset;

        float minY = 0.5f;
        if (targetPos.y < minY)
            targetPos.y = minY;

        float moveSpeed = 50f;
        heldRigidbody.MovePosition(Vector3.Lerp(heldRigidbody.position, targetPos, moveSpeed * Runner.DeltaTime));
    }

    public void ReleaseObject()
    {
        if (!HasStateAuthority) return;

        if (heldCollider != null) heldCollider.enabled = true;
        if (heldRigidbody != null)
        {
            heldRigidbody.useGravity = true;
            heldRigidbody.linearVelocity = Vector3.zero;
        }

        heldObject = null;
        heldRigidbody = null;
        heldCollider = null;
        heldOffset = Vector3.zero;
    }

    public void ForceStopPull()
    {
        if (!HasStateAuthority) return;

        if (heldObject != null) ReleaseObject();

        if (playerAnimator != null)
            playerAnimator.SetBool("IsPulling", false);
    }

    public void TryPunch()
    {
        if (punchTriggered || !punchDelayTimer.ExpiredOrNotRunning(Runner)) return;

        punchTriggered = true;
        ForceStopPull();
        pullHeldPrevious = false;
        temporarilyDisablePullInput = true;
        punchDelayTimer = TickTimer.CreateFromSeconds(Runner, 0.2f);
        StartCoroutine(PunchAfterDelay()); // Bu kısım alternatifle değiştirilecek
    }

    private System.Collections.IEnumerator PunchAfterDelay()
    {
        yield return new WaitForSeconds(0.2f); // GEÇİCİ
        if (!HasStateAuthority) yield break;

        Collider[] hits = Physics.OverlapSphere(pullPoint.position, punchRadius);
        foreach (Collider col in hits)
        {
            if (col.attachedRigidbody == null) continue;
            if (col.attachedRigidbody.gameObject == this.gameObject) continue;

            if (col.attachedRigidbody.TryGetComponent<NetworkObject>(out var net))
            {
                if (net.HasStateAuthority && net.InputAuthority == myOwner) continue;
            }

            Vector3 direction = (col.transform.position - pullPoint.position).normalized;
            direction.y += upwardForceMultiplier;

            col.attachedRigidbody.AddForce(direction * punchForce, ForceMode.Impulse);

            if (col.TryGetComponent<StunnableEnemy>(out var stun))
            {
                stun.Stun(0.7f);
            }

            if (col.TryGetComponent<HealthManager>(out var health))
            {
                health.RPC_TakeDamage(1);

            }
        }

        punchTriggered = false;
        pullLockTimer = TickTimer.CreateFromSeconds(Runner, pullLockDurationAfterPunch);
        pullHeldPrevious = false;
        temporarilyDisablePullInput = false;

        if (playerAnimator != null)
            playerAnimator.SetBool("IsPulling", false);
    }
}
