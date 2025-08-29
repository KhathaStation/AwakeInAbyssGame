using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Orbit Settings (บินวนรอบ Player)")]
    public float orbitRadius = 4f;        // รัศมีการวนรอบ
    public float orbitSpeed = 60f;        // ความเร็วในการวน (องศา/วินาที)
    public float smoothSpeed = 5f;        // ความเนียนในการเคลื่อน

    [Header("Dive Attack Settings (พุ่งเข้าใส่ Player)")]
    public float diveInterval = 10f;      // ทุก ๆ กี่วิจะพุ่งเข้าใส่
    public float diveSpeed = 12f;         // ความเร็วในการพุ่ง
    public float diveDuration = 1.2f;     // เวลาที่ใช้พุ่งใส่ player (จนเลยไป)

    private Transform player;
    private Rigidbody2D rb;
    private float angle;                  // มุมปัจจุบันในการบินวน
    private bool isDiving = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = false;
        rb.gravityScale = 0f; // Enemy ลอย
        angle = Random.Range(0f, 360f);

        // เริ่ม Coroutine ที่จะคอยทำ Dive Attack ทุก ๆ X วิ
        StartCoroutine(DiveAttackLoop());
    }

    void FixedUpdate()
    {
        if (player == null || isDiving) return;

        // ? บินวนรอบ Player
        angle += orbitSpeed * Time.fixedDeltaTime;
        Vector2 orbitPos = (Vector2)player.position +
                           new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * orbitRadius;

        rb.MovePosition(Vector2.Lerp(rb.position, orbitPos, smoothSpeed * Time.fixedDeltaTime));
    }

    IEnumerator DiveAttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(diveInterval);

            if (player != null)
                yield return StartCoroutine(DiveAttack());
        }
    }

    IEnumerator DiveAttack()
    {
        isDiving = true;

        float elapsed = 0f;
        Vector2 start = rb.position;
        Vector2 target = player.position; // จุด player ปัจจุบันตอนเริ่ม Dive

        while (elapsed < diveDuration)
        {
            elapsed += Time.deltaTime;
            // ? พุ่งตรงไปหา player
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, diveSpeed * Time.deltaTime);
            rb.MovePosition(newPos);

            yield return null;
        }

        // ? หลังจากพุ่งเสร็จ ? กลับไปบินวนเหมือนเดิม
        isDiving = false;
    }
}
