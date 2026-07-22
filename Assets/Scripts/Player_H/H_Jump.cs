using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class H_Jump : MonoBehaviour
{
    [SerializeField] private float jumpPower;

    private Rigidbody2D rb;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (!isGrounded)
            return;


        isGrounded = false;

        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

}
