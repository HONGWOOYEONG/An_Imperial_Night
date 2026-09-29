using System;
using System.Collections.Generic;

// 이동 상태와 행동 상태, 행동을 막는 상태를 한 곳에서 조회하기 위한 값입니다.
public enum PlayerState
{
    Idle,
    Move,
    Jump,
    Fall,
    Attacking,
    Defending,
    Ability,
    Dashing,
    Stunned,
    Groggy,
    Dead
}

// 상태 전환 규칙만 관리합니다. 입력 차단과 기능 중단은 PlayerController가 수행합니다.
public sealed class PlayerFSM
{
    private readonly HashSet<string> controlLocks = new HashSet<string>();
    private readonly PlayerMovement movement;
    private PlayerState actionState = PlayerState.Idle;

    public PlayerFSM(PlayerMovement movement) => this.movement = movement;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public PlayerState ActionState => actionState;
    public int ControlLockCount => controlLocks.Count;
    public bool IsControlLocked => controlLocks.Count > 0;
    public event Action<PlayerState> OnStateChanged;

    public bool TryStartAction(PlayerState state)
    {
        // 한 번에 하나의 행동만 실행합니다. 같은 행동의 재요청은 콤보 등에 사용할 수 있습니다.
        if (IsControlLocked || !IsActionState(state)) return false;
        if (actionState != PlayerState.Idle && actionState != state) return false;
        actionState = state;
        RefreshState();
        return true;
    }

    public void EndAction(PlayerState state)
    {
        if (actionState != state) return;
        ResetAction();
    }

    public void ResetAction()
    {
        actionState = PlayerState.Idle;
        RefreshState();
    }

    public bool AddControlLock(string source)
    {
        // 경직과 그로기가 겹쳐도 각 원인이 따로 해제되도록 저장합니다.
        if (string.IsNullOrEmpty(source) || !controlLocks.Add(source)) return false;
        if (controlLocks.Count == 1) actionState = PlayerState.Idle;
        RefreshState();
        return true;
    }

    public bool ReleaseControlLock(string source)
    {
        if (!controlLocks.Remove(source)) return false;
        RefreshState();
        return true;
    }

    public void RefreshState()
    {
        // 사망 > 그로기 > 기타 조작 제한 > 현재 행동 > 지상/공중 이동 순서입니다.
        PlayerState nextState;
        if (controlLocks.Contains("Dead")) nextState = PlayerState.Dead;
        else if (controlLocks.Contains("Groggy")) nextState = PlayerState.Groggy;
        else if (IsControlLocked) nextState = PlayerState.Stunned;
        else if (actionState != PlayerState.Idle) nextState = actionState;
        else if (movement == null) nextState = PlayerState.Idle;
        else if (!movement.IsGrounded)
            nextState = movement.Velocity.y > 0.01f ? PlayerState.Jump : PlayerState.Fall;
        else nextState = movement.HasMoveInput ? PlayerState.Move : PlayerState.Idle;

        if (CurrentState == nextState) return;
        CurrentState = nextState;
        OnStateChanged?.Invoke(nextState);
    }

    private static bool IsActionState(PlayerState state)
    {
        return state == PlayerState.Attacking || state == PlayerState.Defending ||
               state == PlayerState.Ability || state == PlayerState.Dashing;
    }
}
