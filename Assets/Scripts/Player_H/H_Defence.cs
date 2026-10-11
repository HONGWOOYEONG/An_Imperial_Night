using UnityEngine;
using UnityEngine.InputSystem;

public class H_Defence : MonoBehaviour
{
    private bool isDefending = false;
    private bool isParrying = false;

    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private PlayerController playerController;
    private bool isAbilityParrying = false;
    private bool isAbilityDefending = false;

    public bool IsParrying => isParrying || isAbilityParrying;
    public bool IsDefending => isDefending || isAbilityDefending;
    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null) return;
        playerController.ActionsCancelled += CancelDefence;
    }

    public void OnDefence(InputValue value)
    {
        if (!isActiveAndEnabled) return;
        if (value.isPressed)
        {
            // 방어 입력을 받더라도 현재 FSM 상태에서 방어가 가능한지 먼저 확인합니다.
            if (playerController != null && !playerController.TryStartAction(PlayerState.Defending)) return;
        }
        else
        {
            EndManualDefence();
        }
    }

    private void EndManualDefence()
    {
        isDefending = false;
        isParrying = false;
        playerMovement?.SetDefending(false);
        playerController?.EndAction(PlayerState.Defending);
    }

    public void CancelDefence()
    {
        // 강제 중단 시에는 일반 방어뿐 아니라 능력으로 부여된 방어 판정도 해제합니다.
        EndManualDefence();
        isAbilityParrying = false;
        isAbilityDefending = false;
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.ActionsCancelled -= CancelDefence;
        }
        CancelDefence();
    }

    public void BeginDefence()
    {
        if (playerController == null ||
            playerController.FSM.ActionState != PlayerState.Defending)
            return;

        isDefending = true;
        isParrying = true;

        playerMovement?.SetDefending(true);
    }

    public void EndParry()
    {
        isParrying = false;
    }

    public void EndDefence()
    {
        isDefending = false;
        playerMovement?.SetDefending(false);
        playerController?.EndAction(PlayerState.Defending);
    }

    public void StartAbilityParry()
    {
        isAbilityParrying = true;
    }

    public void EndAbilityParry()
    {
        isAbilityParrying = false;
    }

    public void StartAbilityDefence()
    {
        isAbilityDefending = true;
    }

    public void EndAbilityDefence()
    {
        isAbilityDefending = false;
    }


}
