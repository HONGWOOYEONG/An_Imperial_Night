/// 작성자 : 유희일
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스의 허브. 내부 클래스를 생성해 물려주고, 갱신 주기를 한 곳으로 모으고,
/// 따로 관리하기 힘든 이벤트 연결을 여기서 엮는다.
///
/// 여기 두지 않는 것
/// - 판단 : Boss_AI, 이동 : Boss_Moter, 수치 : Boss_Health. 전부 위임하고 여기서는 부르기만 한다.
/// - 피격 수신 : Col_Body의 Boss_HitBox가 Boss_Health로 직접 넘긴다.
/// </summary>
public class Boss_Controller : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Animator anim;

    [Header("타깃")]
    [SerializeField] private PlayerContext firstPlayer;
    [SerializeField] private PlayerContext secondPlayer;

    [Header("패턴")]
    [SerializeField] private List<Boss_PatternSO> patterns;

    [SerializeField, Min(0f)] private float decideInterval = 0.2f;

    [Header("그로기")]
    [SerializeField, Min(0f)] private float groggyDuration = 3f;

    public Boss_FSM FSM { get; private set; }
    public Boss_Context Context { get; private set; }
    public Boss_AI AI { get; private set; }

    public Animator Anim        => anim;
    public Boss_Health Health   => health;
    public Boss_Moter Moter     => moter;

    private Boss_Health health;
    private Boss_Moter moter;

    private void Awake()
    {
        health      = GetComponent<Boss_Health>();
        moter       = GetComponent<Boss_Moter>();

        if (anim == null)
        {
            Debug.LogWarning($"{name} : anim이 비어 있다. Boss_Animator의 Animator를 인스펙터에 물려야 상태가 애니메이션을 세팅한다.", this);
        }

        Context = new Boss_Context(transform);
        AI      = new Boss_AI(this, Context, patterns, decideInterval, groggyDuration);
        FSM     = new Boss_FSM(this);
    }

    private void OnEnable()
    {
        Connect_HealthEvent();
    }

    private void OnDisable()
    {
        Cancel_HealthEvent();
    }

    private void Start()
    {
        Context.SetTargets(firstPlayer, secondPlayer);

        FSM.ChangeState(FSM.Idle);
    }

    private void Update()
    {
        health.Tick();
        FSM.Tick();
    }

    /// <summary>
    /// 런타임에 플레이어가 만들어지는 경우 외부에서 넘긴다. 1인 플레이면 second에 null을 넘긴다.
    /// </summary>
    public void SetTargets(PlayerContext first, PlayerContext second)
    {
        Context.SetTargets(first, second);
    }



    // 후에 있을 연출을 위해 캡슐화해놓는다.
    private void Handle_Groggy()
    {
        FSM.ChangeState(FSM.Groggy);
    }

    private void Handle_Dead()
    {
        FSM.ChangeState(FSM.Dead);
    }
#region 이벤트

    private void Connect_HealthEvent()
    {
        health.OnGroggy += Handle_Groggy;
        health.OnDead += Handle_Dead;
    }
    private void Cancel_HealthEvent()
    {
        health.OnGroggy -= Handle_Groggy;
        health.OnDead -= Handle_Dead;
    }

    private void Connect_StateEvent()
    {
        // FSM.OnCurrentState +=
    }
    private void Cancel_StateEvent()
    {
        // FSM.OnCurrentState -=
    }

#endregion



}
