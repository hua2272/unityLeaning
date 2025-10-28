using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager instance;
    private GameDataManager gameDataManager;
    
    private bool isInitialized = false;
    private PlayerStatus playerStatus;
    private SkillManager skillManager;
    private SkillNodeUI[] skillNodes;
    
    private void Awake()
    {
        Debug.Log("-------SkillTreeManager instance-------");
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        else
        {
            instance = this;
        }
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    
    void Start()
    {
        if (isInitialized) return;
        skillManager = SkillManager.instance;
        playerStatus = PlayerManager.instance.playerStatus;
        gameDataManager = GameDataManager.instance;
        
        // 获取所有子节点的SkillNodeUI组件
        skillNodes = GetComponentsInChildren<SkillNodeUI>(true);
        foreach (SkillNodeUI skillNode in skillNodes)
        {
            skillNode.onNodeClicked.AddListener(() => TryUnlockOrUpgradeSkill(skillNode));
            // if (!skills.ContainsKey(skillName))
            // {
            //     continue;
            // }
            foreach (var unlockedSkill in gameDataManager.unlockedSkills.items)
            {
                if (skillNode.skillName.Equals(unlockedSkill.key))
                {
                    skillNode.currentLevel = unlockedSkill.value;
                    UpdateSkillNode(skillNode.skillName, skillNode);
                }
            }
        }
        isInitialized = true;
    }
    
    //todo
    //1 技能升级时触发实际功能，增加血量，耐力等
    //2 添加技能解锁的条件，技能点消耗，前置技能校验
    //3 技能升级时添加特效（由下往上填充），提示音；升级失败或无法升级时点击图片触发提示音
    private void TryUnlockOrUpgradeSkill(SkillNodeUI skillNodeUI)
    {
        if (skillNodeUI.currentLevel >= skillNodeUI.maxLevel)
        {
            //todo 添加音效
            Debug.Log("已最大级");
            return;
        }

        int availableSkillPoints = playerStatus.skillPoints;
        int upgradeCost = skillNodeUI.upgradeCosts[skillNodeUI.currentLevel];
        if (upgradeCost > availableSkillPoints)
        {
            //todo 添加音效
            Debug.Log("技能点不够");
            return;
        }

        List<string> requiredSkills = skillNodeUI.requiredSkills;
        if (requiredSkills.Count > 0 && !requiredSkills.All(skill => gameDataManager.unlockedSkills.ContainsKey(skill)))
        {
            //todo 添加音效
            Debug.Log("前置技能未解锁");
            return;
        }
        playerStatus.skillPoints = availableSkillPoints - upgradeCost;
        skillNodeUI.currentLevel++;
        UpdateSkillNode(skillNodeUI.skillName, skillNodeUI);
        
        Debug.Log("LV: " + skillNodeUI.currentLevel);
        Debug.Log("effect: " + skillNodeUI.upgradeEffect[skillNodeUI.currentLevel]);
        Debug.Log("cost: " + upgradeCost);
        gameDataManager.unlockedSkills.AddOrUpdate(skillNodeUI.skillName, skillNodeUI.currentLevel);
    }

    
    private void UpdateSkillNode(string skillName, SkillNodeUI skillNodeUI)
    {
        skillNodeUI.UpdateUI();
        switch (skillName)
        {
            case "health":
                playerStatus.health.setValue(skillNodeUI.upgradeEffect[skillNodeUI.currentLevel]); //todo 残血时升级自动满血需要更新UI。bug:血量更新不真实需要测试
                break;
            default:
                skillManager.UpgradeSkill(skillName, skillNodeUI.currentLevel);
                break;
        }
    }
}