using Fusion;
using UnityEngine;

public class AdvancedNetworkTransform : NetworkBehaviour
{
    [Networked] public Vector3 Position { get; set; }
    [Networked] public Quaternion Rotation { get; set; }
    [Networked] public Vector3 Scale { get; set; }

    [Header("Sync Ayarları")]
    public bool syncPosition = true;
    public bool syncRotation = true;
    public bool syncScale = false;

    [Header("Yumuşatma")]
    [Range(1f, 60f)] public float lerpSpeed = 15f;

    public override void Spawned()
    {
        // İlk pozisyon ayarı (server'dan geldiğinde override'la)
        if (!HasStateAuthority)
        {
            transform.position = Position;
            transform.rotation = Rotation;
            transform.localScale = Scale;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            if (syncPosition) Position = transform.position;
            if (syncRotation) Rotation = transform.rotation;
            if (syncScale)    Scale = transform.localScale;
        }
        else
        {
            if (syncPosition)
                transform.position = Vector3.Lerp(transform.position, Position, Runner.DeltaTime * lerpSpeed);

            if (syncRotation)
                transform.rotation = Quaternion.Slerp(transform.rotation, Rotation, Runner.DeltaTime * lerpSpeed);

            if (syncScale)
                transform.localScale = Vector3.Lerp(transform.localScale, Scale, Runner.DeltaTime * lerpSpeed);
        }
    }
}