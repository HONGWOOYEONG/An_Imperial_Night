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
    [Tooltip("쿨다운을 기록하는 딕셔너리 키. 비어 있거나 겹치면 쿨다운이 엉뚱한 패턴과 공유된다.")]
    [SerializeField] private string id;

    [Tooltip("Attack 파라메터로 진입한 뒤 어떤 패턴 애니메이션을 재생할지 고르는 인덱스.")]
    [SerializeField] private int animIndex;

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

    [Header("이동")]
    [Tooltip("패턴 동안 앞으로 나아가는 거리. 0이면 제자리 패턴이다.")]
    [SerializeField] private float moveDistance;

    [Tooltip("포물선의 높이. 0이면 바닥을 따라 미끄러진다.")]
    [SerializeField, Min(0f)] private float moveHeight;

    [Header("공격력")]
    [SerializeField] private float hpDamage = 10f;
    [SerializeField] private float postureDamage = 10f;
    [SerializeField] private float driveDamage;
    [SerializeField] private float knockbackPower;
    [SerializeField] private float stunTime;
    [SerializeField] private DamageType damageType = DamageType.LightAttack;

    public string Id => id;
    public int AnimIndex => animIndex;

    public float Cooldown => cooldown;
    public float BaseWeight => baseWeight;
    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;
    public float PreferredDistance => preferredDistance;

    public float MoveDistance => moveDistance;
    public float MoveHeight => moveHeight;

    public float HpDamage => hpDamage;
    public float PostureDamage => postureDamage;
    public float DriveDamage => driveDamage;
    public float KnockbackPower => knockbackPower;
    public float StunTime => stunTime;
    public DamageType DamageType => damageType;

    // 재생 시간은 여기 적지 않는다. SSM에서 뽑히는 클립이 패턴마다 다르므로,
    // Boss_AttackState가 실제 재생 길이를 재서 이동 속도를 계산한다.

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
