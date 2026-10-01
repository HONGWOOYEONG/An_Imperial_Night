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

    // 라이프를 쓰고 되살아날 때 회복할 체력 비율이다.
    [SerializeField, Range(0f, 1f)] private float reviveHpRatio = 1f;

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

    // 파생값은 필드에 캐싱하지 않고 매번 계산한다. 두 곳이 어긋날 여지를 없앤다.
    // 라이프가 0인 상태로 체력이 0이어야 사망이다.
    public bool IsDead => CurrentHp <= 0f && CurrentLifePoint <= 0;
    public bool IsBroken => CurrentBalance <= 0f;

    public event Action<float, float> OnHpChanged;
    public event Action<float, float> OnBalanceChanged;
    public event Action<int, int> OnLifePointChanged;
    public event Action OnGroggy;
    public event Action OnLifeLost;
    public event Action OnDead;

    // 비교형 타이머. 히트스탑이 걸려도 체감 시간이 밀리지 않도록 게임플레이 시계(Time.time)를 쓴다.
    private float nextBalanceRegenTime;

    private void Awake()
    {
        CurrentHp = maxHp;
        CurrentBalance = maxBalance;
        CurrentLifePoint = maxLifePoint;
    }

    public void TakeDamage(float hpDamage, float balanceDamage)
    {
        if (IsDead) return;

        if (balanceDamage > 0f)
        {
            nextBalanceRegenTime = Time.time + balanceRegenDelay;
        }

        Apply_Hp(-hpDamage);
        Apply_Balance(-balanceDamage);
    }

    // 체간의 자연 회복 규칙이다.
    public void Tick()
    {
        if (IsDead) return;

        if (IsBroken) return;

        if (Time.time < nextBalanceRegenTime) return;
        if (CurrentBalance >= maxBalance) return;

        Apply_Balance(balanceRegenPerSecond * Time.deltaTime);
    }

    // 한 번에 체간을 회복시킬 때.
    public void Init_Balance()
    {
        if (IsDead) return;

        CurrentBalance = maxBalance;
        nextBalanceRegenTime = Time.time;
        OnBalanceChanged?.Invoke(CurrentBalance, maxBalance);
    }

    private void Apply_Hp(float delta)
    {
        if (delta == 0f) return;

        CurrentHp = Mathf.Clamp(CurrentHp + delta, 0f, maxHp);
        OnHpChanged?.Invoke(CurrentHp, maxHp);

        if (CurrentHp > 0f) return;

        // 라이프가 남아 있으면 죽지 않는다.
        if (CurrentLifePoint > 0)
        {
            Consume_Life();
            return;
        }

        OnDead?.Invoke();
    }

    // 라이프를 1 쓰고 체력과 체간을 되돌린다.
    private void Consume_Life()
    {
        CurrentLifePoint--;
        OnLifePointChanged?.Invoke(CurrentLifePoint, maxLifePoint);

        // 회복량이 0이면 되살아난 그 프레임에 다시 0이 되어 라이프가 한꺼번에 날아간다.
        CurrentHp = Mathf.Max(1f, maxHp * reviveHpRatio);
        OnHpChanged?.Invoke(CurrentHp, maxHp);

        Init_Balance();

        OnLifeLost?.Invoke();
    }

    private void Apply_Balance(float delta)
    {
        if (delta == 0f) return;

        bool wasBroken = IsBroken;

        CurrentBalance = Mathf.Clamp(CurrentBalance + delta, 0f, maxBalance);
        OnBalanceChanged?.Invoke(CurrentBalance, maxBalance);

        if (!wasBroken && IsBroken)
        {
            OnGroggy?.Invoke();
        }
    }
}
