using UnityEngine;

public class BossPS_a3 : Boss_Statebase
{
    public BossPS_a3(Boss_Controller boss, string animParamName) : base(boss, animParamName)
    {
        endIntervalTime = 1f;
    }
    public override void Enter()
    {
        base.Enter();
        anim.SetTrigger("next");
    }

    public override void Tick()
    {
        base.Tick();

        if(timer > endIntervalTime)
        {
            boss.FSM.ChangeState( boss.FSM.Idle);
        }
    }
    
}
