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
    bool isRushing = true; //돌진 중인가
    bool startAttack = false; //공격 중인가
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternA 상태 시작");
    }
    public void Exit(E_PuppeteerController controller)
    {
        isRushing = true;
        startAttack = false;
        controller.currentCount = 0;
        controller.isAttaking_A = false;
        controller.HitBox_A.SetActive(false); //히트박스 꺼줌
        Debug.Log("PatternA 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
       if(controller.targetPlayer == null) { return; }

        controller.LookAtLocation(controller.targetPlayer.transform.position.x); //flip

        Vector2 playerPos = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y);
        float distance = Vector2.Distance(controller.transform.position, playerPos);

        if (distance <= range_EandTg && !startAttack) //일정 거리보다 가까워지면
        {
            isRushing = false;
            startAttack = true;
            controller.StartCoroutine(StartAttack(controller));
        }
        //돌진
        if (isRushing)
        {           
            controller.transform.position = Vector2.MoveTowards(controller.transform.position, playerPos, rushSpeed * Time.deltaTime);
        }
    }

    IEnumerator StartAttack(E_PuppeteerController controller)
    {      
        while (controller.currentCount < 5)
        {
            Debug.Log("현재 인형돌진 타수: " + controller.currentCount);

            if (controller.currentCount == 4) //마지막 5타 공격 방향을 위한
            {
                if(controller.rangedDealer != null && controller.meleeDealer != null)
                {
                    Vector2 rangedD = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
                    Vector2 meleeD = new Vector2(controller.meleeDealer.transform.position.x, controller.transform.position.y);
                    float rangedDis = Vector2.Distance(controller.transform.position, rangedD); //적과 원거리 거리 계산
                    float meleeDis = Vector2.Distance(controller.transform.position, meleeD); //적과 근거리 거리 계산

                    controller.targetPlayer = rangedDis > meleeDis ? controller.meleeDealer : controller.rangedDealer;
                    Debug.Log("5타째 타겟 변경 완료: " + controller.targetPlayer.name);
                }             
            }
           
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
