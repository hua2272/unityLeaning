using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class OptionButton : MonoBehaviour
{
    [System.Serializable]
    public class Option
    {
        public string displayText;
        public System.Action onSelected;
    }

    [Header("Option Settings")]
    [SerializeField] private TextMeshProUGUI optionText;
    [SerializeField] private List<Option> options = new List<Option>();
    [SerializeField] private int defaultOptionIndex = 0;

    private int currentOptionIndex = 0;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        currentOptionIndex = defaultOptionIndex;
        UpdateDisplay();
    }

    public void SelectNextOption()
    {
        if (options.Count == 0) return;

        currentOptionIndex++;
        if (currentOptionIndex >= options.Count)
            currentOptionIndex = 0;

        UpdateDisplay();
        options[currentOptionIndex].onSelected?.Invoke();
    }

    public void SelectPreviousOption()
    {
        if (options.Count == 0) return;

        currentOptionIndex--;
        if (currentOptionIndex < 0)
            currentOptionIndex = options.Count - 1;

        UpdateDisplay();
        options[currentOptionIndex].onSelected?.Invoke();
    }

    public void HighlightCurrentOption()
    {
        // 可以在这里添加高亮当前选项的视觉效果
        UpdateDisplay();
    }

    public void ConfirmSelection()
    {
        // 确认选择，可以触发按钮的点击事件或其他逻辑
        if (button != null)
        {
            button.onClick.Invoke();
        }
    }

    public int GetCurrentOptionIndex()
    {
        return currentOptionIndex;
    }

    public string GetCurrentOptionText()
    {
        if (options.Count > 0 && currentOptionIndex < options.Count)
            return options[currentOptionIndex].displayText;
        return "";
    }

    private void UpdateDisplay()
    {
        if (optionText != null && options.Count > 0)
        {
            optionText.text = options[currentOptionIndex].displayText;
        }
    }

    // 公共方法用于动态添加选项
    public void AddOption(string displayText, System.Action onSelected = null)
    {
        options.Add(new Option { displayText = displayText, onSelected = onSelected });
        if (options.Count == 1) // 如果是第一个选项
        {
            currentOptionIndex = 0;
            UpdateDisplay();
        }
    }

    public void ClearOptions()
    {
        options.Clear();
        currentOptionIndex = 0;
        UpdateDisplay();
    }
}