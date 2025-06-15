using UnityEngine;
using System.Collections;

public class RopeSpawner : MonoBehaviour
{
    [Header("Halat Ayarları")]
    public GameObject ropePrefab;
    public Transform firePoint;
    private GameObject currentRope;

    [Header("Gecikmeli Fırlatma (E tuşu)")]
    [SerializeField] private float fireDelay = 0.3f;
    private bool isFiring = false;

    [Header("Cooldown")]
    private bool canFire = true;
    private float fireCooldownTimer = 0f;
    private float cooldownDuration = 2f;

    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!canFire)
        {
            fireCooldownTimer += Time.deltaTime;
            if (fireCooldownTimer >= cooldownDuration)
                canFire = true;
        }
    }

    public void TriggerFire()
    {
        if (!canFire || isFiring) return;
        StartCoroutine(DelayedFire());
    }

    IEnumerator DelayedFire()
    {
        isFiring = true;
        yield return new WaitForSeconds(fireDelay);

        FireRope();

        isFiring = false;
    }

    void FireRope()
    {
        if (ropePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Rope prefab veya firePoint atanmadı.");
            return;
        }

        currentRope = Instantiate(ropePrefab, firePoint.position, Quaternion.identity);
        currentRope.transform.SetParent(null); // Emin ol ki sahneye bağımsız düşüyor
        canFire = false;
        fireCooldownTimer = 0f;

        Rope ropeScript = currentRope.GetComponent<Rope>();
        if (ropeScript != null)
        {
            ropeScript.SetFirePoint(firePoint);
            ropeScript.SetOwner(this.gameObject);
            ropeScript.onRopeEnd += () => { };
        }
    }
}