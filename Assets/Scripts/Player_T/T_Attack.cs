using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditorInternal.VersionControl.ListControl;


public class T_Attack : MonoBehaviour
{

    I_TAttackState currentState;
    Dictionary<string, I_TAttackState> states;

    public float BASE_FPS = 60f;

    public PlayerTAnimation animation;
    public Rigidbody2D rb = null;
    public PlayerMovement movement = null;
    public T_DriveGauge t_DriveGauge = null;
    public T_Jump jump = null;
    public PlayerController playerController = null;
    public Transform createPos = null; //공격이 나가는 위치

    [Header("약공 or 강공")]
    [SerializeField] float triggerTime = 65f;
    [SerializeField] float triggerTimer = 0f;
    private bool isInputKey = false;
    public GameObject commonAttackObject; //1,2,3타에서 사용될 오브젝트
    public GameObject finalAttackObject; //4타에서 사용될 오브젝트

    [Header("강공")]
    public GameObject h_Obj; //강공 오브젝트

    [Header("특공")]
    [SerializeField] float sp_drvieDecrease = 100f;
    public bool sp_isAttaking = false;


    // 동시에 진행 중인 공격 수를 기록해 마지막 공격이 끝날 때 FSM 상태를 해제한다.
    public int activeAttackCount;
    void Start()
    {
        states = new Dictionary<string, I_TAttackState> {
            { "lightAttack", new T_LightAtkState()},
            { "heavyAttack", new T_HeavyAtkState()},
            { "specialAttack",new T_SpecialAtkState() }
        };

        
        movement = GetComponent<PlayerMovement>();
        t_DriveGauge = GetComponent<T_DriveGauge>();
        jump = GetComponent<T_Jump>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        animation = GetComponent<PlayerTAnimation>();

        triggerTime /= BASE_FPS;
    }


    void Update()
    {
        if (isInputKey)
        {
            triggerTimer += Time.deltaTime; 
        }
        if (currentState != null)
        {
            currentState.Update(this);
        }
    }

    private void OnEnable()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null) return;
        playerController.ActionsCancelled += CancelAttack;
    }

    public void OnLightAttack(InputValue value)
    {
        if (!isActiveAndEnabled) return;
        if (value.isPressed)
        {
            if (playerController != null && !playerController.CanAct) return;
            triggerTimer = 0f;
            isInputKey = true;
        }
        else
        {
            //Debug.Log("공격 입력 시간 triggerTimer = " + triggerTimer);
            isInputKey = false;
            if (playerController != null && !playerController.CanAct) return;

            //--입력 시간에 따라서 약공, 강공 상태 변환--
            if (triggerTimer <= triggerTime)
            {
                //--상태를 바꾸지 않고 입력만 전달--
                if (currentState == states["lightAttack"])
                {
                    //--형 변환 T_LightAtkState타입으로 변환을 시도해서 성공하면 해당 객체를 넣고 실패하면 null을 넣음--
                    T_LightAtkState lightState = states["lightAttack"] as T_LightAtkState;

                    lightState?.AddInput(this); //입력 전달
                }
                else
                {
                    if (playerController != null && !playerController.TryStartAction(PlayerState.Attacking)) return;

                    ChangteState(states["lightAttack"]); //짧게 누르면 약공
                }
            }
            else
            {
                if (playerController != null && !playerController.TryStartAction(PlayerState.Attacking)) return;

                ChangteState(states["heavyAttack"]);//길게누르면 강공
            }
        }
    }

    public void OnAbility(InputValue value)
    {
        if (!isActiveAndEnabled) return;
        bool isbunout = t_DriveGauge.isBunOut;
        float currentDriveGauge = t_DriveGauge.driveGauge;
        if (value.isPressed && !isbunout && currentDriveGauge > sp_drvieDecrease && !sp_isAttaking)
        {
            Debug.Log("특수 공격 시작");
            if (playerController != null && !playerController.TryStartAction(PlayerState.Ability)) return;
            t_DriveGauge.DecreaseDriveGauge(sp_drvieDecrease);
            ChangteState(states["specialAttack"]);
        }
    }
    private void ChangteState(I_TAttackState state)
    {
        if (currentState != null)
        {
            rb.gravityScale = 0f;
            currentState.Exit(this);
        }
        currentState = state;
        currentState.Enter(this);
    }

    //--공격이 맞았을 경우 드라이브 게이지가 회복되게 도와주는 함수--
    public void OnAttackHit(float amount)
    {
        t_DriveGauge.HealthSomeOfDriveGauge(amount);
    }

    public float FramesToSeconds(float seconds)
    {
        return seconds / BASE_FPS;
    }

    public void FinishAttack()
    {
        // 남은 공격이 없을 때만 컨트롤러에 공격 종료를 알린다.
        activeAttackCount = Mathf.Max(0, activeAttackCount - 1);
        if (activeAttackCount == 0) playerController?.EndAction(PlayerState.Attacking);
    }

    public void CancelAttack()
    {
        // 공격이 중단되면 코루틴, 이동 제한, 공격 상태를 함께 정리한다.
        StopAllCoroutines();

        if (currentState != null)
        {
            currentState.Exit(this);
            currentState = null;
        }

        activeAttackCount = 0;
        isInputKey = false;
        triggerTimer = 0f;

        if (movement != null)
        {
            movement.ReleaseMovementLock(this);
            movement.RestoreGravity();
        }
        playerController?.EndAction(PlayerState.Attacking);
        playerController?.EndAction(PlayerState.Ability);
    }

    public GameObject InstantiateObject(GameObject obj, Vector2 createPos)
    {
        return Instantiate(obj, createPos, Quaternion.identity);
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.ActionsCancelled -= CancelAttack;
        }
        CancelAttack();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, 20f);
    }

    //--1타 애니메이션 이벤트--
    public void Event_FirstAttack()
    {
        if (currentState is T_LightAtkState lightState)
        {
            lightState.FirstAttack(this);
        }
    }

    //--2타 애니메이션 이벤트--
    public void Event_SecondAttack()
    {
        Debug.Log("2타 애니메이션 이벤트 호출!");

        if (currentState is T_LightAtkState lightState)
        {
            Debug.Log("2타 상태 확인 성공!");

            lightState.SecondAttack(this);
        }
        else
        {
            Debug.LogWarning("현재 상태가 약공 상태가 아님!");
        }
    }

    //--3타 애니메이션 이벤트--
    public void Event_ThirdAttack()
    {
        if (currentState is T_LightAtkState lightState)
        {
            lightState.ThirdAttack(this);
        }
    }

    //--4타 애니메이션 이벤트--
    public void Event_FinalAttack()
    {
        if (currentState is T_LightAtkState lightState)
        {
            lightState.FinalAttack(this);
        }
    }

    public void Event_CheckFirstCombo()
    {
      
        if (currentState is T_LightAtkState lightState)
        {
            Debug.Log(
        $"[콤보 판정] 시간: {Time.time:F3}, " +
        $"inputCount: {lightState.inputCount}"
    );
            lightState.CheckFirstCombo(this);
        }
    }
    public void Event_CheckSecondThirdCombo()
    {
        if (currentState is T_LightAtkState lightState)
        {
            lightState.CheckSecondThirdCombo(this);
        }
    }
}
