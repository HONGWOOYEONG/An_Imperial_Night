/// 작성자 : 유희일
using UnityEngine;

/// <summary>
/// Col_Body에 붙는다. 때리는 쪽이 collision.GetComponent&lt;IDamageReceiver&gt;()로 찾기 때문에
/// 피격 콜라이더와 같은 오브젝트에 있어야 한다. 루트에 두면 공격이 보스를 못 찾는다.
/// 받은 피해를 Boss_Health로 넘기는 것 외에는 아무것도 하지 않는다.
/// </summary>
public class Boss_HitBox : MonoBehaviour, IDamageReceiver
{
    [SerializeField] private Boss_Health health;

    private void Awake()
    {
        if (health == null)
        {
            Debug.LogWarning($"{name} : health가 비어 있다. 루트의 Boss_Health를 인스펙터에 물려야 피격이 들어간다.", this);
        }
    }

    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (health == null) return;

        health.TakeDamage(damageInfo.damage, damageInfo.postureDamage);
    }
}
