/// 작성자 : 유희일
using UnityEngine;

/// <summary>
/// 애니메이션 이벤트가 직접 호출하는 함수만 모아둔 클래스. Boss_Animator 오브젝트에 붙는다.
/// AE_ 접두를 붙이는 이유는, 애니메이션 이벤트 창이 이 오브젝트의 public 함수를 전부 목록에 띄우기 때문이다.
/// 접두가 없으면 애니메이터가 엉뚱한 함수를 걸어도 알아차릴 방법이 없다.
/// </summary>
public class Boss_AnimTrigger : MonoBehaviour
{
    [Tooltip("부모의 Boss_Controller. GetComponentInParent로 찾지 않고 직접 물린다. 프리팹 구조가 바뀌어도 조용히 null이 되지 않는다.")]
    [SerializeField] private Boss_Controller boss;

    [Tooltip("Col_Attack. 애니메이션 클립에서 enabled 커브로 켜고 끌 수 있으면 그쪽이 우선이고, 여기는 클립으로 처리 못 하는 경우에만 쓴다.")]
    [SerializeField] private Collider2D attackCollider;

    private void Awake()
    {
        if (boss == null)
        {
            Debug.LogWarning($"{name} : boss가 비어 있다. 부모의 Boss_Controller를 인스펙터에 물려야 애니메이션 이벤트가 전달된다.", this);
        }

        // 공격 판정은 항상 꺼진 채로 시작한다. 클립이 끄는 프레임을 지나기 전에
        // 다른 상태로 전환되면 판정이 켜진 채로 남아 보스가 스쳐도 맞는다.
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    public void AE_AttackOn()
    {
        if (attackCollider == null) return;

        attackCollider.enabled = true;
    }

    public void AE_AttackOff()
    {
        if (attackCollider == null) return;

        attackCollider.enabled = false;
    }


    public void AE_Teleport()
    {
        Vector2 tpPoint = boss.Context.TargetPosition + (Vector2.right * 5f);
        boss.Moter.Teleport(tpPoint);
    }

    public void AE_Pattern_End() => boss.AI.Pattern_End();





   

    public void AE_Vfx(string key)
    {
        // TODO : Boss_vfx 추가 시 boss.Vfx.Play(key, transform.position) 으로 연결한다.
    }
}
