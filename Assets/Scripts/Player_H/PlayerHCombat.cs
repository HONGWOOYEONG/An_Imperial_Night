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
} // 콤보 공격 저장

public class PlayerHCombat : MonoBehaviour, IDamageReceiver
{
    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    public const float BASE_FPS = 30;

    [Header("MeleeORIGINAL")]
    [SerializeField] private float maxPosture = 1000f;
    [SerializeField] private float currentPosture = 0;
    [SerializeField] private int postureGroggy;

    [Header("Attack")]
    [SerializeField] private float lightAtkDamage = 100.0f;
    [SerializeField] private float heavyAtkDamage = 150.0f; //fsm에 데미지 주는 함수 추가할때 사용
    [SerializeField] private ComboAtkData[] lightCombo;
    [SerializeField] private ComboAtkData heavyCombo; // 추후 강공도 모션 추가될 수 있으니 우선 콤보로 진행
    [SerializeField] private GameObject attackHitbox;

    [Header("Defence")]
    [SerializeField] private int defStartupTime;
    [SerializeField] private int defRecoveryTime;
    [SerializeField] private int parryDurationTime;
    [SerializeField] private float parryOnDrive;

    private bool isDefending = false;
    private bool isParrying = false;
    private bool isAttacking = false;
    private bool isGroggy = false;

    private Coroutine defenceCoroutine;
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (isParrying)
        {
            Debug.Log("패링 성공");
            return;
        }

        if (isDefending)
        {
            Debug.Log("방어 성공");
            currentPosture += damageInfo.postureDamage;
            if(currentPosture >= maxPosture && !isGroggy) StartCoroutine(StartGroggy());
            return;
        }

        playerHealth.DamagedFromAtk(
            damageInfo.damage,
            damageInfo.damageDir,
            damageInfo.knockbackPower,
            damageInfo.stunTime,
            damageInfo.damageType.ToString()
        );
    }

    IEnumerator StartGroggy()
    {
        isGroggy=true;
        playerMovement.ResetControlState();

        playerInput.DeactivateInput(); // 입력 무시
        yield return new WaitForSeconds(FrameToSeconds(postureGroggy));
        
        isGroggy = false;
        currentPosture = 0f;

        if(!playerHealth.IsDead) playerInput.ActivateInput();
    }

    private float FrameToSeconds(int frame)
    {
        return frame / BASE_FPS;
    }

    public void OnDefence(InputValue value)
    {
        if (value.isPressed)
        {
            if(defenceCoroutine != null) StopCoroutine(defenceCoroutine);

            defenceCoroutine = StartCoroutine(StartDefence());
        }
        else
        {
            if(defenceCoroutine != null)
            {
                StopCoroutine(defenceCoroutine);
                defenceCoroutine = null;
            }
            isDefending = false;
            isParrying = false;

            playerMovement.SetDefending(false);
        }
    }

    IEnumerator StartDefence()
    {
        yield return new WaitForSeconds(FrameToSeconds(defStartupTime));

        isDefending = true;
        isParrying = true;

        playerMovement.SetDefending(true);

        yield return new WaitForSeconds(FrameToSeconds(parryDurationTime));

        isParrying = false;
        defenceCoroutine = null;
    }

    public void OnLightAttack(InputValue value)
    {
        if (isAttacking) return;
        if (isDefending)
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
