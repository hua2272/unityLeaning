using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }
    
    private UILangue uiLangue;
    
    [Serializable] public class UIComponent// 存储UI组件引用
    {
        public UIGroup group;
        public CanvasGroup canvasGroup;
        public int textContentId;
        public bool isVisible = true;
    }
    
    [SerializeField] private UIComponent[] uiComponents;
    private Dictionary<UIGroup, UIComponent> uiDictionary = new Dictionary<UIGroup, UIComponent>();
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        uiLangue = UILangue.instance;
        foreach (var component in uiComponents)
        {
            uiDictionary[component.group] = component;// 初始化字典
        }
    }

    public void SetUIVisibility(UIGroup group, bool show, float fadeDuration = 0.2f)// 基础显示/隐藏方法
    {
        if (uiDictionary.TryGetValue(group, out UIComponent component))
        {
            if (show)
            {
                ShowUI(component, fadeDuration);
            }
            else
            {
                HideUI(component, fadeDuration);
            }
        }
    }
    
    public void SwitchScene(UIPreset preset)
    {
        Debug.Log("Switching to " + preset);
        switch (preset)
        {
            case UIPreset.PressStart:
                HideUI(uiDictionary[UIGroup.Title], 0);
                HideUI(uiDictionary[UIGroup.AutoSaveInfo], 0);
                HideUI(uiDictionary[UIGroup.PlayerStatus], 0);
                ShowUI(uiDictionary[UIGroup.PressStart], 1);
                break;
            case UIPreset.Title:
                HideUI(uiDictionary[UIGroup.PressStart], 0);
                HideUI(uiDictionary[UIGroup.AutoSaveInfo], 0);
                HideUI(uiDictionary[UIGroup.PlayerStatus], 0);
                ShowUI(uiDictionary[UIGroup.Title], 1);
                break;
            case UIPreset.Normal:
                HideUI(uiDictionary[UIGroup.PressStart], 0);
                HideUI(uiDictionary[UIGroup.AutoSaveInfo], 0);
                HideUI(uiDictionary[UIGroup.PlayerStatus], 0);
                HideUI(uiDictionary[UIGroup.Title], 0);
                break;
        }
    }
    
    private void ShowUI(UIComponent component, float fadeDuration)
    {
        if (component == null || component.canvasGroup == null) return;
        
        component.isVisible = true;
        StopAllCoroutines();
        StartCoroutine(FadeUI(component, 1f, fadeDuration));
    }
    
    private void HideUI(UIComponent component, float fadeDuration)
    {
        if (component == null || component.canvasGroup == null) return;
        
        component.isVisible = false;
        StopAllCoroutines();
        StartCoroutine(FadeUI(component, 0f, fadeDuration));
    }
    
    private IEnumerator FadeUI(UIComponent component, float targetAlpha, float duration)
    {
        CanvasGroup canvasGroup = component.canvasGroup;
        TextMeshProUGUI textComponent = component.canvasGroup.GetComponentInChildren<TextMeshProUGUI>();
        
        float startAlpha = canvasGroup.alpha;
        Color startTextColor = textComponent != null ? textComponent.color : Color.white;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);// 淡入淡出CanvasGroup
            
            if (textComponent != null)// 如果存在TextMeshProUGUI，单独处理它的颜色透明度
            {
                textComponent.text = uiLangue.Content(component.textContentId);
                Color newColor = textComponent.color;
                newColor.a = Mathf.Lerp(startTextColor.a, targetAlpha, t);
                textComponent.color = newColor;
            }
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        
        if (textComponent != null)
        {
            Color finalColor = textComponent.color;
            finalColor.a = targetAlpha;
            textComponent.color = finalColor;
        }
        canvasGroup.interactable = targetAlpha > 0.5f;
        canvasGroup.blocksRaycasts = targetAlpha > 0.5f;
    }
    
    public void HideAllUI(float fadeDuration = 0.2f)// 转场时隐藏所有UI
    {
        foreach (var kvp in uiDictionary)
        {
            HideUI(kvp.Value, fadeDuration);
        }
    }
}

public enum UIGroup
{
    PressStart,     // 按任意键开始游戏
    Title,          // 标题
    PlayerStatus,   // 玩家信息
    AutoSaveInfo,   // 自动保存信息
    CombatInfo,     // 战斗信息
    Dialogue,       // 对话
    HealthBar,      // 血条
    StaminaBar,     // 精力条
    Minimap         // 小地图
}

public enum UIPreset// UI预设枚举
{
    PressStart,
    Title,
    Normal,
    Combat,
    Exploration,
    Dialogue,
    Cinematic,
    Menu
}