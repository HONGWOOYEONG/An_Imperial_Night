using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private int facingDirection = 1;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private float crouchSpeed;
    private float defenceSpeed;
    private bool isDefending;

    private bool isCrouching;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 10f;

    private bool isGrounded;

    public int FacingDirection => facingDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        crouchSpeed = moveSpeed / 2f;
        defenceSpeed = moveSpeed / 2f;
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (!isGrounded)
            return;

        isGrounded = false;

        rb.AddForce(
            Vector2.up * jumpPower,
            ForceMode2D.Impulse
        );
    }

    public void OnCrouch(InputValue value)
    {
        isCrouching = value.isPressed;
    }

    public void SetDefending(bool defending)
    {
        isDefending = defending;
    }

    private void Move()
    {
        float currentSpeed = moveSpeed;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isDefending)
        {
            currentSpeed = defenceSpeed;
        }

        rb.linearVelocityX = moveInput.x * currentSpeed;

        UpdateFacingDirection();
    }

    private void UpdateFacingDirection()
    {
        if (moveInput.x > 0.01f && facingDirection != 1)
        {
            facingDirection = 1;
            Flip();
        }
        else if (moveInput.x < -0.01f && facingDirection != -1)
        {
            facingDirection = -1;
            Flip();
        }
    }

    private void Flip()
    {
        float yRotation = facingDirection == 1 ? 0f : 180f;

        transform.localRotation = Quaternion.Euler(
            0f,
            yRotation,
            0f
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}