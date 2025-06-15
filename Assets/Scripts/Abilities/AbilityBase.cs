
using UnityEngine;

public abstract class AbilityBase : MonoBehaviour {
    public float cooldownTime = 5f;
    protected float lastUsedTime;

    public bool CanUse => Time.time >= lastUsedTime + cooldownTime;

    public void TryUse(GameObject target) {
        if (CanUse) {
            Activate(target);
            lastUsedTime = Time.time;
        }
    }

    protected abstract void Activate(GameObject target);
}
