using UnityEngine;
using Fusion;

public class RopeSpawner : NetworkBehaviour
{
    [Header("Halat Ayarları")]
    public GameObject ropePrefab;
    public Transform firePoint;

    [Networked] private TickTimer fireCooldownTimer { get; set; }

    private Animator animator;

    public override void Spawned()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public override void FixedUpdateNetwork()
    {
        // Burada cooldown kontrolü yapılır. Saniye bazlı işler rollback için buraya taşındı.
    }

    public void TriggerFire()
    {
        if (!fireCooldownTimer.ExpiredOrNotRunning(Runner)) return;

        FireRope();

        fireCooldownTimer = TickTimer.CreateFromSeconds(Runner, 2f); // 2 saniye cooldown
    }

    private void FireRope()
    {
        if (ropePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Rope prefab veya firePoint atanmadı.");
            return;
        }

        GameObject currentRope = Instantiate(ropePrefab, firePoint.position, Quaternion.identity);
        currentRope.transform.SetParent(null);

        Rope ropeScript = currentRope.GetComponent<Rope>();
        if (ropeScript != null)
        {
            ropeScript.SetFirePoint(firePoint);
            ropeScript.SetOwner(this.gameObject);
        }

        // 🎯 İleride Rope prefab’ı NetworkObject yapılırsa buraya:
        // runner.Spawn(ropePrefab.GetComponent<NetworkObject>(), firePoint.position, Quaternion.identity);
    }
}
