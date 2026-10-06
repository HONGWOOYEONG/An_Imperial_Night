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
    private void OnCollisionEnter2D(Collision2D collision) => TryHit(collision.collider);
    private void OnCollisionStay2D(Collision2D collision) => TryHit(collision.collider);

    private void TryHit(Collider2D other)
    {
        if (!canHit || attackOwner == null || other == null) return;
        if (other.transform.IsChildOf(attackOwner)) return;

        // 플레이어끼리는 공격 대상으로 취급하지 않는다.
        if (other.GetComponentInParent<PlayerHealth>() != null) return;

        IDamageReceiver receiver = other.GetComponentInParent<IDamageReceiver>();
        if (!(receiver is Component receiverComponent)) return;

        Object target = other.attachedRigidbody != null
            ? (Object)other.attachedRigidbody
            : receiverComponent;
        if (!hitTargets.Add(target)) return;

        DamageInfo hitDamage = damageInfo;
        hitDamage.damageDir = ((Vector2)other.bounds.center - (Vector2)attackOwner.position).normalized;
        if (hitDamage.damageDir == Vector2.zero) hitDamage.damageDir = attackOwner.right;

        receiver.ReceiveAttack(hitDamage);
    }
}
