using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenu;          // 主菜单面板
    public GameObject inventoryContent;  // 背包内容
    public GameObject questContent;      // 任务内容
    public GameObject mapContent;        // 地图内容
    public GameObject settingsContent;   // 设置内容
    
    [Header("Tab Buttons")]
    public Button inventoryBtn;
    public Button questBtn;
    public Button mapBtn;
    public Button settingsBtn;
    
    private GameObject currentActiveContent; // 当前显示的内容面板
    private Dictionary<Button, GameObject> buttonContentMap;
    
    private bool isMenuOpen = false;
    
    
    private void Start()
    {
        HideAllContent();
        inventoryContent.SetActive(true);
        print("inventory open");
        // 绑定按钮事件
        inventoryBtn.onClick.AddListener(() => ShowContent(inventoryContent));
        questBtn.onClick.AddListener(() => ShowContent(questContent));
        mapBtn.onClick.AddListener(() => ShowContent(mapContent));
        settingsBtn.onClick.AddListener(() => ShowContent(settingsContent));

        //inventoryBtn.interactable = true; // 强制激活交互
    }

    private void Update()
    {
        //Debug.Log($"按钮可交互状态: {questBtn.interactable}");
        //Debug.Log($"按钮监听器数量: {inventoryBtn.onClick.GetPersistentEventCount()}");
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        mainMenu.SetActive(isMenuOpen);
        Time.timeScale = isMenuOpen ? 0f : 1f; // 暂停游戏逻辑（可选）
        Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked; // 锁定/解锁鼠标（可选）
        Cursor.visible = isMenuOpen;
    }

    private void ShowContent(GameObject contentToShow)
    {
        HideAllContent();               // 隐藏所有内容
        contentToShow.SetActive(true);  // 显示选中的内容
        print(contentToShow + " has been shown");
    }

    private void HideAllContent()
    {
        inventoryContent.SetActive(false);
        questContent.SetActive(false);
        mapContent.SetActive(false);
        settingsContent.SetActive(false);
    }
    
    
    
    private void OnTabButtonClick(Button clickedButton)
    {
        // 隐藏当前内容
        if (currentActiveContent != null)
        {
            currentActiveContent.SetActive(false);
        }
        // 显示新内容
        if (buttonContentMap.TryGetValue(clickedButton, out GameObject content))
        {
            content.SetActive(true);
            currentActiveContent = content;
        }
        // 更新按钮状态（可选：高亮当前选中按钮）
        UpdateButtonStates(clickedButton);
    }
    
    // 更新按钮视觉状态（可选）
    private void UpdateButtonStates(Button activeButton)
    {
        // 示例：修改按钮颜色
        ColorBlock normalColors = new ColorBlock
        {
            normalColor = Color.gray,
            highlightedColor = Color.white,
            pressedColor = Color.white,
            selectedColor = Color.white,
            colorMultiplier = 1,
            fadeDuration = 0.1f
        };
        ColorBlock activeColors = new ColorBlock
        {
            normalColor = Color.white,
            highlightedColor = Color.white,
            pressedColor = Color.white,
            selectedColor = Color.white,
            colorMultiplier = 1,
            fadeDuration = 0.1f
        };
        inventoryBtn.colors = (activeButton == inventoryBtn) ? activeColors : normalColors;
        questBtn.colors = (activeButton == questBtn) ? activeColors : normalColors;
        mapBtn.colors = (activeButton == mapBtn) ? activeColors : normalColors;
        settingsBtn.colors = (activeButton == settingsBtn) ? activeColors : normalColors;
    }
}