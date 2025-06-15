using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
    public bool jumpRequested;
    public bool punchPressed;
    public bool ropePressed;
    public bool pullHeld;
}