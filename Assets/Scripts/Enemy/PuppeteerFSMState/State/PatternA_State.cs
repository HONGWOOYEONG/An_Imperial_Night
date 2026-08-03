using System.Collections;
using UnityEngine;

//인형을 발로 차서 돌진 시키기

public class PatternA_State : IEnemyState
{
    float BASE_FPS = 60f;
    public float patternFrontDeley = 41f; //패턴 선딜레이
    public float[] frontDelay = { 5, 5, 5, 5, 30 }; //선딜레이
    public float[] backDelay = { 3, 3, 3, 3, 50 }; //후딜레이
    public float reBackDelay = 15f; //복귀 후 후딜레이
    public float[] attackHoldTime = { 2, 2, 2, 2, 5 }; //판정 유지시간

    float range_EandTg = 1.2f; //적과 플레이어의 최소 거리, 공격 코루틴이 시작되는 거리
    float rushSpeed = 8f;
    bool isRushing = false; //돌진 중인가
    bool startAttack = false; //공격 중인가
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternA 상태 시작");
        isRushing = true; //돌진 상태 on
    }
    public void Exit(E_PuppeteerController controller)
    {
        controller.currentCount = 0;
        controller.HitBox_A.SetActive(false); //히트박스 꺼줌
        Debug.Log("PatternA 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
       if(controller.targetPlayer == null) { return; }

        if (controller.targetPlayer != null)
        {
            controller.LookAtLocation(controller.targetPlayer.transform.position.x);
        }

        Vector2 playerPos = new Vector2(controller.targetPlayer.transform.position.x, controller.dollTransform.position.y);
        float distance_DandTg = Vector2.Distance(controller.dollTransform.position, playerPos);
        //돌진
        if (isRushing)
        {
            controller.dollTransform.position = Vector2.MoveTowards(controller.dollTransform.position, playerPos, rushSpeed * Time.deltaTime);

            if (distance_DandTg <= range_EandTg && !startAttack)
            {
                isRushing = false;
                controller.isAttaking_A = false;
                controller.StartCoroutine(StartAttack(controller));
            }
        }
    }

    IEnumerator StartAttack(E_PuppeteerController controller)
    {
        startAttack = true; //중복 실행 방지용        

        while (controller.currentCount < 5)
        {
            Debug.Log("현재 인형돌진 타수: " + controller.currentCount);
            if (controller.currentCount == 0)
            {
                yield return new WaitForSeconds(patternFrontDeley / BASE_FPS); //패턴 선딜
            }
            yield return new WaitForSeconds(frontDelay[controller.currentCount] / BASE_FPS); //선딜

         
            controller.isAttaking_A = true;
            controller.HitBox_A.SetActive(true); //히트박스 킴
            yield return new WaitForSeconds(attackHoldTime[controller.currentCount] / BASE_FPS);
            controller.HitBox_A.SetActive(false); //히트박스 끔
            controller.isAttaking_A = false;

            yield return new WaitForSeconds(backDelay[controller.currentCount] / BASE_FPS); //후딜

            if (controller.currentCount == 4)
            {
                yield return new WaitForSeconds(reBackDelay / BASE_FPS); //복귀 후 후딜레이
            }
            controller.currentCount++;
        }

        controller.ChangeState(controller.idle);
    }


}
