using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody))]
public class NetworkRigidbody : NetworkBehaviour
{
    private Rigidbody rb;

    [Networked] private Vector3 NetworkedPosition { get; set; }
    [Networked] private Quaternion NetworkedRotation { get; set; }

    public override void Spawned()
    {
        Debug.Log($"[AUTH TEST] Input: {Object.InputAuthority} / State: {Object.StateAuthority} / IsMine: {Object.HasInputAuthority}");
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            // Yerel oyuncu kendi pozisyonunu sync eder
            NetworkedPosition = rb.position;
            NetworkedRotation = rb.rotation;
        }
        else
        {
            // Remote oyuncular için pozisyonu uygula
            rb.MovePosition(Vector3.Lerp(rb.position, NetworkedPosition, Runner.DeltaTime * 15f));
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, NetworkedRotation, Runner.DeltaTime * 15f));
        }
    }
    
}