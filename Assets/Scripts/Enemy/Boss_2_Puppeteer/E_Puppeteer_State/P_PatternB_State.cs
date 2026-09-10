using System.Collections;
using UnityEngine;

//평타 2회 후 원거리 딜러에게 점프
public class P_PatternB_State : IPuppeteerState
{
    float BASE_FPS = 60f;
    E_PuppeteerAction action = new E_PuppeteerAction();
    Rigidbody2D rb;
    bool startB = false; //공격 시작을 위한 변수

    [Header("딜레이")]
    float firstDelay = 18f;// 1/2타 선딜
    float backDelay = 10f; // 1/2타 후딜
    float hitDuration = 3f; // 판정 유지 시간
    float jumpFirstDelay = 27f; //점프 선딜레이
    float firstAtkDelay = 4f; //착지 후 공격 선딜레이
    float landHitDuration = 5f; //착지 후 공격 판정 유지시간
    float backAtkDelay = 50f; //착지 후 공격 후딜레이

    [Header("점프에 필요한 변수")]
    float jumpForce = 6f; //최고점 높이
    float maxRange = 10f; //최대 사거리 (임의)
    float minJumpDuration = 0.4f; //가까울 때 공중에 머무를 최소 시간 (임의)
    float maxJumpDuration = 1.0f; //멀 때 공중에 머무를 최대 시간 (임의)
    float minDistance = 2.0f; //최소 판단 거리기준 (임의)

    Vector2 targetJumpPos; //점프 목표 위치
    
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternB 상태 시작");
        rb = controller.GetComponent<Rigidbody2D>();

        controller.StartCoroutine(ComboTwoHitJumpAttack(controller));
    }

    public void Exit(E_PuppeteerController controller)
    {
        controller.HitBox_AirB.SetActive(false);
        controller.HitBox_B.SetActive(false);
        targetJumpPos = Vector2.zero;
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
        
    }

    IEnumerator ComboTwoHitJumpAttack(E_PuppeteerController controller) 
    {
        int count = 0;
        while (count < 2)
        {
            yield return new WaitForSeconds(firstDelay / BASE_FPS); //선딜레이
            yield return action.NormalAttack(controller, hitDuration);
            yield return new WaitForSeconds(backDelay / BASE_FPS); //후딜레이
            count++;
        }

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
                targetJumpPos = currentMyPos + (direction * maxRange); //최대 점프착지 위치
            }
            else
            {
                targetJumpPos = targetPos;
            }
            Debug.Log("점프 시작");

            Vector2 startPos = controller.transform.position;
            //거리에 따라서 점프 시간 조정
            float distanceX = Mathf.Abs(targetJumpPos.x - startPos.x);
            // 현재 거리가 최소 사거리 최대 사거리 사이의 어디쯤인지 비율 계산
            float distanceRatio = Mathf.InverseLerp(minDistance, maxRange, distanceX);
            //구해진 거리 비율에 따라 점프 지속 시간을 가변적으로 설정
            float currentJumpDuration = Mathf.Lerp(minJumpDuration, maxJumpDuration, distanceRatio);

            yield return action.JumpAttack(controller, rb, currentJumpDuration, startPos, targetJumpPos, jumpForce);

            //원거리 딜러에게 포물선 점프
            yield return new WaitForSeconds(firstAtkDelay / BASE_FPS); //착지 후 공격 선딜레이 
            action.NormalAttack(controller, landHitDuration);
            yield return new WaitForSeconds(backAtkDelay / BASE_FPS); //착지 후 공격 후딜레이

            controller.ChangeState(controller.states["idle"]);
            startB = false;
        }       
    }

  }
