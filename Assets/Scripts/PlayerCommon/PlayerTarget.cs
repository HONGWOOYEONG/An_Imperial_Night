using UnityEngine;

public class PlayerTarget : MonoBehaviour
{
    private bool isLocking;
    private bool canLock = false;

    private CircleCollider2D radius;
    private Transform target;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        radius = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            target = collision.transform;
            canLock = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform == target)
        {
            target = null;
            canLock = false;
            isLocking = false;
        }
    }

    private void Update()
    {
        if (!isLocking || target == null) return;
        if (playerMovement.HasMoveInput || playerMovement.IsDashing) return;

        FaceTarget();
    }

    public void ToggleLockOn()
    {
        if (!canLock || target == null)
            return;

        isLocking = !isLocking;

        if (isLocking)
        {
            FaceTarget();
        }
    }

    private void FaceTarget()
    {
        float direction = target.position.x - transform.position.x;

        if (direction > 0)
        {
            playerMovement.SetFacingDirection(1);
        }
        else if (direction < 0)
        {
            playerMovement.SetFacingDirection(-1);
        }
    }
}