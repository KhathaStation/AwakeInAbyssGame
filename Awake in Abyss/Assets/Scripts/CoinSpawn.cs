using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawn : MonoBehaviour
{
    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public int coinsPerSet = 10;
    public float verticalSpacing = 0.5f;

    [Header("Spawn Settings")]
    public float spawnAboveCamera = 1f;
    public float yRandomOffset = 0.2f;
    [Range(0f, 1f)] public float doubleColumnChance = 0.3f;
    public float minColumnGap = 2f;
    public float edgeMargin = 0.2f;

    [Header("Pattern Settings")]
    public bool useColumn = true;
    public bool useZigZag = true;
    public bool useStaircase = true;

    [Header("Player Reference")]
    public Transform playerTransform;
    public float startYOffset = 2f; // offset เหนือหัว player

    private Camera cam;
    private float highestYSpawned = 0f;
    private List<GameObject> activeCoins = new List<GameObject>();

    private enum PatternType { Column, ZigZag, Staircase }

    void Start()
    {
        cam = Camera.main;
        SpawnInitialCoinSet();
    }

    void Update()
    {
        SpawnAboveCameraIfNeeded();
        RemoveOffscreenCoins();
    }

    // === Spawn ชุดแรกเหนือ player + เหนือกล้อง ===
    void SpawnInitialCoinSet()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player Transform not assigned in CoinSpawn!");
            return;
        }

        // ตำแหน่งกล้องบนสุด
        Vector3 topOfCamera = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, -cam.transform.position.z));

        // spawn เหนือ player + เผื่อ offset
        float startY = Mathf.Max(playerTransform.position.y + startYOffset, topOfCamera.y);
        highestYSpawned = startY;

        SpawnCoinPattern(startY);
    }

    void SpawnAboveCameraIfNeeded()
    {
        Vector3 topOfCamera = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, -cam.transform.position.z));

        if (highestYSpawned < topOfCamera.y + spawnAboveCamera)
        {
            float startY = highestYSpawned + verticalSpacing * coinsPerSet;
            SpawnCoinPattern(startY);
            highestYSpawned = startY;
        }
    }

    void SpawnCoinPattern(float startY)
    {
        List<PatternType> availablePatterns = new List<PatternType>();
        if (useColumn) availablePatterns.Add(PatternType.Column);
        if (useZigZag) availablePatterns.Add(PatternType.ZigZag);
        if (useStaircase) availablePatterns.Add(PatternType.Staircase);
        if (availablePatterns.Count == 0) return;

        PatternType pattern = availablePatterns[Random.Range(0, availablePatterns.Count)];

        switch (pattern)
        {
            case PatternType.Column:
                SpawnCoinColumnsAtY(startY);
                break;
            case PatternType.ZigZag:
                SpawnZigZag(startY);
                break;
            case PatternType.Staircase:
                SpawnStaircase(startY);
                break;
        }
    }

    void SpawnCoinColumnsAtY(float startY)
    {
        int columns = (Random.value < doubleColumnChance) ? 2 : 1;

        float halfWidth = coinPrefab.GetComponent<SpriteRenderer>().bounds.extents.x;
        float leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0f, -cam.transform.position.z)).x + halfWidth + edgeMargin;
        float rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0f, -cam.transform.position.z)).x - halfWidth - edgeMargin;

        List<float> usedX = new List<float>();

        for (int c = 0; c < columns; c++)
        {
            float xPos = 0f;
            int attempts = 0;

            do
            {
                xPos = Random.Range(leftEdge, rightEdge);
                attempts++;
                if (attempts > 20) break;
            }
            while (usedX.Exists(prevX => Mathf.Abs(prevX - xPos) < minColumnGap));

            usedX.Add(xPos);

            for (int i = 0; i < coinsPerSet; i++)
            {
                float yOffset = startY + i * verticalSpacing + Random.Range(-yRandomOffset, yRandomOffset);
                Vector3 coinPos = new Vector3(xPos, yOffset, 0f);

                GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
                activeCoins.Add(coin);
            }
        }
    }

    void SpawnZigZag(float startY)
    {
        float halfWidth = coinPrefab.GetComponent<SpriteRenderer>().bounds.extents.x;
        float leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0f, -cam.transform.position.z)).x + halfWidth + edgeMargin;
        float rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0f, -cam.transform.position.z)).x - halfWidth - edgeMargin;

        float leftX = Random.Range(leftEdge, (leftEdge + rightEdge) / 2f - 1f);
        float rightX = Random.Range((leftEdge + rightEdge) / 2f + 1f, rightEdge);

        float xPos = leftX;

        for (int i = 0; i < coinsPerSet; i++)
        {
            xPos = (i % 2 == 0) ? leftX : rightX;
            float yOffset = startY + i * verticalSpacing;
            Vector3 coinPos = new Vector3(xPos, yOffset, 0f);

            GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
            activeCoins.Add(coin);
        }
    }

    void SpawnStaircase(float startY)
    {
        float halfWidth = coinPrefab.GetComponent<SpriteRenderer>().bounds.extents.x;
        float leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0f, -cam.transform.position.z)).x + halfWidth + edgeMargin;
        float rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0f, -cam.transform.position.z)).x - halfWidth - edgeMargin;

        bool goingRight = (Random.value > 0.5f);
        float xStart = Random.Range(leftEdge, rightEdge);
        float stepX = 0.5f * (goingRight ? 1 : -1);

        for (int i = 0; i < coinsPerSet; i++)
        {
            float yOffset = startY + i * verticalSpacing;
            float xPos = Mathf.Clamp(xStart + i * stepX, leftEdge, rightEdge);

            Vector3 coinPos = new Vector3(xPos, yOffset, 0f);

            GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
            activeCoins.Add(coin);
        }
    }

    void RemoveOffscreenCoins()
    {
        Vector3 bottomOfCamera = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, -cam.transform.position.z));

        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (activeCoins[i] != null && activeCoins[i].transform.position.y < bottomOfCamera.y - 1f)
            {
                Destroy(activeCoins[i]);
                activeCoins.RemoveAt(i);
            }
        }
    }
}
