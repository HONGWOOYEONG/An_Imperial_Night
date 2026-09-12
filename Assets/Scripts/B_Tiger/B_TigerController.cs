using UnityEngine;

public class B_TigerController : MonoBehaviour
{
    private B_TigerUtilityAI utilityAI;
    private B_TigerFSM tigerFSM;
    private Rigidbody2D rb;

    [Header("Status")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxPosture = 100f;
    [SerializeField] private float currentPosture;
    [SerializeField] private float moveSpeed = 5f;

    private bool isGroggy;
    public bool IsGroggy => isGroggy;

    private void Awake()
    {
        utilityAI = GetComponent<B_TigerUtilityAI>();
        tigerFSM = GetComponent<B_TigerFSM>();
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        currentPosture = 0f;
        isGroggy = false;
    }

    private void Update()
    {
        if (currentPosture >= maxPosture && !isGroggy)
        {
            isGroggy = true;
            tigerFSM.ChangeState(tigerFSM.GroggyState);
        }
    }

    public void MoveTowartTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - rb.position).normalized;

        if (direction.sqrMagnitude <= 0f)
        {
            Stop();
            return;
        }

        RotationToTarget(direction);

        Vector2 nextPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    public void RotationToTarget(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) <= 0.01f)
            return;

        Vector3 scale = transform.localScale;

        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);

        transform.localScale = scale;
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void EndGroggy()
    {
        currentPosture = maxPosture/2;
        isGroggy = false;
    }
}
