using Fusion;
using UnityEngine;

public enum PlayerButtons
{
    Jump,
    Punch,
    Rope
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
    public NetworkButtons buttons; 
    public bool runPressed;
    public bool pullHeld;
}