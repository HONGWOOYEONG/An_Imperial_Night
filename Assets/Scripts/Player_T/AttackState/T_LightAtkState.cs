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
        t_Attack.movement.AddMovementLock(t_Attack); //이동제어
        inputCount = 0;

        // 1타 단계로 변경
        currentStep = AttackStep.First;

        // 콤보 파라미터 초기화
        t_Attack.animation.ResetLightAttack();

        // 첫 번째 공격 애니메이션 실행
        t_Attack.animation.PlayLightAttack();

        t_Attack.activeAttackCount++;
    }

    public void FirstAttack(T_Attack t_Attack)
    {
        if (!isAttacking || currentStep != AttackStep.First)  return;

        //--타겟 갱신--
        targetPos = GetAttackTargetPos(t_Attack);

        //--1타 공격 생성--
        Vector2 newPos = t_Attack.createPos.position;
        GameObject obj_lightatk = t_Attack.InstantiateObject(t_Attack.commonAttackObject, newPos);

        if (obj_lightatk == null) return;

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
    }
    // 1타 애니메이션의 콤보 판정 프레임
    public void CheckFirstCombo(T_Attack t_Attack)
    {
        if (!isAttacking || currentStep != AttackStep.First) return;

        if (inputCount >= 2)
        {
            inputCount = 0;

            // 2, 3타 단계로 변경
            currentStep = AttackStep.SecondThird;
            t_Attack.animation.PlaySecondThirdAttack();
        }
        else
        {
            EndCombo(t_Attack);
        }
    }

    public void SecondAttack(T_Attack t_Attack)
    {
        if (!isAttacking || currentStep != AttackStep.SecondThird) return;

        Debug.Log("콤보공격 2시작");

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
            secondAtkInit.Initialize(damageInfo, t_Attack, targetPos);
        }
    }

    public void ThirdAttack(T_Attack t_Attack)
    { 
        if (!isAttacking || currentStep != AttackStep.SecondThird) return;

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
        CheckSecondThirdCombo(t_Attack);
    }

    public void CheckSecondThirdCombo(T_Attack t_Attack)
    {
        if (!isAttacking ||
            currentStep != AttackStep.SecondThird)
            return;

        //--예약한 공격횟수가 1보다 크면 다음콤보 공격이 나간다--
        if (inputCount >= 1)
        {
            inputCount = 0;
            currentStep = AttackStep.Final;

            t_Attack.animation.PlayFinalAttack();
        }
        //--콤보 종료--
        else
        {
            EndCombo(t_Attack);
        }
    }

    public void FinalAttack(T_Attack t_Attack)
    {
        if (!isAttacking || currentStep != AttackStep.Final) return;

        //--타겟 갱신--
        targetPos = GetAttackTargetPos(t_Attack);
        Vector2 finalPos = t_Attack.createPos.position;
        //--넉백-- 
        Vector2 knockbackDir = -((targetPos - finalPos).normalized);
        if (t_Attack.movement != null)
        {
            t_Attack.movement.KnockBack(knockbackDir, knockbackPower);
            t_Attack.movement.AddMovementLock(t_Attack); //이동제어
            Debug.Log("이동제어 시작");
        }

        //--4타 공격 생성--
        GameObject obj_finalAtk = t_Attack.InstantiateObject(t_Attack.finalAttackObject, finalPos);

        if (obj_finalAtk == null) return;

        OBJ_FinalAttack finalAtkInit = obj_finalAtk.GetComponent<OBJ_FinalAttack>();

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
            finalAtkInit.Initialize(damageInfo, t_Attack, targetPos);
        }
        EndCombo(t_Attack);
    }

   
    //--콤보가 종료될때 초기화--
    private void EndCombo(T_Attack t_Attack)
    {
        inputCount = 0;
        isAttacking = false;
        currentStep = AttackStep.None;
        nearTarget = null;

        //--4타가 끝난 뒤에도 이동이 잠긴 상태로 남을 가능성이 있어서 작성--
        if (t_Attack.movement != null)
        {
            t_Attack.movement.ReleaseMovementLock(t_Attack);
        }
        t_Attack.FinishAttack();

        //--애니메이션 초기화--
        t_Attack.animation.ResetLightAttack();
        // 콤보가 종료됐으므로 Exit 허용
        t_Attack.animation.EndLightAttackAnimation();
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
                break;

            case AttackStep.SecondThird:
                inputCount++;
                break;

            case AttackStep.Final:
                //--4타 이후에는 콤보가 없으므로 추가 입력 무시--
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
