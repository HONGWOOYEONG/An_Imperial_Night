/// 작성자 : 유희일
using UnityEngine;

public abstract class Boss_Statebase : IState
{
    protected readonly Boss_Controller boss;
    protected readonly Animator anim;

    private int animHash;
    protected string animName;
    protected float endIntervalTime = 0f;
    protected float timer = 0f;

    // 없는 상태를 재생하려 했을 때 경고가 매 프레임 쌓이지 않도록 한 번만 알린다.
    public string Name => animName;

    protected Boss_Statebase(Boss_Controller boss, string animStateName)
    {
        this.boss = boss;
        anim = boss.Anim;
        animName = animStateName;
        animHash = Animator.StringToHash(animStateName);
    }

    /// 주의: 파생에서 재정의할 때 base.Enter()를 빠뜨리면 애니메이션이 진입하지 않는다.
    public virtual void Enter()
    {
        timer = 0f;
        boss.Moter.HandleFlip();
        boss.Anim.SetBool(animHash,true);
    }

    /// <summary>
    /// 대부분의 이동은 애니메이터 이벤트로 시작.
    /// </summary>
    public virtual void Tick()
    {
        timer += Time.deltaTime;
    }

    /// <summary>
    /// 기본값은 '가로로 움직이지 않는다'. 움직이는 상태만 재정의한다.
    /// 속도 기반으로 바뀌면서 멈추는 호출을 빠뜨리면 상태가 끝나도 계속 미끄러진다.
    /// </summary>
    public virtual void FixedTick()
    {
        boss.Moter.Stop_Horizontal();
    }

    /// 주의: 파생에서 재정의할 때 base.Exit()를 빠뜨리면 타이머가 남아서
    /// 다음에 이 상태로 들어왔을 때 인터벌을 기다리지 않고 곧바로 넘어간다.
    public virtual void Exit()
    {
        timer = 0f;
        boss.Anim.SetBool(animHash,false);
    }

}
