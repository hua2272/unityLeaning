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
            }
        }
    }
    
    void ClearMenuItems()
    {
        buttonToMenuItemMap.Clear();
        foreach (var item in createdMenuItems)
        {
            Destroy(item);
        }
        createdMenuItems.Clear();
    }
    
    void CreateMenuItems()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            GameObject menuItem = Instantiate(menuItemPrefab, contentParent);       // 实例化预制件
            createdMenuItems.Add(menuItem);
            menuItem.name = $"MenuItem_{i}";                                        // 设置对象名称
            SetupRectTransform(menuItem, i);                                        // 获取RectTransform并设置基本属性
            SetupUIElements(menuItem, menuItems[i]);                                // 设置UI元素
        }
    }
    
    void SetupRectTransform(GameObject menuItem, int index)
    {
        RectTransform rectTransform = menuItem.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;
        rectTransform.anchorMin = new Vector2(0.5f, 1f);                                // 使用顶部居中的锚点
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.sizeDelta = itemSize;                                             // 设置固定尺寸
        float yPosition = -index * (itemSize.y + itemSpacing) - (itemSize.y * 0.5f);    // 第一个菜单项应该在Content顶部，后续项依次向下
        rectTransform.anchoredPosition = new Vector2(0, yPosition);
        //Debug.Log($"修正后 - 菜单项 {index} 位置: {rectTransform.anchoredPosition}");
    }
    
    void SetupUIElements(GameObject menuItem, MenuItemData itemData)
    {
        HorizontalLayoutGroup itemLayout = menuItem.AddComponent<HorizontalLayoutGroup>();// 为菜单项添加水平布局组
        itemLayout.padding = new RectOffset(10, 10, 5, 5);
        itemLayout.spacing = 15f;
        itemLayout.childAlignment = TextAnchor.MiddleCenter;
        itemLayout.childControlWidth = true;
        itemLayout.childControlHeight = true;
        itemLayout.childForceExpandWidth = false;
        itemLayout.childForceExpandHeight = true;
        
        // 查找TextMeshPro组件 - 标题
        Transform titleTransform = menuItem.transform.Find("Title");
        titleTransform.gameObject.SetActive(false);
        /*TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
        titleText.text = itemData.title;
        LayoutElement titleLayout = titleTransform.gameObject.AddComponent<LayoutElement>();// 设置标题的布局元素
        titleLayout.flexibleWidth = 1f;
        titleLayout.preferredWidth = -1f;
        titleText.enableAutoSizing = true;// 设置文本自适应
        titleText.fontSizeMin = 12f;
        titleText.fontSizeMax = 24f;
        titleText.overflowMode = TextOverflowModes.Ellipsis;*/
        
        // 查找Button组件
        Transform buttonTransform = menuItem.transform.Find("Button");
        Button button = buttonTransform.GetComponent<Button>();
        buttonToMenuItemMap[button] = itemData;// 将按钮与菜单项数据关联
        if (!itemData.isOptionButton)
            button.onClick.AddListener(() => itemData.action.Invoke());        //非选项按钮才可以点击
        LayoutElement buttonLayout = buttonTransform.gameObject.AddComponent<LayoutElement>();// 设置按钮的布局元素
        buttonLayout.preferredWidth = 120f; // 稍微加宽以容纳选项文本
        buttonLayout.minWidth = 80f;
        buttonLayout.preferredHeight = 40f;
        buttonLayout.minHeight = 30f;
        RectTransform buttonRect = buttonTransform.GetComponent<RectTransform>();// 设置按钮的RectTransform
        buttonRect.sizeDelta = new Vector2(120f, 40f);

        // 设置按钮文本自适应
        Transform buttonTextTransform = buttonTransform.Find("Text");
        TextMeshProUGUI buttonText = buttonTextTransform.GetComponent<TextMeshProUGUI>();
        buttonText.text = itemData.GetCurrentOptionText();// 设置按钮文本
        buttonText.enableAutoSizing = true;// 设置按钮文本自适应
        buttonText.fontSizeMin = 10f;
        buttonText.fontSizeMax = 16f; // 稍微减小最大字体大小以容纳更长文本
        buttonText.overflowMode = TextOverflowModes.Ellipsis;
        buttonText.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = buttonTextTransform.GetComponent<RectTransform>();// 确保按钮文本填满整个按钮
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void HandleOptionChange(int direction)// 新增：处理左右方向键事件
    {
        Button currentButton = systemManager.GetCurrentSelectedButton();// 获取当前选中的按钮
        if (currentButton == null || !buttonToMenuItemMap.ContainsKey(currentButton)) return;
        MenuItemData menuItem = buttonToMenuItemMap[currentButton];
        if (!menuItem.isOptionButton) return;
        
        int newIndex = (menuItem.currentOptionIndex + direction + menuItem.options.Count) % menuItem.options.Count;// 计算新的选项索引
        menuItem.SetOptionIndex(newIndex);// 使用SetOptionIndex方法更新索引并触发回调
        TextMeshProUGUI buttonText = currentButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = menuItem.GetCurrentOptionText();
        //Debug.Log($"选项改变: {menuItem.title} -> {menuItem.GetCurrentOptionText()} (索引: {menuItem.currentOptionIndex})");
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