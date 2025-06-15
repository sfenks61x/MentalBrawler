// Dosya Adı: ObjectPullerWithJoint.cs (NİHAİ VERSİYON - FUSION 2)
using UnityEngine;
using Fusion;

public class ObjectPullerWithJoint : NetworkBehaviour
{
    [Header("Yumruk Ayarları")]
    public float punchRadius = 1f;
    public float punchForce = 5f;
    public float upwardForceMultiplier = 1f;
    public float punchCooldown = 1.5f;

    // AĞ UYUMLU ZAMANLAYICI
    [Networked] private TickTimer PunchCooldownTimer { get; set; }

    // Bu fonksiyon NetworkPlayerMovement'taki RPC üzerinden çağrılır.
    public void PerformPunch()
    {
        if (!PunchCooldownTimer.ExpiredOrNotRunning(Runner)) 
        {
            return;
        }
        
        PunchCooldownTimer = TickTimer.CreateFromSeconds(Runner, punchCooldown);
        Debug.Log("Yumruk atıldı ve Host tarafından çalıştırıldı!");

        // Fiziksel etki (sadece Host'ta çalışır ve etkisi NetworkTransform ile herkese yansır)
        Collider[] hits = Physics.OverlapSphere(transform.position, punchRadius);
        foreach (Collider col in hits)
        {
            if (col.attachedRigidbody == null || col.gameObject == gameObject) continue;

            Vector3 direction = (col.transform.position - transform.position).normalized;
            direction.y += upwardForceMultiplier;
            col.attachedRigidbody.AddForce(direction * punchForce, ForceMode.Impulse);
        }
    }
    
    // NOT: Çekme (Pull) mekaniğiyle ilgili eski kodlar, ağ uyumsuz olduğu için kaldırılmıştır.
    // Bu mekanik, daha sonra doğru bir şekilde bu temelin üzerine inşa edilebilir.
}