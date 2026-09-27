/// 작성자 : 유희일
using UnityEngine;

/// <summary>
/// 애니메이션 이벤트가 직접 호출하는 함수만 모아둔 클래스. Boss_Animator 오브젝트에 붙는다.
/// AE_ 접두를 붙이는 이유는, 애니메이션 이벤트 창이 이 오브젝트의 public 함수를 전부 목록에 띄우기 때문이다.
/// 접두가 없으면 애니메이터가 엉뚱한 함수를 걸어도 알아차릴 방법이 없다.
/// </summary>
public class Boss_AnimTrigger : MonoBehaviour
{
    [SerializeField] private Boss_Controller boss;
    [SerializeField] private Boss_AttackManager attackManager;
    private Coroutine Summon_Crow;

    void Start()
    {
        if(boss == null)
            Debug.LogWarning($"{name} : boss가 비어 있다.", this);
        if(attackManager == null)
            attackManager = boss.Attack;
    }
    public void AE_AttackOn(int index)
    {
        if (attackManager == null) return;

        Debug.Log($"{name} : 공격 {index}.");

        attackManager.Attack(index);
    }

    public void AE_AttackOff()
    {
        if (attackManager == null) return;
        attackManager.AttackEnd();
    }


    public void AE_Teleport()
    {
        Vector2 tpPoint = boss.Context.TargetPosition + (Vector2.right * 5f);
        boss.Moter.Teleport(tpPoint);
    }

    public void AE_Pattern_End() => boss.AI.Pattern_End();

// D_까마귀 소환
    public void AE_SummonCrow()
    {
        Debug.Log("AE_까마귀 소환.");
        if(Summon_Crow != null)
            StopCoroutine(Summon_Crow);

        Summon_Crow = StartCoroutine(boss.Spawn_CrowCo());
    }
    // 속도 조절은 패턴 스테이트에서 Switch문으로 설정.



   

    public void AE_Vfx(string key)
    {
        // TODO : Boss_vfx 추가 시 boss.Vfx.Play(key, transform.position) 으로 연결한다.
    }
}
