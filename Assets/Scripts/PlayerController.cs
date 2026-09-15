using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Camera")]
    public Transform cameraHolder;
    public float mouseSensitivity = 0.1f;

    [Header("Crouch")]
    public float crouchHeight = 0.6f;
    public float crouchSmoothSpeed = 8f;

    [Header("Sprint Camera")]
    public float normalFOV = 60f;
    public float sprintFOV = 75f;
    public float fovSpeed = 8f;

    private CharacterController controller;
    private Camera playerCamera;

    private float verticalVelocity;
    private float cameraPitch;

    private Vector3 cameraOriginalPosition;
    private bool isCrouching;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (cameraHolder != null)
        {
            cameraOriginalPosition = cameraHolder.localPosition;
        }

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = normalFOV;
        }

        if (!ScoreboardUI.MenuOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (ScoreboardUI.MenuOpen)
            return;

        Move();
        Look();
        UpdateCameraFOV();
        UpdateCrouch();
    }

    void Move()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed)
                input.y += 1;

            if (Keyboard.current.sKey.isPressed)
                input.y -= 1;

            if (Keyboard.current.dKey.isPressed)
                input.x += 1;

            if (Keyboard.current.aKey.isPressed)
                input.x -= 1;
        }

        input = Vector2.ClampMagnitude(input, 1f);

        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        isCrouching =
            Keyboard.current != null &&
            Keyboard.current.cKey.isPressed;

        bool sprinting =
            Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed &&
            !isCrouching;

        float speed;

        if (isCrouching)
        {
            speed = crouchSpeed;
        }
        else if (sprinting)
        {
            speed = sprintSpeed;
        }
        else
        {
            speed = walkSpeed;
        }

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;

            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame &&
                !isCrouching)
            {
                verticalVelocity =
                    Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        cameraHolder.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void UpdateCameraFOV()
    {
        if (playerCamera == null ||
            Keyboard.current == null)
            return;

        bool sprinting =
            Keyboard.current.leftShiftKey.isPressed &&
            !isCrouching;

        float targetFOV =
            sprinting ? sprintFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            fovSpeed * Time.deltaTime
        );
    }

    void UpdateCrouch()
    {
        if (cameraHolder == null)
            return;

        Vector3 targetPosition = cameraOriginalPosition;

        if (isCrouching)
        {
            targetPosition.y -= crouchHeight;
        }

        cameraHolder.localPosition = Vector3.Lerp(
            cameraHolder.localPosition,
            targetPosition,
            crouchSmoothSpeed * Time.deltaTime
        );
    }
}
