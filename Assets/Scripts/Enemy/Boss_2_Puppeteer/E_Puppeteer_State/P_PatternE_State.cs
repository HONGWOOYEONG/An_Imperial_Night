using System.Collections;
using UnityEngine;

//그냥 평타 치기
public class P_PatternE_State : IPuppeteerState
{
    E_PuppeteerAction action = new E_PuppeteerAction();
    Collider2D target;
    float BASE_FPS = 60f;

    float attackDuration = 5f;
    float frontDelay = 120f; //1타 선딜레이
    float backDelay = 30f;


    bool isAttack = false;
    int randAtkNum; //랜덤 공격 숫자를 담을 변수
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternE 상태 시작");
        randAtkNum = Random.Range(1, 4);
        Debug.Log("랜덤한 공격 횟수 : " + randAtkNum);
        controller.StartCoroutine(StartNormalAttack(controller));

    }

    public void Exit(E_PuppeteerController controller)
    {
        isAttack = false;
        controller.isAttaking_E = false;
        controller.HitBox_E.SetActive(false);
        Debug.Log("PatternE 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        if (controller.targetPlayer != null) //flip
        {
            controller.LookAtLocation(controller.targetPlayer.transform.position.x);
        }

    }
    IEnumerator StartNormalAttack(E_PuppeteerController controller)
    {
        int num = 1;
        while(randAtkNum >= num)
        {
            Debug.Log(num + "타 시작");
            yield return new WaitForSeconds(frontDelay / BASE_FPS);           
            action.NormalAttack(controller, attackDuration);
            yield return new WaitForSeconds(backDelay / BASE_FPS);
            num+=1;
            yield return null;
        }
        controller.ChangeState(controller.states["idle"]);
    }
}
