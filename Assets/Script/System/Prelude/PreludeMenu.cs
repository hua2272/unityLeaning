using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class PreludeMenu : MonoBehaviour
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
    private List<MenuItemData> menuItems = new List<MenuItemData>();
    private List<Button> currentPanelButtons = new List<Button>();
    
    [Header("Scroll Settings")]
    private ScrollRect currentScrollRect;
    [SerializeField] private float thresholdTop = 5;                                                //滚动阈值（距离顶部/底部的按钮数量）
    [SerializeField] private float scrollStep = 100f;                                               //每次滚动的距离
    
    [Header("Navigation Style Settings")]
    [SerializeField] private float selectedOffset = 40f;                                            // 选中时向右偏移的距离
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.4f, 1f, 1f);        // 选中时的颜色
    [SerializeField] private Color normalColor = Color.white;                                    // 正常颜色
    [SerializeField] private FontWeight selectedFontWeight = FontWeight.Bold;                       // 选中时的字体粗细
    [SerializeField] private FontWeight normalFontWeight = FontWeight.Regular;                      // 正常字体粗细
    [SerializeField] private Color lightBarColor = new Color(0.2f, 0.4f, 1f, 0.3f);      // 光线条颜色
    [SerializeField] private Vector2 lightBarSize = new Vector2(300f, 10f);                         // 光线条尺寸
    
    [Header("字体自动调整设置")]
    [SerializeField] private float titleFontSizeMin = 12f;                                         // 标题最小字体大小
    [SerializeField] private float titleFontSizeMax = 24f;                                         // 标题最大字体大小
    [SerializeField] private float buttonFontSizeMin = 10f;                                        // 按钮最小字体大小
    [SerializeField] private float buttonFontSizeMax = 16f;                                        // 按钮最大字体大小
    
    [Header("按钮尺寸设置")]
    [SerializeField] private Vector2 buttonSize = new Vector2(120f, 40f);                               // 按钮尺寸
    
    [Header("布局设置")]
    [SerializeField] private float titleButtonSpacing = 800f;                                           // 标题和按钮之间的间距
    
    [Header("功能脚本")]
    private GameSaveManager gameSaveManager;
    private GameLoadManager gameLoadManager;
    private ScreenController screenController;
    private AudioManager audioManager;
    private PlayerInputManager playerInputManager;
    private GameDataManager gameDataManager;
    private UILangue uiLangue;
    
    private List<GameObject> createdMenuItems = new List<GameObject>();
    private RectTransform contentRectTransform;
    
    // 按钮与菜单项的映射
    private Dictionary<Button, MenuItemData> buttonToMenuItemMap = new Dictionary<Button, MenuItemData>();
    
    // 按钮样式数据存储
    private Dictionary<Button, (RectTransform rectTransform, TextMeshProUGUI text, GameObject lightBar, Vector2 originalPosition)> buttonStyleData = 
        new Dictionary<Button, (RectTransform, TextMeshProUGUI, GameObject, Vector2)>();
    
    // 主菜单容器（放在屏幕左下角）
    [SerializeField] private Transform mainMenuContainer;
    private List<GameObject> mainMenuItems = new List<GameObject>();
    
    void Start()
    {
        playerInputManager = PlayerInputManager.instance;
        gameSaveManager =  GameSaveManager.instance;
        gameLoadManager = GameLoadManager.instance;
        screenController = ScreenController.instance;
        audioManager = AudioManager.instance;
        gameDataManager = GameDataManager.instance;
        uiLangue = UILangue.instance;
        InitializeMenu();
        ShowButtons(0);
    }
    
    private void Update()
    {
        if (playerInputManager.isRebinding) return;// 如果正在重绑定，完全跳过所有输入处理
    
        if (playerInputManager.GetButtonDown("UICancel") && panel.activeSelf && currentPanelLevel != 0)
        {
            // 当返回主菜单时，重置菜单项的父级
            ResetMenuItemsParent();
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

        // 重置所有按钮样式
        ResetAllButtonStyles();

        // 计算当前面板的按钮数量
        int panelButtonCount = 0;
        List<int> panelItemIndices = new List<int>();

        // 先收集当前面板的所有菜单项索引
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (menuItems[i].PanelId == panelId)
            {
                panelButtonCount++;
                panelItemIndices.Add(i);
            }
        }

        // 遍历当前面板的菜单项（按原顺序）
        for (int listIndex = 0; listIndex < panelItemIndices.Count; listIndex++)
        {
            int itemIndex = panelItemIndices[listIndex];
            GameObject menuItem = createdMenuItems[itemIndex];

            menuItem.SetActive(true);

            // 主菜单（面板0）特殊处理 - 所有按钮作为一个整体放在左下角
            if (panelId == 0)
            {
                // 使用主菜单容器
                if (mainMenuContainer != null)
                {
                    menuItem.transform.SetParent(mainMenuContainer, false);

                    RectTransform rectTransform = menuItem.GetComponent<RectTransform>();

                    // 设置锚点在左下角
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.pivot = new Vector2(0f, 0f);

                    // 计算反向索引：最后一个按钮在底部（索引为0），第一个按钮在顶部
                    int reverseIndex = panelItemIndices.Count - 1 - listIndex;

                    // 计算y位置：从底部开始向上排列
                    float yPosition = reverseIndex * (itemSize.y + itemSpacing);

                    rectTransform.anchoredPosition = new Vector2(selectedOffset, yPosition);

                    mainMenuItems.Add(menuItem);
                }
                else
                {
                    // 如果没有主菜单容器，直接放在panel下
                    menuItem.transform.SetParent(panel.transform, false);

                    RectTransform rectTransform = menuItem.GetComponent<RectTransform>();

                    // 设置锚点在左下角
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.pivot = new Vector2(0f, 0f);

                    // 计算反向索引
                    int reverseIndex = panelItemIndices.Count - 1 - listIndex;

                    // 从屏幕左下角开始向上排列
                    float yPosition = reverseIndex * (itemSize.y + itemSpacing);
                    rectTransform.anchoredPosition = new Vector2(selectedOffset, yPosition);

                    mainMenuItems.Add(menuItem);
                }

                // 获取按钮组件并添加到当前面板按钮列表
                Button button = menuItem.GetComponentInChildren<Button>();
                if (button != null)
                {
                    // 保持按钮在列表中的顺序不变（第一个按钮在currentPanelButtons[0]）
                    currentPanelButtons.Add(button);

                    // 如果是选项按钮，确保文本是最新的
                    if (menuItems[itemIndex].isOptionButton)
                    {
                        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = menuItems[itemIndex].GetCurrentOptionText();
                        }
                    }

                    // 存储按钮的原始位置（用于样式效果）
                    if (!buttonStyleData.ContainsKey(button))
                    {
                        RectTransform btnRect = button.GetComponent<RectTransform>();
                        TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
                        GameObject lightBar = CreateLightBar(btnRect);

                        buttonStyleData[button] = (btnRect, btnText, lightBar, btnRect.anchoredPosition);
                    }
                }
            }
            else
            {
                // 其他面板使用原来的滚动布局
                menuItem.transform.SetParent(contentParent, false);

                RectTransform rectTransform = menuItem.GetComponent<RectTransform>();
                // 设置滚动布局的锚点
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);

                float yPosition = -listIndex * (itemSize.y + itemSpacing) - (itemSize.y * 0.5f);
                rectTransform.anchoredPosition = new Vector2(0, yPosition);

                // 获取按钮组件并添加到当前面板按钮列表
                Button button = menuItem.GetComponentInChildren<Button>();
                if (button != null)
                {
                    currentPanelButtons.Add(button);

                    // 如果是选项按钮，确保文本是最新的
                    if (menuItems[itemIndex].isOptionButton)
                    {
                        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = menuItems[itemIndex].GetCurrentOptionText();
                        }
                    }

                    // 存储按钮的原始位置（用于样式效果）
                    if (!buttonStyleData.ContainsKey(button))
                    {
                        RectTransform btnRect = button.GetComponent<RectTransform>();
                        TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
                        GameObject lightBar = CreateLightBar(btnRect);

                        buttonStyleData[button] = (btnRect, btnText, lightBar, btnRect.anchoredPosition);
                    }
                }
            }
        }

        // 隐藏不属于当前面板的菜单项
        for (int i = 0; i < createdMenuItems.Count; i++)
        {
            if (!panelItemIndices.Contains(i))
            {
                createdMenuItems[i].SetActive(false);
            }
        }

        // 设置当前滚动区域（非主菜单时）
        if (panelId != 0)
        {
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
        }
        else
        {
            currentScrollRect = null; // 主菜单不需要滚动

            // 调整主菜单容器的位置
            if (mainMenuContainer != null)
            {
                AdjustMainMenuContainerPosition();
            }
        }

        // 更新按钮选中状态
        UpdateButtonSelection();
        // 调试信息
        Debug.Log($"显示面板 {panelId}，找到 {currentPanelButtons.Count} 个按钮");
    }
    
    private void AdjustMainMenuContainerPosition()
    {
        if (mainMenuContainer != null && currentPanelLevel == 0)
        {
            RectTransform containerRect = mainMenuContainer.GetComponent<RectTransform>();
        
            // 将主菜单容器锚点设置为左下角
            containerRect.anchorMin = new Vector2(0f, 0f);
            containerRect.anchorMax = new Vector2(0f, 0f);
            containerRect.pivot = new Vector2(0f, 0f);
        
            // 设置容器位置在左下角，可以留一些边距
            containerRect.anchoredPosition = new Vector2(20f, 20f);
        
            // 计算容器大小
            float totalHeight = currentPanelButtons.Count * (itemSize.y + itemSpacing) - itemSpacing;
            containerRect.sizeDelta = new Vector2(itemSize.x + selectedOffset, totalHeight);
        }
    }
    
    private void UpdateButtonSelection()
    {
        // 重置所有按钮样式
        ResetCurrentPanelButtonStyles();
        
        // 设置当前选中按钮的样式
        Button currentButton = GetCurrentSelectedButton();
        if (currentButton != null && currentButton.interactable)
        {
            SetButtonSelectedStyle(currentButton, true);
        }
    }
    
    private void ResetCurrentPanelButtonStyles()
    {
        foreach (var button in currentPanelButtons)
        {
            SetButtonSelectedStyle(button, false);
        }
    }
    
    private void ResetAllButtonStyles()
    {
        foreach (var button in currentPanelButtons)
        {
            if (buttonStyleData.ContainsKey(button))
            {
                SetButtonSelectedStyle(button, false);
            }
        }
    }
    
    private void SetButtonSelectedStyle(Button button, bool selected)
    {
        if (buttonStyleData.ContainsKey(button))
        {
            var (rectTransform, text, lightBar, originalPosition) = buttonStyleData[button];
            if (selected)
            {
                // 更新文字样式
                if (text != null)
                {
                    text.color = selectedColor;
                    text.fontWeight = selectedFontWeight;
                }
                // 显示光线条
                if (lightBar != null)
                    lightBar.SetActive(true);
            }
            else
            {
                // 更新文字样式
                if (text != null)
                {
                    text.color = normalColor;
                    text.fontWeight = normalFontWeight;
                }
                // 隐藏光线条
                if (lightBar != null)
                    lightBar.SetActive(false);
            }
            // 移除按钮的边框 - 设置颜色为透明
            ColorBlock colors = button.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.1f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
            colors.selectedColor = Color.clear;
            colors.disabledColor = Color.clear;
            button.colors = colors;
        }
    }
    
    private GameObject CreateLightBar(RectTransform buttonTransform)
    {
        // 创建光线条对象
        GameObject lightBar = new GameObject("LightBar");
        lightBar.transform.SetParent(buttonTransform);
        
        // 添加Image组件
        Image image = lightBar.AddComponent<Image>();
        image.color = lightBarColor;
        
        // 设置RectTransform
        RectTransform lightBarTransform = lightBar.GetComponent<RectTransform>();
        lightBarTransform.sizeDelta = lightBarSize;
        lightBarTransform.anchorMin = new Vector2(0f, 0.5f);
        lightBarTransform.anchorMax = new Vector2(0f, 0.5f);
        lightBarTransform.pivot = new Vector2(0f, 0.5f);
        
        // 将光线条放置在文字后方
        lightBarTransform.SetAsFirstSibling();
        
        // 设置位置在按钮左侧
        lightBarTransform.anchoredPosition = new Vector2(-20f, 0f);
        
        // 默认隐藏
        lightBar.SetActive(false);
        
        return lightBar;
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
        string latestSaveFiles = gameSaveManager.GetLatestSaveFiles();
        if (latestSaveFiles != "")
        {
            menuItems.Add(new MenuItemData(0, null, 0, null, "继续游戏",  () => gameLoadManager.LoadLatestGame(latestSaveFiles)));
        }
        menuItems.Add(new MenuItemData(0, null, 0, null, "新游戏", null));
        menuItems.Add(new MenuItemData(0, null, 0, null, "读取存档", () =>
        {
            currentPanelLevel = 1;
            ShowButtons(1.2f);
        }));
        menuItems.Add(new MenuItemData(0, null, 0, null, "设置", () =>
        {
            currentPanelLevel = 1;
            ShowButtons(1);
        }));
        menuItems.Add(new MenuItemData(0, null, 0, null, "退出游戏", gameLoadManager.OnQuitClicked));
        
        menuItems.Add(new MenuItemData(1, null, 0, null, "键盘按键设置", null));
        menuItems.Add(new MenuItemData(1, "screen", 1, "屏幕", new List<string> {"无边框全屏", "窗口化"}, 0, (index) => screenController.ScreenModeChange(index), "ScreenMode"));
        menuItems.Add(new MenuItemData(1, "BGM", 1, "音乐", new List<string> {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}, 4, (index) => audioManager.SetMusicVolume(index), "MusicVolume"));
        menuItems.Add(new MenuItemData(1, "SFX", 1, "音效", new List<string> {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}, 4, (index) => audioManager.SetSFXVolume(index), "SFXVolume"));
        menuItems.Add(new MenuItemData(1, "langue", 1, "语言", new List<string> {"中文", "English", "日本語"}, 0, null, "Langue"));

        List<string> allSaveFiles = GameSaveManager.GetAllSaveFiles();
        for (var i = 0; i < allSaveFiles.Count; i++)
        {
            int currentIndex = i;// 创建局部变量来捕获当前循环的值
            string screenshotPath = Path.Combine(GameSaveManager.GetParentSavePath(), $"screenshot{currentIndex}.png");
            menuItems.Add(new MenuItemData(1.2f, 2, "读取游戏", () => gameLoadManager.LoadGame(currentIndex), screenshotPath, gameDataManager.saveTime));
        }
    }
    
    void ClearMenuItems()
    {
        buttonToMenuItemMap.Clear();
        buttonStyleData.Clear();
        
        foreach (var item in createdMenuItems)
        {
            Destroy(item);
        }
        createdMenuItems.Clear();
        
        foreach (var item in mainMenuItems)
        {
            Destroy(item);
        }
        mainMenuItems.Clear();
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
    
        // 默认使用顶部居中的锚点（用于滚动面板）
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.sizeDelta = itemSize;
    
        // 计算位置（主菜单会在ShowButtons中重新定位）
        float yPosition = -index * (itemSize.y + itemSpacing) - (itemSize.y * 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, yPosition);
    }

    void SetupUIElements(GameObject menuItem, MenuItemData itemData)
    {
        ContentSizeFitter sizeFitter = menuItem.AddComponent<ContentSizeFitter>();                  //ContentSizeFitter 组件来自动调整宽度
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        HorizontalLayoutGroup itemLayout = menuItem.AddComponent<HorizontalLayoutGroup>();
        itemLayout.padding = new RectOffset(10, 10, 5, 5);
        itemLayout.spacing = titleButtonSpacing;                                                                  //标题和按钮之间创建空隙
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
            
            Transform saveTimeTransform = infoTransform.Find("saveTime");
            TextMeshProUGUI saveTime = saveTimeTransform.GetComponent<TextMeshProUGUI>();
            
            RectTransform imageRect = picTransform.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;      // 左下角
            imageRect.anchorMax = Vector2.one;        // 右上角
            imageRect.offsetMin = Vector2.zero;       // 左下的偏移为0
            imageRect.offsetMax = Vector2.zero;       // 右上的偏移为0
            imageRect.pivot = new Vector2(0.5f, 0.5f); // 中心点
            AspectRatioFitter aspectFitter = picTransform.gameObject.AddComponent<AspectRatioFitter>();// 需要保持图片的宽高比
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = 16f / 9f; // 根据你的截图比例调整
            
            // 设置第二个文本的位置（中间偏右）
            RectTransform saveTimeRect = saveTimeTransform.GetComponent<RectTransform>();
            saveTimeRect.anchorMin = new Vector2(0.4f, 0.4f); // 水平居中，垂直偏下
            saveTimeRect.anchorMax = new Vector2(0.8f, 0.6f);
            saveTimeRect.pivot = new Vector2(0, 0.5f);
            saveTimeRect.offsetMin = new Vector2(10, 0);
            saveTimeRect.offsetMax = new Vector2(-10, 0);

            LoadThumbnailAsync(image, itemData.screenshotImage);
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
            
            picTransform.SetAsFirstSibling();
            buttonTransform.SetAsLastSibling();
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
        if (itemData.buttonType == 1 || itemData.buttonType == 3)
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
            titleText.fontSizeMin = titleFontSizeMin;
            titleText.fontSizeMax = titleFontSizeMax;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.alignment = TextAlignmentOptions.Left;
            
            titleTransform.SetAsFirstSibling();
            buttonTransform.SetAsLastSibling();
        }

        if (!itemData.isOptionButton)
            button.onClick.AddListener(() => itemData.action.Invoke());

        // 设置按钮的RectTransform
        RectTransform buttonRect = buttonTransform.GetComponent<RectTransform>();
        buttonRect.sizeDelta = buttonSize;

        // 设置按钮文本
        Transform buttonTextTransform = buttonTransform.Find("Text");
        TextMeshProUGUI buttonText = buttonTextTransform.GetComponent<TextMeshProUGUI>();
        buttonText.text = itemData.GetCurrentOptionText();
        buttonText.enableAutoSizing = true;
        buttonText.fontSizeMin = buttonFontSizeMin;
        buttonText.fontSizeMax = buttonFontSizeMax;
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
    
    private void ResetMenuItemsParent()
    {
        // 将所有菜单项移回contentParent，以便在下次ShowButtons时正确设置位置
        foreach (var menuItem in createdMenuItems)
        {
            menuItem.transform.SetParent(contentParent, false);
        }
    
        // 清空主菜单项列表
        mainMenuItems.Clear();
    }
}