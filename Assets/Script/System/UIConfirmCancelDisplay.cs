using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIConfirmCancelDisplay : MonoBehaviour
{
    private TextMeshProUGUI confirmCancelText;
    private PlayerInputManager inputManager;
    
    void Start()
    {
        CreateDisplay();
        
        // 等待一帧确保PlayerInputManager已初始化
        StartCoroutine(DelayedStart());
    }
    
    System.Collections.IEnumerator DelayedStart()
    {
        yield return null;
        
        // 获取PlayerInputManager实例
        inputManager = PlayerInputManager.instance;
        if (inputManager != null)
        {
            // 监听按键绑定变化事件
            inputManager.OnKeyBindingChanged.AddListener(OnKeyBindingChanged);
        }
        
        UpdateDisplay();
    }
    
    void CreateDisplay()
    {
        // 创建GameObject
        GameObject textObject = new GameObject("ConfirmCancelDisplay");
        
        // 设置为Canvas的子对象
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            textObject.transform.SetParent(canvas.transform);
        }
        else
        {
            // 如果没有找到Canvas，创建一个
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            textObject.transform.SetParent(canvas.transform);
        }
        
        // 添加TextMeshPro组件
        confirmCancelText = textObject.AddComponent<TextMeshProUGUI>();
        
        // 设置文本样式
        confirmCancelText.fontSize = 16;
        confirmCancelText.color = Color.white;
        confirmCancelText.alignment = TextAlignmentOptions.Right;
        
        // 设置RectTransform
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        
        // 设置锚点到右下角
        rectTransform.anchorMin = new Vector2(1, 0);
        rectTransform.anchorMax = new Vector2(1, 0);
        rectTransform.pivot = new Vector2(1, 0);
        
        // 设置位置 - 右下角偏移
        rectTransform.anchoredPosition = new Vector2(-20, 20);
        
        // 设置尺寸
        rectTransform.sizeDelta = new Vector2(300, 30);
        
        // 确保对象在场景切换时不被销毁
        DontDestroyOnLoad(textObject.transform.root.gameObject);
    }
    
    void OnKeyBindingChanged(string actionName, KeyCode newKey)
    {
        // 只有当确认或取消按键发生变化时才更新显示
        if (actionName == "UIConfirm" || actionName == "UICancel")
        {
            UpdateDisplay();
        }
    }
    
    void UpdateDisplay()
    {
        if (confirmCancelText == null || inputManager == null) return;
        
        string confirmKey = GetFormattedKeyName(GetCurrentKeyCode("UIConfirm"));
        string cancelKey = GetFormattedKeyName(GetCurrentKeyCode("UICancel"));
        
        confirmCancelText.text = $"Confirm: {confirmKey}   Cancel: {cancelKey}";
    }
    
    KeyCode GetCurrentKeyCode(string actionName)
    {
        if (inputManager == null) return KeyCode.None;
        
        // 使用反射访问PlayerInputManager的inputActions字段
        var inputManagerType = inputManager.GetType();
        var inputActionsField = inputManagerType.GetField("inputActions", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if (inputActionsField != null)
        {
            var inputActions = inputActionsField.GetValue(inputManager) as System.Collections.Generic.List<PlayerInputManager.InputAction>;
            if (inputActions != null)
            {
                foreach (var action in inputActions)
                {
                    if (action.actionName == actionName)
                    {
                        return action.currentKeyboardKey;
                    }
                }
            }
        }
        
        // 如果反射失败，返回默认值
        return actionName == "UIConfirm" ? KeyCode.Return : KeyCode.Escape;
    }
    
    string GetFormattedKeyName(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.Return => "Enter",
            KeyCode.KeypadEnter => "Enter",
            KeyCode.Escape => "ESC",
            KeyCode.Space => "Space",
            KeyCode.LeftShift => "L-Shift",
            KeyCode.RightShift => "R-Shift",
            KeyCode.LeftControl => "L-Ctrl",
            KeyCode.RightControl => "R-Ctrl",
            KeyCode.LeftAlt => "L-Alt",
            KeyCode.RightAlt => "R-Alt",
            KeyCode.UpArrow => "↑",
            KeyCode.DownArrow => "↓",
            KeyCode.LeftArrow => "←",
            KeyCode.RightArrow => "→",
            KeyCode.Mouse0 => "Mouse0",
            KeyCode.Mouse1 => "Mouse1",
            KeyCode.Mouse2 => "Mouse2",
            _ => keyCode.ToString()
        };
    }
    
    void OnDestroy()
    {
        // 清理事件监听
        if (inputManager != null)
        {
            inputManager.OnKeyBindingChanged.RemoveListener(OnKeyBindingChanged);
        }
    }
}