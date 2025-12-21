using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    
    [System.Serializable] public class UIComponent// 存储UI组件引用
    {
        public UIGroup group;
        public CanvasGroup canvasGroup;
        public bool isVisible = true;
    }
    
    [SerializeField] private UIComponent[] uiComponents;
    private Dictionary<UIGroup, UIComponent> uiDictionary = new Dictionary<UIGroup, UIComponent>();
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
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
    
    public void SetCombatMode(bool inCombat)// 批量控制 - 战斗状态切换
    {
        if (inCombat)
        {
            // 进入战斗：显示战斗相关UI，隐藏非战斗UI
            ShowUI(uiDictionary[UIGroup.HealthBar], 0.3f);
            ShowUI(uiDictionary[UIGroup.StaminaBar], 0.3f);
            ShowUI(uiDictionary[UIGroup.CombatInfo], 0.3f);
        }
        else
        {
            // 脱离战斗：隐藏战斗UI
            HideUI(uiDictionary[UIGroup.HealthBar], 0.5f);
            HideUI(uiDictionary[UIGroup.StaminaBar], 0.5f);
            HideUI(uiDictionary[UIGroup.CombatInfo], 0.5f);
        }
    }
    
    
    public void HideAllUI(float fadeDuration = 0.2f)// 转场时隐藏所有UI
    {
        foreach (var kvp in uiDictionary)
        {
            HideUI(kvp.Value, fadeDuration);
        }
    }
    
    private void ShowUI(UIComponent component, float fadeDuration)
    {
        if (component == null || component.canvasGroup == null) return;
        
        component.isVisible = true;
        StopAllCoroutines();
        StartCoroutine(FadeUI(component.canvasGroup, 1f, fadeDuration));
    }
    
    private void HideUI(UIComponent component, float fadeDuration)
    {
        if (component == null || component.canvasGroup == null) return;
        
        component.isVisible = false;
        StopAllCoroutines();
        StartCoroutine(FadeUI(component.canvasGroup, 0f, fadeDuration));
    }
    
    private IEnumerator FadeUI(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha > 0.5f;
        canvasGroup.blocksRaycasts = targetAlpha > 0.5f;
    }
    
    public void SetUIPreset(UIPreset preset)// 预定义UI配置
    {
        switch (preset)
        {
            case UIPreset.Combat:
                SetCombatMode(true);
                break;
            case UIPreset.Exploration:
                SetCombatMode(false);
                break;
            case UIPreset.Dialogue:
                SetUIVisibility(UIGroup.Dialogue, true);
                SetUIVisibility(UIGroup.HealthBar, false);
                SetUIVisibility(UIGroup.StaminaBar, false);
                break;
            case UIPreset.Cinematic:
                HideAllUI(0.5f);
                break;
        }
    }
}

public enum UIGroup
{
    HealthBar,      // 血条
    StaminaBar,     // 精力条
    PlayerAvatar,   // 头像
    CombatInfo,     // 战斗信息
    Dialogue,       // 对话
    Minimap         // 小地图
}

public enum UIPreset// UI预设枚举
{
    Combat,
    Exploration,
    Dialogue,
    Cinematic,
    Menu
}