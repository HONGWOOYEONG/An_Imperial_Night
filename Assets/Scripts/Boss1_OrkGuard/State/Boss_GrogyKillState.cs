using UnityEngine;

public class Boss_GrogyKillState : Boss_Statebase
{
    public Boss_GrogyKillState(Boss_Controller boss, string animParamName) : base(boss, animParamName) { }

    public override void Exit()
    {
        base.Exit();
        boss.Health.Init_BalanceAndHp();
    }
}
