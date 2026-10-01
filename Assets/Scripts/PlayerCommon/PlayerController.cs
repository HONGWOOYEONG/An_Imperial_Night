using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerHealth), typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerMovement movement;
    private PlayerHealth health;
    private Coroutine stunCoroutine;
    private Coroutine bindCoroutine;
    public bool IsBound { get; private set; }

    public PlayerFSM FSM { get; private set; }
    public PlayerState CurrentState => FSM.CurrentState;
    public bool IsControlLocked => FSM.IsControlLocked;
    // 공격·방어 등의 입력 허용 여부와 일반 이동 허용 여부를 구분합니다.
    public bool CanAct => isActiveAndEnabled && !IsControlLocked;
    public bool CanMove => CanAct && FSM.ActionState != PlayerState.Dashing;
    public bool CanJump => CanAct && FSM.ActionState == PlayerState.Idle && movement.IsGrounded;
    public bool CanDash => CanAct && FSM.ActionState == PlayerState.Idle && movement.IsGrounded;
    public event Action<PlayerState> OnStateChanged;
    public event Action ActionsCancelled;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();

        FSM = new PlayerFSM(movement);
        FSM.OnStateChanged += state => OnStateChanged?.Invoke(state);
    }

    private void FixedUpdate()
    {
        if (!IsControlLocked && FSM.ActionState == PlayerState.Idle)
            FSM.RefreshState();
    }

    public bool TryStartAction(PlayerState state) => CanAct && FSM.TryStartAction(state);
    public void EndAction(PlayerState state) => FSM.EndAction(state);

    public void AddControlLock(string source)
    {
        if (!FSM.AddControlLock(source)) return;
        // 첫 제한이 걸릴 때만 진행 중인 행동과 입력을 중단합니다.
        if (FSM.ControlLockCount != 1) return;
        CancelActions();
        playerInput?.DeactivateInput();
    }

    public void ReleaseControlLock(string source)
    {
        if (!FSM.ReleaseControlLock(source)) return;
        // 경직이 끝나도 그로기·사망 등 다른 제한이 남았다면 입력을 켜지 않습니다.
        if (isActiveAndEnabled && !IsControlLocked && health != null && !health.IsDead)
            playerInput?.ActivateInput();
    }

    public void BeginStun(float duration)
    {
        if (CurrentState == PlayerState.Dead || duration <= 0f) return;
        // 연속 피격 시 이전 경직 타이머를 취소하고 시간을 다시 셉니다.
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        AddControlLock("Stun");
        stunCoroutine = StartCoroutine(StunFor(duration));
    }

    private IEnumerator StunFor(float duration)
    {
        yield return new WaitForSeconds(duration);
        stunCoroutine = null;
        ReleaseControlLock("Stun");
    }// 경직 상태를 유지하는 동안 입력을 차단하고, 시간이 지나면 해제

    public void BeginGroggy() => AddControlLock("Groggy");
    public void EndGroggy() => ReleaseControlLock("Groggy");

    public void BeginBind(float duration)
    {
        if (health != null && health.IsDead) return;
        if (bindCoroutine != null) StopCoroutine(bindCoroutine);
        IsBound = duration > 0f;
        if (!IsBound)
        {
            ReleaseControlLock("Bind");
            bindCoroutine = null;
            return;
        }

        AddControlLock("Bind");
        bindCoroutine = StartCoroutine(BindFor(duration));
    } // 연속 속박 시 이전 타이머를 취소하고 시간을 다시 카운트

    private IEnumerator BindFor(float duration)
    {
        yield return new WaitForSeconds(duration);
        bindCoroutine = null;
        IsBound = false;
        ReleaseControlLock("Bind");
    }// 속박 상태를 유지하는 동안 입력을 차단하고, 시간이 지나면 해제

    public void ClearBind()
    {
        if (bindCoroutine != null) StopCoroutine(bindCoroutine);
        bindCoroutine = null;
        IsBound = false;
        ReleaseControlLock("Bind");
    }// 속박 상태를 해제

    public void OnPlayerDeath()
    {
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }
        FSM.ReleaseControlLock("Stun");
        AddControlLock("Dead");
        ClearBind();
    }// 플레이어가 사망하면 경직 상태를 해제하고, 사망 제한을 걸며, 속박 상태를 해제

    public void OnPlayerRevive()
    {
        ClearBind();
        CancelActions();
        ReleaseControlLock("Dead");
    } // 플레이어가 부활하면 속박 상태를 해제하고, 진행 중인 행동을 취소하며, 사망 제한을 해제

    private void CancelActions()
    {
        // 각 기능이 자기 코루틴·히트박스·방어 판정을 정리하도록 요청
        movement?.ResetControlState();
        ActionsCancelled?.Invoke();
        FSM.ResetAction();
    }// 진행 중인 행동을 취소하고, 이동 상태를 초기화하며, 관련 이벤트를 호출

    private void OnDisable()
    {
        if (FSM == null) return;
        // Disable resets transient actions, but never revives a dead player.
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = null;
        if (bindCoroutine != null) StopCoroutine(bindCoroutine);
        bindCoroutine = null;
        IsBound = false;
        FSM.ReleaseControlLock("Bind");
        CancelActions();
        GetComponent<H_Posture>()?.ResetPosture();
        FSM.ReleaseControlLock("Stun");
        playerInput?.DeactivateInput();
    }// Disable 시 진행 중인 경직·속박 코루틴을 중단하고, 속박 상태를 해제하며, 진행 중인 행동을 취소하고, 입력을 비활성화

    private void OnEnable()
    {
        if (FSM == null) return;
        if (!IsControlLocked && health != null && !health.IsDead)
            playerInput?.ActivateInput();
        else
            playerInput?.DeactivateInput();
    }
}
