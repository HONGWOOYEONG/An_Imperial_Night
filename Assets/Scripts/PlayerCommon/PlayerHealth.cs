using System.Collections;
using UnityEngine;

public enum PlayerType
{
    H,
    T
}

public class PlayerHealth : MonoBehaviour, IDamageReceiver
{
    private GameSessionManager gameSessionManager;
    private PlayerController playerController;
    private PlayerMovement playerMovement;
    private H_Posture hPosture;
    private T_Defence tDefence;
    private T_DriveGauge tDriveGauge;
    private H_Abillity hAbility;
    private PlayerContext playerContext;
    private const float BindDuration = 3f;
    private const float MarkDuration = 10f;

    [Header("Health")]
    [SerializeField] private float maxHP = 1000;
    [SerializeField] private PlayerType playerType;
    [SerializeField, Min(0f)] private float reviveTime = 2f;
    private float currentHP;
    private bool isDead = false;
    private Coroutine reviveCoroutine;

    public PlayerType PlayerType => playerType;
    public bool IsDead => isDead;
    public bool IsRevive { get; private set; }
    public float ReviveTime => reviveTime;
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public bool IsBound => playerController != null && playerController.IsBound;
    public bool IsMarked => playerContext != null && playerContext.IsMarked;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerMovement = GetComponent<PlayerMovement>();
        hPosture = GetComponent<H_Posture>();
        tDefence = GetComponent<T_Defence>();
        tDriveGauge = GetComponent<T_DriveGauge>();
        hAbility = GetComponent<H_Abillity>();
        playerContext = GetComponent<PlayerContext>();
        currentHP = maxHP;
    }

    private void Start()
    {
        gameSessionManager = GameSessionManager.Instance;
    }

    // 플레이어의 단일 피격 진입점입니다. 무적과 H/T 방어를 먼저 확인한 뒤 실제 피해를 적용합니다.
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (isDead || IsRevive) return;
        if (hAbility != null && hAbility.IsAbilityInvincible) return;
        if (hPosture != null && hPosture.TryDefend(damageInfo)) return;
        if (tDefence != null && tDefence.TryDefend(damageInfo)) return;

        if (damageInfo.damageType == DamageType.Bind)
            playerController?.BeginBind(BindDuration);
        else if (damageInfo.damageType == DamageType.Mark)
            playerContext?.ApplyMark(MarkDuration);

        // 방어되지 않은 T의 피격만 여기서 차감합니다. 방어 성공은 T_Defence가 처리합니다.
        if (playerType == PlayerType.T && tDriveGauge != null && damageInfo.driveDamage > 0f)
            tDriveGauge.DecreaseDriveGauge(damageInfo.driveDamage);
        hPosture?.ApplyPostureDamage(damageInfo.postureDamage);
        DamagedFromAtk(damageInfo);
    }

    // 방어 판정이 끝난 피해만 내부에서 적용합니다. 외부 피격은 ReceiveAttack으로 받습니다.
    private void DamagedFromAtk(DamageInfo damageInfo)
    {
        if (isDead) return;

        currentHP = Mathf.Max(0f, currentHP - Mathf.Max(0f, damageInfo.damage));
        if (currentHP <= 0f)
        {
            if (gameSessionManager == null)
                gameSessionManager = GameSessionManager.Instance;

            if (gameSessionManager != null) gameSessionManager.flameRevive(this);
            else Death();
            return;
        }

        // HP는 이 컴포넌트가 관리하고, 경직과 넉백은 각 담당 컴포넌트에 요청합니다.
        playerController?.BeginStun(damageInfo.stunTime);
        playerMovement?.ApplyImpulse(damageInfo.damageDir * damageInfo.knockbackPower);
    }

    public void Death()
    {
        if (isDead) return;
        // 남은 자세/그로기를 정리한 뒤 FSM을 사망 상태로 전환합니다.
        if (reviveCoroutine != null)
        {
            StopCoroutine(reviveCoroutine);
            reviveCoroutine = null;
        }
        IsRevive = false;
        isDead = true;
        playerContext?.ClearMark();
        hPosture?.ResetPosture();
        playerController?.OnPlayerDeath();
    }

    public void Revive()
    {
        if (reviveCoroutine != null) StopCoroutine(reviveCoroutine);
        currentHP = maxHP;
        isDead = false;
        playerContext?.ClearMark();
        IsRevive = reviveTime > 0f;
        reviveCoroutine = IsRevive ? StartCoroutine(EndReviveProtection()) : null;
        hPosture?.ResetPosture();
        playerController?.OnPlayerRevive();
    }

    private IEnumerator EndReviveProtection()
    {
        yield return new WaitForSeconds(reviveTime);
        IsRevive = false;
        reviveCoroutine = null;
    }

    private void OnDisable()
    {
        if (reviveCoroutine != null) StopCoroutine(reviveCoroutine);
        reviveCoroutine = null;
        IsRevive = false;
        playerContext?.ClearMark();
    }
}
