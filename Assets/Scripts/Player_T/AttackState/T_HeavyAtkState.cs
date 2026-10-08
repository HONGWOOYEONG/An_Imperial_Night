using UnityEngine;
using System.Collections;
using Unity.Burst.Intrinsics;


public class T_HeavyAtkState : I_TAttackState
{
    [Header("강공")]
    [SerializeField] float frontDelay = 90f;
    [SerializeField] float backDelay = 0.8f;
    private float heavyAttackRecoilPower = 5f; 
    public void Enter(T_Attack t_Attack)
    {
        t_Attack.activeAttackCount++;
        t_Attack.StartCoroutine(HeavyAttack(t_Attack));
    }

    public void Exit(T_Attack t_Attack)
    {
    }

    public void Update(T_Attack t_Attack)
    {
    }
 

    private IEnumerator HeavyAttack(T_Attack t_attack)
    {
        yield return new WaitForSeconds(t_attack.FramesToSeconds(frontDelay));

        Vector2 newPos = t_attack.createPos.position; //오브젝트 생성 위치
        Vector2 attackerPos = t_attack.transform.position; // 공격자 위치
        Vector2 knockbackDir = -((newPos - attackerPos).normalized); //넉백 방향

        if(t_attack.movement!=null)
        {
            t_attack.movement.AddMovementLock(t_attack);
            Debug.Log("강공 이동제어 시작");
        }
        
        GameObject obj_heavyAttack = t_attack.InstantiateObject(t_attack.h_Obj, newPos);
        OBJ_HeavyAttack heavyAttack = obj_heavyAttack.GetComponent<OBJ_HeavyAttack>();

        DamageInfo heavyDamageInfo = new DamageInfo()
        {
            damage = 7f,
            damageDir = Vector2.zero,
            knockbackPower = 0,
            stunTime = 0,
            damageType = DamageType.HeavyAttack,
            driveDamage = 0
        }; 



        if (heavyAttack != null)
        {
            heavyAttack.Initialize(t_attack, knockbackDir, t_attack.transform , heavyDamageInfo);
        }

        if (t_attack.movement != null)
        {

            t_attack.movement.KnockBack(knockbackDir , heavyAttackRecoilPower);
        }

        yield return new WaitForSeconds(t_attack.FramesToSeconds(backDelay));
        t_attack.FinishAttack();
    }

}
