using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Orbit Settings (�Թǹ�ͺ Player)")]
    public float orbitRadius = 4f;        // ����ա��ǹ�ͺ
    public float orbitSpeed = 60f;        // ��������㹡��ǹ (ͧ��/�Թҷ�)
    public float smoothSpeed = 5f;        // ������¹㹡������͹

    [Header("Dive Attack Settings (��������� Player)")]
    public float diveInterval = 10f;      // �ء � ����Ԩо��������
    public float diveSpeed = 12f;         // ��������㹡�þ��
    public float diveDuration = 1.2f;     // ���ҷ��������� player (������)

    private Transform player;
    private Rigidbody2D rb;
    private float angle;                  // ����Ѩ�غѹ㹡�úԹǹ
    private bool isDiving = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = false;
        rb.gravityScale = 0f; // Enemy ���
        angle = Random.Range(0f, 360f);

        // ����� Coroutine ���Ф�·� Dive Attack �ء � X ��
        StartCoroutine(DiveAttackLoop());
    }

    void FixedUpdate()
    {
        if (player == null || isDiving) return;

        // ? �Թǹ�ͺ Player
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
        Vector2 target = player.position; // �ش player �Ѩ�غѹ�͹����� Dive

        while (elapsed < diveDuration)
        {
            elapsed += Time.deltaTime;
            // ? ��觵ç��� player
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, diveSpeed * Time.deltaTime);
            rb.MovePosition(newPos);

            yield return null;
        }

        // ? ��ѧ�ҡ������� ? ��Ѻ仺Թǹ����͹���
        isDiving = false;
    }
}
