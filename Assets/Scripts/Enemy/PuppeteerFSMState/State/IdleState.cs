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

    int maxHeight_A = 20;
    int height_A = 0;
    int maxHeight_B = 30;
    int height_B = 0;
    int maxHeight_C = 0;
    int height_C = 0;
    int maxHeight_D = 7;
    int height_D = 0;
    int maxHeight_E = 10;
    int height_E = 0;
    
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("Idle 상태 시작");
        //초기화
        height_A = maxHeight_A;
        height_B = maxHeight_B;
        height_C = maxHeight_C;
        height_D = maxHeight_D;
        height_E = maxHeight_E;

        controller.chooseState = null;
        controller.StartCoroutine(Idle(controller));
    }

    public void Exit(E_PuppeteerController controller)
    {
        Debug.Log("Idle 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        
    }

    private IEnumerator Idle(E_PuppeteerController controller)
    {
        yield return new WaitForSeconds(frontDelay / BASE_FPS); //선딜
               
        //쿨타임 중이라면 가중치를 0으로 초기화 
        if (controller.didState_A)
        {
            Debug.Log("A State 쿨타임 true");
            height_A = 0;
        }
        if (controller.didState_B)
        {
            Debug.Log("B State 쿨타임 true");
            height_B = 0;
        }
        if (controller.didState_C)
        {
            Debug.Log("C State 쿨타임 true");
            height_C = 0;
        }
        if (controller.didState_D)
        {
            Debug.Log("D State 쿨타임 true");
            height_D = 0;
        }
        if (controller.didState_E)
        {
            Debug.Log("E State 쿨타임 true");
            height_E = 0;
        }
        ChooseState(controller);
    }

    void ChooseState(E_PuppeteerController controller)
    {
        //가중치 결정 함수들을 실행 시켜서 각각 가중치 결정
        int Height_A = height_A; 
        int Height_B = height_B;
        int Height_C = height_C;
        int Height_D = height_D;
        int Height_E = height_E;
        int finalWeight = Height_A + Height_B + Height_C + Height_D + Height_E ; //전체 가중치
    
        int RandNum = Random.Range(0, finalWeight); //전체 가중치 안에서의 랜덤 가중치값

        //RanNum이 0 ~ HeightState1까지
        if(RandNum < Height_A)//JumpToRangedDealer
        {
            controller.chooseState = controller.A;
        }
        else if ((RandNum -= Height_A) < Height_B)//RunLikeHorse
        {
            controller.chooseState = controller.B;   
        }
        else if((RandNum -= Height_B) < Height_C)
        {
            controller.chooseState = controller.C;
        }
        else if((RandNum -= Height_C) < Height_D)
        {
            controller.chooseState = controller.D;
        }
        else if((RandNum -= Height_D) < Height_E)
        {
            controller.chooseState = controller.E;
        }
        controller.ChangeState(controller.move);
    }

  

}
