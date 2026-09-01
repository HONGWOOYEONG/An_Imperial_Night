using System.Collections;
using UnityEngine;

//그냥 평타 치기
public class P_PatternE_State : IPuppeteerState
{
    Collider2D target;
    float BASE_FPS = 60f;

    float attackDuration = 5f;
    float frontDelay = 120f; //1타 선딜레이
    float backDelay = 30f;
    float moveSpeed = (5f * 1.3f); //회월스님의 1.3배

    bool isAttack = false;
    int randAtkNum; //랜덤 공격 숫자를 담을 변수
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternD 상태 시작");

        randAtkNum = Random.Range(1, 4);
        Debug.Log("랜덤한 공격 횟수 : " + randAtkNum);
        if (controller.targetPlayer != null)
        {
            target = controller.targetPlayer;
        }
    }

    public void Exit(E_PuppeteerController controller)
    {
        isAttack = false;
        controller.isAttaking_E = false;
        controller.HitBox_E.SetActive(false);
        Debug.Log("PatternD 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        if (controller.targetPlayer != null) //flip
        {
            controller.LookAtLocation(controller.targetPlayer.transform.position.x);
        }

        if (!target) { return; }

        Vector2 targetPos = target.transform.position;
        Vector2 myPos = controller.transform.position;
        if (!controller.isInTargetPlayer)
        {
            Debug.Log( target.name + "에게 이동 중");   
            controller.transform.position = Vector2.MoveTowards(myPos, targetPos, moveSpeed * Time.deltaTime);
        } 

        //공격 실행
        if (!isAttack && controller.isInTargetPlayer)
        {
            isAttack = true;
            controller.StartCoroutine(StartAttack(controller));
        }
    }
    IEnumerator StartAttack(E_PuppeteerController controller)
    {
        int num = 1;
        while(randAtkNum >= num)
        {
            Debug.Log(num + "타 시작");
            yield return new WaitForSeconds(frontDelay / BASE_FPS);
            controller.isAttaking_E = true;
            controller.HitBox_E.SetActive(true);
            yield return new WaitForSeconds(attackDuration / BASE_FPS);
            controller.HitBox_E.SetActive(false);
            controller.isAttaking_E = false;
            yield return new WaitForSeconds(backDelay / BASE_FPS);
            num+=1;
            yield return null;
        }
        controller.ChangeState(controller.states["idle"]);
    }
}
