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
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;
    public float rayLength = 0.1f;

    [Header("Camera Reset")]
    public float resetMargin = 0.05f;

    [Header("Score System")]
    public int score = 0;

    [Header("Horizontal Bounce Settings")]
    public float bounceMultiplier = 0.2f;
    public float damping = 0.9f;

    [Header("Smooth Reset Settings")]
    public float resetDuration = 0.5f;

    [Header("Jump / Gravity Settings")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isBoosting = false;
    private bool isPullingDown = false;

    private Coroutine dissolveCoroutine;
    private GameObject currentGroundStand;
    private Coroutine resetCoroutine = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandlePullDown();
    }

    void FixedUpdate()
    {
        CheckGround();
        ApplyBetterJump();
        CheckOutOfCamera();
    }

    void CheckGround()
    {
        if (isBoosting)
        {
            isGrounded = false;
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        bool rayHit = Physics2D.Raycast(groundCheck.position, Vector2.down, rayLength, groundLayer);
        isGrounded = hits.Length > 0 || rayHit;
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

    void ApplyBetterJump()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
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

    // ======= Booster & Coin =======
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            score += 1;
            Debug.Log("Picked up Coin! Score: " + score);
            Destroy(collision.gameObject);
        }

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

    // ======= Horizontal Bounce + Smooth Reset Bottom (Ease-Out) =======
    void CheckOutOfCamera()
    {
        float leftX = Camera.main.ViewportToWorldPoint(new Vector3(0 + resetMargin, 0, -Camera.main.transform.position.z)).x;
        float rightX = Camera.main.ViewportToWorldPoint(new Vector3(1 - resetMargin, 0, -Camera.main.transform.position.z)).x;
        float bottomY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z)).y;

        Vector2 newPos = rb.position;

        // X: Horizontal Bounce
        if (rb.position.x < leftX)
        {
            newPos.x = leftX;
            newPos.y += 0.01f;
            rb.velocity = new Vector2(Mathf.Abs(rb.velocity.x) * bounceMultiplier * damping, rb.velocity.y);
            rb.MovePosition(newPos);
        }
        else if (rb.position.x > rightX)
        {
            newPos.x = rightX;
            newPos.y += 0.01f;
            rb.velocity = new Vector2(-Mathf.Abs(rb.velocity.x) * bounceMultiplier * damping, rb.velocity.y);
            rb.MovePosition(newPos);
        }

        // Y: Smooth Reset ถ้าตกต่ำเกินขอบล่าง
        if (rb.position.y < bottomY && resetCoroutine == null)
        {
            resetCoroutine = StartCoroutine(SmoothResetPosition());
        }
    }

    private IEnumerator SmoothResetPosition()
    {
        Vector3 startPos = rb.position;
        Vector3 targetPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -Camera.main.transform.position.z));
        targetPos.z = 0f;

        rb.velocity = Vector2.zero;

        float elapsed = 0f;

        while (elapsed < resetDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / resetDuration);

            // Ease-out cubic
            float easedT = 1 - Mathf.Pow(1 - t, 3);
            rb.MovePosition(Vector3.Lerp(startPos, targetPos, easedT));

            yield return new WaitForFixedUpdate();
        }

        rb.position = targetPos;
        resetCoroutine = null;
    }

    // ======= Debug Gizmos สำหรับ GroundCheck =======
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        // วาดวงกลม GroundCheck
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // วาด Raycast ลงล่าง
        Gizmos.color = Color.red;
        Vector3 rayEnd = groundCheck.position + Vector3.down * rayLength;
        Gizmos.DrawLine(groundCheck.position, rayEnd);
    }
}
