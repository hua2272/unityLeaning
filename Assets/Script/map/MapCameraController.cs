using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    public Transform player;  // 玩家对象
    public bool followPlayer = true;  // 是否跟随玩家
    public Vector2 offset = Vector2.zero;  // 相机偏移（可选）
    
    public float panSpeed = 10f;
    public Vector2 panLimit; // 地图边界
    
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
    
    void Update()
    {
        if (!followPlayer) // 大地图模式
        {
            Vector3 pos = transform.position;
            // 获取方向键输入
            pos.x += Input.GetAxis("Horizontal") * panSpeed * Time.deltaTime;
            pos.y += Input.GetAxis("Vertical") * panSpeed * Time.deltaTime;
            // 限制地图边界
            pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
            pos.y = Mathf.Clamp(pos.y, -panLimit.y, panLimit.y);
            transform.position = pos;
        }
    }
}