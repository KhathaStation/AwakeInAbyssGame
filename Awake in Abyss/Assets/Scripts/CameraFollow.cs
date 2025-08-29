using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;   // ����Ф÷��������ͧ���

    void Update()
    {
        if (player == null) return;

        // ���˹� Y ��觡�ҧ�ͧ���ͧ (viewport 0.5 = ��ҧ˹�Ҩ�)
        float cameraMidY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)).y;

        // ��� Player �����٧���ҡ�觡�ҧ���ͧ ? ����͹���ͧ���
        if (player.position.y > cameraMidY)
        {
            transform.position = new Vector3(
                transform.position.x,
                player.position.y,   // ����͹���ͧ��� Y �ͧ Player
                transform.position.z
            );
        }
    }
}
