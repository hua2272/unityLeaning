using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomInputSystem : MonoBehaviour
{
    public static CustomInputSystem instance { get; private set; }
    
    [Serializable] public class InputAction
    {
        public string actionName;
        public KeyCode defaultKeyboardKey;
        public string defaultGamepadButton;
        public KeyCode currentKeyboardKey;
        public string currentGamepadButton;
        
        public InputAction(string name, KeyCode keyboardKey, string gamepadButton)
        {
            actionName = name;
            defaultKeyboardKey = keyboardKey;
            defaultGamepadButton = gamepadButton;
            currentKeyboardKey = keyboardKey;
            currentGamepadButton = gamepadButton;
        }
    }
    
    public enum InputDevice { Keyboard, Gamepad }
    
    [Header("Settings")]
    public InputDevice currentDevice = InputDevice.Keyboard;
    public float gamepadCheckInterval = 0.5f;
    public float axisDeadZone = 0.2f;
    
    [Header("Input Actions")]
    public List<InputAction> inputActions = new List<InputAction>();
    
    public event Action<InputDevice> OnDeviceChanged;
    [HideInInspector] public UnityEvent<string> OnActionRebound;
    
    private Dictionary<string, InputAction> actionMap = new Dictionary<string, InputAction>();
    private float lastGamepadCheckTime;
    public bool isRebinding = false;
    private string rebindingAction;
    private bool rebindingForKeyboard;
    
    void Awake()
    {
        Debug.unityLogger.Log("-------CustomInputSystem instance-------");
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializeInputSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        //DetectInputDevice();
        if (isRebinding)
        {
            ProcessRebinding();
        }
    }
    
    void InitializeInputSystem()
    {
        // 创建默认输入映射
        if (inputActions.Count == 0)
        {
            inputActions.Add(new InputAction("MoveUp", KeyCode.W, "DPadUp"));
            inputActions.Add(new InputAction("MoveDown", KeyCode.S, "DPadDown"));
            inputActions.Add(new InputAction("MoveLeft", KeyCode.A, "DPadLeft"));
            inputActions.Add(new InputAction("MoveRight", KeyCode.D, "DPadRight"));
            inputActions.Add(new InputAction("Jump", KeyCode.Space, "ButtonSouth"));
            inputActions.Add(new InputAction("Attack", KeyCode.Mouse0, "ButtonWest"));
            inputActions.Add(new InputAction("Interact", KeyCode.E, "ButtonEast"));
            inputActions.Add(new InputAction("Menu", KeyCode.Escape, "ButtonStart"));
            inputActions.Add(new InputAction("UIConfirm", KeyCode.J, "DPadConfirm"));
            inputActions.Add(new InputAction("UICancel", KeyCode.K, "DPadCancel"));
        }
        // 构建快速查找字典
        foreach (var action in inputActions)
        {
            actionMap[action.actionName] = action;
        }
        // 加载保存的按键设置
        LoadKeyBindings();
    }
    
    #region 输入查询方法
    
    public bool GetButton(string actionName)
    {
        if (!actionMap.ContainsKey(actionName))
        {
            Debug.LogWarning($"Input action '{actionName}' not found!");
            return false;
        }
        var action = actionMap[actionName];
        
        if (currentDevice == InputDevice.Keyboard)
        {
            return Input.GetKey(action.currentKeyboardKey);
        }
        else
        {
            return Input.GetButton(action.currentGamepadButton);
        }
    }
    
    public float GetAxis(string axisName)
    {
        // 处理摇杆输入
        return Input.GetAxis(axisName);
    }
    
    public Vector2 GetMovementAxis()
    {
        if (currentDevice == InputDevice.Keyboard)
        {
            float x = (GetButton("MoveRight") ? 1 : 0) - (GetButton("MoveLeft") ? 1 : 0);
            float y = (GetButton("MoveUp") ? 1 : 0) - (GetButton("MoveDown") ? 1 : 0);
            return new Vector2(x, y).normalized;
        }
        else
        {
            return new Vector2(GetAxis("Horizontal"), GetAxis("Vertical"));
        }
    }
    
    #endregion
    
    #region 按键重绑定
    
    public void StartRebinding(string actionName, bool forKeyboard)
    {
        isRebinding = true;
        rebindingAction = actionName;
        rebindingForKeyboard = forKeyboard;
    }
    
    public void CancelRebinding()
    {
        isRebinding = false;
        rebindingAction = null;
    }
    
    void ProcessRebinding()
    {
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                // 跳过不应该绑定的键
                if (keyCode == KeyCode.None || keyCode == KeyCode.Escape)
                    continue;

                BindKey(rebindingAction, keyCode);
                return;
            }
        }
    }
    
    void BindKey(string actionName, KeyCode newKey)
    {
        if (!actionMap.ContainsKey(actionName)) return;
        
        // 检查按键是否已被使用
        foreach (var action in inputActions)
        {
            if (action.actionName != actionName && action.currentKeyboardKey == newKey)
            {
                Debug.LogWarning($"Key {newKey} is already bound to {action.actionName}");
                return;
            }
        }
        
        actionMap[actionName].currentKeyboardKey = newKey;
        isRebinding = false;
        
        SaveKeyBindings();
        OnActionRebound?.Invoke(actionName);
        
        Debug.Log($"Bound {actionName} to {newKey}");
    }
    
    public void ResetToDefaults()
    {
        foreach (var action in inputActions)
        {
            action.currentKeyboardKey = action.defaultKeyboardKey;
            action.currentGamepadButton = action.defaultGamepadButton;
        }
        
        SaveKeyBindings();
        Debug.Log("All bindings reset to defaults");
    }
    
    #endregion
    
    #region 数据持久化
    
    public void SaveKeyBindings()
    {
        foreach (var action in inputActions)
        {
            PlayerPrefs.SetInt($"Key_{action.actionName}", (int)action.currentKeyboardKey);
            PlayerPrefs.SetString($"Button_{action.actionName}", action.currentGamepadButton);
        }
        PlayerPrefs.Save();
    }
    
    public void LoadKeyBindings()
    {
        foreach (var action in inputActions)
        {
            if (PlayerPrefs.HasKey($"Key_{action.actionName}"))
            {
                int keyValue = PlayerPrefs.GetInt($"Key_{action.actionName}");
                action.currentKeyboardKey = (KeyCode)keyValue;
            }
            
            if (PlayerPrefs.HasKey($"Button_{action.actionName}"))
            {
                action.currentGamepadButton = PlayerPrefs.GetString($"Button_{action.actionName}");
            }
        }
    }
    
    #endregion
}