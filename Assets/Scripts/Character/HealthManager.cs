using UnityEngine;
using Fusion;

public class HealthManager : NetworkBehaviour
{
    [Header("Can Ayarı")]
    [SerializeField] [Min(1)] private int maxHealth = 4;

    [Networked]
    private int currentHealth { get; set; }

    [Networked]
    private bool isDead { get; set; }

    [Header("Referanslar")]
    public Animator animator;
    public Collider[] allColliders;
    public ObjectPullerWithJoint pullerScript;

    public override void Spawned()
    {
        if (currentHealth <= 0)
            currentHealth = maxHealth;

        if (pullerScript == null)
            pullerScript = GetComponent<ObjectPullerWithJoint>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (allColliders == null || allColliders.Length == 0)
            allColliders = GetComponents<Collider>();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

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
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            Invoke(nameof(FreezeAllAxes), 2f);
        }

        Debug.Log("[Death] Y sabitlendi, X-Z kilidi 2sn sonra gelecek.");
    }

    private void FreezeAllAxes()
    {
        if (!this || isDead == false) return;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
            rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public bool IsDead() => isDead;
}
