using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class MeleeComboAtkData
{
    public DamageInfo damageInfo;
    public float comboWaitTime;
    public int hitboxIndex;
} // 근접 공격 데이터를 저장, 다음 공격 입력 가능 시간 + 히트박스 번호를 저장함

[RequireComponent(typeof(PlayerController))]
public class H_Attack : MonoBehaviour
{
    [Header("Light attacks")]
    [SerializeField] private MeleeComboAtkData[] lightAtkData;
    [SerializeField] private GameObject[] lightAtkHitboxes;

    [Header("Combo timing")]
    [SerializeField] private float comboBufferAfterAttack = 0.5f;
    [SerializeField] private string attackEndedParam = "AttackEnded";

    private PlayerMovement movement;
    private PlayerController controller;
    private Animator animator;
    private int attackEndedHash;
    private bool isAttacking;
    private bool bufferedLightAttack;
    private bool inComboGraceWindow;
    private float comboGraceCloseTime;
    private int currentAttackIndex;

    public MeleeComboAtkData CurrentLightAttackData => GetData(lightAtkData, currentAttackIndex);
    private bool OwnsAttack => isActiveAndEnabled && controller != null &&
        controller.CanAct && controller.FSM.ActionState == PlayerState.Attacking;

    private static MeleeComboAtkData GetData(MeleeComboAtkData[] data, int index) =>
        data != null && index >= 0 && index < data.Length ? data[index] : null;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        controller = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        attackEndedHash = Animator.StringToHash(attackEndedParam);
        ConfigureHitboxes(lightAtkHitboxes);
        CloseHitboxes();
    }

    private void OnEnable()
    {
        if (controller != null) controller.ActionsCancelled += CancelAttack;
    }

    private void OnDisable()
    {
        if (controller != null) controller.ActionsCancelled -= CancelAttack;
        CancelAttack();
    }

    private void Update()
    {
        if (!inComboGraceWindow) return;
        if (!OwnsAttack) { CancelAttack(); return; }
        if (Time.time > comboGraceCloseTime) { FinishSequence(); return; }
        if (bufferedLightAttack)
            TryStartLightAttack(currentAttackIndex + 1, true);
    }

    public void OnLightAttack(InputValue value)
    {
        if (!isActiveAndEnabled || !value.isPressed) return;
        if (controller == null || !controller.CanAct) return;

        if (isAttacking || inComboGraceWindow)
        {
            if (OwnsAttack) bufferedLightAttack = true;
            return;
        }

        TryStartLightAttack(0, false);
    }

    private bool TryBeginAttack(MeleeComboAtkData data)
    {
        if (!isActiveAndEnabled || isAttacking || data == null) return false;
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return false;
        if (lightAtkHitboxes == null || data.hitboxIndex < 0 || data.hitboxIndex >= lightAtkHitboxes.Length ||
            lightAtkHitboxes[data.hitboxIndex] == null || lightAtkHitboxes[data.hitboxIndex] == gameObject ||
            lightAtkHitboxes[data.hitboxIndex].GetComponentInChildren<Collider2D>(true) == null)
        {
            Debug.LogWarning("H attack requires a valid hitbox in its Inspector array.", this);
            return false;
        }
        if (controller == null || !controller.TryStartAction(PlayerState.Attacking)) return false;
        CloseHitboxes();
        isAttacking = true;
        bufferedLightAttack = false;
        inComboGraceWindow = false;
        animator.SetBool(attackEndedHash, false);
        return true;
    }

    private void TryStartLightAttack(int index, bool combo)
    {
        if (combo && (!OwnsAttack || !inComboGraceWindow || Time.time > comboGraceCloseTime)) return;
        if (!TryBeginAttack(GetData(lightAtkData, index))) return;
        currentAttackIndex = index;
        animator.SetInteger("ComboIndex", index);
        animator.SetTrigger(combo ? "Combo" : "LightAttack");
    }

    public void EnableLightHitbox()
    {
        if (!OwnsAttack || !isAttacking) return;
        OpenHitbox(lightAtkHitboxes, CurrentLightAttackData);
    }
    public void DisableLightHitbox()
    {
        CloseHitboxes();
    }
    public void AttackMove()
    {
        if (OwnsAttack && isAttacking)
            movement.MoveBy(Vector2.right * movement.FacingDirection * 0.5f);
    }
    public void AttackBackMove()
    {
        if (OwnsAttack && isAttacking)
            movement.MoveBy(Vector2.left * movement.FacingDirection * 0.5f);
    }

    public void EndLightAttack()
    {
        if (!OwnsAttack || !isAttacking) return;
        CloseHitboxes();
        isAttacking = false;
        if (currentAttackIndex >= lightAtkData.Length - 1) { FinishSequence(); return; }
        inComboGraceWindow = true;
        comboGraceCloseTime = Time.time + Mathf.Max(0f, comboBufferAfterAttack);
    }
    private void FinishSequence()
    {
        CloseHitboxes();
        isAttacking = false;
        bufferedLightAttack = false;
        inComboGraceWindow = false;
        if (animator != null) animator.SetBool(attackEndedHash, true);
        controller?.EndAction(PlayerState.Attacking);
    }
    public void CancelAttack()
    {
        if (animator != null)
        {
            animator.ResetTrigger("LightAttack");
            animator.ResetTrigger("Combo");
        }
        FinishSequence();
    }

    private void ConfigureHitboxes(GameObject[] hitboxes)
    {
        if (hitboxes == null) return;
        foreach (var hitbox in hitboxes)
        {
            if (hitbox == null || hitbox == gameObject) continue;
            foreach (var collider in hitbox.GetComponentsInChildren<Collider2D>(true))
            {
                var relay = collider.GetComponent<H_MeleeHitbox>();
                if (relay == null) relay = collider.gameObject.AddComponent<H_MeleeHitbox>();
                relay.Initialize(transform);
            }
        }
    }
    private void OpenHitbox(GameObject[] hitboxes, MeleeComboAtkData data)
    {
        CloseHitboxes();
        if (hitboxes == null || data == null || data.hitboxIndex < 0 || data.hitboxIndex >= hitboxes.Length) return;

        GameObject hitbox = hitboxes[data.hitboxIndex];
        if (hitbox == null || hitbox == gameObject) return;

        DamageInfo damageInfo = data.damageInfo;
        damageInfo.damage = Mathf.Max(0f, damageInfo.damage);
        damageInfo.postureDamage = Mathf.Max(0f, damageInfo.postureDamage);
        damageInfo.driveDamage = Mathf.Max(0f, damageInfo.driveDamage);
        damageInfo.knockbackPower = Mathf.Max(0f, damageInfo.knockbackPower);
        damageInfo.stunTime = Mathf.Max(0f, damageInfo.stunTime);
        damageInfo.damageType = DamageType.LightAttack;
        damageInfo.damageDir = Vector2.zero;

        hitbox.SetActive(true);
        foreach (var meleeHitbox in hitbox.GetComponentsInChildren<H_MeleeHitbox>(true))
            meleeHitbox.BeginAttack(damageInfo);
    }
    private void CloseHitboxes()
    {
        DisableHitboxes(lightAtkHitboxes);
    }
    private void DisableHitboxes(GameObject[] hitboxes)
    {
        if (hitboxes == null) return;
        foreach (var hitbox in hitboxes)
        {
            if (hitbox == null || hitbox == gameObject) continue;
            foreach (var meleeHitbox in hitbox.GetComponentsInChildren<H_MeleeHitbox>(true))
                meleeHitbox.EndAttack();
            hitbox.SetActive(false);
        }
    }
}
