
using UnityEngine;

public class PullAbility : AbilityBase {
    public float pullForce = 20f;

    protected override void Activate(GameObject target) {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null) {
            Vector3 direction = (transform.position - target.transform.position).normalized;
            rb.AddForce(direction * pullForce, ForceMode.Impulse);
        }
    }
}
