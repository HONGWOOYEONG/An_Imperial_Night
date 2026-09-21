using UnityEngine;

public class BTP_GhostBall : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D ballCollider;
    private float timer;
    private bool isGrounded = false;
    private Animator animator;

    [SerializeField] private float groundSearchHeight = 5f;
    [SerializeField] private float groundSearchDistance = 20f;
    
    [SerializeField] private GameObject explosionHitbox;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        ballCollider = GetComponent<Collider2D>();
        timer = 0f;
        isGrounded = false;
    }

    public void ThrowToLandingPoint(float landingX, float flightTime)
    {
        if (!TryGetLandingPoint(landingX, out Vector2 landingPoint))
        {
            Destroy(gameObject);
            return;
        }

        flightTime = Mathf.Max(flightTime, 0.01f);

        // landingPoint = start + velocity * time + 1/2 * gravity * time^2
        Vector2 displacement = landingPoint - rb.position;
        Vector2 gravity = Physics2D.gravity * rb.gravityScale;
        rb.linearVelocity = (displacement - 0.5f * gravity * flightTime * flightTime) / flightTime;
    }

    private bool TryGetLandingPoint(float landingX, out Vector2 landingPoint)
    {
        Vector2 rayOrigin = new Vector2(landingX, rb.position.y + groundSearchHeight);
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, groundSearchDistance);

        foreach (RaycastHit2D hit in hits)
        {
            if (!hit.collider.CompareTag("Ground"))
                continue;

            float clearance = ballCollider.bounds.extents.y;
            landingPoint = hit.point + Vector2.up * clearance;
            return true;
        }

        landingPoint = default;
        return false;
    }

    private void Update()
    {
        if(isGrounded) timer += Time.deltaTime;
        if(timer > 1f) 
        { 
            Destroy(gameObject); 
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            explosion();
        }
    }

    private void explosion()
    {
        if (explosionHitbox != null)
        {
            explosionHitbox.SetActive(true);
        }

        animator.SetTrigger("Explosion");
    }
}
