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

    // 지속 시간과 해제 판단은 Boss_AI가 가진다.
    // 여기서 Pattern_Decide를 부르면 그로기 중에 패턴 판단이 돌아 Move나 패턴으로 끌려나간다.
    // Boss_Controller가 FSM.Tick을 AI.Tick보다 먼저 돌리므로, 그로기가 한 프레임 만에 풀렸다.
    // 비워 두는 것이 "이 상태는 스스로 나가지 않는다"는 선언이다.
    public override void Tick() { }
}
