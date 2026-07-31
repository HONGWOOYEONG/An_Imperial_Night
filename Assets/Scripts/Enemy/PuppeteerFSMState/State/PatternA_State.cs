using System.Collections;
using UnityEngine;

//인형을 발로 차서 돌진 시키기
public class Rush
{
    public int currentCount = 0;

    public float patternFrontDeley = 41f; //패턴 선딜레이
    public float[] frontDelay = { 5, 5, 5, 5, 30 }; //선딜레이
    public float[] backDelay = { 3, 3, 3, 3, 50 }; //후딜레이
    public float reBackDelay = 15f; //복귀 후 후딜레이
    public float[] damage = { 300, 300, 300, 300, 500 }; //피해량
    public float[] groggyDamage = { 270, 270, 270, 270, 320 }; //체간 피해량
    public float[] decreaseDrive = { 250, 250, 250, 250, 500 };//방어 시 드라이브게이지 감소량
    public float[] addGroggy = { 70, 70, 70, 70, 400 }; //방어 시 체간 게이지 증가량
    public float[] attackHoldTime = { 2, 2, 2, 2, 5 }; //판정 유지시간
}

public class PatternA_State : IEnemyState
{
    float BASE_FPS = 60f;
    Rush rush;
    Collider2D target;
    float range_EandTg = 1.2f; //적과 플레이어의 최소 거리, 공격 코루틴이 시작되는 거리
    float rushSpeed = 8f;
    bool isRushing = false; //돌진 중인가
    bool startAttack = false; //공격 중인가
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("인형을 발로 차서 돌진시키기 실행");
        rush = new Rush();
        isRushing = true; //돌진 상태 on
        controller.attackHitBox.SetActive(true); //히트박스 킴
        controller.isAttaking = true;

        if (controller.targetPlayer != null)
        {
            target = controller.targetPlayer;
        }

    }
    public void Exit(E_PuppeteerController controller)
    {
        controller.attackHitBox.SetActive(false); //히트박스 꺼줌
        Debug.Log("인형 돌진 상태 해제");
    }

    public void Update(E_PuppeteerController controller)
    {
        if (target == null)
        {
            return;
        }

        Vector2 playerPos = new Vector2(target.transform.position.x, controller.dollTransform.position.y);
        float distance_DandTg = Vector2.Distance(controller.dollTransform.position, playerPos);
        //돌진
        if (isRushing)
        {
            controller.dollTransform.position = Vector2.MoveTowards(controller.dollTransform.position, playerPos, rushSpeed * Time.deltaTime);

            if (distance_DandTg <= range_EandTg && !startAttack)
            {
                isRushing = false;
                controller.isAttaking = false;
                controller.StartCoroutine(StartAttack(controller));
            }
        }
    }

    IEnumerator StartAttack(E_PuppeteerController controller)
    {
        startAttack = true; //중복 실행 방지용
        rush.currentCount = 0;

        while (rush.currentCount < 5)
        {
            Debug.Log("현재 인형돌진 타수: " + rush.currentCount);
            if (rush.currentCount == 0)
            {
                yield return new WaitForSeconds(rush.patternFrontDeley / BASE_FPS); //패턴 선딜
            }
            yield return new WaitForSeconds(rush.frontDelay[rush.currentCount] / BASE_FPS); //선딜

            controller.dollHitBox.SetDamage(rush.damage[rush.currentCount]); //현재의 데미지 값을 보냄
            controller.isAttaking = true;
            controller.attackHitBox.SetActive(true); //히트박스 킴
            yield return new WaitForSeconds(rush.attackHoldTime[rush.currentCount] / BASE_FPS);
            controller.isAttaking = false;
            controller.attackHitBox.SetActive(false); //히트박스 끔

            yield return new WaitForSeconds(rush.backDelay[rush.currentCount] / BASE_FPS); //후딜

            if (rush.currentCount == 4)
            {
                yield return new WaitForSeconds(rush.reBackDelay / BASE_FPS); //복귀 후 후딜레이
            }
            rush.currentCount++;
        }

        controller.ChangeState(controller.idle);
    }


}
