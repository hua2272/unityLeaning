using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    private bool isInitialized = false;
    public int availableSkillPoints = 10; 								//可用的技能点
    public SkillNodeUI skillNodeUI;

    
    void Awake()
    {
        if (isInitialized) return;
        skillNodeUI = GetComponentInChildren<SkillNodeUI>();
        skillNodeUI.OnNodeClicked.AddListener(TryUnlockOrUpgradeSkill);
        isInitialized = true;
    }
    
    // 尝试解锁或升级技能
    private void TryUnlockOrUpgradeSkill(SkillData skill)
    {
        if (skill.currentLevel == 0)
        {
            //if (!CanUnlockSkill(skill)) return; todo 添加前置解锁条件限制
            availableSkillPoints -= skill.requiredPoints;
            skill.currentLevel = 1;
            skillNodeUI.UpdateUI();
        }
        else if (skill.currentLevel < skill.maxLevel && availableSkillPoints >= skill.requiredPoints)
        {
            availableSkillPoints -= skill.requiredPoints;
            skill.currentLevel++;
            skillNodeUI.UpdateUI();
        }
    }
}