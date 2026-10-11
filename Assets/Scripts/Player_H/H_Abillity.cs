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
    [SerializeField] private int avillityParryingTime = 10;
    [SerializeField] private int avillityDefenceTime = 20;

    private const float BaseFps = 60f;

    private float nextAvillityTime;

    [SerializeField] private Rigidbody2D target;

    private bool isAbilityInvincible = false;
    private bool isUsingAbility = false;

    public bool IsAbilityInvincible => isAbilityInvincible;
    public bool IsUsingAbility => isUsingAbility;

    private static float FramesToSeconds(int frames) => frames / BaseFps;

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
        if (playerController != null && !playerController.TryStartAction(PlayerState.Ability)) return;

        isUsingAbility = true;
        isAbilityInvincible = true;
        nextAvillityTime = Time.time + avillityCooldown;
    }

    // pulse 애니메이션의 마지막 프레임에서 호출합니다.
    public void CompleteAbilitySwap()
    {
        if (!OwnsAbility()) return;

        SwapPosition();
        isAbilityInvincible = false;
        StartCoroutine(PostSwapDefence());
    }

    private void SwapPosition()
    {

        PlayerMovement targetMovement = target.GetComponent<PlayerMovement>();
        Vector2 myPosition = rb.position;
        Vector2 targetPosition = target.position;

        // 플레이어뿐 아니라 테스트용 Rigidbody2D 타깃과도 위치를 교환할 수 있습니다.
        playerMovement.Teleport(targetPosition);
        if (targetMovement != null)
            targetMovement.Teleport(myPosition);
        else
            target.position = myPosition;
        // 순간이동 직후 컨텍스트의 위치 정보도 새 위치로 맞춘다.
        GetComponent<PlayerContext>()?.setTransPosition();
        target.GetComponent<PlayerContext>()?.setTransPosition();

        float tempRotation = playerMovement.Rotation;

        playerMovement.SetRotation(targetMovement != null ? targetMovement.Rotation : target.rotation);
        if (targetMovement != null)
            targetMovement.SetRotation(tempRotation);
        else
            target.rotation = tempRotation;

        playerMovement.SetVelocity(Vector2.zero);
        if (targetMovement != null)
            targetMovement.SetVelocity(Vector2.zero);
        else
            target.linearVelocity = Vector2.zero;

    }

    private IEnumerator PostSwapDefence()
    {
        hDef?.StartAbilityParry();

        yield return new WaitForSeconds(FramesToSeconds(avillityParryingTime));

        if (!OwnsAbility()) yield break;
        hDef?.EndAbilityParry();
        hDef?.StartAbilityDefence();

        yield return new WaitForSeconds(FramesToSeconds(avillityDefenceTime));

        if (!OwnsAbility()) yield break;

        hDef?.EndAbilityParry();
        hDef?.EndAbilityDefence();
        isUsingAbility = false;
        playerController?.EndAction(PlayerState.Ability);
    }

    private bool OwnsAbility()
    {
        return isUsingAbility && playerController != null &&
            playerController.FSM.ActionState == PlayerState.Ability;
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
