using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable] public class MenuItemData
{
    public string title;
    public string buttonText;
    [NonSerialized] private Action action;// 使用委托来存储可调用方法 避免序列化问题
    
    public MenuItemData(string title,  string buttonText, Action action)
    {
        this.title = title;
        this.buttonText = buttonText;
        this.action = action;
    }
    
    public void Invoke()
    {
        action?.Invoke();
    }
}

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
    
    private List<GameObject> createdMenuItems = new List<GameObject>();
    private RectTransform contentRectTransform;
    
    void Start()
    {
        gameSaveManager = GameSaveManager.instance;
        systemManager = SystemManager.instance;
        screenController = ScreenController.instance;
        InitializeMenu();
    }
    
    void InitializeMenu()
    {
        contentRectTransform = contentParent.GetComponent<RectTransform>();
        
        ClearMenuItems();// 清除现有菜单项
        CreateMenuItems();// 创建菜单项
        UpdateContentSize();// 更新Content大小
        InitializeMenuItems();
    }

    void InitializeMenuItems()
    {
        menuItems.Add(new MenuItemData("保存游戏", "1", () => gameSaveManager.SaveGame()));
        menuItems.Add(new MenuItemData("键盘按键设置", "2", () => systemManager.EnterSubPanel(0)));
        menuItems.Add(new MenuItemData("全屏", "3", () => screenController.ScreenModeChange()));
    }
    
    void ClearMenuItems()
    {
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
        // 查找TextMeshPro组件 - 标题
        Transform titleTransform = menuItem.transform.Find("Title");
        TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
        titleText.text = itemData.title;
        
        // 查找Button组件
        Transform buttonTransform = menuItem.transform.Find("Button");
        Button button = buttonTransform.GetComponent<Button>();
        button.onClick.AddListener(() => itemData.Invoke());

        // 设置按钮文本
        Transform buttonTextTransform = buttonTransform.Find("Text");
        TextMeshProUGUI buttonText = buttonTextTransform.GetComponent<TextMeshProUGUI>();
        buttonText.text = itemData.buttonText;
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
}