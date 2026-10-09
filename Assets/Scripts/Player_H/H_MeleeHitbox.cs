using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class H_MeleeHitbox : MonoBehaviour
{
    private Transform attackOwner;
    private DamageInfo damageInfo;
    private bool canHit;
    private readonly HashSet<Object> hitTargets = new HashSet<Object>();

    public void Initialize(Transform owner)
    {
        attackOwner = owner;
    }

    public void BeginAttack(DamageInfo attackDamage)
    {
        damageInfo = attackDamage;
        hitTargets.Clear();
        canHit = true;
    }

    public void EndAttack()
    {
        canHit = false;
        hitTargets.Clear();
    }

    private void OnDisable() => EndAttack();

    private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    private void OnTriggerStay2D(Collider2D other) => TryHit(other);

    private void TryHit(Collider2D other)
    {
        if (!canHit || attackOwner == null || other == null) return;
        if (other.transform.IsChildOf(attackOwner)) return;

        // 플레이어끼리는 공격 대상으로 취급하지 않는다.
        if (other.CompareTag("RangedDealer")) return;

        IDamageReceiver damageReceiver = other.GetComponent<IDamageReceiver>();
        if(damageReceiver == null) damageReceiver = other.GetComponentInParent<IDamageReceiver>();

        Object target = damageReceiver as Component;

        if (target == null || !hitTargets.Add(target))
            return;

        DamageInfo hitDamage = damageInfo;
        hitDamage.damageDir = ((Vector2)other.bounds.center - (Vector2)attackOwner.position).normalized;
        if (hitDamage.damageDir == Vector2.zero) hitDamage.damageDir = attackOwner.right;

        damageReceiver.ReceiveAttack(hitDamage);
    }
}
