using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

//적과 타겟 플레이어간의 일정 거리 이상일 경우에 move State인거고 거리보다 가까워지면 chooseState로 상태 변환됨
public class P_MoveState : IPuppeteerState
{
    [SerializeField] float attackRange = 5f; //공격 state로 바뀌는 사거리
    [SerializeField] float moveSpeed = 5f; // 이동 속도

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
            float rangedDis = Vector2.Distance(controller.transform.position, rangedD); //적과 원거리 거리 계산
            float meleeDis = Vector2.Distance(controller.transform.position, meleeD); //적과 근거리 거리 계산

            //더 가까운 플레이어 캐릭터를 targetPlayer에 넣어줌
            controller.targetPlayer = rangedDis > meleeDis ? controller.meleeDealer : controller.rangedDealer;         
        }
        else if (controller.chooseState is P_PatternB_State) //원거리딜러에게 점프
        {
            int randomNum = Random.Range(1, 3);
            controller.targetPlayer = (randomNum == 1) ? controller.meleeDealer : controller.rangedDealer;
        }
        else if (controller.chooseState is P_PatternC_State)
        {
            //적이 가운데이 있는지 구하기
            float ragedX = controller.rangedDealer.transform.position.x;
            float meleeX = controller.meleeDealer.transform.position.x;
            float myX = controller.transform.position.x;
            float minX = Mathf.Min(ragedX, meleeX);
            float maxX = Mathf.Max(ragedX, meleeX);

            if(minX <= myX && myX <= maxX)
            {
                //적 사이에 있음
                controller.isCase_1 = true;
                controller.countAtk = 3;
                //가장 가까운 적 구하기
                Vector2 myPos = controller.transform.position;
                Vector2 rangedPos = new Vector2(controller.rangedDealer.transform.position.x, myPos.y);
                Vector2 meleePos = new Vector2(controller.meleeDealer.transform.position.x, myPos.y);
                float distance_EandR = Vector2.Distance(myPos, rangedPos);
                float distance_EandM = Vector2.Distance(myPos, meleePos);

                //타겟 설정
                controller.targetPlayer = (distance_EandR <= distance_EandM) ? controller.rangedDealer : controller.meleeDealer;    
                
            }
            else
            {
                //적 사이에 있지 않음
                controller.countAtk = 2;
                //가장 가까운 적 구하기
                Vector2 myPos = controller.transform.position;
                Vector2 rangedPos = new Vector2(controller.rangedDealer.transform.position.x, myPos.y);
                Vector2 meleePos = new Vector2(controller.meleeDealer.transform.position.x, myPos.y);
                float distance_EandR = Vector2.Distance(myPos, rangedPos);
                float distance_EandM = Vector2.Distance(myPos, meleePos);

                controller.targetPlayer = (distance_EandR <= distance_EandM) ? controller.rangedDealer : controller.meleeDealer;
            }
        }
        else if (controller.chooseState is P_PatternD_State)
        {
            Vector2 rangedD = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
            Vector2 damageD = new Vector2(controller.meleeDealer.transform.position.x, controller.transform.position.y);
            float distanace_RandD = Vector2.Distance(rangedD, damageD); //회월 스님과 태자 전하의 거리를 계산
             
            if (distanace_RandD < range_RandD) //계산한 거리가 n보다 가깝다면
            {
                controller.isFar = false;
                controller.targetPlayer = controller.rangedDealer; //태자전하가 목표가 됨
            }
            else //태자전하 회월스님의 거리가 n보다 멀다면 target을 정해주지 않고 바로 다음 상태로 넘어감
            {
                controller.isFar = true;
                controller.middlePoint = (rangedD + damageD) / 2.0f; //회월스님과 태자전하의 가운데 값         
                controller.targetPlayer = null;
            }
            //예외 상황 target이 정해질수 없음 왜냐 태자전하와 회월 스님의 거리가 n보다 멀 경우
        }
        else if (controller.chooseState is P_PatternE_State)
        {
            int randomNum = Random.Range(1, 3);
            controller.targetPlayer = (randomNum == 1) ? controller.meleeDealer : controller.rangedDealer;
        }
    }
}
