using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Player_H
{
    public class H_Animation : MonoBehaviour
    {
        private Animator animator;
        private PlayerMovement movement;
        private PlayerController controller;

        [Header("Movement animation")]
        [SerializeField] private float directionChangeGraceTime = 0.25f;

        private bool isDead = false;
        private float lastMoveInputTime = float.NegativeInfinity;


        // Use this for initialization
        void Awake()
        {
            animator = GetComponent<Animator>();
            if(animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            movement = GetComponent<PlayerMovement>();
            controller = GetComponent<PlayerController>();
        }

        void Start()
        {
            UpdateAnimation(controller.CurrentState);
        }

        private void UpdateAnimation(PlayerState state)
        {
            if(state == PlayerState.Dead)
            {
                playerDeath();
                return;
            }

            // Dead 상태를 벗어나 다시 상태 변경이 오면 부활한 것으로 처리합니다.
            isDead = false;
            animator.SetBool("IsDead", false);

            animator.SetBool("Defence", state == PlayerState.Defending);

            switch (state)
            {
                case PlayerState.Jump:
                    animator.SetTrigger("Jump");
                    break;
                case PlayerState.Dashing:
                    animator.SetTrigger("Dash");
                    break;
                case PlayerState.Ability:
                    animator.SetTrigger("Abillity");
                    break;
                case PlayerState.Stunned:
                    animator.SetTrigger("Hit");
                    break;
                case PlayerState.Groggy:
                    animator.SetTrigger("Groggy");
                    break;
            }
        }

        void Update()
        {
            bool hasMoveInput = movement.HasMoveInput;
            if (hasMoveInput)
                lastMoveInputTime = Time.time;

            bool shouldPlayMove = hasMoveInput ||
                Time.time - lastMoveInputTime < directionChangeGraceTime;

            animator.SetBool("IsGrounded", movement.IsGrounded);
            animator.SetBool("IsMoving", shouldPlayMove);

        }

        private void OnEnable()
        {
            if (controller != null)
                controller.OnStateChanged += UpdateAnimation;
        }

        private void OnDisable()
        {
            if (controller != null)
                controller.OnStateChanged -= UpdateAnimation;
        }

        private void playerDeath()
        {
            if (isDead) return;
            isDead = true;
            animator.SetBool("Defence", false);
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Death");
        }
    }
}
