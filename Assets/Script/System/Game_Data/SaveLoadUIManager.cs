using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.IO;

public class SaveLoadUIManager : MonoBehaviour
{
    public static SaveLoadUIManager instance;

    [Header("UI配置")] [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float itemSpacing = 10f;
    [SerializeField] private Vector2 itemSize = new Vector2(800, 150);
    [SerializeField] private GameObject saveLoadPanel;

    [Header("UI元素")] [SerializeField] private TextMeshProUGUI panelTitle;

    [Header("按钮文本")] [SerializeField] private string saveButtonText = "保存";
    [SerializeField] private string loadButtonText = "读取";
    [SerializeField] private string deleteButtonText = "删除";
    [SerializeField] private string emptySlotText = "空存档槽";

    [Header("颜色设置")] [SerializeField] private Color selectedBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color normalBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
    [SerializeField] private Color selectedButtonColor = Color.yellow;
    [SerializeField] private Color normalButtonColor = Color.white;

    // 存档槽位UI元素引用
    private class SaveSlotElements
    {
        public GameObject slotObject;
        public Button actionButton;
        public Button deleteButton;
        public TextMeshProUGUI slotNumberText;
        public TextMeshProUGUI sceneNameText;
        public TextMeshProUGUI playerInfoText;
        public TextMeshProUGUI saveTimeText;
        public TextMeshProUGUI playTimeText;
        public RawImage screenshotImage;
        public TextMeshProUGUI actionButtonText;
        public TextMeshProUGUI deleteButtonText;
        public Image backgroundImage;

        public int slotId;
    }

    private List<SaveSlotElements> saveSlots = new List<SaveSlotElements>();
    private int currentSelectedSlot = 0;
    private bool isSaveMode = true; // true=保存模式, false=读取模式

    // 功能脚本引用
    private GameSaveManager gameSaveManager;
    private GameLoadManager gameLoadManager;
    private AudioManager audioManager;
    private PlayerInputManager playerInputManager;

    // 滚动相关
    private RectTransform contentRectTransform;
    [SerializeField] private float scrollStep = 150f;
    [SerializeField] private float thresholdTop = 2;

    void Awake()
    {
        Debug.Log("<color=#FF0000>-------SaveLoadUIManager instance-------</color>");
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerInputManager = PlayerInputManager.instance;
        gameSaveManager = GameSaveManager.instance;
        gameLoadManager = GameLoadManager.instance;
        audioManager = AudioManager.instance;

        InitializeUI();
        RefreshSaveSlots();
    }

    void Update()
    {
        if (!saveLoadPanel.activeSelf || playerInputManager.isRebinding) return;

        HandleKeyboardNavigation();

        if (playerInputManager.GetButtonDown("UICancel"))
        {
            ClosePanel();
        }
    }

    private void InitializeUI()
    {
        contentRectTransform = contentParent.GetComponent<RectTransform>();

        // 清除现有槽位
        foreach (Transform child in contentParent)
        {
            if (child != null)
                Destroy(child.gameObject);
        }

        saveSlots.Clear();

        // 创建10个存档槽位UI
        for (int i = 0; i < GameSaveManager.MAX_SAVE_SLOTS; i++)
        {
            GameObject slotObj = Instantiate(saveSlotPrefab, contentParent);
            slotObj.name = $"SaveSlot_{i}";

            // 获取所有UI组件引用
            SaveSlotElements slotElements = new SaveSlotElements
            {
                slotObject = slotObj,
                slotId = i
            };
            
            // 设置RectTransform - 关键修改部分
            Transform canvas = slotObj.transform.Find("Canvas");
            RectTransform rectTransform = canvas.GetComponentInChildren<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchorMin = new Vector2(0.5f, 1f);                                // 使用顶部居中的锚点
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.sizeDelta = itemSize; 
            float yPosition = -i * (itemSize.y + itemSpacing) - (itemSize.y * 0.5f);    // 第一个菜单项应该在Content顶部，后续项依次向下
            rectTransform.anchoredPosition = new Vector2(0, yPosition);

            // 获取背景Image
            slotElements.backgroundImage = canvas.Find("Background").GetComponent<Image>();
            
            Transform actionButtonTransform = canvas.Find("ActionButton");// 获取按钮
            slotElements.actionButton = actionButtonTransform.GetComponent<Button>();
            Transform buttonTextTransform = actionButtonTransform.Find("Text");// 获取按钮文本
            slotElements.actionButtonText = buttonTextTransform.GetComponent<TextMeshProUGUI>();
            
            Transform deleteButtonTransform = canvas.Find("DeleteButton");
            slotElements.deleteButton = deleteButtonTransform.GetComponent<Button>();
            Transform deleteTextTransform = deleteButtonTransform.Find("Text");// 获取删除按钮文本
            slotElements.deleteButtonText = deleteTextTransform.GetComponent<TextMeshProUGUI>();
        }
        //UpdateContentSize();// 更新内容区域大小
    }

    private void RefreshSaveSlots()
    {
        List<SaveSlotInfo> saveInfos = gameSaveManager.GetAllSaveSlotsInfo();

        for (int i = 0; i < saveSlots.Count; i++)
        {
            SaveSlotElements slot = saveSlots[i];

            if (i < saveInfos.Count)
            {
                SaveSlotInfo info = saveInfos[i];

                if (info.exists)
                {
                    // 有存档数据
                    UpdateSlotUI(slot,
                        $"存档 {info.slotId + 1}",
                        info.scene,
                        $"等级: {info.playerLevel}",
                        info.saveTime,
                        $"游戏时间: {FormatPlayTime(info.playTime)}",
                        info.screenshot
                    );

                    // 设置按钮文本
                    SetActionButtonText(slot, isSaveMode ? saveButtonText : loadButtonText);
                    SetDeleteButtonActive(slot, true);
                    SetActionButtonInteractable(slot, true);
                }
                else
                {
                    // 空槽位
                    UpdateSlotUI(slot,
                        $"存档 {info.slotId + 1}",
                        emptySlotText,
                        "",
                        "",
                        "",
                        null
                    );

                    // 只有保存模式才允许在空槽位操作
                    if (isSaveMode)
                    {
                        SetActionButtonText(slot, saveButtonText);
                        SetActionButtonInteractable(slot, true);
                    }
                    else
                    {
                        SetActionButtonText(slot, loadButtonText);
                        SetActionButtonInteractable(slot, false);
                    }

                    SetDeleteButtonActive(slot, false);
                }
            }
        }

        // 更新选中状态
        UpdateSlotSelection();
    }

    private void UpdateSlotUI(SaveSlotElements slot,
        string slotNumber,
        string sceneName,
        string playerInfo,
        string saveTime,
        string playTime,
        Texture2D screenshot)
    {
        if (slot.slotNumberText != null) slot.slotNumberText.text = slotNumber;
        if (slot.sceneNameText != null) slot.sceneNameText.text = sceneName;
        if (slot.playerInfoText != null) slot.playerInfoText.text = playerInfo;
        if (slot.saveTimeText != null) slot.saveTimeText.text = saveTime;
        if (slot.playTimeText != null) slot.playTimeText.text = playTime;

        if (slot.screenshotImage != null)
        {
            if (screenshot != null)
            {
                slot.screenshotImage.texture = screenshot;
                slot.screenshotImage.gameObject.SetActive(true);
            }
            else
            {
                slot.screenshotImage.gameObject.SetActive(false);
            }
        }
    }

    private void SetActionButtonText(SaveSlotElements slot, string text)
    {
        if (slot.actionButtonText != null)
        {
            slot.actionButtonText.text = text;
        }
    }

    private void SetActionButtonInteractable(SaveSlotElements slot, bool interactable)
    {
        if (slot.actionButton != null)
        {
            slot.actionButton.interactable = interactable;

            // 更新按钮颜色
            var colors = slot.actionButton.colors;
            colors.normalColor = interactable ? normalButtonColor : Color.gray;
            slot.actionButton.colors = colors;
        }
    }

    private void SetDeleteButtonActive(SaveSlotElements slot, bool active)
    {
        if (slot.deleteButton != null)
        {
            slot.deleteButton.gameObject.SetActive(active);
        }
    }

    private string FormatPlayTime(int seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    public void OpenSavePanel()
    {
        isSaveMode = true;
        panelTitle.text = "保存游戏";
        saveLoadPanel.SetActive(true);
        Time.timeScale = 0;
        currentSelectedSlot = 0;
        RefreshSaveSlots();
        UpdateSlotSelection();

        // 滚动到顶部
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public void OpenLoadPanel()
    {
        isSaveMode = false;
        panelTitle.text = "读取游戏";
        saveLoadPanel.SetActive(true);
        Time.timeScale = 0;
        currentSelectedSlot = 0;
        RefreshSaveSlots();
        UpdateSlotSelection();

        // 滚动到顶部
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void ClosePanel()
    {
        saveLoadPanel.SetActive(false);
        Time.timeScale = 1;
        audioManager.PlayUISound(UISoundType.Navigate);
    }

    private void OnSlotActionButtonClicked(int slotId)
    {
        audioManager.PlayUISound(UISoundType.Confirm);

        if (isSaveMode)
        {
            // 保存游戏
            gameSaveManager.SaveGame(slotId);
            Debug.Log($"游戏已保存到槽位 {slotId}");

            // 刷新显示
            RefreshSaveSlots();

            // 可以添加保存成功的提示
            ShowMessage($"已保存到存档 {slotId + 1}");
        }
        else
        {
            // 加载游戏
            List<SaveSlotInfo> saveInfos = gameSaveManager.GetAllSaveSlotsInfo();
            if (slotId < saveInfos.Count && saveInfos[slotId].exists)
            {
                // 设置当前存档槽位
                GameDataManager.instance.SetCurrentSaveSlot(slotId);

                // 关闭面板
                ClosePanel();

                // 加载游戏
                gameLoadManager.LoadGame(slotId);
            }
        }
    }

    private void OnDeleteButtonClicked(int slotId)
    {
        audioManager.PlayUISound(UISoundType.Navigate);

        // 显示确认对话框
        // 这里可以添加一个确认对话框
        // 暂时直接删除
        gameSaveManager.DeleteSave(slotId);
        RefreshSaveSlots();

        ShowMessage($"已删除存档 {slotId + 1}");
    }

    private void ShowMessage(string message)
    {
        // 这里可以添加一个消息提示UI
        Debug.Log(message);
    }

    private void HandleKeyboardNavigation()
    {
        if (playerInputManager.GetButtonDown("UIUp"))
        {
            audioManager.PlayUISound(UISoundType.Navigate);
            currentSelectedSlot--;
            if (currentSelectedSlot < 0)
                currentSelectedSlot = saveSlots.Count - 1;

            UpdateSlotSelection();
            HandleScroll(currentSelectedSlot, true);
        }
        else if (playerInputManager.GetButtonDown("UIDown"))
        {
            audioManager.PlayUISound(UISoundType.Navigate);
            currentSelectedSlot++;
            if (currentSelectedSlot >= saveSlots.Count)
                currentSelectedSlot = 0;

            UpdateSlotSelection();
            HandleScroll(currentSelectedSlot, false);
        }
        else if (playerInputManager.GetButtonDown("UIConfirm"))
        {
            if (currentSelectedSlot >= 0 && currentSelectedSlot < saveSlots.Count)
            {
                SaveSlotElements slot = saveSlots[currentSelectedSlot];
                if (slot.actionButton != null && slot.actionButton.interactable)
                {
                    slot.actionButton.onClick.Invoke();
                }
            }
        }
        else if (playerInputManager.GetButtonDown("UILeft") || playerInputManager.GetButtonDown("UIRight"))
        {
            // 左右键可以切换到删除按钮（如果有）
            if (currentSelectedSlot >= 0 && currentSelectedSlot < saveSlots.Count)
            {
                SaveSlotElements slot = saveSlots[currentSelectedSlot];
                if (slot.deleteButton != null && slot.deleteButton.gameObject.activeSelf &&
                    slot.deleteButton.interactable)
                {
                    slot.deleteButton.onClick.Invoke();
                }
            }
        }
    }

    private void UpdateSlotSelection()
    {
        for (int i = 0; i < saveSlots.Count; i++)
        {
            SaveSlotElements slot = saveSlots[i];

            // 更新背景颜色来表示选中状态
            if (slot.backgroundImage != null)
            {
                slot.backgroundImage.color =
                    (i == currentSelectedSlot) ? selectedBackgroundColor : normalBackgroundColor;
            }

            // 更新按钮颜色
            if (slot.actionButton != null)
            {
                var colors = slot.actionButton.colors;
                colors.normalColor = (i == currentSelectedSlot && slot.actionButton.interactable)
                    ? selectedButtonColor
                    : normalButtonColor;
                slot.actionButton.colors = colors;
            }
        }
    }

    private void HandleScroll(int buttonIndex, bool isUpward)
    {
        if (scrollRect == null || saveSlots.Count == 0) return;

        int buttonCount = saveSlots.Count;
        float thresholdBottom = buttonCount - thresholdTop;

        if (isUpward && (buttonIndex < thresholdTop || buttonIndex > thresholdBottom)) return;
        if (!isUpward && (buttonIndex < thresholdTop || buttonIndex > thresholdBottom)) return;

        ScrollContent(isUpward);
    }

    private void ScrollContent(bool scrollUp)
    {
        float currentPosition = scrollRect.verticalNormalizedPosition;
        RectTransform content = scrollRect.content;
        float contentHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        if (contentHeight <= viewportHeight) return;

        float scrollAmount = scrollStep / (contentHeight - viewportHeight);

        if (scrollUp)
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(currentPosition + scrollAmount);
        else
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(currentPosition - scrollAmount);
    }

    private void UpdateContentSize()
    {
        if (contentRectTransform == null) return;

        float totalHeight = saveSlots.Count * (itemSize.y + itemSpacing) - itemSpacing;
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);
    }
}