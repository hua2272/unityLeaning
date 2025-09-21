using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SkillNodeUI : MonoBehaviour, IPointerClickHandler
{
    public SkillData skillData;
    public SkillTreeManager  skillTreeManager;
    
    
    public Image skillIcon;
    public Button skillButton;
    public GameObject[] levelIndicators;
    
    [HideInInspector] public UnityEvent<SkillData> OnNodeClicked;
    
    private void Start()
    {
        skillTreeManager = GetComponentInParent<SkillTreeManager>();
        UpdateUI();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnSkillButtonClick");
        OnNodeClicked?.Invoke(skillData);
    }
    
    // 更新UI显示
    public void UpdateUI()
    {
        if (skillData == null) return;
        
        // 设置技能图标
        skillIcon.sprite = skillData.isUnlocked ? skillData.unlockedSprite : skillData.lockedSprite;
        
        // 更新等级指示器
        for (int i = 0; i < levelIndicators.Length; i++)
        {
            if (levelIndicators[i] != null)
            {
                // 如果技能已解锁且当前等级大于这个点的索引，则激活这个点
                levelIndicators[i].SetActive(skillData.isUnlocked && i < skillData.currentLevel);
            }
        }
        
        // 更新按钮交互状态
        if (skillData.isUnlocked)
        {
            // 如果技能已解锁
            if (skillData.currentLevel >= skillData.maxLevel)
            {
                skillButton.interactable = false;
            }
            else
            {
                skillButton.interactable = skillTreeManager.availableSkillPoints >= skillData.requiredPoints;
            }
        }
        else
        {
            // 如果技能未解锁
            bool canUnlock = skillTreeManager.CanUnlockSkill(skillData);
            skillButton.interactable = canUnlock;
        }
    }
}