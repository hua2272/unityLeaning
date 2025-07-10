using UnityEngine;
using UnityEngine.UI;

public class MiniMapController : MonoBehaviour
{
    [Header("References")]
    public Camera mapCamera;
    public RawImage miniMapRender;
    public RawImage fullMapRender;
    public RectTransform miniMapPlayerIcon;
    public RectTransform fullMapPlayerIcon;
    public GameObject fullMapPanel;
    
    [Header("Settings")]
    public float miniMapSize = 200f;
    public float fullMapSize = 800f;
    public string playerTag = "Player";
    
    public Player player;
    private bool isFullMapActive = false;
    private RenderTexture miniMapTexture;
    private RenderTexture fullMapTexture;

    void Start()
    {
        // 初始化渲染纹理
        miniMapTexture = new RenderTexture((int)miniMapSize, (int)miniMapSize, 16);
        fullMapTexture = new RenderTexture((int)fullMapSize, (int)fullMapSize, 16);
        // 设置纹理
        miniMapRender.texture = miniMapTexture;
        fullMapRender.texture = fullMapTexture;
        // 初始设置相机
        mapCamera.targetTexture = miniMapTexture;
        // 确保大地图初始关闭
        fullMapPanel.SetActive(false);
    }

    void Update()
    {
        // 切换地图状态
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }
        // 更新玩家位置标记
        UpdatePlayerIcon();
    }

    void ToggleMap()
    {
        isFullMapActive = !isFullMapActive;
        if (isFullMapActive)
        {
            // 进入大地图时暂停跟随
            mapCamera.GetComponent<MapCameraController>().followPlayer = false;
            // 调整相机范围以显示整个地图
            mapCamera.orthographicSize = fullMapSize / 2f;
            fullMapPanel.SetActive(true);
        }
        else
        {
            // 返回小地图时恢复跟随
            mapCamera.GetComponent<MapCameraController>().followPlayer = true;
            // 恢复小地图范围
            mapCamera.orthographicSize = miniMapSize / 2f;
            fullMapPanel.SetActive(false);
        }
    }

    void UpdatePlayerIcon()
    {
        if (player == null) return;
        // 计算玩家在地图上的位置(0-1范围)
        Vector3 viewportPos = mapCamera.WorldToViewportPoint(player.transform.position);
        // 更新小地图图标位置
        miniMapPlayerIcon.anchorMin = viewportPos;
        miniMapPlayerIcon.anchorMax = viewportPos;
        // 更新大地图图标位置
        fullMapPlayerIcon.anchorMin = viewportPos;
        fullMapPlayerIcon.anchorMax = viewportPos;
    }
}