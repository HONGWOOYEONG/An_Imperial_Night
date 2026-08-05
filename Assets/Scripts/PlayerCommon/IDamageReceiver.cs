using UnityEngine;

public enum DamageType
{
    LightAttack,
    HeavyAttack,
    SpecialAttack
}

public struct DamageInfo
{
    public float damage;
    public Vector2 damageDir;
    public float knockbackPower;
    public float stunTime;
    public DamageType damageType;
    public float postureDamage; // 근접 캐릭터 방어 시 체간 증가량
    public float driveDamage;   // 원거리 캐릭터 방어 시 드라이브 감소량
}

public interface IDamageReceiver
{
    void ReceiveAttack(DamageInfo damageInfo);
}