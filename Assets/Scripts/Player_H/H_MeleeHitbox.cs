using UnityEngine;

// Configured by H_Attack using its existing Inspector hitbox arrays.
[DisallowMultipleComponent]
public sealed class H_MeleeHitbox : MonoBehaviour
{
    private H_Attack owner;
    private GameObject hitboxRoot;

    public void Initialize(H_Attack attack, GameObject root)
    {
        owner = attack;
        hitboxRoot = root;
    }

    private void OnTriggerEnter2D(Collider2D other) => owner?.ReceiveHit(hitboxRoot, other);
    private void OnTriggerStay2D(Collider2D other) => owner?.ReceiveHit(hitboxRoot, other);
    private void OnCollisionEnter2D(Collision2D collision) => owner?.ReceiveHit(hitboxRoot, collision.collider);
    private void OnCollisionStay2D(Collision2D collision) => owner?.ReceiveHit(hitboxRoot, collision.collider);
}
