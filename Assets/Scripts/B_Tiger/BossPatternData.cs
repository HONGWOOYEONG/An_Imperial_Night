using UnityEngine;

[CreateAssetMenu(fileName = "NewPatternData", menuName = "Boss/Tiger/Pattern")]
public class BossPatternData : ScriptableObject
{
    public string patternId;
    //public AnimationClip anim;
    //���� �ִϸ��̼� �̺�Ʈ ����ϱ� ���� �ۼ�
    public float skillCooldown;
    public float baseWeight;
    public float minDistance;
    public float maxDistance;
    public float preferredDistance;
    public string target;
    public float HPDamage;
    public float postureDamage;
    public string damageType;
    public float driveDamage;
    public float knockbackPower;
    public float stunTime;
}
