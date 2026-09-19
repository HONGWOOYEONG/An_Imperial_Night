/// 작성자 : 유희일
using System;
using UnityEngine;

/// <summary>
/// 보스의 상태 인스턴스를 전부 소유하고, 상태 전환 절차(Exit → 교체 → Enter)를 돌리는 유일한 지점이다.
/// 전환 순서를 한 클래스가 통째로 쥐고 있어야 "Exit이 돌기 전에 다음 Enter가 먼저 도는" 순서 사고가 안 난다.
/// MonoBehaviour가 아니다. 생성과 Tick 호출은 Boss_Controller가 책임진다.
///
/// 여기 두지 않는 것
/// - 어떤 상태로 갈지의 판단 : 대기/이동/공격은 Boss_AI, 그로기/사망은 Boss_Health가 호출한다.
/// - 상태 안에서 실제로 무엇을 하는가 : Boss_Statebase 파생 클래스가 가진다.
/// </summary>
public class Boss_FSM
{
    public IState Current { get; private set; }

    public event Action<string> OnCurrentState;
    public Boss_IdleState Idle { get; private set; }
    public Boss_MoveState Move { get; private set; }
    public Boss_DeadState Dead { get; private set; }
    public Boss_GroggyState Groggy { get; private set; }

    public Boss_PatternState Pattern { get; private set; }

    public Boss_FSM(Boss_Controller boss)
    {
        Idle = new Boss_IdleState(boss, "idle");
        Move = new Boss_MoveState(boss, "move");
        Dead = new Boss_DeadState(boss, "dead");
        Groggy = new Boss_GroggyState(boss, "grogy");

        Pattern = new Boss_PatternState(boss);
    }

    public void ChangeState(IState next)
    {
        if (next == null) return;

        if (next == Current) return;

        Current?.Exit();
        Current = next;
        Current.Enter();
        Debug.Log("현재 상태 : " + Current.Name);
        OnCurrentState?.Invoke(Current.Name);
    }

    public void Tick() => Current?.Tick();
    public void FixedTick() => Current?.FixedTick();
}
