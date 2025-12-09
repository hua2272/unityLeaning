using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class MenuController : MonoBehaviour
{
    [Header("菜单配置")]
    [SerializeField] private GameObject menuItemPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float itemSpacing = 10f;
    [SerializeField] private Vector2 itemSize = new Vector2(300, 80);
    [SerializeField] private GameObject panel;                                       // 总面板
    private int currentPanelLevel = 0;
    private int currentButtonIndex = 0;
    [SerializeField] private Sprite defaultThumbnail;
    
    [Header("菜单项数据")]
    [SerializeField] private List<MenuItemData> menuItems = new List<MenuItemData>();
    private List<Button> currentPanelButtons = new List<Button>();
    
    [Header("Scroll Settings")]
    private ScrollRect currentScrollRect;
    [SerializeField] private float thresholdTop = 5;                                 //滚动阈值（距离顶部/底部的按钮数量）
    [SerializeField] private float scrollStep = 100f;                                //每次滚动的距离
    
    [Header("功能脚本")]
    private GameSaveManager gameSaveManager;
    private GameLoadManager gameLoadManager;
    private SystemManager systemManager;
    private ScreenController screenController;
    private AudioManager audioManager;
    private PlayerInputManager playerInputManager;
    private GameDataManager gameDataManager;
    
    private List<GameObject> createdMenuItems = new List<GameObject>();
    private RectTransform contentRectTransform;
    
    // 按钮与菜单项的映射
    private Dictionary<Button, MenuItemData> buttonToMenuItemMap = new Dictionary<Button, MenuItemData>();
    
    // void OnValidate()// 在Inspector中修改数据时更新菜单
    // {
    //     if (Application.isPlaying && contentParent != null)
    //     {
    //         InitializeMenu();
    //     }
    // }
    
    void Start()
    {
        playerInputManager = PlayerInputManager.instance;
        gameSaveManager = GameSaveManager.instance;
        gameLoadManager = GameLoadManager.instance;
        systemManager = SystemManager.instance;
        screenController = ScreenController.instance;
        audioManager = AudioManager.instance;
        gameDataManager = GameDataManager.instance;
        InitializeMenu();
    }
    
    private void Update()
    {
        if (playerInputManager.isRebinding) return;// 如果正在重绑定，完全跳过所有输入处理

        if (playerInputManager.GetButtonDown("UIMenu"))
        {
            if (panel.activeSelf)
            {
                panel.SetActive(false);
                Time.timeScale = 1;
                currentPanelLevel = 0;
            }
            else
            {
                panel.SetActive(true);
                Time.timeScale = 0;
                ShowButtons(0);
                UpdateButtonSelection();
            }
        }
        if (playerInputManager.GetButtonDown("UICancel") && panel.activeSelf)
        {
            currentPanelLevel -= 1;
            ShowButtons(currentPanelLevel);
        }
        
        if (panel.activeSelf)
            HandleKeyboardNavigation();
    }
    
    private void HandleKeyboardNavigation() //统一的键盘导航处理
    {
        if (playerInputManager.GetButtonDown("UIUp"))
        {
            audioManager.PlayUISound(UISoundType.Navigate);
            HandleScroll(currentButtonIndex, true);
            currentButtonIndex--;
            if (currentButtonIndex < 0)
                currentButtonIndex = currentPanelButtons.Count - 1;
            UpdateButtonSelection();
        }
        else if (playerInputManager.GetButtonDown("UIDown"))
        {
            audioManager.PlayUISound(UISoundType.Navigate);
            HandleScroll(currentButtonIndex, false);
            currentButtonIndex++;
            if (currentButtonIndex >= currentPanelButtons.Count)
                currentButtonIndex = 0;
            UpdateButtonSelection();
        }
        else if (playerInputManager.GetButtonDown("UILeft"))
        {
            audioManager.PlayUISound(UISoundType.Navigate);
            HandleOptionChange(-1);
        }
        else if (playerInputManager.GetButtonDown("UIRight"))
        {
            audioManager.PlayUISound(UISoundType.Navigate);
            HandleOptionChange(1);
        }
        else if (playerInputManager.GetButtonDown("UIConfirm"))
        {
            Button currentButton = GetCurrentSelectedButton();
            if (currentButton != null)
                currentButton.onClick.Invoke();
        }
    }
    
    private Button GetCurrentSelectedButton()
    {
        if (currentPanelButtons == null || currentPanelButtons.Count == 0)
            return null;
        
        // 确保索引在有效范围内
        if (currentButtonIndex < 0)
            currentButtonIndex = 0;
        if (currentButtonIndex >= currentPanelButtons.Count)
            currentButtonIndex = currentPanelButtons.Count - 1;
        
        return currentPanelButtons[currentButtonIndex];
    }
    
    

    void ShowButtons(float panelId)
    {
        if (panelId < 0)
        {
            panel.SetActive(false);
            Time.timeScale = 1;
            return;
        }
        
        currentPanelButtons.Clear();
        
        // 计算当前面板的按钮数量
        int panelButtonCount = 0;
        foreach (var menuItem in menuItems)
        {
            if (menuItem.PanelId == panelId)
                panelButtonCount++;
        }
        
        int buttonIndex = 0;
        // 遍历所有创建的菜单项
        for (int i = 0; i < createdMenuItems.Count; i++)
        {
            GameObject menuItem = createdMenuItems[i];
            
            // 检查菜单项是否属于当前面板
            if (i < menuItems.Count && menuItems[i].PanelId == panelId)
            {
                menuItem.SetActive(true);
                
                // 重新计算并设置位置 - 关键修改！
                RectTransform rectTransform = menuItem.GetComponent<RectTransform>();
                float yPosition = -buttonIndex * (itemSize.y + itemSpacing) - (itemSize.y * 0.5f);
                rectTransform.anchoredPosition = new Vector2(0, yPosition);
                buttonIndex++;
                
                // 获取按钮组件并添加到当前面板按钮列表
                Button button = menuItem.GetComponentInChildren<Button>();
                if (button != null)
                {
                    currentPanelButtons.Add(button);
                    // 如果是选项按钮，确保文本是最新的
                    if (menuItems[i].isOptionButton)
                    {
                        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = menuItems[i].GetCurrentOptionText();
                        }
                    }
                }
            }
            else
            {
                menuItem.SetActive(false);
            }
        }
        
        // 设置当前滚动区域
        currentScrollRect = scrollRect;
        // 重置当前选中的按钮索引
        currentButtonIndex = 0;
        // 更新内容大小以确保滚动正常工作
        UpdateContentSize();
        // 如果需要，重置滚动位置到顶部
        if (currentScrollRect != null)
        {
            currentScrollRect.verticalNormalizedPosition = 1f; // 顶部
        }
        // 更新按钮选中状态
        UpdateButtonSelection();
        // 调试信息
        Debug.Log($"显示面板 {panelId}，找到 {currentPanelButtons.Count} 个按钮");
    }
    
    private void UpdateButtonSelection()
    {
        ResetCurrentPanelButtonColors();// 重置所有按钮颜色
        Button currentButton = GetCurrentSelectedButton();// 设置当前选中按钮的颜色
        if (currentButton != null && currentButton.interactable)
        {
            var colors = currentButton.colors;
            colors.normalColor = Color.yellow;
            colors.selectedColor = Color.yellow;
            currentButton.colors = colors;
        }
    }
    private void ResetCurrentPanelButtonColors()
    {
        foreach (var button in currentPanelButtons)
        {
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.selectedColor = Color.white;
            button.colors = colors;
        }
    }
    
    private void HandleScroll(int buttonIndex, bool isUpward)                                     //处理滚动逻辑
    {
        int buttonCount = currentPanelButtons.Count;
        float thresholdBottom = buttonCount - thresholdTop;
        if (currentScrollRect == null || buttonCount == 0) return;
        if (buttonIndex == 0 && isUpward)
        {
            currentScrollRect.verticalNormalizedPosition = 0f;
            return;
        }

        if (buttonIndex == buttonCount - 1 && !isUpward)
        {
            currentScrollRect.verticalNormalizedPosition = 1f;
            return;
        }
        if (isUpward && (buttonIndex < thresholdTop || buttonIndex > thresholdBottom)) return;    //向上滚动：如果新索引在顶部阈值范围内
        if (!isUpward && (buttonIndex < thresholdTop || buttonIndex > thresholdBottom)) return;   //向下滚动：如果新索引在底部阈值范围内
        ScrollContent(isUpward);
    }
    
    private void ScrollContent(bool scrollUp)                                                     //滚动内容
    {
        float currentPosition = currentScrollRect.verticalNormalizedPosition;// 获取当前滚动位置
        
        // 计算滚动步长（基于内容高度）
        RectTransform content = currentScrollRect.content;
        float contentHeight = content.rect.height;
        float viewportHeight = currentScrollRect.viewport.rect.height;
        
        if (contentHeight <= viewportHeight) return; // 内容不足一屏，不需要滚动
        
        float scrollAmount = scrollStep / (contentHeight - viewportHeight);
        
        // 根据方向调整滚动位置
        if (scrollUp)
            currentScrollRect.verticalNormalizedPosition = Mathf.Clamp01(currentPosition + scrollAmount);
        else
            currentScrollRect.verticalNormalizedPosition = Mathf.Clamp01(currentPosition - scrollAmount);
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
        menuItems.Add(new MenuItemData(0, 0, null, "继续游戏", null));
        menuItems.Add(new MenuItemData(0, 0, null, "读取存档", () =>
        {
            currentPanelLevel = 1;
            ShowButtons(1.2f);
        }));
        menuItems.Add(new MenuItemData(0, 0, null, "保存游戏", () =>
        {
            currentPanelLevel = 1;
            ShowButtons(1.1f);
        }));
        menuItems.Add(new MenuItemData(0, 0, null, "设置", () =>
        {
            currentPanelLevel = 1;
            ShowButtons(1);
        }));
        menuItems.Add(new MenuItemData(0, 0, null, "返回主界面", null));
        menuItems.Add(new MenuItemData(1, 0, null, "键盘按键设置", () => systemManager.EnterSubPanel(0)));
        menuItems.Add(new MenuItemData(1, 1, "屏幕", new List<string> {"无边框全屏", "窗口化"}, 0, (index) => screenController.ScreenModeChange(index), "ScreenMode"));
        menuItems.Add(new MenuItemData(1, 1, "音乐", new List<string> {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}, 4, (index) => audioManager.SetMusicVolume(index), "MusicVolume"));
        menuItems.Add(new MenuItemData(1, 1, "音效", new List<string> {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}, 4, (index) => audioManager.SetSFXVolume(index), "SFXVolume"));
        Dictionary<string, GameData> gameData = gameDataManager.LoadAllGameDataFromDirectory("C:\\Users\\Administrator\\AppData\\LocalLow\\DefaultCompany\\Platform Jump\\Saves");
        Debug.Log("test1: " + gameData["gameSave1.dat"].saveTime);
        menuItems.Add(new MenuItemData(1.1f, 2, "保存游戏1", () => gameSaveManager.SaveGame(1), "E:\\pics\\Picture\\1.png", gameDataManager.saveTime, gameDataManager.playTime));
        menuItems.Add(new MenuItemData(1.1f, 2, "保存游戏2", () => gameSaveManager.SaveGame(2), "E:\\pics\\Picture\\1.png", "2025/01/01", "25h"));
        menuItems.Add(new MenuItemData(1.1f, 2, "保存游戏3",null, "E:\\pics\\Picture\\1.png", "2025/01/01", "25h"));
        menuItems.Add(new MenuItemData(1.1f, 2, "保存游戏4",null, "E:\\pics\\Picture\\1.png", "2025/01/01", "25h"));
        menuItems.Add(new MenuItemData(1.1f, 2, "保存游戏5",null, "E:\\pics\\Picture\\1.png", "2025/01/01", "25h"));
        menuItems.Add(new MenuItemData(1.1f, 2, "保存游戏6",null, "E:\\pics\\Picture\\1.png", "2025/01/01", "25h"));
        menuItems.Add(new MenuItemData(1.1f, 2, "保存游戏7",null, "E:\\pics\\Picture\\1.png", "2025/01/01", "25h"));
        menuItems.Add(new MenuItemData(1.1f, 2, "保存游戏8",null, "E:\\pics\\Picture\\1.png", "2025/01/01", "25h"));
        menuItems.Add(new MenuItemData(1.1f, 2, "保存游戏9",null, "E:\\pics\\Picture\\1.png", "2025/01/01", "25h"));
        menuItems.Add(new MenuItemData(1.1f, 2, "保存游戏10",null, "E:\\pics\\Picture\\1.png", "2025/01/01", "25h"));
        
        menuItems.Add(new MenuItemData(1.2f, 2, "读取游戏1", () => gameLoadManager.LoadGame(1), "E:\\pics\\Picture\\1.png", gameDataManager.saveTime, gameDataManager.playTime));
        menuItems.Add(new MenuItemData(1.2f, 2, "读取游戏2", () => gameLoadManager.LoadGame(2), "E:\\pics\\Picture\\1.png", gameDataManager.saveTime, gameDataManager.playTime));
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
    }

    void SetupUIElements(GameObject menuItem, MenuItemData itemData)
    {
        ContentSizeFitter sizeFitter = menuItem.AddComponent<ContentSizeFitter>();                  //ContentSizeFitter 组件来自动调整宽度
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        HorizontalLayoutGroup itemLayout = menuItem.AddComponent<HorizontalLayoutGroup>();
        itemLayout.padding = new RectOffset(10, 10, 5, 5);
        itemLayout.spacing = 800f;                                                                  //标题和按钮之间创建空隙
        itemLayout.childControlWidth = true;
        itemLayout.childControlHeight = true;
        itemLayout.childForceExpandWidth = false;
        itemLayout.childForceExpandHeight = true;
        
        Transform titleTransform = menuItem.transform.Find("Title");// 查找 标题
        Transform infoTransform = menuItem.transform.Find("Info");// 查找 info
        Transform buttonTransform = menuItem.transform.Find("Button");// 查找 Button
        
        Button button = buttonTransform.GetComponent<Button>();
        buttonToMenuItemMap[button] = itemData; // 将按钮与菜单项数据关联
        
        if (itemData.buttonType == 2)
        {
            titleTransform.gameObject.SetActive(false);
            infoTransform.gameObject.SetActive(true);
            Transform picTransform = infoTransform.Find("screenshotImage");
            Image image = picTransform.GetComponent<Image>();
            
            Transform playTimeTransform = infoTransform.Find("playTime");
            TextMeshProUGUI playTime = playTimeTransform.GetComponent<TextMeshProUGUI>();
            
            Transform saveTimeTransform = infoTransform.Find("saveTime");
            TextMeshProUGUI saveTime = saveTimeTransform.GetComponent<TextMeshProUGUI>();

            LoadThumbnailAsync(image, itemData.screenshotImage);
            playTime.text = itemData.playTime;
            saveTime.text = itemData.saveTime;
            
            itemLayout.childAlignment = TextAnchor.MiddleLeft; // 标题不为空时，左对齐（标题在左，按钮在右）
            LayoutElement infoLayout = infoTransform.gameObject.AddComponent<LayoutElement>(); // 配置标题的布局元素 - 占据左侧空间
            infoLayout.flexibleWidth = 1f; // 标题占据剩余空间
            infoLayout.preferredWidth = -1f;
            infoLayout.minWidth = 50f;
            
            LayoutElement buttonLayout = buttonTransform.gameObject.AddComponent<LayoutElement>(); // 配置按钮的布局元素 - 固定宽度在右侧
            buttonLayout.preferredWidth = 120f;
            buttonLayout.minWidth = 80f;
            buttonLayout.preferredHeight = 40f;
            buttonLayout.minHeight = 30f;
            buttonLayout.flexibleWidth = 0f; // 按钮不拉伸
            
            // infoTransform.SetAsFirstSibling();
            // buttonTransform.SetAsLastSibling();
        }

        // 设置标题文本
        if (itemData.buttonType == 0)
        {
            infoTransform.gameObject.SetActive(false);
            titleTransform.gameObject.SetActive(false);
            itemLayout.childAlignment = TextAnchor.MiddleCenter; // 标题为空时，按钮居中
            LayoutElement buttonLayout = buttonTransform.gameObject.AddComponent<LayoutElement>(); // 配置按钮的布局元素
            buttonLayout.preferredWidth = 120f;
            buttonLayout.minWidth = 80f;
            buttonLayout.preferredHeight = 40f;
            buttonLayout.minHeight = 30f;
        }
        if (itemData.buttonType == 1)
        {
            infoTransform.gameObject.SetActive(false);
            titleTransform.gameObject.SetActive(true);
            TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
            titleText.text = itemData.title;
            itemLayout.childAlignment = TextAnchor.MiddleLeft; // 标题不为空时，左对齐（标题在左，按钮在右）
            LayoutElement titleLayout = titleTransform.gameObject.AddComponent<LayoutElement>(); // 配置标题的布局元素 - 占据左侧空间
            titleLayout.flexibleWidth = 1f; // 标题占据剩余空间
            titleLayout.preferredWidth = -1f;
            titleLayout.minWidth = 50f;

            LayoutElement buttonLayout = buttonTransform.gameObject.AddComponent<LayoutElement>(); // 配置按钮的布局元素 - 固定宽度在右侧
            buttonLayout.preferredWidth = 120f;
            buttonLayout.minWidth = 80f;
            buttonLayout.preferredHeight = 40f;
            buttonLayout.minHeight = 30f;
            buttonLayout.flexibleWidth = 0f; // 按钮不拉伸
            
            titleText.enableAutoSizing = true;// 设置标题文本自适应
            titleText.fontSizeMin = 12f;
            titleText.fontSizeMax = 24f;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.alignment = TextAlignmentOptions.Left;
            
            titleTransform.SetAsFirstSibling();
            buttonTransform.SetAsLastSibling();
        }

        if (!itemData.isOptionButton)
            button.onClick.AddListener(() => itemData.action.Invoke());

        // 设置按钮的RectTransform
        RectTransform buttonRect = buttonTransform.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(120f, 40f);

        // 设置按钮文本
        Transform buttonTextTransform = buttonTransform.Find("Text");
        TextMeshProUGUI buttonText = buttonTextTransform.GetComponent<TextMeshProUGUI>();
        buttonText.text = itemData.GetCurrentOptionText();
        buttonText.enableAutoSizing = true;
        buttonText.fontSizeMin = 10f;
        buttonText.fontSizeMax = 16f;
        buttonText.overflowMode = TextOverflowModes.Ellipsis;
        buttonText.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = buttonTextTransform.GetComponent<RectTransform>();     // 确保按钮文本填满整个按钮
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void HandleOptionChange(int direction)// 新增：处理左右方向键事件
    {
        Button currentButton = GetCurrentSelectedButton();// 获取当前选中的按钮
        if (currentButton == null || !buttonToMenuItemMap.ContainsKey(currentButton)) return;
        MenuItemData menuItem = buttonToMenuItemMap[currentButton];
        if (!menuItem.isOptionButton) return;
        
        int newIndex = (menuItem.currentOptionIndex + direction + menuItem.options.Count) % menuItem.options.Count;// 计算新的选项索引
        menuItem.SetOptionIndex(newIndex);// 使用SetOptionIndex方法更新索引并触发回调
        TextMeshProUGUI buttonText = currentButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = menuItem.GetCurrentOptionText();
    }

    void UpdateContentSize()
    {
        if (contentRectTransform == null) return;
        
        float totalHeight = menuItems.Count * (itemSize.y + itemSpacing) - itemSpacing;
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);
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
    
    public void LoadThumbnailAsync(Image thumbnailImage, string thumbnailPath)
    {
        StartCoroutine(LoadThumbnailCoroutine(thumbnailImage, thumbnailPath));
    }
    
    private IEnumerator LoadThumbnailCoroutine(Image thumbnailImage, string thumbnailPath)
    {
        Transform thumbnailTransform = thumbnailImage.transform;// 保存引用
        thumbnailImage.sprite = defaultThumbnail;// 先显示默认图片
    
        yield return null; // 等待一帧，确保默认图片已显示
    
        // 检查对象是否被销毁
        if (thumbnailImage == null || thumbnailTransform == null)
            yield break; // 静默退出，不打印日志
    
        if (!File.Exists(thumbnailPath))
            yield break;// 使用默认图片，不打印警告
        
        try
        {
            // 同步读取文件（适用于小文件）
            byte[] imageData = File.ReadAllBytes(thumbnailPath);
        
            // 再次检查对象是否存在
            if (thumbnailImage == null)
            {
                yield break;
            }
        
            // 创建纹理
            Texture2D texture = new Texture2D(2, 2);
        
            if (texture.LoadImage(imageData))
            {
                // 再次检查对象是否存在
                if (thumbnailImage == null)
                {
                    Destroy(texture);
                    yield break;
                }
            
                // 创建Sprite
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.Tight,
                    Vector4.zero,
                    false
                );
            
                // 赋值给Image
                thumbnailImage.sprite = sprite;
            
                // 清理旧纹理
                Sprite oldSprite = thumbnailImage.sprite;
                if (oldSprite != null && oldSprite.texture != texture && oldSprite != defaultThumbnail)
                {
                    Destroy(oldSprite.texture);
                }
            }
            else
            {
                Destroy(texture);
            }
        }
        catch (System.Exception)
        {
            // 读取失败，使用默认图片
            // 不打印错误，避免日志污染
        }
    }
}