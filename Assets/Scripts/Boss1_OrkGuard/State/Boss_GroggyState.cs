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

    // 지속 시간과 해제 판단은 Boss_AI가 가진다. 여기서는 판단을 요청만 한다.
    public override void Tick()
    {
        boss.AI.Decide();
    }
}
