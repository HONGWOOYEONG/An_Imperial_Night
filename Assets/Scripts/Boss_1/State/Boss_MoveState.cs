/// 작성자 : 유희일
using UnityEngine;

public class Boss_MoveState : Boss_Statebase
{
    public Boss_MoveState(Boss_Controller boss, Animator anim) : base(boss, anim, "move") { }

    public override void Tick()
    {
        boss.Moter.FacePlayer(boss.Context.TargetPosition);

        // 바라보는 방향으로만 걷는다. 방향 판단은 FacePlayer 한 곳에만 둔다.
        boss.Moter.Move(boss.Moter.Facing);

        boss.AI.Decide();
    }

    public override void Exit()
    {
        base.Exit();

        // 주의: 여기서 멈추지 않으면 다음 상태로 넘어가도 이동 방향이 남아 계속 걸어간다.
        boss.Moter.Stop();
    }
}
