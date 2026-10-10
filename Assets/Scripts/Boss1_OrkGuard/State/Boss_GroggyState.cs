/// 작성자 : 유희일
using UnityEngine;

public class Boss_GroggyState : Boss_Statebase
{
    public Boss_GroggyState(Boss_Controller boss, string animParamName) : base(boss, animParamName) { }

    public override void Enter()
    {
        base.Enter();

        boss.AI.Begin_Groggy();
    }
    public override void Exit()
    {
        base.Exit();
        boss.Health.Init_Balance();
    }
}
