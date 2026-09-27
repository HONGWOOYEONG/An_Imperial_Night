using System.Collections.Generic;
using UnityEngine;

public class Boss_AttackManager : MonoBehaviour
{
    private Boss_PatternSO currentPattern ;
    private DamageInfo myDamageInfo = new();
    [SerializeField] private LayerMask whatIsTarget;
    [SerializeField] private Collider2D[] hitBoxes;
    private readonly HashSet<IDamageReceiver> hitTargets = new();
    void Awake()
    {
        AttackEnd();
    }

    public void SetPatternToAttack(Boss_PatternSO pt)
    {
        if (pt == null) return;
        currentPattern = pt;

        // 리펙터링 내용:Boss_PatternSO 안에 나열된 데미지 정보를 DamageInfo로 축약.
        myDamageInfo.damage = currentPattern.HpDamage;
        myDamageInfo.driveDamage = currentPattern.DriveDamage;
        myDamageInfo.postureDamage = currentPattern.PostureDamage;
        myDamageInfo.knockbackPower = currentPattern.KnockbackPower;
        myDamageInfo.damageType = currentPattern.DamageType;
        myDamageInfo.stunTime = currentPattern.StunTime;

        // damageDir 구현은 남겨놓는다.
    }
    public void Attack(int index)
    {
        if (hitBoxes == null || index < 0 || index >= hitBoxes.Length)
        {
            Debug.LogWarning($"{this} : 잘못된 히트박스 번호 {index}.");
            return;
        }
        hitTargets.Clear();
        if (hitBoxes[index] != null)
            hitBoxes[index].enabled = true;
    }

    public void AttackEnd()
    {
        hitTargets.Clear();
        if (hitBoxes == null) return;
        foreach(var col in hitBoxes)
            if (col != null) col.enabled = false; 
    }

    public void ApplyAttack(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & whatIsTarget) == 0) return;

        var target = other.GetComponent<IDamageReceiver>();
        if (target == null || !hitTargets.Add(target)) return;

        myDamageInfo.damageDir = (other.transform.position - transform.position).normalized;
        target.ReceiveAttack(myDamageInfo);
    }
}
