using UnityEngine;

//플레이어블 캐릭터 사이로 들어가기

public class P_PatternD_State : IPuppeteerState
{
    float moveSpeed = (5f * 1.2f);
    float range_RandD = 6f; //회월, 태자 사거리 (미정)
    float distance_RandD; //회월, 태자 거리 계산
    float distance_EandR = 0f; //적과 태자 전하의 거리
    float distance_EandM = 0f; //적과 중간지점의 거리

    Collider2D target;
    Vector2 myPos;
    bool endStart = false;
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternD 상태 시작");
    }

    public void Exit(E_PuppeteerController controller)
    {
        distance_RandD = 0;
        distance_EandR = 0;
        distance_EandM = 0;
        controller.isFar = false;
        endStart = false;
        target = null;
        controller.middlePoint = Vector2.zero;
        Debug.Log("PatternD 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        if (controller.isFar) //두 사이가 멀면 가운데 포인트의 방향을 바라봄
        {
            controller.LookAtLocation(controller.middlePoint.x);
        }
        else if (controller.targetPlayer != null) //flip
        {
            controller.LookAtLocation(controller.targetPlayer.transform.position.x);
        }
   
        myPos = controller.transform.position; //적의 위치
       
        if (controller.isFar == true) //가운데가 목표인 경우
        {
            Debug.Log("isFar = true");
            if (controller.middlePoint == Vector2.zero) return;

            distance_EandM = Vector2.Distance(controller.middlePoint, myPos); //적이랑 가운데 위치의 거리를 계산

            if (distance_EandM > 0.5f)  
            {
                Debug.Log("가운데로 이동");
                controller.transform.position = Vector2.MoveTowards(myPos, controller.middlePoint, moveSpeed * Time.deltaTime);
            }
            else
            {             
                controller.ChangeState(controller.idle);
            }
        }
        else //태자 전하가 목표인 경우
        {
            Debug.Log("isFar = false");
            if (controller.targetPlayer != null)
            {
                Vector2 targetPos = new Vector2(controller.targetPlayer.transform.position.x, myPos.y); //태자 위치
                if (!controller.isInTargetPlayer)
                {
                    Debug.Log(controller.targetPlayer.name + "이동");
                    controller.transform.position = Vector2.MoveTowards(myPos, targetPos, moveSpeed * Time.deltaTime);
                }
                else
                {
                    controller.ChangeState(controller.idle);
                }
            }
        }
         
    }

    
}
