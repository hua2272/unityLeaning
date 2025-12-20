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
    [SerializeField] private float thresholdTop = 5;                                 //滚动阈值（距离顶部/底部的按钮数量）
    [SerializeField] private float scrollStep = 100f;                                //每次滚动的距离
    
    [Header("Navigation Style Settings")]
    [SerializeField] private float selectedOffset = 30f;                            // 选中时向右偏移的距离
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.4f, 1f, 1f);   // 选中时的颜色
    [SerializeField] private Color normalColor = Color.white;                       // 正常颜色
    [SerializeField] private FontWeight selectedFontWeight = FontWeight.Bold;       // 选中时的字体粗细
    [SerializeField] private FontWeight normalFontWeight = FontWeight.Regular;      // 正常字体粗细
    [SerializeField] private Color lightBarColor = new Color(0.2f, 0.4f, 1f, 0.3f); // 光线条颜色
    [SerializeField] private Vector2 lightBarSize = new Vector2(200f, 10f);         // 光线条尺寸
    
    [Header("功能脚本")]
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

                // 主菜单（面板0）特殊处理 - 所有按钮都放在左下角
                if (panelId == 0)
                {
                    // 设置父级为panel，而不是contentParent
                    menuItem.transform.SetParent(panel.transform, false);

                    RectTransform rectTransform = menuItem.GetComponent<RectTransform>();

                    // 确保锚点在左下角
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.pivot = new Vector2(0f, 0f);

                    // 从屏幕左下角开始排列，向上排列
                    float yPosition = buttonIndex * (itemSize.y + itemSpacing);
                    rectTransform.anchoredPosition = new Vector2(selectedOffset, yPosition);

                    mainMenuItems.Add(menuItem);
                }
                else
                {
                    // 其他面板使用原来的滚动布局
                    // 需要将菜单项移回contentParent
                    menuItem.transform.SetParent(contentParent, false);

                    RectTransform rectTransform = menuItem.GetComponent<RectTransform>();
                    // 设置滚动布局的锚点
                    rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    rectTransform.pivot = new Vector2(0.5f, 1f);

                    float yPosition = -buttonIndex * (itemSize.y + itemSpacing) - (itemSize.y * 0.5f);
                    rectTransform.anchoredPosition = new Vector2(0, yPosition);
                }

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
                menuItem.SetActive(false);
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
        }

        // 更新按钮选中状态
        UpdateButtonSelection();
        // 调试信息
        Debug.Log($"显示面板 {panelId}，找到 {currentPanelButtons.Count} 个按钮");
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
        menuItems.Add(new MenuItemData(0, 0, null, "继续游戏", null));
        menuItems.Add(new MenuItemData(0, 0, null, "读取存档", () =>
        {
            currentPanelLevel = 1;
            ShowButtons(1.2f);
        }));
        menuItems.Add(new MenuItemData(0, 0, null, "设置", () =>
        {
            currentPanelLevel = 1;
            ShowButtons(1);
        }));
        menuItems.Add(new MenuItemData(1, 0, null, "键盘按键设置", null));
        menuItems.Add(new MenuItemData(1, 1, "屏幕", new List<string> {"无边框全屏", "窗口化"}, 0, (index) => screenController.ScreenModeChange(index), "ScreenMode"));
        menuItems.Add(new MenuItemData(1, 1, "音乐", new List<string> {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}, 4, (index) => audioManager.SetMusicVolume(index), "MusicVolume"));
        menuItems.Add(new MenuItemData(1, 1, "音效", new List<string> {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}, 4, (index) => audioManager.SetSFXVolume(index), "SFXVolume"));
        menuItems.Add(new MenuItemData(1, 1, "语言", new List<string> {"中文", "English", "日本語"}, 1, (index) => uiLangue.ChangeLanguage(index), "Langue"));
        
        menuItems.Add(new MenuItemData(1.2f, 2, "读取游戏1", () => gameLoadManager.LoadGame(1), "E:\\pics\\Picture\\1.png", gameDataManager.saveTime, gameDataManager.playTime));
        menuItems.Add(new MenuItemData(1.2f, 2, "读取游戏2", () => gameLoadManager.LoadGame(2), "E:\\pics\\Picture\\1.png", gameDataManager.saveTime, gameDataManager.playTime));
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
    
        // 默认使用左下角的锚点，这样方便主菜单布局
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0f, 0f);
        rectTransform.sizeDelta = itemSize;
    
        // 暂时设置一个默认位置，实际位置会在ShowButtons中调整
        rectTransform.anchoredPosition = new Vector2(0, 0);
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
            
            RectTransform imageRect = picTransform.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;      // 左下角
            imageRect.anchorMax = Vector2.one;        // 右上角
            imageRect.offsetMin = Vector2.zero;       // 左下的偏移为0
            imageRect.offsetMax = Vector2.zero;       // 右上的偏移为0
            imageRect.pivot = new Vector2(0.5f, 0.5f); // 中心点
            AspectRatioFitter aspectFitter = picTransform.gameObject.AddComponent<AspectRatioFitter>();// 需要保持图片的宽高比
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = 16f / 9f; // 根据你的截图比例调整
            
            // 设置第一个文本的位置（中间偏左）
            RectTransform playTimeRect = playTimeTransform.GetComponent<RectTransform>();
            playTimeRect.anchorMin = new Vector2(0.4f, 0.6f); // 水平居中，垂直偏上
            playTimeRect.anchorMax = new Vector2(0.8f, 0.8f);
            playTimeRect.pivot = new Vector2(0, 0.5f);
            playTimeRect.offsetMin = new Vector2(10, 0);
            playTimeRect.offsetMax = new Vector2(-10, 0);
    
            // 设置第二个文本的位置（中间偏右）
            RectTransform saveTimeRect = saveTimeTransform.GetComponent<RectTransform>();
            saveTimeRect.anchorMin = new Vector2(0.4f, 0.4f); // 水平居中，垂直偏下
            saveTimeRect.anchorMax = new Vector2(0.8f, 0.6f);
            saveTimeRect.pivot = new Vector2(0, 0.5f);
            saveTimeRect.offsetMin = new Vector2(10, 0);
            saveTimeRect.offsetMax = new Vector2(-10, 0);

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