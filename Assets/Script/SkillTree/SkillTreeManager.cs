using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    private bool isInitialized = false;
    public Dictionary<string, int> unlockedSkills = new Dictionary<string, int>();
    private SkillNodeUI skillNodeUI;
    private PlayerStatus playerStatus;
    
    void Awake()
    {
        if (isInitialized) return;

        playerStatus = PlayerManager.instance.playerStatus;
        Debug.Log("Skill Points: " + playerStatus.skillPoints.getValue());
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
        if (skillNodeUI.currentLevel >= skillNodeUI.maxLevel)
        {
            //todo 添加音效
            Debug.Log("已最大级");
            return;
        }

        int availableSkillPoints = playerStatus.skillPoints.getValue();
        int upgradeCost = skillNodeUI.upgradeCosts[skillNodeUI.currentLevel];
        if (upgradeCost > availableSkillPoints)
        {
            //todo 添加音效
            Debug.Log("技能点不够");
            return;
        }

        List<string> requiredSkills = skillNodeUI.requiredSkills;
        if (requiredSkills.Count > 0 && !requiredSkills.All(skill => unlockedSkills.ContainsKey(skill)))
        {
            //todo 添加音效
            Debug.Log("前置技能未解锁");
            return;
        }
        playerStatus.skillPoints.setValue(availableSkillPoints - upgradeCost);
        skillNodeUI.currentLevel++;
        Debug.Log("LV: " + skillNodeUI.currentLevel);
        Debug.Log("cost: " + upgradeCost);
        skillNodeUI.UpdateUI();
        unlockedSkills[skillNodeUI.skillName] = skillNodeUI.currentLevel;               //索引器，key存在则更新value，不存在则新增
    }
}