using UnityEngine;
using UnityEngine.InputSystem.XR;

//적과 타겟 플레이어간의 일정 거리 이상일 경우에 move State인거고 거리보다 가까워지면 chooseState로 상태 변환됨
public class P_MoveState : IPuppeteerState
{
    float moveSpeed = 5f; // 이동 속도

    Vector2 enemyPos;
    Vector2 playerPos;


    [Header("PatternD")]
    float range_RandD = 6f; //두 사람의 거리를 비교할 때 사용할 변수

    public void Enter(E_PuppeteerController controller) //이 State가 실행될 때 처음에 한번 실행됨
    {
        Debug.Log("Move 상태 시작");
        DecideTarget(controller); 
    }

    public void Exit(E_PuppeteerController controller)
    {
        Debug.Log("Move 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    { 
       
        if (controller.targetPlayer == null)
        {
            if (controller.chooseState != null)
            {
                controller.ChangeState(controller.chooseState);
            }
        }
        else
        {
            controller.LookAtLocation(controller.targetPlayer.transform.position.x); //flip

            enemyPos = controller.transform.position; //현재 나의 위치
            playerPos = new Vector2(controller.targetPlayer.transform.position.x, enemyPos.y); //플레이어의 위치

            if (!controller.isInTargetPlayer)
            {
                controller.transform.position = Vector2.MoveTowards(enemyPos, playerPos, moveSpeed * Time.deltaTime); //타겟 방향으로 이동
            }

            if (controller.isInTargetPlayer)
            {
                Debug.Log("현재 타겟 플레이어 " + controller.targetPlayer.name);
                controller.ChangeState(controller.chooseState);
            }
        }
    }

    //타겟 결정
    void DecideTarget(E_PuppeteerController controller)
    {
        if (controller.chooseState == null) return;

        if (controller.chooseState is P_PatternA_State) //선택된 상태가 인형돌진이라면
        {
            Vector2 rangedD = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
            Vector2 meleeD = new Vector2(controller.meleeDealer.transform.position.x, controller.transform.position.y);
            float disToRanged = Vector2.Distance(controller.transform.position, rangedD); //적과 원거리 거리 계산
            float disToMelee = Vector2.Distance(controller.transform.position, meleeD); //적과 근거리 거리 계산

            //더 가까운 플레이어 캐릭터를 targetPlayer에 넣어줌
            controller.targetPlayer = disToRanged > disToMelee ? controller.meleeDealer : controller.rangedDealer;         
        }
        else if (controller.chooseState is P_PatternB_State) //원거리딜러에게 점프
        {
            int randomNum = Random.Range(1, 3);
            controller.targetPlayer = (randomNum == 1) ? controller.meleeDealer : controller.rangedDealer;
        }
        else if (controller.chooseState is P_PatternC_State)
        {
            Vector2 myPos = controller.transform.position;
            Vector2 rangedPos = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
            Vector2 meleePos = new Vector2(controller.meleeDealer.transform.position.x, controller.transform.position.y);
            float disToRanged = Vector2.Distance(myPos, rangedPos);
            float disToMelee = Vector2.Distance(myPos, meleePos);

            controller.targetPlayer = disToRanged >= disToMelee ? controller.meleeDealer : controller.rangedDealer;

        }
        else if (controller.chooseState is P_PatternD_State)
        {
            Vector2 rangedDis = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
            Vector2 meleeDis = new Vector2(controller.meleeDealer.transform.position.x, controller.transform.position.y);
            float distanceBetweenPlayers = Vector2.Distance(rangedDis, meleeDis); //회월 스님과 태자 전하의 거리를 계산
             
            if (distanceBetweenPlayers < range_RandD) //계산한 거리가 n보다 가깝다면
            {
                controller.isFar = false;
                controller.targetPlayer = controller.rangedDealer; //태자전하가 목표가 됨
            }
            else //태자전하 회월스님의 거리가 n보다 멀다면 target을 정해주지 않고 바로 다음 상태로 넘어감
            {
                controller.isFar = true;
                controller.middlePoint = (rangedDis + meleeDis) / 2.0f; //회월스님과 태자전하의 가운데 값         
                controller.targetPlayer = null;
            }
            //예외 상황 target이 정해질수 없음 왜냐 태자전하와 회월 스님의 거리가 n보다 멀 경우
        }
        else if (controller.chooseState is P_PatternE_State)
        {
            int randomNum = Random.Range(1, 3);
            controller.targetPlayer = (randomNum == 1) ? controller.meleeDealer : controller.rangedDealer;
        }  
        else if (controller.chooseState is P_PatternF_State)
        {
            Debug.Log("타겟 없음");
            controller.targetPlayer = null; 
        }  
        else if (controller.chooseState is P_PatternG_State)
        {
            controller.targetPlayer = controller.rangedDealer; //태자 전하가 
        }
    }
}
