using UnityEngine;
using Fusion;

public class HealthManager : NetworkBehaviour
{
    [Header("Can Ayarı")]
    [SerializeField] [Min(1)] private int maxHealth = 4;
    private int currentHealth;

    [Header("Referanslar")]
    public Animator animator;
    public Collider[] allColliders;
    public ObjectPullerWithJoint pullerScript;

    private bool isDead = false;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
        Debug.Log($"[Awake] maxHealth: {maxHealth}, currentHealth: {currentHealth}");
    }

    public override void Spawned()
    {
        Debug.Log("[Spawned] Fusion tarafından çağrıldı");

        if (currentHealth <= 0)
            currentHealth = maxHealth;

        // Otomatik referans alma
        if (pullerScript == null)
            pullerScript = GetComponent<ObjectPullerWithJoint>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (allColliders == null || allColliders.Length == 0)
            allColliders = GetComponents<Collider>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"[Damage] Hasar alındı: {amount}, Kalan Can: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        if (animator != null)
            animator.SetTrigger("Die");

        foreach (var col in allColliders)
            col.enabled = false;

        if (pullerScript != null)
            pullerScript.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // İlk anda sadece Y sabitlenir
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

            // 2 saniye sonra X ve Z de sabitlenecek
            StartCoroutine(LockXZAfterDelay(rb, 2f));
        }

        Debug.Log("[Death] Y sabitlendi, X-Z kilidi 2sn sonra gelecek.");
    }
    
    private System.Collections.IEnumerator LockXZAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            Debug.Log("[Freeze] X ve Z sabitlendi, artık tamamen kıpırdamaz.");
        }
    }


    public bool IsDead()
    {
        return isDead;
    }
}
