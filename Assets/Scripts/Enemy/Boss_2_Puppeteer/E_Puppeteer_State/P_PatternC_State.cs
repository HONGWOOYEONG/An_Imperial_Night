using System.Collections;
using UnityEngine;

//헤비 레리어트→거미줄 깔기→머리 박치기(가불)
public class P_PatternC_State : IPuppeteerState
{
    E_PuppeteerAction action = new E_PuppeteerAction();
    private float addRange = 3f;

    [Header("HeavyLariat")]
    private float frontDelayLariat = 5f;
    private float backDelayLariat = 5f;

    [Header("HeadButt")]
    private float frontDelayButt = 5f;
    private float backDelayButt = 5f;

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
        yield return new WaitForSeconds(frontDelayLariat/controller.BASE_FPS);
        action.HeavyLariat(controller);
        yield return new WaitForSeconds(backDelayLariat/controller.BASE_FPS);


        Debug.Log("현재 타겟 : " + controller.targetPlayer.name);
        Vector2 myPos = controller.transform.position;
        Vector2 targetPos = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y);
        Vector2 dirToTarget = (targetPos - myPos).normalized; //방향
        Vector2 spiderWebPos = targetPos + dirToTarget * addRange; //거미줄 위치
        action.ShootWeb(controller.spiderWeb,controller.throwFire.transform.position ,spiderWebPos);

        Debug.Log(action.isThrowSpiderWeb);
        if (!action.isThrowSpiderWeb) 
        {
            //거미줄 던지기가 끝나는 시점에 머리박치기 실행
            yield return new WaitForSeconds(frontDelayButt / controller.BASE_FPS);
            yield return action.HeadButt(controller);
            yield return new WaitForSeconds(backDelayButt / controller.BASE_FPS);
        }
       
    }

}
