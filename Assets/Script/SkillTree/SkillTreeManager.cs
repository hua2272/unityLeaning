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
    private void TryUnlockOrUpgradeSkill()
    {
        if (skillNodeUI.currentLevel > skillNodeUI.maxLevel || skillNodeUI.requiredPoints > availableSkillPoints) return;
        Debug.Log("TryUnlockOrUpgradeSkill, lv: " + skillNodeUI.currentLevel);
        availableSkillPoints -= skillNodeUI.requiredPoints;
        skillNodeUI.currentLevel++;
        skillNodeUI.UpdateUI();
    }
}