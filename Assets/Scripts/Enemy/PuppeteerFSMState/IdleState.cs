using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
//가만히 있는 상태
//모든 상태가 Idle을 거쳐감
public class IdleState : IEnemyState
{
    float BASE_FPS = 60f;
    

    [Header("딜레이")]
    float FrontDelay = 5f;
    float backDelay = 10f;


    [Header("가중치")]
    int maxHeight = 3;
    int height_JTRD = 3; //1
    int heught_RLH = 0; //2 ..미구현
    int height_DR = 3; //3


    public void Enter(PuppeteerController controller)
    {
        controller.StartCoroutine(Idle(controller));
        controller.chooseState = null;
    }

    public void Exit(PuppeteerController controller)
    {
    }

    public void Update(PuppeteerController controller)
    {
        
    }

    private IEnumerator Idle(PuppeteerController controller)
    {
        yield return new WaitForSeconds(FrontDelay / BASE_FPS); //선딜
               
        //쿨타임 중이라면 가중치를 0으로 초기화 (이 코드를 Enter에 작성할지 Idle코루틴에 넣을지 고민 중)
        if (controller.didStateJTRD)
        {
            height_JTRD = 0;
        }
        if (controller.didStateRLH)
        {
            heught_RLH = 0;
        }
        if (controller.didStateDR)
        {
            height_DR = 0;
        }

        ChooseState(controller);

        yield return new WaitForSeconds(backDelay / BASE_FPS); //후딜
    }

    void ChooseState(PuppeteerController controller)
    {
        //가중치 결정 함수들을 실행 시켜서 각각 가중치 결정
        int HeightState1 = height_JTRD; 
        int HeightState2 = heught_RLH;
        int HeightState3 = height_DR;

        int finalWeight = HeightState1 + HeightState2 + HeightState3; //전체 가중치
        int RandNum = Random.Range(1, finalWeight); //전체 가중치 안에서의 랜덤 가중치값

        //RanNum이 0 ~ HeightState1까지
        if(0 < RandNum && RandNum >= HeightState1)//JumpToRangedDealer
        {
            controller.chooseState = new JumpToRangedDealerState();
        }
        ////RanNum이 Height1 ~ HeightState2까지
        else if (HeightState1 < RandNum && RandNum >= HeightState2)//RunLikeHorse
        {
            controller.chooseState = new RunLikeHorseState();   
        }
        else //DollRush
        {
            controller.chooseState = new DollRushState();
        }
    }

  

}
