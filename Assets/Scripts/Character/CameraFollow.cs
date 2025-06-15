using UnityEngine;
using Fusion;

public class CameraFollow : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 10f, -6.5f); // Kamera pozisyon farkı
    public float smoothSpeed = 5f;                       // Kamera takip yumuşaklığı
    public Vector3 fixedEulerRotation = new Vector3(45f, 0f, 0f); // Sabit bakış açısı

    private Transform target; // Takip edilecek hedef (Player)

    // Hedef atanır
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(fixedEulerRotation);
    }
}