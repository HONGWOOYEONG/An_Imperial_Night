using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private int facingDirection = 1;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashPower = 30f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashDuration = 0.15f;

    private float nextDashTime;
    private float defaultGravityScale;

    private float crouchSpeed;
    private float defenceSpeed;

    private bool isDefending;
    private bool isCrouching;
    private bool isDashing;

    private Coroutine dashCoroutine;

    public bool IsCrouching => isCrouching;
    public bool IsDashing => isDashing;
    public int FacingDirection => facingDirection;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 6f;

    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        defaultGravityScale = rb.gravityScale;

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

    public void ResetControlState()
    {
        bool wasDashing = isDashing;

        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }

        moveInput = Vector2.zero;

        isCrouching = false;
        isDefending = false;
        isDashing = false;

        rb.gravityScale = defaultGravityScale;

        // 공중에서 초기화되더라도 Y축 낙하는 중력에 맡김
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 대시 도중 취소되어도 쿨타임 적용
        if (wasDashing)
        {
            nextDashTime = Time.time + dashCooldown;
        }
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
        Debug.Log($"Crouch: {isCrouching}");
    }

    public void SetDefending(bool defending)
    {
        isDefending = defending;
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (isDashing || Time.time < nextDashTime)
            return;

        dashCoroutine = StartCoroutine(StartDash());
    }

    private IEnumerator StartDash()
    {
        isDashing = true;

        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(
            facingDirection * dashPower,
            0f
        );

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = defaultGravityScale;

        isDashing = false;
        nextDashTime = Time.time + dashCooldown;
        dashCoroutine = null;
    }

    private void Move()
    {
        if (isDashing)
            return;

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