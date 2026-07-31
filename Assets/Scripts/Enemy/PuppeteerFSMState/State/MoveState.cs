using UnityEngine;
using UnityEngine.InputSystem.XR;

//적과 타겟 플레이어간의 일정 거리 이상일 경우에 move State인거고 거리보다 가까워지면 chooseState로 상태 변환됨
public class MoveState : IEnemyState
{
    [SerializeField] float attackRange = 5f; //공격 state로 바뀌는 사거리
    [SerializeField] float moveSpeed = 5f; // 이동 속도
  
    float distance = 0f; //나와 플레이어의 거리

    [Header("PatternD")]
    float range_RandD = 4f; //두 사람의 거리를 비교할 때 사용할 변수

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
        if(controller.targetPlayer == null)
        {
            return;
        }
        Vector2 enemyPos = controller.transform.position; //현재 나의 위치
        Vector2 playerPos = new Vector2(controller.targetPlayer.transform.position.x, enemyPos.y); //플레이어의 위치
        distance = Vector3.Distance(enemyPos, playerPos);
        //타겟 플레이어가 적과의 거리가 attackRange보다 가까워지면
        if (distance <= attackRange)
        {
            controller.ChangeState(controller.chooseState);
        }
        else
        { 
            float step = moveSpeed * Time.deltaTime;
            controller.transform.position = Vector2.MoveTowards(enemyPos, playerPos, step); //타겟 방향으로 이동
        }
          
    }

    //타겟 결정
    void DecideTarget(E_PuppeteerController controller)
    {
        if (controller.chooseState is PatternB_State) //원거리딜러에게 점프
        {
            int randomNum = Random.Range(1, 3);
            if (randomNum == 1) //회월
            {
                controller.targetPlayer = controller.damageDealer;
            }
            else if (randomNum == 2) //태자
            {
                controller.targetPlayer = controller.rangedDealer;
            }
        }
        else if (controller.chooseState is PatternA_State) //선택된 상태가 인형돌진이라면
        {
            Vector2 rangedD = controller.rangedDealer.transform.position;
            Vector2 damageD = controller.damageDealer.transform.position;
            float rangedDis = Vector2.Distance(controller.transform.position, rangedD); //거리 계산
            float damageDis = Vector2.Distance(controller.transform.position, damageD); //거리 계산

            //더 가까운 플레이어 캐릭터를 targetPlayer에 넣어줌
            controller.targetPlayer = rangedDis > damageDis ? controller.damageDealer : controller.rangedDealer;
            if (rangedDis > damageDis)
            {
                controller.targetPlayer = controller.damageDealer;
            }
            else
            {
                controller.targetPlayer = controller.rangedDealer;
            }
        }
        else if (controller.chooseState is PatternC_State)
        {
            //보류
        }
        else if (controller.chooseState is PatternD_State)
        {
            Vector2 rangedD = new Vector2(controller.rangedDealer.transform.position.x, 0);
            Vector2 damageD = new Vector2(controller.damageDealer.transform.position.x, 0);
            float distanace_RandD = Vector2.Distance(rangedD, damageD); //회월 스님과 태자 전하의 거리를 계산

            if (distanace_RandD > range_RandD)
            {
                controller.isFar = false;
                controller.targetPlayer = controller.rangedDealer;
            }
            else //태자전하 회월스님의 거리가 n보다 멀다면 target을 정해주지 않고 바로 다음 상태로 넘어감
            {
                controller.isFar = true;
                controller.ChangeState(controller.chooseState);
            }
            //에외 상황 target이 정해질수 없음 왜냐 태자전하와 회월 스님의 거리가 n보다 멀 경우
        }
        else if (controller.chooseState is PatternE_State)
        {
            int randomNum = Random.Range(1, 3);
            if(randomNum == 1) //회월
            {
                controller.targetPlayer = controller.damageDealer;                
            }
            else if(randomNum == 2) //태자
            {
                controller.targetPlayer = controller.rangedDealer;           
            }
        }
    }
}
