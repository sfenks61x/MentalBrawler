using UnityEngine;
using Fusion;

public class Rope : NetworkBehaviour
{
    public float growSpeed = 20f;
    public float maxLength = 20f;
    public float shrinkSpeed = 10f;
    public float pullSpeed = 10f;
    public LayerMask hitLayer;

    private float currentLength = 0f;

    [Networked] private bool hit { get; set; }
    [Networked] private Vector3 pullTarget { get; set; }

    public Transform anchor;
    public Transform ropeBody;

    private GameObject ropeOwner;
    private GameObject targetToPull;
    private Vector3 initialForward;
    private Transform firePoint;

    public GameObject GetTarget() => targetToPull;

    public void SetFirePoint(Transform fp)
    {
        firePoint = fp;
        initialForward = fp.forward;
        transform.rotation = Quaternion.LookRotation(initialForward, Vector3.up);
    }

    public void SetOwner(GameObject owner)
    {
        ropeOwner = owner;
    }

    public override void FixedUpdateNetwork()
    {
        if (!hit)
        {
            if (firePoint != null)
                transform.position = firePoint.position;

            float growth = growSpeed * Runner.DeltaTime;
            currentLength += growth;

            if (currentLength >= maxLength)
            {
                SelfDestruct();
                return;
            }

            if (ropeBody != null)
                ropeBody.localScale += new Vector3(0, 0, growth);
        }
        else
        {
            if (!HasStateAuthority) return;

            if (targetToPull != null)
            {
                if (targetToPull.TryGetComponent<HealthManager>(out var health) && health.IsDead())
                {
                    Debug.Log("[Rope] Hedef öldü, halat siliniyor.");
                    SelfDestruct();
                    return;
                }

                if (targetToPull.TryGetComponent(out Rigidbody rb))
                {
                    Vector3 direction = pullTarget - targetToPull.transform.position;
                    float distance = direction.magnitude;

                    if (distance < 0.5f)
                    {
                        rb.linearVelocity = Vector3.zero;
                        SelfDestruct();
                        return;
                    }

                    rb.linearVelocity = direction.normalized * pullSpeed;
                }
            }

            if (ropeBody != null)
                ropeBody.localScale = Vector3.Lerp(ropeBody.localScale, Vector3.zero, Runner.DeltaTime * shrinkSpeed);
        }

        if (anchor != null && ropeBody != null)
            anchor.localPosition = new Vector3(0, 0, ropeBody.localScale.z);
    }

    // 🛠 HATALI override yerine normal fonksiyon tanımı
    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority || hit) return;

        if (((1 << other.gameObject.layer) & hitLayer) == 0)
            return;

        if (other.gameObject == ropeOwner || other.transform.IsChildOf(ropeOwner.transform))
            return;

        hit = true;
        targetToPull = other.gameObject;
        pullTarget = ropeOwner.transform.position + Vector3.up * 1.0f;

        if (targetToPull.TryGetComponent<HealthManager>(out var health))
        {
            health.RPC_TakeDamage(1);
        }
    }

    void SelfDestruct()
    {
        if (Object.HasStateAuthority && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }
}
