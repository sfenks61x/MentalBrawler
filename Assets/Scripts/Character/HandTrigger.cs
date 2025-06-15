using UnityEngine;

public class HandTrigger : MonoBehaviour
{
    [Tooltip("Sadece bu layer'daki objeler tutulabilir.")]
    public LayerMask pullableLayer;
    public GameObject currentTouchingObject;

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & pullableLayer) != 0)
        {
            currentTouchingObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (currentTouchingObject == other.gameObject)
        {
            currentTouchingObject = null;
        }
    }
}