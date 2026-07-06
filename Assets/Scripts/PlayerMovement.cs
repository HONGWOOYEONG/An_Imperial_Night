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

    [Header("방어")]
    [SerializeField] float d_driveDecease = 10f; //방어 시 감소하는 드라이브 게이지
    [SerializeField] float d_startDelay = 1f; //방어 시작 딜레이
    private bool isDefending;

    private bool isCrouching;

    public int FacingDirection => facingDirection; // 대쉬나 공격시에 이거 가져다가 쓰면 됨

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        crouchSpeed = moveSpeed / 2;
        defenceSpeed = moveSpeed / 2;
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnCrouch(InputValue value)
    {
        isCrouching = value.isPressed;
    }

    public void SetDefending(bool defending)
    {
        isDefending = defending;
    } // 방어 구현시에 이거 참조해서 가져다 쓰면 됨

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
        UpdateFacingDirection(); // 입력 방향 따라 바꾸기
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
        transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}