using UnityEngine;

//말처럼 달리기
public class PatternC_State : IEnemyState
{
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternC 상태 시작");
    }

    public void Exit(E_PuppeteerController controller)
    {
        Debug.Log("PatternC 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        throw new System.NotImplementedException();
    }

   
}
