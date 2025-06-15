using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    [Header("Hareket Ayarları")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 540f;

    [Header("Zıplama Ayarları")]
    public float jumpForce = 8f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("Referanslar")]
    public Transform groundCheck;
    
    private Rigidbody _rb;
    private Animator _animator;
    private RopeSpawner _ropeSpawner;
    private ObjectPullerWithJoint _puller;

    [Networked] private NetworkBool IsGrounded { get; set; }
    [Networked] private float NetworkedSpeedRatio { get; set; } 

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _ropeSpawner = GetComponent<RopeSpawner>();
        _puller = GetComponent<ObjectPullerWithJoint>();

        if (Object.HasInputAuthority)
        {
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null) cam.SetTarget(transform);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData data)) return;
        
        float speed = data.runPressed ? runSpeed : walkSpeed;
        Vector3 moveDirection = new Vector3(data.move.x, 0, data.move.y).normalized;
        
        // DÜZELTME: Obsolete uyarsını gideriyoruz.
        _rb.linearVelocity = new Vector3(moveDirection.x * speed, _rb.linearVelocity.y, moveDirection.z * speed);

        if (moveDirection != Vector3.zero)
        {
            _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Runner.DeltaTime));
        }

        IsGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);

        if (IsGrounded && data.buttons.IsSetDown(PlayerButtons.Jump))
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        
        if (data.buttons.IsSetDown(PlayerButtons.Punch)) RPC_TriggerPunch();
        if (data.buttons.IsSetDown(PlayerButtons.Rope)) RPC_TriggerRope();
        
        // DÜZELTME: Obsolete uyarsını gideriyoruz.
        NetworkedSpeedRatio = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude / runSpeed;
    }

    public override void Render()
    {
        _animator.SetFloat("Speed", NetworkedSpeedRatio);
        _animator.SetBool("IsJumping", !IsGrounded);
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_TriggerPunch()
    {
        _animator.SetTrigger("Punch");
        if (_puller != null) _puller.PerformPunch();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_TriggerRope()
    {
        if (_ropeSpawner != null) _ropeSpawner.TriggerFire();
    }
}