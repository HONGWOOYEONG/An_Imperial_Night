using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
//가만히 있는 상태, 상태를 선택하게 됨
//모든 상태가 Idle을 거쳐감
public class IdleState : IEnemyState
{
    float BASE_FPS = 60f;

    float frontDelay = 3f; //Idle진입 후 판단 대기시간
    [Header("가중치")]

    int maxHeight_A = 20; int height_A = 0;//20
    int maxHeight_B = 30; int height_B = 0; //30
    int maxHeight_C = 0; int height_C = 0;
    int maxHeight_D = 7; int height_D = 0; //7
    int maxHeight_E = 10; int height_E = 0; //10
    

    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("Idle 상태 시작");
        //초기화
       

        controller.chooseState = null;
        controller.StartCoroutine(Idle(controller));
    }

    public void Exit(E_PuppeteerController controller)
    {
        Debug.Log("Idle 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        if (controller.targetPlayer != null)
        {
            controller.LookAtLocation(controller.targetPlayer.transform.position.x);
        }
    }

    private IEnumerator Idle(E_PuppeteerController controller)
    {
        yield return new WaitForSeconds(frontDelay / BASE_FPS); //선딜

        while (controller.chooseState == null) //null이 아닐 때까지 
        {
            ChooseState(controller);
            if (controller.chooseState == null)
            {
                yield return new WaitForSeconds(0.2f);
            }
            yield return null;
        }

        controller.ChangeState(controller.move);
    }

    void ChooseState(E_PuppeteerController controller)
    {
        height_A = maxHeight_A;
        height_B = maxHeight_B;
        height_C = maxHeight_C;
        height_D = maxHeight_D;
        height_E = maxHeight_E;
        //가중치 결정 함수들을 실행 시켜서 각각 가중치 결정
        if (controller.didState_A) height_A = 0;
        if (controller.didState_B) height_B = 0;
        if (controller.didState_C) height_C = 0;
        if (controller.didState_D) height_D = 0;
        if (controller.didState_E) height_E = 0;
        int finalWeight = height_A + height_B + height_C + height_D + height_E; //전체 가중치

        if (finalWeight <= 0)
        {          
            return;
        }

        int RandNum = Random.Range(0, finalWeight); //전체 가중치 안에서의 랜덤 가중치값

        if (RandNum < height_A)
        {
            controller.chooseState = controller.A;
        }
        else if ((RandNum -= height_A) < height_B)
        {
            controller.chooseState = controller.B;   
        }
        else if((RandNum -= height_B) < height_C)
        {
            controller.chooseState = controller.C;
        }
        else if((RandNum -= height_C) < height_D)
        {
            controller.chooseState = controller.D;
        }
        else if((RandNum -= height_D) < height_E)
        {
            controller.chooseState = controller.E;
        }
       
    } 
}
