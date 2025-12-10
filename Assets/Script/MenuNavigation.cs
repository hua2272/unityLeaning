using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MenuNavigation : MonoBehaviour
{
    [Header("Navigation Settings")]
    public float selectedOffset = 30f; // 选中时向右偏移的距离
    public Color selectedColor = Color.blue; // 选中时的颜色
    public Color normalColor = Color.white; // 正常颜色
    public FontWeight selectedFontWeight = FontWeight.Bold; // 选中时的字体粗细
    public FontWeight normalFontWeight = FontWeight.Thin; // 正常字体粗细
    
    [Header("Light Bar Settings")]
    public Color lightBarColor = new Color(0.2f, 0.4f, 1f, 0.3f); // 光线条颜色
    public Vector2 lightBarSize = new Vector2(200f, 10f); // 光线条尺寸
    
    private List<Button> buttons = new List<Button>();
    private List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();
    private List<RectTransform> buttonTransforms = new List<RectTransform>();
    private List<GameObject> lightBars = new List<GameObject>();
    private List<Vector2> originalPositions = new List<Vector2>();
    
    [Header("Audio Settings")]
    private AudioManager audioManager;
    
    private int currentSelectedIndex = 0;
    private bool inputAvailable = true;
    private float inputCooldown = 0.2f;
    private float lastInputTime = 0f;

    void Start()
    {
        audioManager = AudioManager.instance;
        InitializeButtons();
        UpdateButtonAppearance();
    }

    void Update()
    {
        HandleInput();
    }

    private void InitializeButtons()
    {
        // 获取Panel下所有的按钮
        Button[] foundButtons = GetComponentsInChildren<Button>();
        buttons.AddRange(foundButtons);

        foreach (Button button in buttons)
        {
            // 获取按钮的RectTransform
            RectTransform buttonTransform = button.GetComponent<RectTransform>();
            buttonTransforms.Add(buttonTransform);
            originalPositions.Add(buttonTransform.anchoredPosition);

            // 获取按钮的TextMeshPro组件
            TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                buttonTexts.Add(textComponent);
                
                // 移除按钮的边框 - 设置颜色为透明
                ColorBlock colors = button.colors;
                colors.normalColor = Color.clear;
                colors.highlightedColor = new Color(1f, 1f, 1f, 0.1f);
                colors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
                colors.selectedColor = Color.clear;
                colors.disabledColor = Color.clear;
                button.colors = colors;

                // 创建光线条
                CreateLightBar(buttonTransform, textComponent.rectTransform);
            }
            else
            {
                Debug.LogWarning("Button missing TextMeshProUGUI component: " + button.name);
                buttonTexts.Add(null);
                lightBars.Add(null);
            }
        }

        // 隐藏所有光线条
        foreach (GameObject lightBar in lightBars)
        {
            if (lightBar != null)
                lightBar.SetActive(false);
        }
    }

    private void CreateLightBar(RectTransform buttonTransform, RectTransform textTransform)
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
        
        // 设置位置在文字左侧
        lightBarTransform.anchoredPosition = new Vector2(-20f, 0f);
        
        lightBars.Add(lightBar);
    }

    private void HandleInput()
    {
        if (!inputAvailable) return;

        float vertical = Input.GetAxisRaw("Vertical");
        
        // 使用冷却时间防止快速连续输入
        if (Time.time - lastInputTime < inputCooldown) return;

        if (vertical > 0.5f) // W键或上箭头
        {
            MoveSelection(-1);
            lastInputTime = Time.time;
        }
        else if (vertical < -0.5f) // S键或下箭头
        {
            MoveSelection(1);
            lastInputTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Return))
        {
            PressSelectedButton();
            lastInputTime = Time.time;
        }
    }

    private void MoveSelection(int direction)
    {
        if (buttons.Count == 0) return;

        // 取消当前选中状态
        SetButtonSelected(currentSelectedIndex, false);

        // 计算新的选中索引
        currentSelectedIndex += direction;
        if (currentSelectedIndex < 0)
            currentSelectedIndex = buttons.Count - 1;
        else if (currentSelectedIndex >= buttons.Count)
            currentSelectedIndex = 0;

        // 应用新的选中状态
        SetButtonSelected(currentSelectedIndex, true);
    }

    private void SetButtonSelected(int index, bool selected)
    {
        if (index < 0 || index >= buttons.Count) return;

        // 移动位置
        Vector2 newPosition = originalPositions[index];
        if (selected)
        {
            newPosition.x += selectedOffset;
        }
        buttonTransforms[index].anchoredPosition = newPosition;

        // 更新文字样式
        if (buttonTexts[index] != null)
        {
            buttonTexts[index].color = selected ? selectedColor : normalColor;
            buttonTexts[index].fontWeight = selected ? selectedFontWeight : normalFontWeight;
        }

        // 显示/隐藏光线条
        if (lightBars[index] != null)
        {
            lightBars[index].SetActive(selected);
        }
    }

    private void PressSelectedButton()
    {
        if (buttons.Count > 0 && currentSelectedIndex >= 0 && currentSelectedIndex < buttons.Count)
        {
            if (currentSelectedIndex == 1)
            {
                audioManager.StopBackgroundMusic();
            }
            buttons[currentSelectedIndex].onClick.Invoke();
        }
    }

    private void UpdateButtonAppearance()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            SetButtonSelected(i, i == currentSelectedIndex);
        }
    }
}