/// 작성자 : 유희일
using UnityEngine;

/// <summary>
/// 패턴 하나의 수치를 담는 데이터 에셋. 절차는 갖지 않는다.
/// 패턴을 고르는 것은 Boss_AI, 실행하는 것은 Boss_AttackState다.
/// </summary>
[CreateAssetMenu(fileName = "Boss_Pattern", menuName = "Boss/Boss_1/Pattern")]


public class Boss_PatternSO : ScriptableObject
{
    [Header("식별")]
    [Tooltip("이 패턴의 유일한 번호. 쿨다운 키, Boss_AI의 상태 연결 switch, 애니메이터 patternId 조건이 " +
             "전부 이 값 하나만 본다. 다른 패턴과 겹치면 쿨다운이 함께 잠기고 엉뚱한 상태가 실행된다.")]
    [SerializeField, Min(0)] private int id = 1;

    [Header("선택 조건")]
    [Tooltip("이 패턴을 쓴 뒤 다시 쓸 수 있을 때까지의 시간.")]
    [SerializeField, Min(0f)] private float cooldown = 3f;

    [Tooltip("선택 확률의 기본 가중치. 0이면 절대 뽑히지 않는다.")]
    [SerializeField, Min(0f)] private float baseWeight = 1f;

    [Tooltip("이 거리 밖이면 후보에서 제외한다.")]
    [SerializeField, Min(0f)] private float minDistance;
    [SerializeField, Min(0f)] private float maxDistance = 5f;

    [Tooltip("가장 높은 점수를 받는 거리. min~max 사이에 두지 않으면 이 패턴의 점수가 항상 낮게 나온다.")]
    [SerializeField, Min(0f)] private float preferredDistance = 2f;

    [Header("공격력")]
    [SerializeField] private float hpDamage = 10f;
    [SerializeField] private float postureDamage = 10f;
    [SerializeField] private float driveDamage;
    [SerializeField] private float knockbackPower;
    [SerializeField] private float stunTime;
    [SerializeField] private DamageType damageType = DamageType.LightAttack;

    public int Id => id;

    public float Cooldown => cooldown;
    public float BaseWeight => baseWeight;
    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;
    public float PreferredDistance => preferredDistance;

    public float HpDamage => hpDamage;
    public float PostureDamage => postureDamage;
    public float DriveDamage => driveDamage;
    public float KnockbackPower => knockbackPower;
    public float StunTime => stunTime;
    public DamageType DamageType => damageType;

    // 재생 시간은 여기 적지 않는다. SSM에서 뽑히는 클립이 패턴마다 다르므로,
    // Boss_AttackState가 실제 재생 길이를 재서 상태를 끝낸다.

    /// <summary>
    /// 이 패턴이 플레이어에게 넘길 피해 정보. 방향은 보스가 바라보는 쪽이므로 런타임에 받는다.
    /// </summary>
    public DamageInfo BuildDamageInfo(Vector2 direction)
    {
        return new DamageInfo
        {
            damage = hpDamage,
            damageDir = direction,
            knockbackPower = knockbackPower,
            stunTime = stunTime,
            damageType = damageType,
            postureDamage = postureDamage,
            driveDamage = driveDamage
        };
    }
}
