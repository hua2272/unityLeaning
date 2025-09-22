using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SkillNodeUI : MonoBehaviour, IPointerClickHandler
{
    public SkillData skillData;
    //public SkillTreeManager  skillTreeManager;
    
    public Image currentSkillIcon;
    public Image[] levelLockedIcon;
    public Image levelUnlockedIcon;
    
    [HideInInspector] public UnityEvent<SkillData> OnNodeClicked;
    
    private void Start()
    {
        //skillTreeManager = GetComponentInParent<SkillTreeManager>();
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
        currentSkillIcon.sprite = skillData.currentLevel == 0 ? skillData.lockedSprite : skillData.unlockedSprite;         //设置技能图标
        
        // 更新等级指示器
        for (int i = 0; i < skillData.currentLevel; i++)
        {
            if (levelLockedIcon[i] != null)
            {
                //levelIndicators[i] = !skillData.isLocked && i < skillData.currentLevel ? levelLockedIcon : levelUnlockedIcon;
                levelLockedIcon[i] = levelUnlockedIcon;
            }
        }
    }
}