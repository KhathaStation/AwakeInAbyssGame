using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;   // ตัวละครที่จะให้กล้องตาม

    void Update()
    {
        if (player == null) return;

        // ตำแหน่ง Y กึ่งกลางของกล้อง (viewport 0.5 = กลางหน้าจอ)
        float cameraMidY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)).y;

        // ถ้า Player ขึ้นไปสูงกว่ากึ่งกลางกล้อง ? เลื่อนกล้องขึ้น
        if (player.position.y > cameraMidY)
        {
            transform.position = new Vector3(
                transform.position.x,
                player.position.y,   // เลื่อนกล้องตาม Y ของ Player
                transform.position.z
            );
        }
    }
}
