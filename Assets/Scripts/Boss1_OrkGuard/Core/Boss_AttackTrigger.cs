using UnityEngine;

public class Boss_AttackTrigger : MonoBehaviour
{
    private Boss_AttackManager manager;
    void Awake()
    {
        manager = GetComponentInParent<Boss_AttackManager>();
    }
    void OnTriggerEnter2D(Collider2D collision) => manager.ApplyAttack(collision);
}
