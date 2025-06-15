using UnityEngine;
using System;

public class Rope : MonoBehaviour
{
    public float growSpeed = 20f;
    public float maxLength = 20f;
    public float shrinkSpeed = 10f;
    public float pullSpeed = 10f;
    public LayerMask hitLayer;

    private float currentLength = 0f;
    private bool hit = false;
    public event System.Action onRopeEnd;

    public Transform anchor;
    public Transform ropeBody;

    private GameObject ropeOwner;
    private GameObject targetToPull;
    private Vector3 pullTarget;

    private Vector3 initialForward;
    private Transform firePoint;

    public GameObject GetTarget()
    {
        return targetToPull;
    }

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

    void Start()
    {
        if (ropeBody != null)
        {
            ropeBody.localScale = new Vector3(0.1f, 0.1f, 0.01f);
        }
    }

    void Update()
    {
        if (!hit)
        {
            if (firePoint != null)
                transform.position = firePoint.position;

            float growth = growSpeed * Time.deltaTime;
            currentLength += growth;

            if (currentLength >= maxLength)
            {
                DestroyRope();
                return;
            }

            if (ropeBody != null)
            {
                ropeBody.localScale += new Vector3(0, 0, growth);
            }
        }
        else
        {
            // ✅ Hedef ölmüşse halat otomatik silinsin
            if (targetToPull != null)
            {
                if (targetToPull.TryGetComponent<HealthManager>(out var health) && health.IsDead())
                {
                    Debug.Log("[Rope] Hedef öldü, halat siliniyor.");
                    DestroyRope();
                    return;
                }

                if (targetToPull.TryGetComponent(out Rigidbody rb))
                {
                    Vector3 direction = pullTarget - targetToPull.transform.position;
                    float distance = direction.magnitude;

                    if (distance < 0.5f)
                    {
                        rb.linearVelocity = Vector3.zero;
                        DestroyRope();
                        return;
                    }

                    rb.linearVelocity = direction.normalized * pullSpeed;
                }
            }

            if (ropeBody != null)
            {
                ropeBody.localScale = Vector3.Lerp(ropeBody.localScale, Vector3.zero, Time.deltaTime * shrinkSpeed);
            }
        }

        if (anchor != null && ropeBody != null)
        {
            anchor.localPosition = new Vector3(0, 0, ropeBody.localScale.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hitLayer) != 0 && !hit)
        {
            if (other.gameObject == ropeOwner || other.transform.IsChildOf(ropeOwner.transform))
            {
                return;
            }

            hit = true;
            targetToPull = other.gameObject;
            pullTarget = ropeOwner.transform.position + Vector3.up * 1.0f;

            // ✅ Yeni eklenen: Can azaltma çağrısı
            if (other.TryGetComponent<HealthManager>(out var health))
            {
                health.TakeDamage(1);
            }
        }
    }

    void DestroyRope()
    {
        if (targetToPull != null && targetToPull.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
        }

        onRopeEnd?.Invoke();

        // Ana halat objesinin kökünü tamamen sil
        Destroy(transform.root.gameObject);
    }

}
