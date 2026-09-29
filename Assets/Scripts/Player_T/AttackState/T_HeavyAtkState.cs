using UnityEngine;
using System.Collections;


public class T_HeavyAtkState : I_TAttackState
{
    [Header("강공")]
    [SerializeField] float frontDelay = 90f;
    [SerializeField] float backDelay = 0.8f;
    private float knockbackPower = 7f;
    public void Enter(T_Attack t_Attack)
    {
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
        yield return new WaitForSeconds(t_attack.SecondsToFrames(frontDelay));

        Vector2 newPos = t_attack.createPos.position; //오브젝트 생성 위치
        Vector2 attackerPos = t_attack.transform.position; // 공격자 위치
        Vector2 knockbackDir = -((newPos - attackerPos).normalized); //넉백 방향

        GameObject obj_heavyAttack = t_attack.InstantiateObject(t_attack.h_Obj, newPos);
        OBJ_HeavyAttack heavyAttack = obj_heavyAttack.GetComponent<OBJ_HeavyAttack>();
        heavyAttack.Initialize(knockbackDir);
        if (t_attack.movement != null)
        {
            t_attack.movement.KnockBack(knockbackDir , knockbackPower);
        }

        yield return new WaitForSeconds(t_attack.SecondsToFrames(backDelay));
    }

}
