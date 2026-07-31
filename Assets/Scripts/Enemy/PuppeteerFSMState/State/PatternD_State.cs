using UnityEngine;

//플레이어블 캐릭터 사이로 들어가기

public class PatternD_State : IEnemyState
{
    float moveSpeed;
    float range_RandD = 6f; //회월, 태자 사거리 (미정)
    float distance_RandD; //회월, 태자 거리 계산
    float range_EandR = 3f; //적과 태자 전하의 사거리 
    float distance_EandR = 0f; //적과 태자 전하의 거리
    float distance_EandM = 0f; //적과 중간지점의 거리
  


    Collider2D target;
    Vector2 middlePoint; //중간 지점
    Vector2 myPos;
    bool endStart = false;
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternD 상태 시작");

        //변수 초기화
        target = null;
        middlePoint = Vector2.zero;
        distance_RandD = 0;
        distance_EandR = 0;
        distance_EandM = 0;
        moveSpeed = (5f * 1.2f);

        Vector2 rangedD = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
        Vector2 damageD = new Vector2(controller.damageDealer.transform.position.x, controller.transform.position.y);
        distance_RandD = Vector2.Distance(rangedD, damageD);
        if(distance_RandD > range_RandD)
        {
            controller.isFar = true;
        }

        if (controller.isFar == true)
        {
            middlePoint = (rangedD + damageD) / 2.0f;
            Debug.Log(middlePoint);
        }
        endStart = true;
    }

    public void Exit(E_PuppeteerController controller)
    {
        Debug.Log("PatternD 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        if(!endStart) { return; }
        myPos = controller.transform.position;
       
        if (controller.isFar == true)
        {
            if (middlePoint == Vector2.zero) return;
            distance_EandM = Vector2.Distance(middlePoint, myPos); //적이랑 가운데 위치의 거리를 계산

            if (distance_EandM > 1.0f)  
            {
                Debug.Log("가운데로 이동");
                controller.transform.position = Vector2.MoveTowards(myPos, middlePoint, moveSpeed * Time.deltaTime);
            }
            else
            {
                controller.isFar = false;
                endStart = false;
                controller.ChangeState(controller.idle);
            }
        }
        else //태자 전하가 목표인 경우
        {
            target = controller.rangedDealer;

            if(target != null)
            {
                Vector2 targetPos = new Vector2(target.transform.position.x, myPos.y); //태자 위치
                distance_EandR = Vector2.Distance(targetPos, myPos); //태자랑 적 거리 계산
                if (distance_EandR > range_EandR)
                {
                    Debug.Log(target.name + "이동");
                    controller.transform.position = Vector2.MoveTowards(myPos, targetPos, moveSpeed * Time.deltaTime);
                }
                else
                {
                    endStart = false;
                    controller.ChangeState(controller.idle);
                }
            }
        }
         
    }

    
}
