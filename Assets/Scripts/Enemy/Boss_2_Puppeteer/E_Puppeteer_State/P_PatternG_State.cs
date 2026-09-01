using System.Collections;
using UnityEngine;


//점프 공격→몸통 돌리기→패턴 a 5타
public class P_PatternG_State : IPuppeteerState
{
    private Rigidbody2D rb;
    private Vector2 myPos;
    private Vector2 rangedPos;
    private Vector2 meleePos;
    private float disToRanged;    
    private float disToMelee;
    private float moveSpeed = 8f;

    [Header("JumpAttack")]
    private float frontDelay_Jump = 3f;//임의
    private float backDelay_Jump = 3f; //임의
    private Vector2 targetDestination;
    private float jumpDuration = 1f; //임의
    private float jumpForce = 6f; //임의

    [Header("SpinBody")]
    private bool isSpining = true;
    private float frontDelay_Spin = 3f;//임의
    private float backDelay_Spin = 3f; //임의
    private float spinDuration = 1f; //임의 , 몸통돌리기 할 때 걸리는 시간

    [Header("FiveHitCombo")]
    private float[] frontDelay_Five = { 5, 5, 5, 5, 30 };//임의
    private float[] backDelay_Five = { 3, 3, 3, 3, 50 }; //임의
    private AnimationCurve rotationCurve = AnimationCurve.Linear(0, 0, 1, 1); //회전 가감속 곡선

    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("패턴 G 시작");
        if (controller.rb != null)
        {
            rb = controller.rb;
        }

        controller.StartCoroutine(StartAttack(controller));
     
    }

    public void Exit(E_PuppeteerController controller)
    {
        Debug.Log("패턴 G 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        if (controller.targetPlayer == null) { return; }

        if (!isSpining)
        {
            controller.LookAtLocation(controller.targetPlayer.transform.position.x); //flip
        }
    }

    private IEnumerator StartAttack(E_PuppeteerController controller)
    {
        yield return controller.StartCoroutine(JumpAttack(controller)); //1타
        SetNearTarget(controller);
        yield return controller.StartCoroutine(MoveTarget(controller));
        yield return controller.StartCoroutine(SpinBody(controller)); //2타
        SetNearTarget(controller);
        yield return controller.StartCoroutine(MoveTarget(controller));
        yield return controller.StartCoroutine(FiveHitCombo(controller)); //3타

        controller.ChangeState(controller.states["idle"]);
    }
   
    private IEnumerator MoveTarget(E_PuppeteerController controller)
    {
        Debug.Log("이동 중");
        while (!controller.isInTargetPlayer)
        {
            if (controller.targetPlayer == null) break;

            Vector2 targetPos = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y);
            controller.transform.position = Vector2.MoveTowards(controller.transform.position, targetPos, moveSpeed * Time.deltaTime);

            yield return null;
        }
    }
    private void SetNearTarget(E_PuppeteerController controller)
    {
        myPos = controller.transform.position;
        rangedPos = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
        meleePos = new Vector2(controller.meleeDealer.transform.position.x, controller.transform.position.y);
        disToRanged = Vector2.Distance(myPos, rangedPos);
        disToMelee = Vector2.Distance(myPos, meleePos);
        controller.targetPlayer = disToRanged >= disToMelee ? controller.meleeDealer : controller.rangedDealer;
        Debug.Log("가장 가까운 타겟 : " + controller.targetPlayer);
    }
    private IEnumerator JumpAttack(E_PuppeteerController controller) //점프 공격
    {
        Debug.Log("점프 공격 시작");
        yield return new WaitForSeconds(frontDelay_Jump/ controller.BASE_FPS);  

        if (controller.targetPlayer != null)
        {
            targetDestination = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y);
        }
        Vector2 startPos = controller.transform.position;


        if (rb != null) rb.linearVelocity = Vector2.zero;

        float timer = 0f;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;

            Vector2 currentPos = Vector2.Lerp(startPos, targetDestination, t);

            currentPos.y += 4 * jumpForce * t * (1 - t); //포물선 점프 공식

            controller.transform.position = currentPos;
            yield return null;
        }
        controller.transform.position = targetDestination;

        yield return new WaitForSeconds(backDelay_Jump / controller.BASE_FPS);
        Debug.Log("점프 공격 종료");
    }

    private IEnumerator SpinBody(E_PuppeteerController controller) //몸통 돌리기
    {
        Debug.Log("몸통 돌리기 시작");
        isSpining = true;
        yield return new WaitForSeconds(frontDelay_Spin / controller.BASE_FPS);

        float timer = 0f;
        //360도 몸통 돌려 공격
        Vector3 startEuler = controller.transform.eulerAngles;

        while (timer < spinDuration)
        {
            timer += Time.deltaTime;

            float normalizedTime = timer / spinDuration; // 0~1 사이의 진행 비율 계산
            float curveTime = rotationCurve.Evaluate(normalizedTime); // AnimationCurve를 적용해 비율 조절 (Linear라면 등속)

            float currentAngle = 360f * curveTime;

            controller.transform.eulerAngles = new Vector3(startEuler.x, startEuler.y+ currentAngle, 0); //시작 회전값에 현재 각도만큼 더해서 회전 적용
            Debug.Log("현재 회전 값 : " + controller.transform.rotation.eulerAngles);
            yield return null;
        }

        yield return new WaitForSeconds(backDelay_Spin / controller.BASE_FPS);
        isSpining = false;
        Debug.Log("몸통 돌리기 종료");
    }
    private IEnumerator FiveHitCombo(E_PuppeteerController controller) //5타 공격
    {
        Debug.Log("5타 공격 시작");
        int num = 0;
        while (num < 5)
        {
            Debug.Log("현재 공격 타수 : "+ num );
            yield return new WaitForSeconds(frontDelay_Five[num] / controller.BASE_FPS);

            yield return new WaitForSeconds(backDelay_Five[num] / controller.BASE_FPS);
            num++;
        }

        Debug.Log("5타 공격 종료");
    }
}
