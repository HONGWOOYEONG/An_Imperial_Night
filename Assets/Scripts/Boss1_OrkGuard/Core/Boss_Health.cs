/// 작성자 : 유희일
/// 보스의 체력과 관련된 모든 변수와 함수를 책임진다.
/// 체력이나 체간이 0일때 로직은 이벤트로 처리한다.
/// 체력과 체간도 이벤트로 처리해서 ui에 반영한다.
/// 
/// 체력/체간, 감소/회복
/// 
using System;
using UnityEngine;

public class Boss_Health : MonoBehaviour
{
    [Header("체력")]
    [SerializeField, Min(1f)] private float maxHp = 1000f;

    [Header("라이프포인트")]
    [SerializeField, Min(0)] private int maxLifePoint = 1;


    [Header("밸런스")]
    [SerializeField, Min(1f)] private float maxBalance = 100f;

    [SerializeField, Min(0f)] private float balanceRegenDelay = 3f;

    [SerializeField, Min(0f)] private float balanceRegenPerSecond = 20f;

    public float CurrentHp { get; private set; }
    public float CurrentBalance { get; private set; }
    public int CurrentLifePoint { get; private set; }

    public float MaxHp => maxHp;
    public float MaxBalance => maxBalance;
    public int MaxLifePoint => maxLifePoint;

    public bool IsDead {get; private set;} = false;
    // 파생값은 필드에 캐싱하지 않고 매번 계산한다. 두 곳이 어긋날 여지를 없앤다.
    // 체간이 0이거나 체력이 0이면 그로기 상태이다. 체력회복으로 자동 탈출한다.
    public bool IsGrogy => CurrentBalance <= 0f || CurrentHp <= 0f;
    private bool wasGrogy; // 그로기 상태가 되기 직전의 상태를 캐싱. 그로기 상태가 되면 true, 아니면 false.
    // ui갱신용 이벤트
    public event Action<float, float> OnHpChanged;
    public event Action<float, float> OnBalanceChanged;
    public event Action<int, int> OnLifePointChanged;

    // 상태 변경 이벤트, fsm에서 호출하면 될것 같은데 이건 굳이 구현해야하나? 
    public event Action OnGrogy;
    public event Action OnGrogyKill;
    public event Action OnDead;

    // 비교형 타이머. 히트스탑이 걸려도 체감 시간이 밀리지 않도록 게임플레이 시계(Time.time)를 쓴다.
    private float nextBalanceRegenTime;

    private bool hasGrogyKill = false;

    private void Awake()
    {
        CurrentHp = maxHp;
        CurrentBalance = maxBalance;
        CurrentLifePoint = maxLifePoint;
    }

    void OnEnable()
    {
        OnGrogy += () => sendHealthInfo();

        OnGrogyKill += () => sendHealthInfo();

        OnDead += () => sendHealthInfo();
    }

    // 체간의 자연 회복 규칙이다.
    public void Tick()
    {
        if (IsDead) return;

        if (IsGrogy) return;

        if (Time.time < nextBalanceRegenTime) return;
        if (CurrentBalance >= maxBalance) return;

        Apply_Balance(balanceRegenPerSecond * Time.deltaTime);
    }

#region 데미지 로직

    [ContextMenu("피격테스트")]
    public void TakeDamageTest()=>TakeDamage(10,10);
    public void TakeDamage(float hpDamage, float balanceDamage)
    {
        if (IsDead) return;
        if (IsGrogy && !hasGrogyKill)
        { // 그로기 상태에서 데미지를 받으면 라이프 소모와 애니메이션 출력. 
            GrogyKill();
            return;
        }

        if (balanceDamage > 0f) // 데미지 직후 일정시간 밸런스 회복 금지.
            nextBalanceRegenTime = Time.time + balanceRegenDelay;

        // 그로기를 한번만 호출하는 조건.IsBroken는 체력과 체간 기반으로 자동계산.
        wasGrogy = IsGrogy;

            Apply_Hp(-hpDamage);
            Apply_Balance(-balanceDamage);

        if (!wasGrogy && IsGrogy)
            OnGrogy?.Invoke();
    }


    private void Apply_Hp(float delta)
    {
        if (delta == 0f) return;

        CurrentHp = Mathf.Clamp(CurrentHp + delta, 0f, maxHp);
        OnHpChanged?.Invoke(CurrentHp, maxHp);
        sendHealthInfo();
    }
    
    private void Apply_Balance(float delta)
    {
        if (delta == 0f) return;

        CurrentBalance = Mathf.Clamp(CurrentBalance + delta, 0f, maxBalance);
        OnBalanceChanged?.Invoke(CurrentBalance, maxBalance);
        sendHealthInfo();
    }

    // 라이프를 1소모. OnGrogyKill 이벤트에 등록되어있다. 애니메이션 전환은 controller에서 처리한다.
    private void GrogyKill()
    {
        hasGrogyKill = true; // 그로기 킬로 진입했으니 더이상 그로기 킬 이벤트를 호출하지 않는다.
        wasGrogy = true;
        if (CurrentLifePoint == 0)
        {
            IsDead = true;
            OnDead?.Invoke();
        }
        else if (CurrentLifePoint > 0)
        {
            CurrentLifePoint--;
            OnGrogyKill?.Invoke();
            OnLifePointChanged?.Invoke(CurrentLifePoint, maxLifePoint);
        }
    }

#endregion



    // 그로기 기간동안 체력이나체간을 회복하지 않는다.
    // 그로기 Exit()에서 호출할 회복 로직이다.
    public void Init_BalanceAndHp()
    {
        if (IsDead) return;

        Init_Balance();

        if (hasGrogyKill) // 그로기 킬을 하면 체력까지 회복한다.
            Init_Health();
        if (CurrentHp <= 0) // 그로기 킬이 아니면 체력은 조금만 회복
            Apply_Hp(maxHp * 0.1f);
        hasGrogyKill = false; // 그로기 킬 이벤트를 다시 호출할 수 있도록 허용한다.
    }
    // 한 번에 체간을 회복시킬 때.
    private void Init_Balance()
    {
        if (IsDead) return;

        CurrentBalance = maxBalance;
        nextBalanceRegenTime = Time.time;
        OnBalanceChanged?.Invoke(CurrentBalance, maxBalance);
    }
    private void Init_Health()
    {
        if (IsDead) return;

        CurrentHp = maxHp;
        OnHpChanged?.Invoke(CurrentHp, maxHp);
    }


    // 디버깅용 함수
    private void sendHealthInfo()
    {
        Debug.Log($"남은 밸런스:{CurrentBalance}\n남은 체력:{CurrentHp}\n남은 라이프:{CurrentLifePoint}\n"); 
    }
}
