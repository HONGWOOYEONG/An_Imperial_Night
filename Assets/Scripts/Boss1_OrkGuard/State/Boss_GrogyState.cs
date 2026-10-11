/// 작성자 : 유희일
using UnityEngine;

public class Boss_GrogyState : Boss_Statebase
{
    public Boss_GrogyState(Boss_Controller boss, string animParamName) : base(boss, animParamName) { }
    public override void Enter()
    {
        base.Enter();
        boss.AI.Begin_Groggy();
    }
    public override void Tick()
    {
        base.Tick();
        if (Time.time >= boss.AI.GrogyEndTime)
        {
            boss.Health.Init_BalanceAndHp(); // 그로기 시간이 지나서 회복.
            boss.FSM.ChangeState(boss.FSM.Idle);
        }
    }
    public override void Exit()
    {
        base.Exit();
        boss.AI.End_Groggy();
    }
}
