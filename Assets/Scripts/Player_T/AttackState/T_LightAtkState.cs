using UnityEngine;
using System.Collections;
using System;

public class RangeCombo
{
    public float[] damage = { 100, 20, 20, 20 }; //임의
    public float[] frontDelay = { 13, 13, 10, 90 };
    public float[] backDelay = { 22, 5, 15, 20 };
}
public class T_LightAtkState : I_TAttackState
{
    private enum AttackStep 
    {
        None,
        First,
        SecondThird,    
        Final           
    }

    //-- 공격 설정 --
    [SerializeField] float attackrange = 20f; //공격 거리
    [SerializeField] float viewAngle = 85f; //시야각
    private DamageInfo damageInfo;

    //-- 타겟 --
    private Collider2D nearTarget;
    private float shortest = float.MaxValue;
    private Vector2 targetPos; //타겟 위치

    //-- 콤보 --
    private RangeCombo combo = new RangeCombo();
    private AttackStep currentStep = AttackStep.None; //현재 공격 단계
    public int inputCount = 0;
    //isAttaking을 사용해서 콤보를 실행할지 처음부터 실행될지 여부를 판단
    private bool isAttacking = false; //현재 콤보 진행 여부

    private float knockbackPower = 2f;
    public void Enter(T_Attack t_Attack)
    {
        if (isAttacking) return; //현재 콤보 공격이라면 다시 시작하지 않음
        StartCombo(t_Attack);
    }

    private void StartCombo(T_Attack t_Attack)
    {
        isAttacking = true;
        inputCount = 0;
        currentStep = AttackStep.First;

        // 콤보 파라미터 초기화
        t_Attack.animation.ResetLightAttack();

        // 첫 번째 공격 애니메이션 실행
        t_Attack.animation.PlayLightAttack();

        t_Attack.activeAttackCount++;
        t_Attack.StartCoroutine(FirstAttack(t_Attack));
    }

    private IEnumerator FirstAttack(T_Attack t_Attack)
    {
        Debug.Log("콤보공격 1 시작");
        currentStep = AttackStep.First;
        inputCount = 0;

        yield return new WaitForSeconds(t_Attack.FramesToSeconds(combo.frontDelay[0]));

        //--타겟 갱신--
        targetPos = GetAttackTargetPos(t_Attack);

        //--1타 공격 생성--
        Vector2 newPos = t_Attack.createPos.position;
        GameObject obj_lightatk = t_Attack.InstantiateObject(t_Attack.commonAttackObject, newPos);
        OBJ_LightAttack firstAtkInit = obj_lightatk.GetComponent<OBJ_LightAttack>();
        if (firstAtkInit != null)
        {
            Vector2 dir = (targetPos - newPos).normalized;
            damageInfo = new DamageInfo()
            {
                damage = combo.damage[0],
                damageDir = dir,
                knockbackPower = 0,
                stunTime = 0,
                damageType = DamageType.LightAttack,
                driveDamage = 0
            };
            firstAtkInit.Initialize(damageInfo, t_Attack, targetPos);
        }

        yield return new WaitForSeconds(t_Attack.FramesToSeconds(combo.backDelay[0]));

        //--예약한 공격횟수가 2보다 크면 다음콤보 공격이 나간다--
        if (inputCount >= 2)
        {
            inputCount = 0;
            t_Attack.animation.PlaySecondThirdAttack();
            yield return t_Attack.StartCoroutine(SecondThirdAttack(t_Attack));
        }
        //--콤보 종료--
        else
        {
            t_Attack.animation.animator.SetBool("isSecondThirdAtk", false);
            EndCombo(t_Attack);
        }
    }

    private IEnumerator SecondThirdAttack(T_Attack t_Attack)
    {
        Debug.Log("콤보공격 2,3 시작");
        currentStep = AttackStep.SecondThird;
        inputCount = 0;

        yield return new WaitForSeconds(t_Attack.FramesToSeconds(combo.frontDelay[1]));

        //--타겟 갱신--
        targetPos = GetAttackTargetPos(t_Attack);

        //--2타 공격 생성--
        Vector2 secondPos = t_Attack.createPos.position;
        GameObject obj_secondAtk = t_Attack.InstantiateObject(t_Attack.commonAttackObject, secondPos);
        OBJ_LightAttack secondAtkInit = obj_secondAtk.GetComponent<OBJ_LightAttack>();
        if (secondAtkInit != null)
        {
            Vector2 dir = (targetPos - secondPos).normalized;
            damageInfo = new DamageInfo()
            {
                damage = combo.damage[1],
                damageDir = dir,
                knockbackPower = 0,
                stunTime = 0,
                damageType = DamageType.LightAttack,
                driveDamage = 0
            };
            secondAtkInit.Initialize(damageInfo,t_Attack, targetPos);
        }

        yield return new WaitForSeconds(t_Attack.FramesToSeconds(combo.backDelay[1]));

        yield return new WaitForSeconds(t_Attack.FramesToSeconds(combo.frontDelay[2]));

        //--타겟 갱신--
        targetPos = GetAttackTargetPos(t_Attack);

        //--3타 공격 생성--
        Vector2 thirdPos = t_Attack.createPos.position;
        GameObject obj_thirdAtk = t_Attack.InstantiateObject(t_Attack.commonAttackObject, thirdPos);
        OBJ_LightAttack thirdAtkInit = obj_thirdAtk.GetComponent<OBJ_LightAttack>();
        if (thirdAtkInit != null)
        {
            Vector2 dir = (targetPos - thirdPos).normalized;
            damageInfo = new DamageInfo()
            {
                damage = combo.damage[2],
                damageDir = dir,
                knockbackPower = 0,
                stunTime = 0,
                damageType = DamageType.LightAttack,
                driveDamage = 0
            };
            thirdAtkInit.Initialize(damageInfo, t_Attack, targetPos);
        }

        yield return new WaitForSeconds(t_Attack.FramesToSeconds(combo.backDelay[2]));

        //--예약한 공격횟수가 1보다 크면 다음콤보 공격이 나간다--
        if (inputCount >= 1)
        {
            inputCount = 0;
            t_Attack.animation.PlayFinalAttack();
            yield return t_Attack.StartCoroutine(FinalAttack(t_Attack));
        }
        //--콤보 종료--
        else
        {
            t_Attack.animation.animator.SetBool("isFinalAtk", false);
            EndCombo(t_Attack);
        }
    }

    private IEnumerator FinalAttack(T_Attack t_Attack)
    {
        Debug.Log("콤보공격 4 시작");
        currentStep = AttackStep.Final;

        yield return new WaitForSeconds(t_Attack.FramesToSeconds(combo.frontDelay[3]));

        //--타겟 갱신--
        targetPos = GetAttackTargetPos(t_Attack);
        Vector2 finalPos = t_Attack.createPos.position;
        //--넉백-- 
        Vector2 knockbackDir = -((targetPos - finalPos).normalized);
        t_Attack.movement.KnockBack(knockbackDir, knockbackPower);

        //--4타 공격 생성--
        GameObject obj_finalAtk = t_Attack.InstantiateObject(t_Attack.finalAttackObject, finalPos);
        OBJ_FinalAttack finalAtkInit = obj_finalAtk.GetComponent<OBJ_FinalAttack>();

        if (t_Attack.movement != null)
        {
            t_Attack.movement.AddMovementLock(t_Attack); //이동제어
            Debug.Log("이동제어 시작");
        }

        if (finalAtkInit != null)
        {
            Vector2 dir = (targetPos - finalPos).normalized;
            damageInfo = new DamageInfo()
            {
                damage = combo.damage[3],
                damageDir = dir,
                knockbackPower = 0,
                stunTime = 0,
                damageType = DamageType.Mark,
                driveDamage = 0
            };
            finalAtkInit.Initialize(damageInfo, t_Attack , targetPos);
        }
        yield return new WaitForSeconds(t_Attack.FramesToSeconds(combo.backDelay[3]));
        //--콤보 종료--
        EndCombo(t_Attack);
    }

    //--콤보가 종료될때 초기화--
    private void EndCombo(T_Attack t_Attack)
    {
        inputCount = 0;
        isAttacking = false;
        currentStep = AttackStep.None;
        nearTarget = null;

        t_Attack.FinishAttack();
        t_Attack.animation.ResetLightAttack();
    }


    //--입력을 받아 다음 콤보로 이어갈지 결정--
    public void AddInput(T_Attack t_Attack)
    {
        //--콤보가 끝난 상태에서 새로운 1타를 실행 하기 위함--
        if (!isAttacking)
        {
            if (t_Attack != null)
            {
                if (t_Attack.playerController != null && !t_Attack.playerController.TryStartAction(PlayerState.Attacking))
                    return;

                StartCombo(t_Attack);
            }
            return;
        }

        //--현재 공격 단계에 따라 입력 저장--
        switch (currentStep)
        {
            case AttackStep.First:
                inputCount++;
                Debug.Log("first " + inputCount +" 현재 currentStep "+ currentStep);
                break;

            case AttackStep.SecondThird:
                inputCount++;
                Debug.Log("SecondThird " + inputCount + " 현재 currentStep " + currentStep);
                break;

            case AttackStep.Final:
                //--4타 이후에는 콤보가 없으므로 추가 입력 무시--
                Debug.Log("fianl " + inputCount + " 현재 currentStep " + currentStep);
                break;
        }
    }
    private Vector2 GetAttackTargetPos(T_Attack t_Attack)
    {
        FindToNearTarget(t_Attack);

        //--가까운 적이 있다면 적의 위치를 targetPos로 지정--
        if (nearTarget != null)
        {
            ITargetable target =
            nearTarget.GetComponent<ITargetable>();

            if (target != null && target.TargetPoint != null)
            {
                return target.TargetPoint.position;
            }

            //--TargetPoint가 없는 적이면 기존 Pivot 사용(예외처리)--
            return nearTarget.transform.position;
        }

        //--적이 없으면 플레이어가 바라보는 방향으로 공격--
        Vector2 lookDir =
            Vector2.right * t_Attack.movement.FacingDirection;

        return (Vector2)t_Attack.createPos.position
            + lookDir * 10f;
    }

    //--Overlap안에 들어온 적 오브젝트 중 플레이어와 가장 거리가 짧은 적을 감지해 nearTarget을 지정--
    private void FindToNearTarget(T_Attack t_Attack)
    {
        shortest = float.MaxValue;
        nearTarget = null;

        Collider2D[] targets = Physics2D.OverlapCircleAll(t_Attack.transform.position, attackrange);
        foreach (Collider2D target in targets)
        {
            if (target.CompareTag("Enemy"))
            {
                Vector2 targetPos = target.transform.position;
                Vector2 playerPos = t_Attack.transform.position;

                Vector2 dir = (targetPos - playerPos).normalized;
                Vector2 myForward = Vector2.right * t_Attack.movement.FacingDirection; ;
                float angle = Vector2.Angle(myForward, dir); //바라보는 시야각도
                if (angle <= viewAngle)
                {
                    float distance = Vector2.Distance(playerPos, targetPos);
                    if (distance < shortest)
                    {
                        shortest = distance;
                        nearTarget = target;
                    }
                }
            }
        }
    }


    public void Exit(T_Attack t_Attack) //--공격 도중 강제로 취소 됐을 때--
    {
        inputCount = 0;
        isAttacking = false;
        currentStep = AttackStep.None;

        nearTarget = null;
        shortest = float.MaxValue;
    }

    public void Update(T_Attack t_Attack)
    {
    }
}
