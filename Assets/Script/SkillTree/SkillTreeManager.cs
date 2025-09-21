using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    private bool isInitialized = false;
    public List<SkillData> allSkills = new List<SkillData>();
    public int availableSkillPoints = 10; 								//可用的技能点
    public SkillNodeUI skillNodeUI;

    
    void Awake()
    {
        if (isInitialized) return;
        skillNodeUI = GetComponentInChildren<SkillNodeUI>();
        skillNodeUI.OnNodeClicked.AddListener(TryUnlockOrUpgradeSkill);
        isInitialized = true;
    }

    // 检查技能是否满足解锁条件
    public bool CanUnlockSkill(SkillData skill)
    {
        // 检查技能点是否足够
        if (availableSkillPoints < skill.requiredPoints) return false;
        // 检查是否已解锁
        if (skill.isUnlocked) return false;
        // 检查依赖技能
        foreach (var requiredSkill in skill.requiredSkills)
        {
            if (requiredSkill == null) continue;
            // 检查依赖技能是否已解锁并达到所需等级
            if (!requiredSkill.isUnlocked || requiredSkill.currentLevel < skill.requiredSkillLevel)
                return false;
        }
        return true;
    }
    
    // 尝试解锁或升级技能
    private void TryUnlockOrUpgradeSkill(SkillData skill)
    {
        if (skill.isUnlocked)
        {
            // 如果已解锁，尝试升级
            TryUpgradeSkill(skill);
        }
        else
        {
            // 如果未解锁，尝试解锁
            TryUnlockSkill(skill);
        }
    }
    
    // 尝试解锁技能
    private void TryUnlockSkill(SkillData skill)
    {
        if (!CanUnlockSkill(skill)) return;
        availableSkillPoints -= skill.requiredPoints;
        skill.isUnlocked = true;
        skill.currentLevel = 1;
    }
    
    // 尝试升级技能
    private void TryUpgradeSkill(SkillData skill)
    {
        if (skill.currentLevel < skill.maxLevel && availableSkillPoints >= skill.requiredPoints)
        {
            availableSkillPoints -= skill.requiredPoints;
            skill.currentLevel++;
        }
    }
    
    // 添加技能点（用于测试）
    public void AddSkillPoints(int points)
    {
        availableSkillPoints += points;
    }
    
    // 获取技能状态描述
    public string GetSkillStatus(SkillData skill)
    {
        if (skill.isUnlocked)
            return $"已解锁 (等级 {skill.currentLevel}/{skill.maxLevel})";
        
        // 检查依赖技能
        foreach (var requiredSkill in skill.requiredSkills)
        {
            if (requiredSkill == null) continue;
            if (!requiredSkill.isUnlocked)
                return $"需要先解锁 {requiredSkill.skillName}";
            if (requiredSkill.currentLevel < skill.requiredSkillLevel)
                return $"{requiredSkill.skillName} 需要达到等级 {skill.requiredSkillLevel}";
        }
        return "可以解锁";
    }
}