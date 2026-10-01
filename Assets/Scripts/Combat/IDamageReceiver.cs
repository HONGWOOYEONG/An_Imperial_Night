using System;
using UnityEngine;

public enum DamageType
{
    LightAttack = 0,
    HeavyAttack = 1,
    SpecialAttack = 2,
    //플레이어가 공격을 할때, 사용하는 타입
    BossNormalAttack = 3,
    UnblockableAttack = 4,
    Bind = 5,
    Mark = 6
    //보스가 공격을 할때, 사용하는 타입  
}
[Serializable]
public struct DamageInfo
{
    public float damage;
    public Vector2 damageDir; 
    public float knockbackPower;
    public float stunTime;
    public DamageType damageType;
    public float postureDamage;
    public float driveDamage;
}

public interface IDamageReceiver
{
    void ReceiveAttack(DamageInfo damageInfo);
}
