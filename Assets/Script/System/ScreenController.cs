using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Screen Modes")]
    public bool borderlessFullscreen = false;
    
    [Header("Window Resize Settings")]
    public bool enableWindowResize = true;
    public float resizeBorderThickness = 10f;
    public Vector2 minWindowSize = new Vector2(400, 300);
    
    [Header("UI References")]
    public GameObject fullscreenUI;
    public GameObject windowedUI;
    
    private RectTransform canvasRect;
    private bool isResizing = false;
    private ResizeDirection currentResizeDirection;
    
    private enum ResizeDirection
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    void Start()
    {
        // 获取Canvas的RectTransform
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }
        
        // 应用初始屏幕模式
        ApplyScreenMode();
    }

    void Update()
    {
        // 快捷键切换全屏/窗口模式
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleScreenMode();
        }
        
        // 在窗口模式下检测鼠标位置以改变光标
        if (!borderlessFullscreen && enableWindowResize)
        {
            HandleResizeCursor();
        }
    }

    public void ScreenModeChange()
    {
        int globalOptionIndex = SystemManager.instance.globalOptionIndex;
        if (globalOptionIndex == 0)
        {
            Debug.Log("全屏");
        }
        else
        {
            Debug.Log("窗口化");
        }
    }

    /// <summary>
    /// 方法一：切换到无边全屏模式
    /// </summary>
    public void SetBorderlessFullscreen()
    {
        borderlessFullscreen = true;
        ApplyScreenMode();
    }

    /// <summary>
    /// 方法二：切换到窗口化模式
    /// </summary>
    public void SetWindowedMode()
    {
        borderlessFullscreen = false;
        ApplyScreenMode();
    }

    /// <summary>
    /// 切换屏幕模式
    /// </summary>
    public void ToggleScreenMode()
    {
        borderlessFullscreen = !borderlessFullscreen;
        ApplyScreenMode();
    }

    /// <summary>
    /// 应用当前屏幕模式设置
    /// </summary>
    private void ApplyScreenMode()
    {
        if (borderlessFullscreen)
        {
            EnterBorderlessFullscreen();
        }
        else
        {
            EnterWindowedMode();
        }
        
        UpdateUI();
    }

    /// <summary>
    /// 进入无边全屏模式
    /// </summary>
    private void EnterBorderlessFullscreen()
    {
        // 获取当前显示器的分辨率
        Resolution currentResolution = Screen.currentResolution;
        
        // 设置无边全屏模式
        Screen.SetResolution(currentResolution.width, currentResolution.height, FullScreenMode.FullScreenWindow);
        
        Debug.Log($"进入无边全屏模式: {currentResolution.width}x{currentResolution.height}");
    }

    /// <summary>
    /// 进入窗口化模式
    /// </summary>
    private void EnterWindowedMode()
    {
        // 设置窗口化模式，使用屏幕的80%作为初始大小
        int width = (int)(Screen.currentResolution.width * 0.8f);
        int height = (int)(Screen.currentResolution.height * 0.8f);
        
        Screen.SetResolution(width, height, FullScreenMode.Windowed);
        
        Debug.Log($"进入窗口化模式: {width}x{height}");
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateUI()
    {
        if (fullscreenUI != null)
            fullscreenUI.SetActive(borderlessFullscreen);
        
        if (windowedUI != null)
            windowedUI.SetActive(!borderlessFullscreen);
    }

    /// <summary>
    /// 处理调整大小的光标显示
    /// </summary>
    private void HandleResizeCursor()
    {
        if (isResizing) return;

        Vector2 mousePosition = Input.mousePosition;
        ResizeDirection direction = GetResizeDirection(mousePosition);
        
        switch (direction)
        {
            case ResizeDirection.Top:
            case ResizeDirection.Bottom:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                // 这里可以设置垂直调整光标
                break;
                
            case ResizeDirection.Left:
            case ResizeDirection.Right:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                // 这里可以设置水平调整光标
                break;
                
            case ResizeDirection.TopLeft:
            case ResizeDirection.BottomRight:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                // 这里可以设置对角线调整光标
                break;
                
            case ResizeDirection.TopRight:
            case ResizeDirection.BottomLeft:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                // 这里可以设置对角线调整光标
                break;
                
            default:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                break;
        }
    }

    /// <summary>
    /// 获取调整大小的方向
    /// </summary>
    private ResizeDirection GetResizeDirection(Vector2 mousePosition)
    {
        if (!enableWindowResize || borderlessFullscreen)
            return ResizeDirection.None;

        float x = mousePosition.x;
        float y = mousePosition.y;
        float width = Screen.width;
        float height = Screen.height;

        bool top = y >= height - resizeBorderThickness;
        bool bottom = y <= resizeBorderThickness;
        bool left = x <= resizeBorderThickness;
        bool right = x >= width - resizeBorderThickness;

        if (top && left) return ResizeDirection.TopLeft;
        if (top && right) return ResizeDirection.TopRight;
        if (bottom && left) return ResizeDirection.BottomLeft;
        if (bottom && right) return ResizeDirection.BottomRight;
        if (top) return ResizeDirection.Top;
        if (bottom) return ResizeDirection.Bottom;
        if (left) return ResizeDirection.Left;
        if (right) return ResizeDirection.Right;

        return ResizeDirection.None;
    }

    /// <summary>
    /// 鼠标按下事件
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!enableWindowResize || borderlessFullscreen)
            return;

        Vector2 mousePosition = eventData.position;
        currentResizeDirection = GetResizeDirection(mousePosition);
        
        if (currentResizeDirection != ResizeDirection.None)
        {
            isResizing = true;
        }
    }

    /// <summary>
    /// 鼠标拖拽事件
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isResizing || borderlessFullscreen)
            return;

        Vector2 delta = eventData.delta;
        ResizeWindow(delta);
    }

    /// <summary>
    /// 调整窗口大小
    /// </summary>
    private void ResizeWindow(Vector2 delta)
    {
        int newWidth = Screen.width;
        int newHeight = Screen.height;

        switch (currentResizeDirection)
        {
            case ResizeDirection.Right:
                newWidth += (int)delta.x;
                break;
            case ResizeDirection.Left:
                newWidth -= (int)delta.x;
                break;
            case ResizeDirection.Top:
                newHeight += (int)delta.y;
                break;
            case ResizeDirection.Bottom:
                newHeight -= (int)delta.y;
                break;
            case ResizeDirection.TopRight:
                newWidth += (int)delta.x;
                newHeight += (int)delta.y;
                break;
            case ResizeDirection.TopLeft:
                newWidth -= (int)delta.x;
                newHeight += (int)delta.y;
                break;
            case ResizeDirection.BottomRight:
                newWidth += (int)delta.x;
                newHeight -= (int)delta.y;
                break;
            case ResizeDirection.BottomLeft:
                newWidth -= (int)delta.x;
                newHeight -= (int)delta.y;
                break;
        }

        // 限制最小窗口大小
        newWidth = (int)Mathf.Max(newWidth, minWindowSize.x);
        newHeight = (int)Mathf.Max(newHeight, minWindowSize.y);

        // 应用新的窗口大小
        Screen.SetResolution(newWidth, newHeight, FullScreenMode.Windowed);
    }

    /// <summary>
    /// 鼠标释放时停止调整大小
    /// </summary>
    public void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            isResizing = false;
        }
    }

    /// <summary>
    /// 在Inspector中显示当前分辨率信息
    /// </summary>
    void OnGUI()
    {
        if (borderlessFullscreen)
        {
            GUI.Label(new Rect(10, 10, 300, 25), $"模式: 无边全屏 - {Screen.width}x{Screen.height}");
        }
        else
        {
            GUI.Label(new Rect(10, 10, 300, 25), $"模式: 窗口化 - {Screen.width}x{Screen.height}");
            GUI.Label(new Rect(10, 35, 300, 25), "拖拽窗口边缘调整大小");
        }
    }

    /// <summary>
    /// 公共方法：获取当前屏幕模式
    /// </summary>
    public string GetCurrentMode()
    {
        return borderlessFullscreen ? "无边全屏模式" : "窗口化模式";
    }

    /// <summary>
    /// 公共方法：获取当前分辨率
    /// </summary>
    public string GetCurrentResolution()
    {
        return $"{Screen.width} x {Screen.height}";
    }
}