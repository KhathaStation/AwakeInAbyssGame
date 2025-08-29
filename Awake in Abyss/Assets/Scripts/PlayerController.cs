using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Pull-down Bounce")]
    public float pullForce = 20f;

    [Header("Boost (Float)")]
    public float boostHeight = 5f;
    public float boostDuration = 10f;
    public float boostMoveSpeed = 8f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Camera Reset")]
    public float resetMargin = 0.05f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isBoosting = false;
    private bool isPullingDown = false;

    private Coroutine dissolveCoroutine;
    private GameObject currentGroundStand;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
        HandlePullDown();
    }

    void LateUpdate()
    {
        CheckOutOfCamera();
    }

    void CheckGround()
    {
        if (isBoosting)
        {
            isGrounded = false;
            return;
        }

        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        isGrounded = false;
        foreach (Collider2D col in groundColliders)
        {
            if (col.CompareTag("Ground") || col.CompareTag("Stand"))
            {
                isGrounded = true;
                break;
            }
        }
    }

    void HandleMovement()
    {
        if (!isBoosting)
        {
            float moveDirection = Input.GetAxis("Horizontal");
            rb.velocity = new Vector2(moveDirection * MoveSpeed, rb.velocity.y);
        }
    }

    void HandleJump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void HandlePullDown()
    {
        if (!isGrounded && Input.GetKeyDown(KeyCode.LeftShift))
        {
            rb.velocity = new Vector2(rb.velocity.x, -pullForce);
            isPullingDown = true;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Stand")))
        {
            if (isPullingDown && rb.velocity.y <= 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                isPullingDown = false;
            }
            else if (!isBoosting && rb.velocity.y <= 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Stand"))
        {
            currentGroundStand = collision.gameObject;
            if (Input.GetAxis("Vertical") < 0 && dissolveCoroutine == null)
            {
                dissolveCoroutine = StartCoroutine(ChangeTagAfterDelay(currentGroundStand, 3f));
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Stand") && dissolveCoroutine != null)
        {
            StopCoroutine(dissolveCoroutine);
            dissolveCoroutine = null;
        }
    }

    IEnumerator ChangeTagAfterDelay(GameObject platform, float delay)
    {
        yield return new WaitForSeconds(delay);
        platform.tag = "Ground";
        dissolveCoroutine = null;
    }

    // ======= Booster Boost Float =======
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Booster"))
        {
            StartCoroutine(BoostFloatCoroutine());
            Destroy(collision.gameObject);
        }
    }

    private IEnumerator BoostFloatCoroutine()
    {
        isBoosting = true;

        float elapsed = 0f;
        float startY = transform.position.y;
        float targetY = startY + boostHeight;

        while (elapsed < boostDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / boostDuration;

            float newY = Mathf.Lerp(startY, targetY, t);
            float moveX = Input.GetAxis("Horizontal") * boostMoveSpeed * Time.deltaTime;

            rb.MovePosition(new Vector2(transform.position.x + moveX, newY));

            yield return null;
        }

        isBoosting = false;
    }

    // ======= Reset Position กลาง Camera ทุกครั้งที่หลุดจอ =======
    void CheckOutOfCamera()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);

        if (viewPos.x < 0 - resetMargin || viewPos.x > 1 + resetMargin ||
            viewPos.y < 0 - resetMargin || viewPos.y > 1 + resetMargin)
        {
            Vector3 camCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -Camera.main.transform.position.z));
            camCenter.z = 0f;

            rb.position = camCenter;
            rb.velocity = Vector2.zero;
        }
    }
}
