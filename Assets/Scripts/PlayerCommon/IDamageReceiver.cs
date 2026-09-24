using System;
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
    public float postureDamage; // ���� ĳ���� ��� �� ü�� ������
    public float driveDamage;   // ���Ÿ� ĳ���� ��� �� ����̺� ���ҷ�
}

public interface IDamageReceiver
{
    void ReceiveAttack(DamageInfo damageInfo);
}