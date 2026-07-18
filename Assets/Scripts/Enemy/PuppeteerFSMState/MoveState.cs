using UnityEngine;
using UnityEngine.InputSystem.XR;

public class MoveState : IEnemyState
{
    [SerializeField] float attackRange = 5f; //공격 state로 바뀌는 사거리
    [SerializeField] float detectRange = 15f; //감지 사거리
    [SerializeField] float moveSpeed = 5f; // 이동 속도
    Collider2D targetPlayer;
    Vector3 targetDir;
    float distance = 0f; //나와 플레이어의 거리
    public void Enter(PuppeteerController controller)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(controller.transform.position, detectRange);
        foreach (Collider2D collider in colliders)
        {
            if (controller.chooseState is JumpToRangedDealerState)
            {
                if (collider.gameObject.CompareTag("RangedDealer")) //태자가 targetPlayer가 됨
                {
                    targetPlayer = collider;
                }
            }
            else
            {

            }
            //조건식 (거리가 가장 짧은? 아니면 랜덤? ..)

            //targetPlayer = collider

            //공격할 player를 정해줌
            //그리고 Update문에서 계속 그 플레이어의 위치 값을 계산하면서 사거리가 일정 사거리 이내이면 공격 
        }

    }

    public void Exit(PuppeteerController controller)
    {
        Debug.Log("Move 상태 해제");
    }

    public void Update(PuppeteerController controller)
    {
        //얘가 적과의 거리가 attackRange보다 가까워지면
        if (attackRange >= distance)
        {
            controller.ChangeState(controller.chooseState);
        }

        Vector2 enemyPos = controller.transform.position; //현재 나의 위치
        Vector2 playerPos = targetPlayer.transform.position; //플레이어의 위치
        distance = Vector2.Distance(enemyPos, playerPos);
        targetDir = (playerPos - enemyPos).normalized; //이동할 방향

        controller.transform.position += targetDir * moveSpeed; //타겟 방향으로 이동
    }
}
