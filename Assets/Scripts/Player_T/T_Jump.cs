using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
public class T_Jump : MonoBehaviour
{
    private T_Attack attack;
    private PlayerMovement movement;
    private Rigidbody2D rb;
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

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attack = GetComponent<T_Attack>();
        movement = GetComponent<PlayerMovement>();
        //normalGravity = rb.gravityScale;
    }

    private void Update()
    {
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

    public void OnJump(InputValue value)
    {
        if (value.isPressed) //눌렀을 때
        {
            if (!isGrounded) return;
            movement.SetJumping(true);
            currentCharge = 0f;
            startTime = Time.time;
        }
        else { // 뗐을 때
            if (!isGrounded) return;
            movement.SetJumping(false);
            duration = Time.time - startTime;
          //  Debug.Log("현재 시간 - 시작 시간 = " + duration);
            if(duration < 0.1f) //기본 점프
            {
                //Debug.Log("기본 점프");
                BasicJump();
            }
            else //차지 점프
            {
                //Debug.Log("차지 점프");
                duration = duration > maxTime ? maxTime : duration; 
                Charging();
            }
        }
    }

    private void BasicJump()
    {
        //기본 애니메이션
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    private void Charging()
    {
      
       while(duration > 0f)
        {
            duration -= nextTime;
            currentCharge += addCharge;
        }
        float Ratio = currentCharge / maxCharge;
        float Mult = Mathf.Lerp(1f, 2f, Ratio);
       // Debug.Log(Mult * jumpPower);
        rb.AddForce(Vector2.up * (jumpPower * Mult), ForceMode2D.Impulse);


    }

  
    private void OnCollisionEnter2D(Collision2D collision) //바닥에 착지 할때
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isJumping = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision) //뛰어 오르기 시작할 때
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            isJumping = true;
        }
    }
}
