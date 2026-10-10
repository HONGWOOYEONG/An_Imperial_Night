/// 작성자 : 유희일
using UnityEngine;

public class Boss_MoveState : Boss_Statebase
{
    public Boss_MoveState(Boss_Controller boss, string animParamName) : base(boss, animParamName) { }
    public override void Enter()
    {
        base.Enter();
        boss.Moter.HandleFlip();
    }

    // 바라보는 방향으로만 걷는다. 멈추는 것은 Boss_Statebase의 기본 FixedTick이 맡으므로
    // 이 상태를 벗어나면 따로 멈추는 호출이 필요 없다.
    // Move_FixedTick은 facing 방향으로만 걷는다. 걷는 중에 플레이어를 다시 보지 않으면
    // 플레이어가 등 뒤로 넘어갔을 때 반대쪽으로 계속 걸어간다.
    public override void Tick()
    {
        base.Tick();
        if (boss.Context.DistanceToTarget <= boss.Moter.StopDistance)
        {
            boss.FSM.ChangeState(boss.FSM.Idle);
            return;
        }
    }

    public override void FixedTick()
    {
        boss.Moter.Move_FixedTick();
    }

}
