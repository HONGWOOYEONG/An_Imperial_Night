using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
//체간
//드라이브 게이지
//피해량
//히트 박스(공중 히트박스 ,  착지 공격 히트박스)


//평타 2회 후 원거리 딜러에게 점프
public class PatternB_State : IEnemyState
{
    Enemy_MoveBox moveBox;
    float BASE_FPS = 60f;
    Collider2D target;
    Rigidbody2D rb;
    bool startB = false; //공격 시작을 위한 변수

    [Header("딜레이")]
    float firstDelay = 18f;// 1/2타 선딜
    float backDelay = 10f; // 1/2타 후딜
    float HitDuration = 3f; // 판정 유지 시간
    float jumpFirstDelay = 27f; //점프 선딜레이
    float firstAtkDelay = 4f; //착지 후 공격 선딜레이
    float landHitDuration = 5f; //착지 후 공격 판정 유지시간
    float backAtkDelay = 50f; //착지 후 공격 후딜레이

    [Header("처음 대상에게 걸어갈 때")]
    Vector2 targetPos;
    Vector2 myPos;
    float moveSpeed = 5f; //걸어갈 속도

    [Header("점프에 필요한 변수")]
    float jumpForce = 6f; //최고점 높이
    float maxRange = 8f; //최대 사거리
    float jumpDuration = 1f; //점프 지속 시간
    float gravityPower = 1.5f; //떨어질 때 힘
    Vector2 targetDestination; //점프 목표 위치
    
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternB 상태 시작");
        rb = controller.GetComponent<Rigidbody2D>();

        if (controller.targetPlayer != null)
        {
            target = controller.targetPlayer;
        }
    }

    public void Exit(E_PuppeteerController controller)
    {
        controller.HitBox_AirB.SetActive(false);
        controller.HitBox_B.SetActive(false);
        Debug.Log("PatternB 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
       if(rb != null) //정점 부분에서 히트박스 킴
        {
            if (Mathf.Abs(rb.linearVelocity.y)< 0.3) {
                controller.isAttaking_AirB = true;
                controller.HitBox_AirB.SetActive(true);
                //공격 히트박스 사라질때까지? 라는 말이 무슨 말임..?
            }
        }

        if (controller.targetPlayer != null)
        {
            controller.LookAtLocation(controller.targetPlayer.transform.position.x);
        }

        if (target == null) { return; }

        targetPos = target.transform.position;
        myPos = controller.transform.position;

        if (!controller.isInTargetPlayer && !startB) //move 히트박스 안에 대상이 있지않으면 대상에게 이동
        {
            Debug.Log( target + "에게 이동 중");
            controller.transform.position = Vector2.MoveTowards(myPos, targetPos, moveSpeed * Time.deltaTime);
        }
        ////점프하고 떨어질 때 중력 
        //if (rb.linearVelocity.y < 0) 
        //{
        //    Vector2 vel = rb.linearVelocity;
        //    vel.y += Physics2D.gravity.y * gravityPower * Time.deltaTime;
        //    rb.linearVelocity = vel;
        //}

        if (controller.isInTargetPlayer && !startB)
        {
            startB = true;
            controller.StartCoroutine(StartNormalAttack(controller));
        }

    }

    IEnumerator StartNormalAttack(E_PuppeteerController controller) 
    {
        yield return new WaitForSeconds(firstDelay / BASE_FPS); //1타 선딜레이
        //여기에 히트박스 키고 끄는 코드를 넣어줌
        controller.isAttaking_B = true;
        controller.HitBox_B.SetActive(true);
        Debug.Log("1타");
        yield return new WaitForSeconds(HitDuration / BASE_FPS); //1타 판정 유지시간
        controller.HitBox_B.SetActive(false);
        controller.isAttaking_B = false;
        yield return new WaitForSeconds(backDelay / BASE_FPS); //1타 후딜레이

        yield return new WaitForSeconds(firstDelay / BASE_FPS); //2타 선딜레이
        controller.isAttaking_B = true;
        controller.HitBox_A.SetActive(true);
        Debug.Log("2타");
        yield return new WaitForSeconds(HitDuration / BASE_FPS); //2타 판정 유지시간
        controller.HitBox_B.SetActive(false);
        controller.isAttaking_B = false;

        yield return new WaitForSeconds(backDelay / BASE_FPS); //2타 후딜레이
        yield return new WaitForSeconds(jumpFirstDelay / BASE_FPS); //점프 선딜레이

        target = controller.rangedDealer; //타겟을 원거리 딜러로 변경

        if(target != null)
        {
            Vector2 currentMyPos = controller.transform.position;
            Vector2 targetPos = new Vector2(target.transform.position.x, controller.transform.position.y);

            float distance = Vector2.Distance(currentMyPos, targetPos); //거리

            if (distance > maxRange) //일정 거리가 n보다 크면 
            {
                Vector2 direction = (targetPos - currentMyPos).normalized; //방향
                targetDestination = currentMyPos + (direction * maxRange); //최대 점프착지 위치
            }
            else
            {
                targetDestination = targetPos;
            }
            controller.StartCoroutine(StartJumpAttack(controller));
        }       
    }

    IEnumerator StartJumpAttack(E_PuppeteerController controller)
    {
        Debug.Log("점프 시작");
        float timer = 0f;
        
        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;

            Vector2 currentPos = Vector2.Lerp(controller.transform.position, targetDestination, t);
            currentPos.y += 4 * jumpForce * t * (1 - t); //포물선 점프 공식

            controller.transform.position = currentPos;
            yield return null;

        }
        controller.transform.position = targetDestination;
        //원거리 딜러에게 포물선 점프
        yield return new WaitForSeconds(firstAtkDelay / BASE_FPS); //착지 후 공격 선딜레이 
        controller.HitBox_B.SetActive(true);
        yield return new WaitForSeconds(landHitDuration / BASE_FPS); //착지 후 공격 판정 유지시간
        controller.HitBox_B.SetActive(false);
        yield return new WaitForSeconds(backAtkDelay / BASE_FPS); //착지 후 공격 후딜레이

        controller.ChangeState(controller.idle); 
        startB = false;
    }
}
