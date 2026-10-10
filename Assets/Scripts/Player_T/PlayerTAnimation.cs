using System;
using UnityEngine;

public class PlayerTAnimation : MonoBehaviour
{
    public Animator animator;
    private PlayerController controller;
    private PlayerMovement movement;

    [Header("idle 전환")]
    [SerializeField] private float idleDelay = 0.1f;
    private float lastMoveTime;

    private bool isDead = false;

    private void Awake()
    {
        animator   = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        movement   = GetComponent<PlayerMovement>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateAnimation(controller.CurrentState);
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isGrounded", movement.IsGrounded);
        animator.SetBool("hasMoveInput", movement.HasMoveInput);
        //--이 코드를 작성한 이유는 HasMoveInput 이 부분이 왼쪽으로 걷다가 오른쪽으로(그 반대의 경우도 같음) 걸으면
        //0이 되는 경우가 나와서 idle 처리가 되어서 딜레이를 약간 주게 했음--
        if (controller.CurrentState == PlayerState.Move)
        {
            lastMoveTime = Time.time;
        }
        else if (controller.CurrentState == PlayerState.Idle)
        {
            // 마지막 이동 이후 0.1초 이상 지났다면
            if (Time.time - lastMoveTime >= idleDelay)
            {
                animator.SetBool("isRun", false);
            }
        }
    }
    //컴포넌트가 활성화 될 때
    private void OnEnable()
    {
        if (controller != null)
        {
            controller.OnStateChanged += UpdateAnimation; //이벤트에서 함수 추가
        }
    }

    //컴포넌트가 비활성화 될 때
    private void OnDisable()
    {
        if (controller != null)
        {
            controller.OnStateChanged -= UpdateAnimation; //이벤트에서 함수 제거
        }
    }

    private void UpdateAnimation(PlayerState state)
    {
        if(state == PlayerState.Dead)
        {
            //--죽는 애니메이션 실행--
            PlayDeath();
            return;
        }

        if (isDead) return;

        switch (state)
        {
            case PlayerState.Idle:
                // Idle 전환은 Update에서 처리
                break;

            case PlayerState.Move:
                // 이동 애니메이션 실행
                lastMoveTime = Time.time;
                animator.SetBool("isRun", true);
                break;

            case PlayerState.Jump:
                animator.SetBool("isRun", false);
                break;

            case PlayerState.Fall:
                animator.SetBool("isRun", false);
                break;

            case PlayerState.Attacking:
                animator.SetBool("isRun", false);
                break;

            case PlayerState.Stunned:
                animator.SetBool("isRun", false);
                animator.SetTrigger("Hit");
                break;

            case PlayerState.Groggy:
                animator.SetBool("isRun", false);
                break;

            case PlayerState.Defending:
                animator.SetBool("isRun", false);
                break;

            case PlayerState.Ability:
                animator.SetBool("isRun", false);
                break;

            case PlayerState.Dashing:
                animator.SetBool("isRun", false);
                break;
        }
    }
    // 일반 점프 애니메이션
    public void PlayNormalJump()
    {
        if (isDead) return;
        animator.SetTrigger("NormalJump");
    }

    //--차지 점프 애니메이션--
    public void PlayChargeJump()
    {
        if (isDead) return;
        animator.SetTrigger("ChargeJump");
    }
    #region  약 공격 애니메이션
    //--1타--
    public void PlayLightAttack()
    {
        if (isDead) return;

        animator.SetBool("isRun", false);
        animator.SetTrigger("LightAttack");
    }
    //--2,3타--
    public void PlaySecondThirdAttack()
    {
        if (isDead) return;
        animator.SetBool("isSecondThirdAtk", true);
    }
    //--4타--
    public void PlayFinalAttack()
    {
        if (isDead) return;
        animator.SetBool("isFinalAtk", true);
    }
    //--콤보 애니메이션 파라미터 초기화--
    public void ResetLightAttack()
    {
        animator.SetBool("isSecondThirdAtk", false);
        animator.SetBool("isFinalAtk", false);

        animator.ResetTrigger("LightAttack");
    }
    #endregion

    //--사망 애니메이션--
    private void PlayDeath()
    {
        if (isDead) return;
        isDead = true;
        animator.SetBool("isRun", false);
        animator.SetBool("isDead", true);
    }

}
