using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;
//가만히 있는 상태, 상태를 선택하게 됨
//모든 상태가 Idle을 거쳐감
public class P_IdleState : IPuppeteerState
{
    float BASE_FPS = 60f;

    float frontDelay = 3f; //Idle진입 후 판단 대기시간
    [Header("가중치")]

    int[] maxHeight = new int[] { 0, 0, 20, 0, 0, 0, 0 }; //최대 가중치
    int[] height = new int[] { 0, 0, 0, 0, 0, 0, 0 }; //실제 가중치

   IPuppeteerState[] states = new IPuppeteerState[] {
    new P_PatternA_State(),
    new P_PatternB_State(),
    new P_PatternC_State(),
    new P_PatternD_State(),
    new P_PatternE_State(),
    new P_PatternF_State(),
    new P_PatternG_State()
    };


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

        controller.ChangeState(controller.states["move"]);
    }

    void ChooseState(E_PuppeteerController controller)
    {
        //초기화
        for(int i=0; i<height.Length; i++)
        {
            height[i] = maxHeight[i];
        }

        // 가중치 결정 함수들을 실행 시켜서 각각 가중치 결정
        for (int i=0; i < controller.cooldowns.Count; i++)
        {
            //만약 cooldowns안에 states[i]라는 타입이 있다면 가중치를 0으로 결정
            if (controller.cooldowns.TryGetValue(states[i].GetType(), out float endTime))
            {
                if(Time.time < endTime) //아직 쿨타임이 돌지 않았음
                {
                    height[i] = 0;
                }
            }
        }



        #region C 패턴 선택 조건
        Vector2 rangedPos_C = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
        Vector2 meleePos_C = new Vector2(controller.meleeDealer.transform.position.x, controller.transform.position.y);
        float disToRanged_C = Vector2.Distance(rangedPos_C, controller.transform.position);
        float disToMelee_C = Vector2.Distance(meleePos_C, controller.transform.position);
        Vector2 nearPlayer_C = disToRanged_C >= disToMelee_C ? meleePos_C : rangedPos_C; //가장 가까운 플레이어 값
        float disToNearPlayer_C = Vector2.Distance(nearPlayer_C, controller.transform.position);

        if (disToNearPlayer_C > controller.rangeToPlayer_C) //적과의 거리가 n1초과이면
        {
            height[2] = 0;
        }

        #endregion

        #region F 패턴 선택 조건
        Vector2 rangedPos_F = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
        Vector2 meleePos_F = new Vector2(controller.meleeDealer.transform.position.x, controller.transform.position.y);
        float disToRanged_F = Vector2.Distance(rangedPos_F, controller.transform.position);
        float disToMelee_F = Vector2.Distance(meleePos_F, controller.transform.position);
        Vector2 nearPlayer_F = disToRanged_F >= disToMelee_F ? meleePos_F : rangedPos_F; //가장 가까운 플레이어 값
        float disToNearPlayer_F = Vector2.Distance(nearPlayer_F, controller.transform.position);
        if (disToNearPlayer_F < controller.rangeToPlayer_F) //적과의 거리가 n1미만이면
        {
            height[5] = 0;
        }
        #endregion

        #region G 패턴 선택 조건
        Vector2 myPos_G = controller.transform.position;
        Vector2 rangedPos_G = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
        float disToRanged_G = Vector2.Distance(myPos_G, rangedPos_G);
        if (disToRanged_G < controller.minRange || disToRanged_G > controller.maxRange)
        {
            height[6] = 0;
        }
        #endregion


        int finalWeight = 0;
        for(int i=0; i<height.Length; i++)
        {
            finalWeight += height[i]; //전체 가중치
        }

        if (finalWeight <= 0)
        {          
            return;
        }

        int RandNum = Random.Range(0, finalWeight); //전체 가중치 안에서의 랜덤 가중치값

        for(int i=0; i<height.Length; i++)
        {
            if (i == 0)
            {
                if (RandNum < height[i])
                {
                    controller.chooseState = states[i];
                }
            }
            else
            {
                if ((RandNum -= height[i - 1]) < height[i])
                {
                    controller.chooseState = states[i];
                }
            }
          
        }

    } 
}
