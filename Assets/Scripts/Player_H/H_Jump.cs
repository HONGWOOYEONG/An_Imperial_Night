using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class H_Jump : MonoBehaviour
{
    [SerializeField] private float jumpPower;

    private PlayerMovement movement;
    private PlayerController playerController;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        playerController = GetComponent<PlayerController>();
    }

    public void OnJump(InputValue value)
    {
        if (!isActiveAndEnabled) return;
        if (!value.isPressed)
            return;

        // 접지 판정은 H/T 점프가 따로 계산하지 않고 PlayerMovement에서 읽습니다.
        if (!movement.IsGrounded) return;
        if (playerController != null && !playerController.CanJump) return;


        movement.Jump(jumpPower);
    }


}
