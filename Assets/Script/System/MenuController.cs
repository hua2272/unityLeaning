using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuController : MonoBehaviour
{
    [Header("菜单配置")]
    [SerializeField] private GameObject menuItemPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float itemSpacing = 10f;
    [SerializeField] private Vector2 itemSize = new Vector2(300, 80);
    
    [Header("菜单项数据")]
    [SerializeField] private List<MenuItemData> menuItems = new List<MenuItemData>();
    
    [Header("功能脚本")]
    private GameSaveManager gameSaveManager;
    private SystemManager systemManager;
    private ScreenController screenController;
    private AudioManager audioManager;
    
    private List<GameObject> createdMenuItems = new List<GameObject>();
    private RectTransform contentRectTransform;
    
    // 新增：按钮与菜单项的映射
    private Dictionary<Button, MenuItemData> buttonToMenuItemMap = new Dictionary<Button, MenuItemData>();
    
    void Start()
    {
        gameSaveManager = GameSaveManager.instance;
        systemManager = SystemManager.instance;
        screenController = ScreenController.instance;
        audioManager = AudioManager.instance;
        
        systemManager.OnOptionChanged += HandleOptionChange;// 订阅左右方向键事件
        InitializeMenu();
        ApplySavedOptions();// 应用已保存的选项
    }
    
    void OnDestroy()// 取消订阅事件
    {
        if (systemManager != null)
        {
            systemManager.OnOptionChanged -= HandleOptionChange;
        }
    }
    
    void InitializeMenu()
    {
        contentRectTransform = contentParent.GetComponent<RectTransform>();
        
        ClearMenuItems();// 清除现有菜单项
        InitializeMenuItems(); // 初始化菜单项数据
        CreateMenuItems();// 创建菜单项
        UpdateContentSize();// 更新Content大小
    }

    void InitializeMenuItems()
    {
        menuItems.Clear();
        menuItems.Add(new MenuItemData("保存游戏", "保存", () => gameSaveManager.SaveGame()));
        menuItems.Add(new MenuItemData("键盘按键设置", "设置", () => systemManager.EnterSubPanel(0)));
        menuItems.Add(new MenuItemData("屏幕", new List<string> {"无边框全屏", "窗口化"}, 
            0, (index) => screenController.ScreenModeChange(index), "ScreenMode"));
        menuItems.Add(new MenuItemData("音乐", new List<string> {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}, 
            4, (index) => audioManager.SetMusicVolume(index), "MusicVolume"));
        menuItems.Add(new MenuItemData("音效", new List<string> {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}, 
            4, (index) => audioManager.SetSFXVolume(index), "SFXVolume"));
    }
    
    // 新增：应用已保存的选项
    private void ApplySavedOptions()
    {
        foreach (var menuItem in menuItems)
        {
            if (menuItem.isOptionButton)
            {
                menuItem.ApplyCurrentOption();
                Debug.Log($"应用已保存选项: {menuItem.title} -> {menuItem.currentOptionIndex}");
            }
        }
    }
    
    void ClearMenuItems()
    {
        buttonToMenuItemMap.Clear();
        foreach (var item in createdMenuItems)
        {
            if (item != null)
                Destroy(item);
        }
        createdMenuItems.Clear();
    }
    
    void CreateMenuItems()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            CreateMenuItem(menuItems[i], i);
        }
    }
    
    void CreateMenuItem(MenuItemData itemData, int index)
    {
        // 实例化预制件
        GameObject menuItem = Instantiate(menuItemPrefab, contentParent);
        createdMenuItems.Add(menuItem);
        
        // 设置对象名称
        menuItem.name = $"MenuItem_{index}";
        
        // 获取RectTransform并设置基本属性
        RectTransform rectTransform = menuItem.GetComponent<RectTransform>();
        SetupRectTransform(rectTransform, index);
        
        // 设置UI元素
        SetupUIElements(menuItem, itemData, index);
    }
    
    void SetupRectTransform(RectTransform rectTransform, int index)
    {
        // 重置变换
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;
    
        // 使用顶部居中的锚点
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
    
        // 设置固定尺寸
        rectTransform.sizeDelta = itemSize;
    
        // 修正位置计算：从Content顶部开始向下排列
        // 第一个菜单项应该在Content顶部，后续项依次向下
        float yPosition = -index * (itemSize.y + itemSpacing) - (itemSize.y * 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, yPosition);
    
        Debug.Log($"修正后 - 菜单项 {index} 位置: {rectTransform.anchoredPosition}");
    }
    
    void SetupUIElements(GameObject menuItem, MenuItemData itemData, int index)
    {
        // 为菜单项添加水平布局组
        HorizontalLayoutGroup itemLayout = menuItem.GetComponent<HorizontalLayoutGroup>();
        if (itemLayout == null)
        {
            itemLayout = menuItem.AddComponent<HorizontalLayoutGroup>();
        }
        itemLayout.padding = new RectOffset(10, 10, 5, 5);
        itemLayout.spacing = 15f;
        itemLayout.childAlignment = TextAnchor.MiddleCenter;
        itemLayout.childControlWidth = true;
        itemLayout.childControlHeight = true;
        itemLayout.childForceExpandWidth = false;
        itemLayout.childForceExpandHeight = true;
        
        // 查找TextMeshPro组件 - 标题
        Transform titleTransform = menuItem.transform.Find("Title");
        TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
        titleText.text = itemData.title;
                
        // 设置标题的布局元素
        LayoutElement titleLayout = titleTransform.GetComponent<LayoutElement>();
        if (titleLayout == null)
        {
            titleLayout = titleTransform.gameObject.AddComponent<LayoutElement>();
        }

        titleLayout.flexibleWidth = 1f;
        titleLayout.preferredWidth = -1f;
        
        // 设置文本自适应
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 12f;
        titleText.fontSizeMax = 24f;
        titleText.overflowMode = TextOverflowModes.Ellipsis;
        
        // 查找Button组件
        Transform buttonTransform = menuItem.transform.Find("Button");
        Button button = buttonTransform.GetComponent<Button>();
        
        // 将按钮与菜单项数据关联
        buttonToMenuItemMap[button] = itemData;
        
        button.onClick.AddListener(() =>
        {
            itemData.Invoke();
            // 如果是选项按钮，更新按钮文本
            if (itemData.isOptionButton)
            {
                UpdateButtonText(buttonTransform, itemData);
            }
        });

        // 设置按钮的布局元素
        LayoutElement buttonLayout = buttonTransform.GetComponent<LayoutElement>();
        if (buttonLayout == null)
        {
            buttonLayout = buttonTransform.gameObject.AddComponent<LayoutElement>();
        }

        buttonLayout.preferredWidth = 120f; // 稍微加宽以容纳选项文本
        buttonLayout.minWidth = 80f;
        buttonLayout.preferredHeight = 40f;
        buttonLayout.minHeight = 30f;

        // 设置按钮的RectTransform
        RectTransform buttonRect = buttonTransform.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.sizeDelta = new Vector2(120f, 40f);
        }

        // 设置按钮文本
        UpdateButtonText(buttonTransform, itemData);

        // 设置按钮文本自适应
        Transform buttonTextTransform = buttonTransform.Find("Text");
        TextMeshProUGUI buttonText = buttonTextTransform.GetComponent<TextMeshProUGUI>();

        // 设置按钮文本自适应
        buttonText.enableAutoSizing = true;
        buttonText.fontSizeMin = 10f;
        buttonText.fontSizeMax = 16f; // 稍微减小最大字体大小以容纳更长文本
        buttonText.overflowMode = TextOverflowModes.Ellipsis;
        buttonText.alignment = TextAlignmentOptions.Center;

        // 确保按钮文本填满整个按钮
        RectTransform textRect = buttonTextTransform.GetComponent<RectTransform>();
        if (textRect != null)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }

    // 新增：处理左右方向键事件
    private void HandleOptionChange(int direction)
    {
        // 获取当前选中的按钮
        Button currentButton = systemManager.GetCurrentSelectedButton();
        if (currentButton != null && buttonToMenuItemMap.ContainsKey(currentButton))
        {
            MenuItemData menuItem = buttonToMenuItemMap[currentButton];
            if (menuItem.isOptionButton)
            {
                // 计算新的选项索引
                int newIndex = (menuItem.currentOptionIndex + direction + menuItem.options.Count) % menuItem.options.Count;
                
                // 使用SetOptionIndex方法更新索引并触发回调
                menuItem.SetOptionIndex(newIndex);
                
                // 更新按钮文本显示
                Transform buttonTransform = currentButton.transform;
                UpdateButtonText(buttonTransform, menuItem);
                
                Debug.Log($"选项改变: {menuItem.title} -> {menuItem.GetCurrentOptionText()} (索引: {menuItem.currentOptionIndex})");
            }
        }
    }

    // 更新按钮文本的辅助方法
    void UpdateButtonText(Transform buttonTransform, MenuItemData itemData)
    {
        Transform buttonTextTransform = buttonTransform.Find("Text");
        TextMeshProUGUI buttonText = buttonTextTransform.GetComponent<TextMeshProUGUI>();

        if (itemData.isOptionButton)
        {
            buttonText.text = itemData.GetCurrentOptionText();
        }
        else
        {
            buttonText.text = itemData.buttonText;
        }
    }

    void UpdateContentSize()
    {
        if (contentRectTransform == null) return;
        
        float totalHeight = menuItems.Count * (itemSize.y + itemSpacing) - itemSpacing;
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);
    }
    
    // 在Inspector中修改数据时更新菜单
    void OnValidate()
    {
        if (Application.isPlaying && contentParent != null)
        {
            InitializeMenu();
        }
    }
    
    // 新增：调试方法，查看当前所有菜单项状态
    [ContextMenu("Debug Menu Items")]
    void DebugMenuItems()
    {
        foreach (var item in menuItems)
        {
            if (item.isOptionButton)
            {
                Debug.Log($"{item.title}: {item.GetCurrentOptionText()} (索引: {item.currentOptionIndex})");
            }
        }
    }
    
    // 新增：重置所有选项到默认值
    [ContextMenu("Reset All Options")]
    public void ResetAllOptions()
    {
        foreach (var menuItem in menuItems)
        {
            if (menuItem.isOptionButton)
            {
                menuItem.SetOptionIndex(0); // 重置到第一个选项
            }
        }
    }
}