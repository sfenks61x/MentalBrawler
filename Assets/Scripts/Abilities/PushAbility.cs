
using UnityEngine;

public class PushAbility : AbilityBase {
    public float pushForce = 20f;

    protected override void Activate(GameObject target) {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null) {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            rb.AddForce(direction * pushForce, ForceMode.Impulse);
        }
    }
}
