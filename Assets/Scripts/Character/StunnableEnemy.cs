using UnityEngine;
using System.Collections;

public class StunnableEnemy : MonoBehaviour
{
    private bool isStunned = false;
    private Rigidbody rb;
    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public void Stun(float duration)
    {
        if (!isStunned)
            StartCoroutine(StunCoroutine(duration));
    }

    IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;

        // Rigidbody hareketi durdur
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Animator durdur
        if (animator != null)
            animator.enabled = false;

        // Diğer bileşenleri burada durdurabilirsin (örneğin AI)
        // GetComponent<EnemyAI>()?.enabled = false;

        yield return new WaitForSeconds(duration);

        // Eski hâline dön
        if (rb != null)
            rb.isKinematic = false;

        if (animator != null)
            animator.enabled = true;

        // GetComponent<EnemyAI>()?.enabled = true;

        isStunned = false;
    }
}