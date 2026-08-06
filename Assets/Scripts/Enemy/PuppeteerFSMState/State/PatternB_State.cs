using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

//평타 2회 후 원거리 딜러에게 점프
public class PatternB_State : IEnemyState
{
    float BASE_FPS = 60f;
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
    float maxRange = 10f; //최대 사거리 (미정)
    float jumpDuration = 0f; //점프 지속 시간
                             // float gravityPower = 1.5f; //떨어질 때 힘
    float minJumpDuration = 0.4f; //가까울 때 공중에 머무를 최소 시간
    float maxJumpDuration = 1.0f; //멀 때 공중에 머무를 최대 시간
    float minDistance = 2.0f; //최소 판단 거리기준

    Vector2 targetDestination; //점프 목표 위치
    
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternB 상태 시작");
        rb = controller.GetComponent<Rigidbody2D>();
    }

    public void Exit(E_PuppeteerController controller)
    {
        controller.HitBox_AirB.SetActive(false);
        controller.HitBox_B.SetActive(false);
        Debug.Log("PatternB 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        //if(rb != null) //정점 부분에서 히트박스 킴
        // {
        //     if (Mathf.Abs(rb.linearVelocity.y) < 0.3) {
        //         controller.isAttaking_AirB = true;
        //         controller.HitBox_AirB.SetActive(true);
        //     }
        // }

        if (controller.targetPlayer == null) { return; }
     
        controller.LookAtLocation(controller.targetPlayer.transform.position.x); //flip
        
        targetPos = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y);

        if (!controller.isInTargetPlayer && !startB)
        {
            Debug.Log(controller.targetPlayer + "에게 이동 중");
            controller.transform.position = Vector2.MoveTowards(controller.transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
        if (controller.isInTargetPlayer && !startB)
        {
            startB = true;
            controller.StartCoroutine(StartNormalAttack(controller));
        }

        ////점프하고 떨어질 때 중력 
        //if (rb.linearVelocity.y < 0) 
        //{
        //    Vector2 vel = rb.linearVelocity;
        //    vel.y += Physics2D.gravity.y * gravityPower * Time.deltaTime;
        //    rb.linearVelocity = vel;
        //}


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
        controller.HitBox_B.SetActive(true);
        Debug.Log("2타");
        yield return new WaitForSeconds(HitDuration / BASE_FPS); //2타 판정 유지시간
        controller.HitBox_B.SetActive(false);
        controller.isAttaking_B = false;

        yield return new WaitForSeconds(backDelay / BASE_FPS); //2타 후딜레이
        yield return new WaitForSeconds(jumpFirstDelay / BASE_FPS); //점프 선딜레이

        controller.targetPlayer = controller.rangedDealer; //타겟을 원거리 딜러로 변경
        Debug.Log("타겟을 원거리 딜러로 변경");

        if(controller.targetPlayer != null)
        {
            Vector2 currentMyPos = controller.transform.position;
            Vector2 targetPos = new Vector2(controller.targetPlayer.transform.position.x, currentMyPos.y);

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

        Vector2 startPos = controller.transform.position;

        //거리에 따라서 점프 시간 조정
        float distanceX = Mathf.Abs(targetDestination.x - startPos.x);
        // 현재 거리가 최소 사거리 최대 사거리 사이의 어디쯤인지 비율 계산
        float distanceRatio = Mathf.InverseLerp(minDistance, maxRange, distanceX);
        //구해진 거리 비율에 따라 점프 지속 시간을 가변적으로 설정
        float currentJumpDuration = Mathf.Lerp(minJumpDuration, maxJumpDuration, distanceRatio);

        if (rb != null) rb.linearVelocity = Vector2.zero;

        while (timer < currentJumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / currentJumpDuration;

            Vector2 currentPos = Vector2.Lerp(startPos, targetDestination, t);
            
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
    
}}
