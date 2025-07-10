using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    public Transform player;  // 玩家对象
    public bool followPlayer = true;  // 是否跟随玩家
    public Vector2 offset = Vector2.zero;  // 相机偏移（可选）
    
    void LateUpdate()
    {
        if (followPlayer && player != null)
        {
            // 更新相机位置（保持Z轴不变）
            Vector3 newPos = player.position;
            newPos.x += offset.x;
            newPos.y += offset.y;
            newPos.z = transform.position.z;  // 保持相机Z轴不变
            transform.position = newPos;
        }
    }
}