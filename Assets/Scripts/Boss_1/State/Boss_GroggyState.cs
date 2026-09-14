/// 작성자 : 유희일
using UnityEngine;

public class Boss_GroggyState : Boss_Statebase
{
    public Boss_GroggyState(Boss_Controller boss, Animator anim) : base(boss, anim, "groggy") { }

    public override void Enter()
    {
        base.Enter();

        // 패턴 도중에 그로기가 터질 수 있다. 끊지 않으면 공중에 뜬 채로 그로기 애니메이션이 재생된다.
        boss.Moter.CancelKinematicMove();
        boss.Moter.Stop();

        boss.AI.Begin_Groggy();
    }

    // 지속 시간과 해제 판단은 Boss_AI가 가진다. 여기서는 판단을 요청만 한다.
    public override void Tick()
    {
        boss.AI.Decide();
    }
}
