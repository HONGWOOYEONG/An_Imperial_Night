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
    float jumpForce = 4f; //최고점 높이
    float maxRange = 5f; //최대 사거리
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
        Debug.Log("PatternB 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        if (target == null) { return; }
        targetPos = target.transform.position;
        myPos = controller.transform.position;

        if (!controller.isInTargetPlayer)
        {
            Debug.Log("대상에게 이동 중");
            controller.transform.position = Vector2.MoveTowards(myPos, targetPos, moveSpeed * Time.deltaTime);
        }
        //점프하고 떨어질 때 중력 
        if (rb.linearVelocity.y < 0) 
        {
            Vector2 vel = rb.linearVelocity;
            vel.y += Physics2D.gravity.y * gravityPower * Time.deltaTime;
            rb.linearVelocity = vel;
        }

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
        //.SetActive(true);
        Debug.Log("1타");
        yield return new WaitForSeconds(HitDuration / BASE_FPS); //1타 판정 유지시간
        //.SetActive(false);
        yield return new WaitForSeconds(backDelay / BASE_FPS); //1타 후딜레이

        yield return new WaitForSeconds(firstDelay / BASE_FPS); //2타 선딜레이
        //.SetActive(true);
        Debug.Log("2타");
        yield return new WaitForSeconds(HitDuration / BASE_FPS); //2타 판정 유지시간
        //.SetActive(false);
        yield return new WaitForSeconds(backDelay / BASE_FPS); //2타 후딜레이
        yield return new WaitForSeconds(jumpFirstDelay / BASE_FPS); //점프 선딜레이

        target = controller.rangedDealer;

        //최대 사거리에 따라 원래 원딜 위치로 점프를 하느냐 최대 거리로 점프를 하느냐 
        float distance = Vector2.Distance(myPos, target.transform.position);
        float directionX = (target.transform.position.x > myPos.x) ? 1f : -1f;
        if (distance > maxRange)
        {         
            targetDestination = new Vector2(myPos.x + (maxRange * directionX), myPos.y);
        }
        else
        {
            targetDestination = target.transform.position;
        }
        controller.StartCoroutine(StartJumpAttack(controller));
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
        controller.transform.position = target.transform.position;
        //원거리 딜러에게 포물선 점프
        yield return new WaitForSeconds(firstAtkDelay / BASE_FPS); //착지 후 공격 선딜레이 
       //.SetActive(true)
        yield return new WaitForSeconds(landHitDuration / BASE_FPS); //착지 후 공격 판정 유지시간
       //.SetActive(false)
        yield return new WaitForSeconds(backAtkDelay / BASE_FPS); //착지 후 공격 후딜레이

        controller.ChangeState(controller.idle);
        startB = false;
    }
}
