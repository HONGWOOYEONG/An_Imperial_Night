/// 작성자 : 유희일
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.Mathematics;
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
    private static WaitForSeconds _waitForSeconds0_2 = new(0.2f);
    [Header("참조")]
    [SerializeField] private Animator anim;
    [SerializeField] private TextMeshPro stateText;


    [Header("타깃")]
    [SerializeField] private PlayerContext firstPlayer;
    [SerializeField] private PlayerContext secondPlayer;

    [Header("패턴")]
    [SerializeField] private List<Boss_PatternSO> patterns;
    [SerializeField] private GameObject crow_obj;
    [SerializeField] private List<Transform> spawnPoints;



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

    // 구독은 Awake 다음인 OnEnable에서 한다. FSM을 Awake에서 만들므로 여기서는 반드시 존재한다.
    private void OnEnable()
    {
        Connect_HealthEvent();
        Connect_StateEvent();
    }

    private void OnDisable()
    {
        Cancel_HealthEvent();
        Cancel_StateEvent();
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
        AI.Tick();
    }

    // 이동과 돌진은 물리 스텝에서만 돈다. Update에서 rb를 밀면 벽 판정이
    // 프레임레이트에 따라 달라져서 같은 돌진이 어떤 날은 벽을 뚫는다.
    private void FixedUpdate()
    {
        FSM.FixedTick();
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
    // 디버그용 표시라 안 물려 있는 씬이 많다. 가드가 없으면 상태가 바뀔 때마다 NRE로 죽는다.
    private void Handle_StateText(string curState)
    {
        if (stateText == null) return;

        stateText.text = curState;
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

    // 델리게이트에는 괄호 없는 메서드 이름만 넘긴다. Handle_StateText() 처럼 괄호를 붙이면
    // "지금 호출해서 그 반환값을 구독한다"는 뜻이 되고, 반환형이 void라 컴파일이 안 된다.
    private void Connect_StateEvent()
    {
        FSM.OnCurrentState += Handle_StateText;
    }

    // 붙인 것과 띄는 것은 반드시 같은 메서드여야 한다. 이름이 다르면 해제가 조용히 실패해
    // 재활성화할 때마다 구독이 쌓이고 한 번 전환에 핸들러가 여러 번 돌게 된다.
    private void Cancel_StateEvent()
    {
        FSM.OnCurrentState -= Handle_StateText;
    }

#endregion

#region 스킬
    // 
[ContextMenu("test crow")]
public void Test_Spawn_CrowCo() => StartCoroutine( Spawn_CrowCo());
    public IEnumerator Spawn_CrowCo()
    {
        foreach (var point in spawnPoints)
        {
            StartCoroutine(Spawn_Crow(point));
            yield return _waitForSeconds0_2;
        }
    }
    private IEnumerator Spawn_Crow(Transform spawnPoint)
    {
        var crow = Instantiate(crow_obj,spawnPoint.position,quaternion.identity);
        crow.GetComponent<Crow>().SetCrow(target: Context.CurrentTarget.trans, centerPoint: spawnPoint);
        yield return null;
    }
#endregion

}
