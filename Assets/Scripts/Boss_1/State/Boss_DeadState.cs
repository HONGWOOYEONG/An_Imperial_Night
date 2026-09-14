/// 작성자 : 유희일
using UnityEngine;

public class Boss_DeadState : Boss_Statebase
{
    public Boss_DeadState(Boss_Controller boss, Animator anim) : base(boss, anim, "dead") { }

    public override void Enter()
    {
        base.Enter();

        boss.Moter.CancelKinematicMove();
        boss.Moter.Stop();
    }

    // 사망 뒤에는 아무 판단도 하지 않는다. Tick을 비워 두는 것이 그 선언이다.
    // 나가는 전환도 없다. 되살아나는 것은 라이프포인트가 Boss_Health에서 처리하므로
    // 이 상태에 들어왔다는 것은 라이프가 0이라는 뜻이다.
}
