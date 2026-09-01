using System.Collections;
using UnityEngine;

//헤비 레리어트→거미줄 깔기→머리 박치기(가불)
public class P_PatternC_State : IPuppeteerState
{
    E_PuppeteerAction action = new E_PuppeteerAction();
    private float addRange = 3f;

    [Header("HeavyLariat")]
    private float frontDelay_Lariat = 5f;
    private float backDelay_Lariat = 5f;

    [Header("HeadButt")]
    private float frontDelay_Head = 5f;
    private float backDelay_Head = 5f;

    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternC 상태 시작");
        controller.StartCoroutine(StartAttak(controller));

    }

    public void Exit(E_PuppeteerController controller)
    {
      
        Debug.Log("PatternC 상태 종료");
    }
    
    public void Update(E_PuppeteerController controller)
    {
       
    }
    private IEnumerator StartAttak(E_PuppeteerController controller)
    {
        yield return controller.StartCoroutine(action.HeavyLariat(controller, frontDelay_Lariat, backDelay_Lariat));
        Debug.Log("현재 타겟 : " + controller.targetPlayer.name);
        Vector2 myPos = controller.transform.position;
        Vector2 targetPos = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y);
        Vector2 dirToTarget = (targetPos - myPos).normalized; //방향
        Vector2 spiderWebPos = targetPos + dirToTarget * addRange; //거미줄 위치
        Debug.Log("거미줄 위치 = " + spiderWebPos);
        action.CreateSpiderWeb(controller.spiderWeb,controller.throwFire.transform.position ,spiderWebPos);

        if (!action.isThrowSpiderWeb)
        {
            yield return controller.StartCoroutine(action.HeadButt(controller, frontDelay_Head, backDelay_Head));
        }
        controller.ChangeState(controller.states["idle"]);
    }

}
