using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ComboAtkData
{
    public int startUpTime; //선딜레이
    public int activeTime; // 약공 판정 시간
    public int recoveryTime; // 후딜레이
    public int comboWaitTime; // 후딜 이후 다음 입력 대기 시간
}

public class PlayerHAtck : MonoBehaviour
{
    private PlayerHDef playerHDef;
    public const float BASE_FPS = 30;

    [Header("Attack")]
    [SerializeField] private float lightAtkDamage = 100.0f;
    [SerializeField] private float heavyAtkDamage = 150.0f; //fsm에 데미지 주는 함수 추가할때 사용
    [SerializeField] private ComboAtkData[] lightCombo;
    [SerializeField] private ComboAtkData heavyCombo; // 추후 강공도 모션 추가될 수 있으니 우선 콤보로 진행
    [SerializeField] private GameObject attackHitbox;

    private bool isAttacking = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHDef = GetComponent<PlayerHDef>();
    }

    private float FrameToSeconds(int frame)
    {
        return frame / BASE_FPS;
    }

    public void OnLightAttack(InputValue value)
    {
        if (isAttacking) return;
        if (IsDefending)
        {
            StartCoroutine(StartHeavyAttack());
            return;
        }
        StartCoroutine(StartLightAttack());
    }
    
    IEnumerator StartLightAttack()
    {
        isAttacking = true;
        ComboAtkData attack = lightCombo[0];
        yield return new WaitForSeconds(FrameToSeconds(attack.startUpTime));

        attackHitbox.SetActive(true);
        Debug.Log("약공");
        yield return new WaitForSeconds(FrameToSeconds(attack.activeTime));
        attackHitbox.SetActive(false);

        yield return new WaitForSeconds(FrameToSeconds(attack.recoveryTime));
        isAttacking = false;
    }

    IEnumerator StartHeavyAttack()
    {
        isAttacking = true;
        ComboAtkData attack = heavyCombo;
        yield return new WaitForSeconds(FrameToSeconds(attack.startUpTime));

        attackHitbox.SetActive(true);
        Debug.Log("강공");
        yield return new WaitForSeconds(FrameToSeconds(attack.activeTime));
        attackHitbox.SetActive(false);

        yield return new WaitForSeconds(FrameToSeconds(attack.recoveryTime));
        isAttacking = false;
    }
}
