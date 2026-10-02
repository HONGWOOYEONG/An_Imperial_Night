using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class H_Abillity : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private PlayerController playerController;
    private H_Defence hDef;

    [Header("PositionSwap")]
    [SerializeField] private float avillityCooldown = 2f;
    [SerializeField] private int avillityStartupTime = 8;
    [SerializeField] private int avillityParryingTime = 10;
    [SerializeField] private int avillityDefenceTime = 20;

    private float nextAvillityTime;

    [SerializeField] private Rigidbody2D target;

    public const float BASE_FPS = 60;

    private bool isAbilityInvincible = false;
    private bool isUsingAbility = false;

    public bool IsAbilityInvincible => isAbilityInvincible;
    public bool IsUsingAbility => isUsingAbility;

    private float FrameToSeconds(int frame)
    {
        return frame / BASE_FPS;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerController = GetComponent<PlayerController>();
        hDef = GetComponent<H_Defence>();
    }

    private void OnEnable()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null) return;
        playerController.ActionsCancelled += CancelAbility;
    }

    public void OnAbility(InputValue value)
    {
        if (!isActiveAndEnabled) return;
        if (!value.isPressed) return;
        if (Time.time < nextAvillityTime) return;
        if (isUsingAbility) return;
        if (target == null) return;
        if (target.GetComponent<PlayerMovement>() == null) return;
        if (playerController != null && !playerController.TryStartAction(PlayerState.Ability)) return;

        StartCoroutine(StartAvillity());
    }

    IEnumerator StartAvillity()
    {
        isUsingAbility = true;
        nextAvillityTime = Time.time + avillityCooldown;

        // 위치 교환 전 시전 구간
        // 이 시간 동안에는 공격을 완전히 무시
        isAbilityInvincible = true;

        yield return new WaitForSeconds(
            FrameToSeconds(avillityStartupTime)
        );

        PlayerMovement targetMovement = target.GetComponent<PlayerMovement>();
        Vector2 myPosition = rb.position;
        Vector2 targetPosition = target.position;

        // 위치 교환도 각 플레이어의 이동 컴포넌트를 통해 Rigidbody에 반영한다.
        playerMovement.Teleport(targetPosition);
        targetMovement.Teleport(myPosition);
        // 순간이동 직후 컨텍스트의 위치 정보도 새 위치로 맞춘다.
        GetComponent<PlayerContext>()?.setTransPosition();
        target.GetComponent<PlayerContext>()?.setTransPosition();

        float tempRotation = playerMovement.Rotation;

        playerMovement.SetRotation(targetMovement.Rotation);
        targetMovement.SetRotation(tempRotation);

        playerMovement.SetVelocity(Vector2.zero);
        targetMovement.SetVelocity(Vector2.zero);

        isAbilityInvincible = false;

        hDef.StartAbilityParry();

        yield return new WaitForSeconds(FrameToSeconds(avillityParryingTime));

        hDef.EndAbilityParry();

        hDef.StartAbilityDefence();

        yield return new WaitForSeconds(FrameToSeconds(avillityDefenceTime));

        hDef.EndAbilityDefence();

        isUsingAbility = false;
        playerController?.EndAction(PlayerState.Ability);
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.ActionsCancelled -= CancelAbility;
        }
        CancelAbility();
    }

    public void CancelAbility()
    {
        StopAllCoroutines();
        isAbilityInvincible = false;
        isUsingAbility = false;
        if (hDef != null)
        {
            hDef.EndAbilityParry();
            hDef.EndAbilityDefence();
        }
        playerController?.EndAction(PlayerState.Ability);
    }

}