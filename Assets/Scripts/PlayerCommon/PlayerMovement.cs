using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerTarget playerTarget;
    private PlayerController playerController;


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

    private Coroutine dashCoroutine;
    private Coroutine knockbackCoroutine;
    // 기능별 이동 정지와 장판별 감속을 구분해 한 효과가 다른 효과를 해제하지 않게 합니다.
    private readonly HashSet<object> movementLocks = new HashSet<object>();
    private readonly Dictionary<object, float> slowSources = new Dictionary<object, float>();

    public bool HasMoveInput => Mathf.Abs(moveInput.x) > 0.01f;


    public bool IsJumpCharging => isJumpCharging;
    public int FacingDirection => facingDirection;
    public bool IsDashing => isDashing;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 6f;

    
    private bool isGrounded;
    public bool IsGrounded => isGrounded;
    public Vector2 Velocity => rb.linearVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTarget = GetComponentInChildren<PlayerTarget>();
        playerController = GetComponent<PlayerController>();

        defaultGravityScale = rb.gravityScale;

        jumpSpeed = moveSpeed / 2f;
        defenceSpeed = moveSpeed / 2f;

        isMoving = true;
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
        // 피격 중단 시 대시·넉백 등 일시적인 조작 상태만 초기화합니다.
        // 외부 효과가 등록한 movementLocks/slowSources는 여기서 지우지 않습니다.
        bool wasDashing = isDashing;

        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
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

    public void OnCrouch(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (playerController != null && !playerController.CanAct) return;
        playerTarget?.ToggleLockOn();
    } //추후 인풋 매니저 설정


    public void SetDefending(bool defending)
    {
        isDefending = defending;
    }

    public void SetJumping(bool jumping)
    {
        isJumpCharging = jumping;
    }

    public void AddMovementLock(object source)
    {
        if (source != null) movementLocks.Add(source);
    }

    public void ReleaseMovementLock(object source)
    {
        if (source != null) movementLocks.Remove(source);
    }

    public void SetSlowMove(object source, float multiplier)
    {
        // 같은 플레이어에게 여러 감속 효과가 겹치면 가장 낮은 배율을 사용합니다.
        if (source == null) return;
        slowSources[source] = Mathf.Clamp01(multiplier);
        RefreshSlowMove();
    }

    public void ClearSlowMove(object source)
    {
        if (source == null) return;
        slowSources.Remove(source);
        RefreshSlowMove();
    }

    private void RefreshSlowMove()
    {
        isSlowMoving = slowSources.Count > 0;
        percent = 1f;
        foreach (float multiplier in slowSources.Values)
            percent = Mathf.Min(percent, multiplier);
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (isDashing || Time.time < nextDashTime)
            return;
        // 실제 속도 변경 전에 FSM에 대시 진입 가능 여부를 묻습니다.
        if (!isGrounded) return;
        if (playerController != null && !playerController.CanDash) return;
        if (playerController != null && !playerController.TryStartAction(PlayerState.Dashing)) return;

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
        playerController?.EndAction(PlayerState.Dashing);
    }

    public void Move()
    {
        // 컨트롤러의 행동 제한과 외부 이동 제한을 모두 통과해야 입력 속도를 적용합니다.
        if (!isMoving || movementLocks.Count > 0 || (playerController != null && !playerController.CanMove))
            return;

        if (isDashing)
            return;

        float currentSpeed = moveSpeed;

        if (isJumpCharging)
        {
            currentSpeed = jumpSpeed;
        }
        else if (isDefending)
        {
            currentSpeed = defenceSpeed;
        }
        if (isSlowMoving) currentSpeed *= percent;

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
    public void SetFacingDirection(int direction)
    {
        if (direction == 0)
            return;

        if (facingDirection == direction)
            return;

        facingDirection = direction;
        Flip();
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
    
    public void KnockBack(Vector2 dir, float power) //방향과 세기를 받아 넉백
    {
        if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir.normalized * Mathf.Max(0f, power), ForceMode2D.Impulse);
        knockbackCoroutine = StartCoroutine(ResumeAfterKnockback());
    }

    public void KnockBackToPoint(Vector2 destination, float speed)
    {
        if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
        knockbackCoroutine = StartCoroutine(MoveToKnockbackPoint(destination, speed));
    }

    private IEnumerator MoveToKnockbackPoint(Vector2 destination, float speed)
    {
        while (Vector2.Distance(rb.position, destination) > 0.01f)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, destination, Mathf.Max(0.01f, speed) * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        yield return ResumeAfterKnockback();
    }

    private IEnumerator ResumeAfterKnockback()
    {
        // 넉백 속도를 일반 이동이 즉시 덮어쓰지 않도록 잠시 이동 입력을 보류합니다.
        yield return new WaitForSeconds(0.2f);
        isMoving = true;
        knockbackCoroutine = null;
    }

    // H/T 기능은 Rigidbody2D를 직접 수정하는 대신 아래 물리 동작을 요청합니다.
    public void ApplyImpulse(Vector2 impulse) => rb.AddForce(impulse, ForceMode2D.Impulse);
    public void Jump(float power)
    {
        isGrounded = false;
        ApplyImpulse(Vector2.up * power);
    }
    public void SetVelocity(Vector2 velocity) => rb.linearVelocity = velocity;
    public void SetGravityScale(float gravityScale) => rb.gravityScale = gravityScale;
    public void RestoreGravity() => rb.gravityScale = defaultGravityScale;
    public float GravityScale => rb.gravityScale;
    public void MoveBy(Vector2 displacement) => rb.position += displacement;
    public void Teleport(Vector2 position) => rb.position = position;
    public float Rotation => rb.rotation;
    public void SetRotation(float rotation) => rb.rotation = rotation;

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
