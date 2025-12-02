using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuItemData
{
    public float PanelId;
    public float NextPanelId;
    public string title;
    public string buttonText;
    [NonSerialized] public Action action;

    // 选项相关字段
    public bool isOptionButton = false;
    public List<string> options = new List<string>();
    public int currentOptionIndex = 0;
    [NonSerialized] private Action<int> onOptionChanged; // 选项改变时的回调
    
    private string saveKey;//保存标识符

    // 普通按钮构造函数
    public MenuItemData(float panelId, float nextPanelId, string title, string buttonText, Action action)
    {
        this.PanelId = panelId;
        this.NextPanelId = nextPanelId;
        this.title = title;
        this.buttonText = buttonText;
        this.action = action;
        this.isOptionButton = false;
    }

    // 选项按钮构造函数
    public MenuItemData(float panelId, float nextPanelId, string title, List<string> options, int defaultIndex, Action<int> onOptionChanged, string saveKey = null)
    {
        this.PanelId = panelId;
        this.NextPanelId = nextPanelId;
        this.title = title;
        this.options = options;
        this.currentOptionIndex = defaultIndex;
        this.onOptionChanged = onOptionChanged;
        this.isOptionButton = true;
        this.saveKey = saveKey ?? $"MenuOption_{title}";
        
        // 加载保存的选项
        LoadOption();
        UpdateButtonText();
    }

    // 新增：直接设置选项索引
    public void SetOptionIndex(int newIndex)
    {
        if (options.Count > 0 && newIndex >= 0 && newIndex < options.Count)
        {
            currentOptionIndex = newIndex;
            buttonText = options[currentOptionIndex];
            SaveOption(); // 保存选项
            onOptionChanged?.Invoke(currentOptionIndex);
        }
    }

    private void UpdateButtonText()
    {
        if (options.Count > 0 && currentOptionIndex < options.Count)
        {
            buttonText = options[currentOptionIndex];
        }
    }

    // 获取当前选项文本
    public string GetCurrentOptionText()
    {
        if (options.Count > 0 && currentOptionIndex < options.Count)
        {
            return options[currentOptionIndex];
        }

        return buttonText;
    }
    
    // 新增：保存选项到 PlayerPrefs
    private void SaveOption()
    {
        if (!string.IsNullOrEmpty(saveKey))
        {
            PlayerPrefs.SetInt(saveKey, currentOptionIndex);
            PlayerPrefs.Save();
            Debug.Log($"保存选项: {title} -> {currentOptionIndex}");
        }
    }
    
    // 新增：从 PlayerPrefs 加载选项
    private void LoadOption()
    {
        if (!string.IsNullOrEmpty(saveKey) && PlayerPrefs.HasKey(saveKey))
        {
            int savedIndex = PlayerPrefs.GetInt(saveKey);
            if (savedIndex >= 0 && savedIndex < options.Count)
            {
                currentOptionIndex = savedIndex;
                Debug.Log($"加载选项: {title} -> {currentOptionIndex}");
            }
        }
    }
}