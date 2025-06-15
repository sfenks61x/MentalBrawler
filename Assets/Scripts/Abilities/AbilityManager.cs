
using UnityEngine;

public class AbilityManager : MonoBehaviour {
    public PullAbility pull;
    public PushAbility push;
    public GameObject target;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) pull.TryUse(target);
        if (Input.GetKeyDown(KeyCode.E)) push.TryUse(target);
    }
}
