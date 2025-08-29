using System.Collections.Generic;
using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject GroundPrefab;
    public Transform player;       // อ้างอิง Player
    public Camera mainCamera;      // อ้างอิง Camera
    public float LevelWidth = 3f;
    public float MinY = 1.5f;
    public float MaxY = 2.5f;
    public int PlatformsPerBatch = 20;

    private float highestY;
    private List<GameObject> activePlatforms = new List<GameObject>();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Spawn platform รอบ player ก่อน
        float playerY = player.position.y;
        for (int i = 0; i < 5; i++)
        {
            float spawnY = playerY + i * Random.Range(MinY, MaxY);
            float spawnX = Random.Range(-LevelWidth, LevelWidth);
            Vector2 spawnPos = new Vector2(spawnX, spawnY);

            GameObject platform = Instantiate(GroundPrefab, spawnPos, Quaternion.identity);
            activePlatforms.Add(platform);
            highestY = spawnY;
        }
    }

    void Update()
    {
        float camTopY = mainCamera.transform.position.y + mainCamera.orthographicSize;

        // ถ้ากล้องใกล้สูงถึง platform สูงสุด ? spawn batch ใหม่
        if (camTopY + 5f > highestY)
        {
            for (int i = 0; i < PlatformsPerBatch; i++)
            {
                SpawnPlatform();
            }
        }

        // ลบ platform ที่กล้องเลยไปแล้ว (ล่างกล้อง)
        float camBottomY = mainCamera.transform.position.y - mainCamera.orthographicSize;
        for (int i = activePlatforms.Count - 1; i >= 0; i--)
        {
            if (activePlatforms[i].transform.position.y < camBottomY - 2f)
            {
                Destroy(activePlatforms[i]);
                activePlatforms.RemoveAt(i);
            }
        }
    }

    void SpawnPlatform()
    {
        float spawnY = highestY + Random.Range(MinY, MaxY);
        float spawnX = Random.Range(-LevelWidth, LevelWidth);
        Vector2 spawnPos = new Vector2(spawnX, spawnY);

        GameObject platform = Instantiate(GroundPrefab, spawnPos, Quaternion.identity);
        activePlatforms.Add(platform);

        highestY = spawnY;
    }
}
