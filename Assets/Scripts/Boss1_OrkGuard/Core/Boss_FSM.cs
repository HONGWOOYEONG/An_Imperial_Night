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

    // 구독자가 한 명도 없으면 이 필드는 null이다. event로 두면 밖에서 새 대입(=)을 못 하고
    // += 로만 붙을 수 있어, 나중에 구독자가 늘어도 먼저 붙은 쪽이 조용히 날아가지 않는다.
    public event Action<string> OnCurrentState;
    public Boss_IdleState Idle { get; private set; }
    public Boss_MoveState Move { get; private set; }
    public Boss_GroggyState Groggy { get; private set; }
    public Boss_DeadState Dead { get; private set; }
    public BossPS_a1 A1 { get;private set; }
    public BossPS_a2 A2 { get;private set; }
    public BossPS_a3 A3 { get;private set; }

    public Boss_FSM(Boss_Controller boss)
    {
        Idle = new Boss_IdleState(boss, "idle");
        Move = new Boss_MoveState(boss, "move");
        Groggy = new Boss_GroggyState(boss, "groggy");
        Dead = new Boss_DeadState(boss, "dead");

        A1 = new BossPS_a1(boss, "a1");
        A2 = new BossPS_a2(boss, "a2");
        A3 = new BossPS_a3(boss, "a3");
    }

    public void ChangeState(IState next)
    {
        // 구성 실수로 null이 와도 현재 상태를 유지한다. 여기서 Current를 비우면
        // 그 프레임부터 Tick이 통째로 멈춰 보스가 가만히 서 있는 버그가 된다.
        if (next == null) return;

        // 같은 상태로의 재진입은 막는다. Enter가 두 번 돌면 애니메이션 파라메터가 다시 세팅되어
        // 재생 중이던 동작이 처음으로 되감긴다. 공격을 연달아 잇는 것은 Boss_AttackState가
        // 내부에서 다음 패턴 인덱스로 넘기는 방식으로 처리하고, 상태 전환으로 잇지 않는다.
        if (next == Current) return;

        Current?.Exit();
        Current = next;
        Current.Enter();
        Debug.Log("현재 상태 : " + Current.Name);
        OnCurrentState?.Invoke(Current.Name);
    }

    // 보스의 갱신 주기는 Boss_Controller의 Update 하나로 통일한다.
    public void Tick() => Current?.Tick();

    // 물리 갱신도 Boss_Controller의 FixedUpdate 하나로 통일한다.
    public void FixedTick() => Current?.FixedTick();
}
