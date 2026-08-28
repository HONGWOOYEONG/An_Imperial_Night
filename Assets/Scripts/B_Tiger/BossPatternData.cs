using UnityEngine;

[CreateAssetMenu(fileName = "NewPatternData", menuName = "Boss/Tiger/Pattern")]
public class BossPatternData : ScriptableObject
{
    public string patternId;
    //public AnimationClip anim;
    //추후 애니메이션 이벤트 사용하기 위해 작성
    public float skillCooldown;
    public float baseWeight;
    public float minDistance;
    public float maxDistance;
    public float preferredDistance;
    public string target;
}
