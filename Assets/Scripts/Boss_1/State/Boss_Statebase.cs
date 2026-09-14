/// 작성자 : 유희일
using UnityEngine;

/// <summary>
/// 모든 보스 상태의 공통 뼈대. 진입할 때 애니메이션 파라메터를 켜고 이탈할 때 끄는 일을
/// 파생 클래스가 손대지 않아도 되게 이 층에서 처리한다.
/// 상태가 쓰는 Boss_Controller와 Animator는 생성자로 주입받아 소유한다.
/// 상태가 직접 GetComponent로 찾아오면 참조를 쥐는 곳이 상태 수만큼 늘어나고,
/// 애니메이터를 다른 오브젝트로 옮겼을 때 어디가 깨지는지 추적이 안 된다.
///
/// 여기 두지 않는 것
/// - 어떤 상태로 갈지의 판단 : 대기/이동/공격은 Boss_AI, 그로기/사망은 Boss_Health가 호출한다.
/// - 패턴 수치 : Boss_Pattern이 가진다.
/// </summary>
public abstract class Boss_Statebase : IState
{
    protected readonly Boss_Controller boss;
    protected readonly Animator anim;

    // 문자열로 SetBool을 부르면 호출할 때마다 해싱한다. 생성 시점에 한 번만 변환해서 들고 있는다.
    private readonly int animHash;

    protected Boss_Statebase(Boss_Controller boss, Animator anim, string animParamName)
    {
        this.boss = boss;
        this.anim = anim;
        animHash = Animator.StringToHash(animParamName);
    }

    /// 주의: 파생에서 재정의할 때 base.Enter()를 빠뜨리면 애니메이션이 진입하지 않는다.
    public virtual void Enter()
    {
        anim.SetBool(animHash, true);
    } 

    public virtual void Tick() { }

    /// 주의: 파생에서 재정의할 때 base.Exit()를 빠뜨리면 파라메터가 켜진 채로 남아
    /// 다음 상태로 넘어가도 이전 애니메이션이 계속 재생된다.
    public virtual void Exit() 
    {
        anim.SetBool(animHash, false);
    }
}
