using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public string description;
    public Sprite lockedSprite;    // 未解锁时的灰色图标
    public Sprite unlockedSprite;  // 解锁后的彩色图标
    public int maxLevel = 4;       // 最大等级
    public int requiredPoints = 1; // 每次升级需要的点数
    
    [Header("技能依赖")]
    public List<SkillData> requiredSkills; // 需要先解锁的技能列表
    public int requiredSkillLevel = 1;     // 需要的前置技能等级
    
    [HideInInspector]
    public int currentLevel = 0;   // 当前等级
    [HideInInspector]
    public bool isUnlocked = false; // 是否已解锁
}