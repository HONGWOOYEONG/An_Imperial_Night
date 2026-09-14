/// 작성자 : 유희일
using UnityEngine;

public class Boss_IdleState : Boss_Statebase
{
    public Boss_IdleState(Boss_Controller boss, Animator anim) : base(boss, anim, "idle") { }

    public override void Enter()
    {
        base.Enter();

        boss.Moter.Stop();
    }

    public override void Tick()
    {
        boss.Moter.FacePlayer(boss.Context.TargetPosition);
        boss.AI.Decide();
    }
}
