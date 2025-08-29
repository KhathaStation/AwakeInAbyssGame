using System.Collections.Generic;
using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject GroundPrefab;
    public Transform player;       // ��ҧ�ԧ Player
    public Camera mainCamera;      // ��ҧ�ԧ Camera
    public float LevelWidth = 3f;
    public float MinY = 1.5f;
    public float MaxY = 2.5f;
    public int PlatformsPerBatch = 20;

    private float highestY;
    private List<GameObject> activePlatforms = new List<GameObject>();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Spawn platform �ͺ player ��͹
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

        // ��ҡ��ͧ����٧�֧ platform �٧�ش ? spawn batch ����
        if (camTopY + 5f > highestY)
        {
            for (int i = 0; i < PlatformsPerBatch; i++)
            {
                SpawnPlatform();
            }
        }

        // ź platform �����ͧ�������� (��ҧ���ͧ)
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
