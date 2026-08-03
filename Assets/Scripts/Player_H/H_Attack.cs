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

    [Header("공격 종료 후 콤보 입력 유예시간")]
    [Tooltip("공격 모션이 끝난 뒤에도 이 시간(초) 동안은 콤보 입력을 받아준다.")]
    [SerializeField] private float comboBufferAfterAttack = 0.5f;

    [Header("Animator 파라미터 이름")]
    [Tooltip("Animator에 만들어야 하는 Bool 파라미터 이름. 이 값이 true가 되어야 각 공격 State -> Exit 전이가 발동함.")]
    [SerializeField] private string attackEndedParam = "AttackEnded";

    private int attackEndedParamHash;

    private float lightAtkNextTime;
    private float heavyAtkNextTime;

    private int currentComboIndex;
    private int currentAttackIndex;
    private int currentHeavyAttackIndex;

    [Header("공격 상태")]
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool bufferdLightAtk;

    private bool inComboGraceWindow;   // 공격 애니메이션 종료 후 유예 구간인지
    private float comboGraceCloseTime; // 유예 구간이 끝나는 시각

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

        attackEndedParamHash = Animator.StringToHash(attackEndedParam);

        DisableAllLightHitboxes();
        DisableAllHeavyHitboxes();
    }

    private void Update()
    {
        if (isInputKey) triggerTimer += Time.deltaTime;

        // 공격중도 아니고 유예구간도 아닌데 콤보 만료시간이 지났으면 콤보 인덱스 리셋
        if (!isAttacking && !inComboGraceWindow && currentComboIndex != 0 && Time.time > lightAtkNextTime)
        {
            currentComboIndex = 0;
        }

        // 유예 구간(공격 끝 + comboBufferAfterAttack) 종료 체크
        if (inComboGraceWindow && Time.time > comboGraceCloseTime)
        {
            inComboGraceWindow = false;
            bufferdLightAtk = false;
            currentComboIndex = 0;

            // 유예시간 동안 콤보 입력이 안 들어왔으니, 이제 Animator한테
            // "공격 완전히 끝났다" 신호를 줘서 Exit(Idle 등)로 넘어가게 한다.
            if (animator != null) animator.SetBool(attackEndedParamHash, true);
        }
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

            // 공격 재생 중이거나, 공격 끝난 직후 유예구간이면 입력을 버퍼링
            if (isAttacking || inComboGraceWindow)
            {
                bufferdLightAtk = true;
                TryExecuteBufferedLightAttack();
                return;
            }
            else
            {
                LightAttack();
            }
        }
        else
        {
            isInputKey = false;

            if (triggerTimer > FrameToSeconds(triggerTime))
            {
                HeavyAttack();
            }
        }
    }

    private void LightAttack()
    {
        if (lightAtkData == null || lightAtkData.Length == 0)
        {
            return;
        }

        if (!isAttacking && !inComboGraceWindow && Time.time > lightAtkNextTime) currentComboIndex = 0;
        if (currentComboIndex < 0 || currentComboIndex >= lightAtkData.Length) currentComboIndex = 0;

        currentAttackIndex = currentComboIndex;

        isAttacking = true;
        bufferdLightAtk = false;
        inComboGraceWindow = false;

        if (animator != null)
        {
            // 새 공격 시작이므로 Exit 신호는 반드시 false로 초기화
            animator.SetBool(attackEndedParamHash, false);
            animator.SetInteger("ComboIndex", currentAttackIndex);
            animator.SetTrigger("LightAttack");
        }

        Debug.Log("현재 콤보 인덱스 = " + currentAttackIndex);

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
        bufferdLightAtk = false;
        inComboGraceWindow = false;

        currentHeavyAttackIndex = 0;

        if (animator != null)
        {
            animator.SetBool(attackEndedParamHash, false);
            animator.SetTrigger("HeavyAttack");
        }

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

    // 버퍼링된 입력이 있으면 콤보 실행을 시도한다.
    // 아직 현재 공격 애니메이션이 재생중(isAttacking == true)이면 여기서는 실행하지 않고
    // EndLightAttack()에서 다시 호출된다.
    private void TryExecuteBufferedLightAttack()
    {
        if (!bufferdLightAtk) return;
        if (isAttacking) return;

        // 마지막 콤보(5타)까지 나간 상태라면 더 이상 이어가지 않고 무조건 Exit로 보낸다.
        // (currentComboIndex는 다음에 나갈 콤보 인덱스를 가리키므로,
        //  currentAttackIndex가 배열의 마지막 인덱스라는 건 방금 마지막 타를 쳤다는 뜻)
        if (currentAttackIndex >= lightAtkData.Length - 1)
        {
            bufferdLightAtk = false;
            return;
        }

        ExecuteBufferedLightAttack();
    }

    private void ExecuteBufferedLightAttack()
    {
        if (!bufferdLightAtk) return;

        bufferdLightAtk = false;
        inComboGraceWindow = false;

        DisableAllLightHitboxes();

        // 다음에 재생할 콤보 인덱스로 갱신 (예: 0 -> 1, ATK1 -> ATK2)
        currentComboIndex++;
        if (currentComboIndex >= lightAtkData.Length) currentComboIndex = 0;

        currentAttackIndex = currentComboIndex;

        isAttacking = true;

        Debug.Log("Combo Trigger, ComboIndex = " + currentAttackIndex);

        if (animator != null)
        {
            // 다음 콤보로 이어지므로 Exit 신호는 다시 false로
            animator.SetBool(attackEndedParamHash, false);
            // Animator Condition(ComboIndex Equals N)이 참조하는 값이므로
            // 반드시 "다음에 재생될" 인덱스를 넣어야 한다.
            animator.SetInteger("ComboIndex", currentAttackIndex);
            animator.SetTrigger("Combo");
        }

        lightAtkNextTime = Time.time + lightAtkComboExpireTime;
    }

    public void AttackMove()
    {
        float distance = 0.5f;

        rb.position += Vector2.right * movement.FacingDirection * distance;
    }

    public void AttackBackMove()
    {
        float distance = 0.5f;

        rb.position += Vector2.left * movement.FacingDirection * distance;
    }

    // AnimationEvent로 호출됨 (공격 모션에서 타격 판정이 끝나는 시점 등에 걸어두면 됨)
    public void EndLightAttack()
    {
        DisableAllLightHitboxes();

        isAttacking = false;

        // 마지막 콤보(5타)까지 쳤다면 더 이상 이어갈 콤보가 없으므로
        // 유예시간 없이 바로 Exit 신호를 준다. 다음 입력은 처음(1타)부터 다시 시작.
        if (currentAttackIndex >= lightAtkData.Length - 1)
        {
            bufferdLightAtk = false;
            inComboGraceWindow = false;
            currentComboIndex = 0;

            if (animator != null) animator.SetBool(attackEndedParamHash, true);
            return;
        }

        // 공격이 끝났다고 바로 Exit 신호를 주지 않고, 유예시간을 부여해서
        // 그 사이에 들어오는 콤보 입력을 받아준다.
        inComboGraceWindow = true;
        comboGraceCloseTime = Time.time + comboBufferAfterAttack;

        // 유예시간 시작 시점에 이미 버퍼링된 입력이 있으면 바로 다음 콤보로 진행
        if (bufferdLightAtk)
        {
            TryExecuteBufferedLightAttack();
        }

        // 참고: 여기서 AttackEnded 파라미터를 true로 세팅하지 않는다.
        // Update()에서 유예시간이 만료됐는데도 콤보 입력이 없을 때만 true로 세팅한다.
    }

    public void EndHeavyAttack()
    {
        DisableAllHeavyHitboxes();

        isAttacking = false;
        bufferdLightAtk = false;
        inComboGraceWindow = false;

        // 강공은 콤보 유예 없이 끝나면 바로 Exit 허용
        if (animator != null) animator.SetBool(attackEndedParamHash, true);
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
        bufferdLightAtk = false;
        inComboGraceWindow = false;

        if (animator != null) animator.SetBool(attackEndedParamHash, false);
    }
}