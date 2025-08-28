using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    // Variables for movement and jump speed
    public float MoveSpeed = 5f;
    public float jumpForce;

    // Variable to check if the character is on the ground
    public Transform groundCheck;
    private bool isGrounded;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Rigidbody2D component of the character
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the character is on the ground using the "Ground" tag
        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(groundCheck.position, 0.2f);
        isGrounded = false;
        foreach (Collider2D col in groundColliders)
        {
            if (col.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
                break;
            }
        }

        // Jump control
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Movement control for horizontal movement
        float moveDirection = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveDirection * MoveSpeed, rb.velocity.y);

        // Stop horizontal movement when no input is given
        if (moveDirection == 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    // This function is called when another object's collider hits this object's collider
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object has the "Ground" tag
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Make the character "bounce" upwards
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }
}
