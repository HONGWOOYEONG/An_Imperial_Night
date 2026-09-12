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
    [HideInInspector] public float percent;

    private float nextDashTime;
    private float defaultGravityScale;

    private float jumpSpeed;
    private float defenceSpeed;

    private bool isDefending;
    private bool isJumpCharging;
    private bool isDashing;
    [HideInInspector] public bool isSlowMoving;
    [HideInInspector] public bool isMoving;
    private bool isKnockBack;

    private Coroutine dashCoroutine;


    public bool IsJumpCharging => isJumpCharging;
    public int FacingDirection => facingDirection;
    public bool IsDashing => isDashing;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 6f;

    
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        defaultGravityScale = rb.gravityScale;

        jumpSpeed = moveSpeed / 2f;
        defenceSpeed = moveSpeed / 2f;

        isSlowMoving = false;
        isMoving = true;
        isKnockBack = false;
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void OnMove(InputValue value)
    {
        if (!isMoving)
            return;
        if (isKnockBack)
            return;
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

        isJumpCharging = false;
        isDefending = false;
        isDashing = false;
        isMoving = true;

        rb.gravityScale = defaultGravityScale;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (wasDashing)
        {
            nextDashTime = Time.time + dashCooldown;
        }
    }

    //public void OnJump(InputValue value)
    //{
    //    if (!value.isPressed)
    //        return;

    //    if (!isGrounded)
    //        return;
      

    //    isGrounded = false;

    //    rb.AddForce(
    //        Vector2.up * jumpPower,
    //        ForceMode2D.Impulse
    //    );
    //}



    public void SetDefending(bool defending)
    {
        isDefending = defending;
    }

    public void SetJumping(bool jumping)
    {
        isJumpCharging = jumping;
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

    public void Move()
    {
        float currentSpeed;
        if (isDashing || isKnockBack)
            return;

         currentSpeed = moveSpeed;

        if (isJumpCharging)
        {
            currentSpeed = jumpSpeed;
        }
        else if (isDefending)
        {
            currentSpeed = defenceSpeed;
        }
        else if (isSlowMoving)
        {
            currentSpeed = SlowMove(percent);
        }
        //Debug.Log(currentSpeed);
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
    
    public void KnockBack(Vector2 dir) //방향을 매개변수로 가져와서 그 방향으로 넉백
    {
        //f패턴에서 넉백을 사용할 때 거미줄 덩어리 지점까지 넉백된다고 하는데 그렇게 구현하면
        //넉백이 아니라 밀리는 형상이 나올거 같음 그래서 일단 뒤로 밀리게 구현해놓음
        isKnockBack = true;
        float KnockBackPower = 15f;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(dir * KnockBackPower, ForceMode2D.Impulse);
            Debug.Log("넉백");
        }
        StartCoroutine(EndKnockBack());
    }

    private IEnumerator EndKnockBack()
    {
        yield return new WaitForSeconds(0.2f); // 넉백 지속 시간
        isKnockBack = false;
    }

    public float SlowMove(float percent) //플레이어가 느려지는 함수
    {
        return moveSpeed * percent;
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