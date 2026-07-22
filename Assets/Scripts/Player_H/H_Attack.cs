using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class MeleeComboAtkData
{
    public float damage;
    public float postureDamage;
    public float knockbackPower;
    public float stunTime;
    public float comboWaitTime;
    public int hitboxIndex;
}

public class H_Attack : MonoBehaviour
{
    private const float BASE_FPS = 60f;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private Animator animator;

    [Header("약공 데이터")]
    [SerializeField] private MeleeComboAtkData[] lightAtkData;

    [Header("강공 데이터")]
    [SerializeField] private MeleeComboAtkData[] heavyAtkData;

    [Header("약공 히트박스")]
    [SerializeField] private GameObject[] lightAtkHitboxes;

    [Header("강공 히트박스")]
    [SerializeField] private GameObject[] heavyAtkHitboxes;

    [Header("약공 or 강공")]
    [SerializeField] private float triggerTime = 65f;
    [SerializeField] private float triggerTimer = 0f;
    private bool isInputKey = false;

    [Header("약공 콤보")]
    [SerializeField] private float lightAtkComboExpireTime = 2f;

    private float lightAtkNextTime;
    private float heavyAtkNextTime;

    private int currentComboIndex;
    private int currentAttackIndex;
    private int currentHeavyAttackIndex;

    [Header("공격 상태")]
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool canCombo;
    [SerializeField] private bool bufferdLightAtk;

    public MeleeComboAtkData CurrentLightAttackData
    {
        get
        {
            if (lightAtkData == null) return null;
            if (currentAttackIndex < 0 || currentAttackIndex >= lightAtkData.Length) return null;

            return lightAtkData[currentAttackIndex];
        }
    }

    public MeleeComboAtkData CurrentHeavyAttackData
    {
        get
        {
            if (heavyAtkData == null) return null;
            if (currentHeavyAttackIndex < 0 || currentHeavyAttackIndex >= heavyAtkData.Length) return null;

            return heavyAtkData[currentHeavyAttackIndex];
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        if (animator == null) animator = GetComponentInChildren<Animator>();

        DisableAllLightHitboxes();
        DisableAllHeavyHitboxes();
    }

    private void Update()
    {
        if (isInputKey) triggerTimer += Time.deltaTime;
    }

    private float FrameToSeconds(float frame)
    {
        return frame / BASE_FPS;
    }

    public void OnLightAttack(InputValue value)
    {
        if (value.isPressed)
        {
            triggerTimer = 0f;
            isInputKey = true;
        }
        else
        {
            isInputKey = false;

            Debug.Log("현재 triggerTimer = " + triggerTimer);

            if (triggerTimer <= FrameToSeconds(triggerTime))
            {
                if (isAttacking)
                {
                    bufferdLightAtk = true;

                    if (canCombo) ExecuteBufferedLightAttack();

                    return;
                }

                LightAttack();
            }
            else
            {
                if (isAttacking) return;

                HeavyAttack();
            }
        }
    }

    private void LightAttack()
    {
        if (lightAtkData == null || lightAtkData.Length == 0)
        {
            Debug.LogWarning("약공 데이터가 설정되지 않았습니다.");
            return;
        }

        if (!isAttacking && Time.time > lightAtkNextTime) currentComboIndex = 0;
        if (currentComboIndex < 0 || currentComboIndex >= lightAtkData.Length) currentComboIndex = 0;

        currentAttackIndex = currentComboIndex;

        isAttacking = true;
        canCombo = false;
        bufferdLightAtk = false;

        if (animator != null)
        {
            animator.SetInteger("ComboIndex", currentAttackIndex);
            animator.SetTrigger("LightAttack");
        }

        Debug.Log("현재 콤보 인덱스 = " + currentAttackIndex);

        currentComboIndex = (currentComboIndex + 1) % lightAtkData.Length;
        lightAtkNextTime = Time.time + lightAtkComboExpireTime;
    }

    public void HeavyAttack()
    {
        if (isAttacking) return;

        if (heavyAtkData == null || heavyAtkData.Length == 0)
        {
            Debug.LogWarning("강공 데이터가 설정되지 않았습니다.");
            return;
        }

        isAttacking = true;
        canCombo = false;
        bufferdLightAtk = false;

        currentHeavyAttackIndex = 0;

        if (animator != null) animator.SetTrigger("HeavyAttack");

        Debug.Log("강공 실행");
    }

    public void EnableLightHitbox()
    {
        if (lightAtkData == null) return;
        if (currentAttackIndex < 0 || currentAttackIndex >= lightAtkData.Length) return;

        DisableAllLightHitboxes();

        int hitboxIndex = lightAtkData[currentAttackIndex].hitboxIndex;

        if (lightAtkHitboxes == null) return;

        if (hitboxIndex < 0 || hitboxIndex >= lightAtkHitboxes.Length)
        {
            Debug.LogWarning("약공 히트박스 인덱스가 배열 범위를 벗어났습니다: " + hitboxIndex);
            return;
        }

        if (lightAtkHitboxes[hitboxIndex] != null) lightAtkHitboxes[hitboxIndex].SetActive(true);
    }

    public void DisableLightHitbox()
    {
        if (lightAtkData == null) return;
        if (currentAttackIndex < 0 || currentAttackIndex >= lightAtkData.Length) return;

        int hitboxIndex = lightAtkData[currentAttackIndex].hitboxIndex;

        if (lightAtkHitboxes == null) return;
        if (hitboxIndex < 0 || hitboxIndex >= lightAtkHitboxes.Length) return;

        if (lightAtkHitboxes[hitboxIndex] != null) lightAtkHitboxes[hitboxIndex].SetActive(false);
    }

    public void EnableHeavyHitbox()
    {
        if (heavyAtkData == null) return;
        if (currentHeavyAttackIndex < 0 || currentHeavyAttackIndex >= heavyAtkData.Length) return;

        DisableAllHeavyHitboxes();

        int hitboxIndex = heavyAtkData[currentHeavyAttackIndex].hitboxIndex;

        if (heavyAtkHitboxes == null) return;

        if (hitboxIndex < 0 || hitboxIndex >= heavyAtkHitboxes.Length)
        {
            Debug.LogWarning("강공 히트박스 인덱스가 배열 범위를 벗어났습니다: " + hitboxIndex);
            return;
        }

        if (heavyAtkHitboxes[hitboxIndex] != null) heavyAtkHitboxes[hitboxIndex].SetActive(true);
    }

    public void DisableHeavyHitbox()
    {
        if (heavyAtkData == null) return;
        if (currentHeavyAttackIndex < 0 || currentHeavyAttackIndex >= heavyAtkData.Length) return;

        int hitboxIndex = heavyAtkData[currentHeavyAttackIndex].hitboxIndex;

        if (heavyAtkHitboxes == null) return;
        if (hitboxIndex < 0 || hitboxIndex >= heavyAtkHitboxes.Length) return;

        if (heavyAtkHitboxes[hitboxIndex] != null) heavyAtkHitboxes[hitboxIndex].SetActive(false);
    }

    public void OpenComboWindow()
    {
        canCombo = true;

        if (bufferdLightAtk) ExecuteBufferedLightAttack();
    }

    public void CloseComboWindow()
    {
        canCombo = false;
    }

    private void ExecuteBufferedLightAttack()
    {
        if (!bufferdLightAtk) return;
        if (!canCombo) return;

        bufferdLightAtk = false;
        canCombo = false;

        DisableAllLightHitboxes();

        LightAttack();
    }

    public void EndLightAttack()
    {
        DisableAllLightHitboxes();

        isAttacking = false;
        canCombo = false;
        bufferdLightAtk = false;
    }

    public void EndHeavyAttack()
    {
        DisableAllHeavyHitboxes();

        isAttacking = false;
        canCombo = false;
        bufferdLightAtk = false;
    }

    private void DisableAllLightHitboxes()
    {
        if (lightAtkHitboxes == null) return;

        foreach (GameObject hitbox in lightAtkHitboxes)
        {
            if (hitbox != null) hitbox.SetActive(false);
        }
    }

    private void DisableAllHeavyHitboxes()
    {
        if (heavyAtkHitboxes == null) return;

        foreach (GameObject hitbox in heavyAtkHitboxes)
        {
            if (hitbox != null) hitbox.SetActive(false);
        }
    }

    private void OnDisable()
    {
        DisableAllLightHitboxes();
        DisableAllHeavyHitboxes();

        isInputKey = false;
        isAttacking = false;
        canCombo = false;
        bufferdLightAtk = false;
    }
}