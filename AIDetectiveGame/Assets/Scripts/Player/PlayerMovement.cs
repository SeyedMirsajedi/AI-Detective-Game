using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Visual Reference")]
    [SerializeField] private SpriteRenderer visualRenderer;

    private Rigidbody2D playerRigidbody;
    private float horizontalInput;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        ReadMovementInput();
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        Vector2 currentVelocity = playerRigidbody.linearVelocity;

        playerRigidbody.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            currentVelocity.y
        );
    }

    private void ReadMovementInput()
    {
        horizontalInput = 0f;

        if (Keyboard.current == null)
        {
            return;
        }

        bool moveLeft =
            Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed;

        bool moveRight =
            Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed;

        if (moveLeft && !moveRight)
        {
            horizontalInput = -1f;
        }
        else if (moveRight && !moveLeft)
        {
            horizontalInput = 1f;
        }
    }

    private void UpdateFacingDirection()
    {
        if (visualRenderer == null)
        {
            return;
        }

        if (horizontalInput < 0f)
        {
            visualRenderer.flipX = true;
        }
        else if (horizontalInput > 0f)
        {
            visualRenderer.flipX = false;
        }
    }
}