using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
public class T_Jump : MonoBehaviour
{
    private T_Attack attack;
    private PlayerMovement movement;
    private PlayerMovement playerMovement;
    private PlayerController playerController;
    public bool isJumping = false;
    [Header("ChargeJump")]

    private float currentTime = 0f;
    private float nextTime = 0.25f;
    [SerializeField]private float maxTime = 1.25f;
    [SerializeField]private float currentCharge = 0f;
    [SerializeField] private float maxCharge = 250f;
    private float addCharge = 50f;
    private float startTime = 0f;
    private float duration = 0f;
    private float normalGravity;
    [SerializeField]private float fallGravity = 7f;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 6f;

    private bool isChargingJump;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerController = GetComponent<PlayerController>();
        attack = GetComponent<T_Attack>();
        movement = GetComponent<PlayerMovement>();
        //normalGravity = rb.gravityScale;
    }

    private void Update()
    {
        isJumping = !playerMovement.IsGrounded;
        if (!attack.sp_isAttaking)
        {
            //ApplyGravity();
        }
    }

    //private void ApplyGravity()
    //{
    //    if (rb.linearVelocityY > 0)
    //    {
    //        rb.gravityScale = normalGravity;
    //    }
    //    else
    //    {
    //        rb.gravityScale = fallGravity;
    //    }
    //}

    private void OnEnable()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null) return;
        playerController.ActionsCancelled += CancelJump;
    }

    public void OnJump(InputValue value)
    {
        if (!isActiveAndEnabled) return;
        if (value.isPressed) //눌렀을 때
        {
            // 착지 판정은 PlayerMovement의 공용 접지 상태를 사용한다.
            if (!playerMovement.IsGrounded) return;
            if (playerController != null && !playerController.CanJump) return;
            movement.SetJumping(true);
            currentCharge = 0f;
            startTime = Time.time;
            isChargingJump = true;
        }
        else { // 뗐을 때
            if (!isChargingJump) return;
            if (playerController != null && !playerController.CanJump)
            {
                CancelJump();
                return;
            }
            isChargingJump = false;
            movement.SetJumping(false);
            duration = Time.time - startTime;
          //  Debug.Log("현재 시간 - 시작 시간 = " + duration);
            if(duration < 0.5f) //기본 점프
            {
                Debug.Log("기본 점프");
                BasicJump();
            }
            else //차지 점프
            {
                Debug.Log("차지 점프");
                ChargingJump();
            }
        }
    }

    public void CancelJump()
    {
        // 입력이 제한되면 충전 중인 점프를 취소한다.
        isChargingJump = false;
        movement?.SetJumping(false);
    }

    private void BasicJump()
    {
        //기본 애니메이션
        isJumping = true;
        playerMovement.Jump(jumpPower);
    }

    private void ChargingJump()
    {
      
       while(duration > 0f)
        {
            duration -= nextTime;
            currentCharge += addCharge;
        }
        float Ratio = Mathf.Clamp((Time.time - startTime) / maxTime, 0f, 1f);
        float Mult = Mathf.Lerp(1f, 2f, Ratio);
       // Debug.Log(Mult * jumpPower);
        isJumping = true;
        playerMovement.Jump(jumpPower * Mult);


    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.ActionsCancelled -= CancelJump;
        }
        CancelJump();
    }

}
