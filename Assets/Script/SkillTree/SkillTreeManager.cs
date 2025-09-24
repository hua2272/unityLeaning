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
    
    //todo
    //1 技能升级时触发实际功能，增加血量，耐力等
    //2 添加技能解锁的条件，技能点消耗，前置技能校验
    //3 技能升级时添加特效（由下往上填充），提示音；升级失败或无法升级时点击图片触发提示音
    private void TryUnlockOrUpgradeSkill()
    {
        if (skillNodeUI.currentLevel > skillNodeUI.maxLevel || skillNodeUI.requiredPoints > availableSkillPoints) return;
        Debug.Log("TryUnlockOrUpgradeSkill, lv: " + skillNodeUI.currentLevel);
        availableSkillPoints -= skillNodeUI.requiredPoints;
        skillNodeUI.currentLevel++;
        skillNodeUI.UpdateUI();
    }
}