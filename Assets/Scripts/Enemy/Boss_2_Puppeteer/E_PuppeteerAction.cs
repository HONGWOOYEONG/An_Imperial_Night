using System.Collections;
using UnityEngine;


public class E_PuppeteerAction : MonoBehaviour
{

    [Header("패턴 C")]
    Vector2 targetPos_C;
    public bool isThrowSpiderWeb = true;
    private float buttPosXOffset = 5f;
    private float speedButt = 10f;
    private float buttDuration = 0.5f;// 머리박치기 유지시간

    //일반 공격(평타)
    public IEnumerator NormalAttack(E_PuppeteerController controller , float attackHoldTime) 
    //(controller, 공격 판점 유지 시간)
    {
        Debug.Log("일반 공격");
        controller.isAttaking_A = true;
        controller.HitBox_A.SetActive(true); //히트박스 킴
        yield return new WaitForSeconds(attackHoldTime / controller.BASE_FPS); //판정 유지 시간
        controller.HitBox_A.SetActive(false); //히트박스 끔
        controller.isAttaking_A = false;
    }
    //돌진
    public void Rush(E_PuppeteerController controller, Vector2 targetPos,float rushSpeed) 
    {
        Debug.Log("돌진");
        controller.transform.position = Vector2.MoveTowards(controller.transform.position, targetPos, rushSpeed * Time.deltaTime);
    }
    //점프 공격
    public IEnumerator JumpAttack(E_PuppeteerController controller, Rigidbody2D rb, float currentJumpDuration, Vector2 startPos, Vector2 targetPos, float jumpForce ) 
    //(controller, 점프할 대상의 rb, 점프 지속 시간, 점프 시작할 위치, 점프가 끝날 위치, 점프할 때 가할 힘)
    {
        Debug.Log("점프 공격");
        float timer = 0f;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        while (timer < currentJumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / currentJumpDuration;

            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);

            currentPos.y += 4 * jumpForce * t * (1 - t); //포물선 점프 공식

            controller.transform.position = currentPos;
            yield return null;
        }
        controller.transform.position = targetPos;
    }

    //헤비레리어트 
    public void HeavyLariat(E_PuppeteerController controller)
    {
        SetNearTargetPlayer(controller);
        Vector2 nearTargetPos = controller.transform.position;
        Debug.Log("해비 레리어트 시작");
        
        //.SetAcitve("true");
        //헤이레이어트 공격

        Debug.Log("해비 레리어트 종료");
    }

   public void SpinBody(E_PuppeteerController controller) //몸통 돌리기
    {
        Debug.Log("몸통 돌리기 시작");
     
        Debug.Log("몸통 돌리기 종료");
    }

    //머리박치기
    public IEnumerator HeadButt(E_PuppeteerController controller)
    {
        Debug.Log("머리박치기 시작");
        //현재 플레이어 x값과 적의 x값을 비교해서 오른쪽에 있으면 + 왼쪽에 있으면 -
        float myX = controller.transform.position.x;    
        float targetX = controller.targetPlayer.transform.position.x;

        if (myX > targetX) // 적이 오른쪽에 있다면
        {
            targetPos_C = new Vector2(controller.transform.position.x - buttPosXOffset, controller.transform.position.y);
        }
        else //적이 왼쪽에 있다면
        {
            targetPos_C = new Vector2(controller.transform.position.x + buttPosXOffset, controller.transform.position.y);
        }
            
        // 일정 시간 동안 돌진
        float elapsed = 0f;
        controller.hitBoxHeadButt.SetActive(true);
        while (elapsed < buttDuration) // 0.3초 동안 돌진
        {
            controller.transform.position = Vector2.MoveTowards(controller.transform.position, targetPos_C, speedButt * Time.deltaTime );
            elapsed += Time.deltaTime;
            yield return null;
        }
        controller.hitBoxHeadButt.SetActive(false);
    }

    //거미줄 뿌리기
    public void ShootWeb(GameObject spiderWeb,Vector2 firePos ,Vector2 targetPos)
    //(프리팹, 생성될 위치, 생성한 후 이동할 위치)
    {
        Debug.Log("거미줄 생성 완료");
        GameObject web = Instantiate(spiderWeb, firePos, Quaternion.identity);
        SpiderWeb spider = web.GetComponent<SpiderWeb>();
        if (spider != null)
        {
            spider.endPos = targetPos;
        }
        
        isThrowSpiderWeb = false;

    }


    #region 타겟 관련 함수


    //적과 가장 가까운 플레이어를 targetPlayer로 설정하는 함수
    public void SetNearTargetPlayer(E_PuppeteerController controller)
    {
        Vector2 myPos = controller.transform.position;
        Vector2 rangedPos = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
        Vector2 meleePos = new Vector2(controller.meleeDealer.transform.position.x, controller.transform.position.y);
        float disToRanged = Vector2.Distance(myPos, rangedPos);
        float disToMelee = Vector2.Distance(myPos, meleePos);

        controller.targetPlayer = disToRanged >= disToMelee ? controller.meleeDealer : controller.rangedDealer;
    }

    #endregion
}
