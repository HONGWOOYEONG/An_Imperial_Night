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
    private readonly int animHash;
    public float endIntervalTime = 0f;
    private float timer = 0f;
    private readonly string animName;

    public string Name => animName;

    protected Boss_Statebase(Boss_Controller boss, string animParamName)
    {
        this.boss = boss;
        anim = boss.Anim;
        animName = animParamName;
        animHash = Animator.StringToHash(animParamName);
    }

    /// 주의: 파생에서 재정의할 때 base.Enter()를 빠뜨리면 애니메이션이 진입하지 않는다.
    public virtual void Enter()
    {
        anim.SetBool(animHash, true);
        timer = 0f;
    } 
    /// <summary>
    /// 마지막 프레임에서 멈추는 인터벌 시간 존재. 패턴간 부드러운 전환을 위해 삽입.
    /// </summary>
    public virtual void Tick()
    {
        bool isAnimOnecLoop = anim.GetCurrentAnimatorStateInfo(0).normalizedTime>=1f;
        if (isAnimOnecLoop)
            timer += Time.deltaTime;
    }

    /// 주의: 파생에서 재정의할 때 base.Exit()를 빠뜨리면 파라메터가 켜진 채로 남아
    /// 다음 상태로 넘어가도 이전 애니메이션이 계속 재생된다.
    public virtual void Exit() 
    {
        anim.SetBool(animHash, false);
        timer = 0f;
    }
}
