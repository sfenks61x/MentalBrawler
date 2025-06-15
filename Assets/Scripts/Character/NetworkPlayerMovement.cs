using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(RopeSpawner))]
[RequireComponent(typeof(ObjectPullerWithJoint))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    [Header("Hareket Ayarları")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 180f;

    [Header("Zıplama Ayarları")]
    public float jumpForce = 5f;
    public float forwardJumpMultiplier = 1f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("Referanslar")]
    public Transform groundCheck;
    public Animator animator;
    public Transform punchPoint;

    private Rigidbody rb;
    private RopeSpawner ropeSpawner;
    private ObjectPullerWithJoint puller;
    private NetworkObject netObj;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool isRunning;
    private float smoothSpeed;
    
    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        animator = GetComponent<Animator>();
        ropeSpawner = GetComponent<RopeSpawner>();
        puller = GetComponent<ObjectPullerWithJoint>();

        netObj = GetComponent<NetworkObject>();

        puller.myOwner = netObj.HasStateAuthority ? Runner.LocalPlayer : default;

        Debug.Log($"[SPAWNED] Object: {gameObject.name} | PlayerRef: {netObj.InputAuthority} | IsMine: {netObj.HasInputAuthority} | StateAuth: {netObj.HasStateAuthority}");

        if (netObj.HasInputAuthority)
        {
            Debug.Log("💡 [CAMERA] Input authority var, kamera atanıyor.");
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                cam.SetTarget(transform);
            }
        }
        else
        {
            Debug.Log("⚠️ [CAMERA] Input authority yok, bu oyuncu başka bir client’a ait.");
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData inputData)) return;
        if (!Object.HasStateAuthority) return; // Pozisyon senkronizasyonu için bu kontrol olmalı

        // Hareket
        moveInput = inputData.move;
        isRunning = Keyboard.current.leftShiftKey.isPressed;
        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);
        moveDir = transform.TransformDirection(moveDir);
        rb.MovePosition(rb.position + moveDir * speed * Runner.DeltaTime);

        if (HasInputAuthority && moveDir.magnitude > 0.1f)
        {
            Quaternion rot = Quaternion.LookRotation(moveDir, Vector3.up);
            Quaternion smoothRot = Quaternion.RotateTowards(transform.rotation, rot, rotationSpeed * Runner.DeltaTime);
            transform.rotation = smoothRot;
        }
        
        // Zemin kontrolü (artık groundCheck kullanılıyor)
        Ray ray = new Ray(groundCheck.position, Vector3.down);
        isGrounded = Physics.Raycast(ray, groundCheckDistance + 0.1f, groundLayer);

        // Zıplama
        if (inputData.jumpRequested && isGrounded)
        {
            Vector3 jumpDir = rb.transform.forward * forwardJumpMultiplier + Vector3.up;
            rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);
        }

        // Yumruk
        if (inputData.punchPressed)
        {
            animator.SetTrigger("Punch");
            puller.TryPunch();
        }

        // Halat
        if (inputData.ropePressed)
        {
            ropeSpawner.TriggerFire();
        }

        // Animasyonlar
        smoothSpeed = Mathf.Lerp(smoothSpeed, moveInput.magnitude, Runner.DeltaTime * 10f);
        animator.SetFloat("Speed", smoothSpeed);
        animator.SetBool("IsJumping", !isGrounded);
    }
}
