using System.Collections.Generic;
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

[RequireComponent(typeof(PlayerController))]
public class H_Attack : MonoBehaviour
{
    private const float BASE_FPS = 60f;
    [Header("Light attacks")]
    [SerializeField] private MeleeComboAtkData[] lightAtkData;
    [SerializeField] private GameObject[] lightAtkHitboxes;
    [Header("Heavy attacks")]
    [SerializeField] private MeleeComboAtkData[] heavyAtkData;
    [SerializeField] private GameObject[] heavyAtkHitboxes;
    [Header("Input and combo timing")]
    [SerializeField] private float triggerTime = 65f;
    [SerializeField] private float comboBufferAfterAttack = 0.5f;
    [SerializeField] private string attackEndedParam = "AttackEnded";

    private PlayerMovement movement;
    private PlayerController controller;
    private Animator animator;
    private int attackEndedHash;
    private bool isInputKey;
    private float triggerTimer;
    private bool isAttacking;
    private bool bufferedLightAttack;
    private bool inComboGraceWindow;
    private float comboGraceCloseTime;
    private int currentAttackIndex;
    private int currentHeavyAttackIndex;
    private DamageType attackType;
    private DamageInfo attackDamage;
    private GameObject activeHitbox;
    private readonly HashSet<Object> hitTargets = new HashSet<Object>();
    private readonly HashSet<int> retiredAnimationStates = new HashSet<int>();
    private bool waitingForAnimationExit;
    private int attackAnimationState;
    private int attackEndFrame = -1;

    public MeleeComboAtkData CurrentLightAttackData => GetData(lightAtkData, currentAttackIndex);
    public MeleeComboAtkData CurrentHeavyAttackData => GetData(heavyAtkData, currentHeavyAttackIndex);
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
        ConfigureHitboxes(heavyAtkHitboxes);
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
        // Do not reuse a cancelled/finished animation while its outgoing events can fire.
        if (waitingForAnimationExit && !HasAttackAnimation())
        {
            waitingForAnimationExit = false;
            retiredAnimationStates.Clear();
        }
        if (isInputKey) triggerTimer += Time.deltaTime;
        if (!inComboGraceWindow) return;
        if (!OwnsAttack) { CancelAttack(); return; }
        if (Time.time > comboGraceCloseTime) { FinishSequence(); return; }
        // Defer chaining until all events from the previous frame have been processed.
        if (bufferedLightAttack && Time.frameCount > attackEndFrame)
            TryStartLightAttack(currentAttackIndex + 1, true);
    }

    public void OnLightAttack(InputValue value)
    {
        if (!isActiveAndEnabled) return;
        if (value.isPressed)
        {
            if (controller == null || !controller.CanAct || waitingForAnimationExit) return;
            isInputKey = true;
            triggerTimer = 0f;
            if (isAttacking || inComboGraceWindow)
            {
                if (attackType == DamageType.LightAttack && OwnsAttack) bufferedLightAttack = true;
                return;
            }
            TryStartLightAttack(0, false);
        }
        else
        {
            if (!isInputKey) return;
            isInputKey = false;
            if (triggerTimer > triggerTime / BASE_FPS) HeavyAttack();
        }
    }

    private bool TryBeginAttack(DamageType type, MeleeComboAtkData data, GameObject[] hitboxes)
    {
        if (!isActiveAndEnabled || isAttacking || waitingForAnimationExit || data == null) return false;
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return false;
        if (hitboxes == null || data.hitboxIndex < 0 || data.hitboxIndex >= hitboxes.Length ||
            hitboxes[data.hitboxIndex] == null || hitboxes[data.hitboxIndex] == gameObject ||
            hitboxes[data.hitboxIndex].GetComponentInChildren<Collider2D>(true) == null)
        {
            Debug.LogWarning("H attack requires a valid hitbox in its Inspector array.", this);
            return false;
        }
        if (controller == null || !controller.TryStartAction(PlayerState.Attacking)) return false;
        CloseHitboxes();
        hitTargets.Clear();
        attackAnimationState = 0;
        attackType = type;
        attackDamage = new DamageInfo
        {
            damage = Mathf.Max(0f, data.damage),
            postureDamage = Mathf.Max(0f, data.postureDamage),
            knockbackPower = Mathf.Max(0f, data.knockbackPower),
            stunTime = Mathf.Max(0f, data.stunTime),
            damageType = type
        };
        isAttacking = true;
        bufferedLightAttack = false;
        inComboGraceWindow = false;
        animator.SetBool(attackEndedHash, false);
        return true;
    }

    private void TryStartLightAttack(int index, bool combo)
    {
        if (combo && (!OwnsAttack || !inComboGraceWindow || Time.time > comboGraceCloseTime)) return;
        if (!TryBeginAttack(DamageType.LightAttack, GetData(lightAtkData, index), lightAtkHitboxes)) return;
        currentAttackIndex = index;
        animator.SetInteger("ComboIndex", index);
        animator.SetTrigger(combo ? "Combo" : "LightAttack");
    }

    public void HeavyAttack()
    {
        if (!TryBeginAttack(DamageType.HeavyAttack, GetData(heavyAtkData, 0), heavyAtkHitboxes)) return;
        currentHeavyAttackIndex = 0;
        animator.SetTrigger("HeavyAttack");
    }

    // AnimationEvent parameters retain existing clip function names without asset edits.
    private bool AcceptEvent(AnimationEvent evt, DamageType? type = null)
    {
        if (!OwnsAttack || !isAttacking || (type.HasValue && attackType != type.Value)) return false;
        if (evt != null && evt.isFiredByAnimator)
        {
            string endEvent = attackType == DamageType.LightAttack ? nameof(EndLightAttack) : nameof(EndHeavyAttack);
            bool matchesAttack = false;
            var clip = evt.animatorClipInfo.clip;
            if (clip == null) return false;
            foreach (var clipEvent in clip.events)
                if (clipEvent.functionName == endEvent) { matchesAttack = true; break; }
            if (!matchesAttack) return false;
            int state = evt.animatorStateInfo.fullPathHash;
            if (retiredAnimationStates.Contains(state)) return false;
            if (attackAnimationState != 0 && attackAnimationState != state) return false;
            attackAnimationState = state;
        }
        return true;
    }

    public void EnableLightHitbox(AnimationEvent evt = null)
    {
        if (!AcceptEvent(evt, DamageType.LightAttack)) return;
        OpenHitbox(lightAtkHitboxes, CurrentLightAttackData);
    }
    public void EnableHeavyHitbox(AnimationEvent evt = null)
    {
        if (!AcceptEvent(evt, DamageType.HeavyAttack)) return;
        OpenHitbox(heavyAtkHitboxes, CurrentHeavyAttackData);
    }
    public void DisableLightHitbox(AnimationEvent evt = null)
    {
        if (AcceptEvent(evt, DamageType.LightAttack)) CloseHitboxes();
    }
    public void DisableHeavyHitbox(AnimationEvent evt = null)
    {
        if (AcceptEvent(evt, DamageType.HeavyAttack)) CloseHitboxes();
    }
    public void AttackMove(AnimationEvent evt = null)
    {
        if (AcceptEvent(evt)) movement.MoveBy(Vector2.right * movement.FacingDirection * 0.5f);
    }
    public void AttackBackMove(AnimationEvent evt = null)
    {
        if (AcceptEvent(evt)) movement.MoveBy(Vector2.left * movement.FacingDirection * 0.5f);
    }

    public void EndLightAttack(AnimationEvent evt = null)
    {
        if (!AcceptEvent(evt, DamageType.LightAttack)) return;
        RetireAnimation();
        CloseHitboxes();
        isAttacking = false;
        attackEndFrame = Time.frameCount;
        if (currentAttackIndex >= lightAtkData.Length - 1) { FinishSequence(); return; }
        inComboGraceWindow = true;
        comboGraceCloseTime = Time.time + Mathf.Max(0f, comboBufferAfterAttack);
    }
    public void EndHeavyAttack(AnimationEvent evt = null)
    {
        if (!AcceptEvent(evt, DamageType.HeavyAttack)) return;
        RetireAnimation();
        FinishSequence();
    }

    private void RetireAnimation()
    {
        if (attackAnimationState != 0) retiredAnimationStates.Add(attackAnimationState);
    }
    private void FinishSequence()
    {
        CloseHitboxes();
        isAttacking = false;
        bufferedLightAttack = false;
        inComboGraceWindow = false;
        waitingForAnimationExit = true;
        if (animator != null) animator.SetBool(attackEndedHash, true);
        controller?.EndAction(PlayerState.Attacking);
    }
    public void CancelAttack()
    {
        RetireAnimation();
        isInputKey = false;
        triggerTimer = 0f;
        hitTargets.Clear();
        if (animator != null)
        {
            animator.ResetTrigger("LightAttack");
            animator.ResetTrigger("HeavyAttack");
            animator.ResetTrigger("Combo");
        }
        FinishSequence();
    }

    private bool HasAttackAnimation()
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return false;
        for (int layer = 0; layer < animator.layerCount; layer++)
        {
            if (HasAttackClip(animator.GetCurrentAnimatorClipInfo(layer))) return true;
            if (animator.IsInTransition(layer) && HasAttackClip(animator.GetNextAnimatorClipInfo(layer))) return true;
        }
        return false;
    }
    private static bool HasAttackClip(AnimatorClipInfo[] clips)
    {
        foreach (var clip in clips)
            foreach (var evt in clip.clip.events)
                if (evt.functionName == nameof(EndLightAttack) || evt.functionName == nameof(EndHeavyAttack)) return true;
        return false;
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
                relay.Initialize(this, hitbox);
            }
        }
    }
    private void OpenHitbox(GameObject[] hitboxes, MeleeComboAtkData data)
    {
        CloseHitboxes();
        activeHitbox = hitboxes[data.hitboxIndex];
        if (activeHitbox != null) activeHitbox.SetActive(true);
    }
    private void CloseHitboxes()
    {
        activeHitbox = null;
        DisableHitboxes(lightAtkHitboxes);
        DisableHitboxes(heavyAtkHitboxes);
    }
    private void DisableHitboxes(GameObject[] hitboxes)
    {
        if (hitboxes == null) return;
        foreach (var hitbox in hitboxes)
            if (hitbox != null && hitbox != gameObject) hitbox.SetActive(false);
    }
    internal void ReceiveHit(GameObject source, Collider2D other)
    {
        if (!OwnsAttack || !isAttacking || source == null || source != activeHitbox || !source.activeInHierarchy) return;
        // Co-op players and this player's own colliders are not attack targets.
        if (other.GetComponentInParent<PlayerHealth>() != null || other.transform.IsChildOf(transform)) return;
        var receiver = other.GetComponentInParent<IDamageReceiver>();
        if (!(receiver is Component component)) return;
        Object identity = other.attachedRigidbody != null ? (Object)other.attachedRigidbody : component;
        if (!hitTargets.Add(identity)) return;
        var damage = attackDamage;
        damage.damageDir = ((Vector2)other.bounds.center - (Vector2)transform.position).normalized;
        if (damage.damageDir == Vector2.zero) damage.damageDir = Vector2.right * movement.FacingDirection;
        receiver.ReceiveAttack(damage);
    }
}
