using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatus : MonoBehaviour
{
    [Header("Major state")]
    public Status health;
    public Status stamina;                      //耐力
    public Status extraHealth;
    public Status extraStamina;
    public Status experiencePoints;             //经验点
    public Status skillPoints;                  //技能点
    
    [Header("Defense / Offence")]
    public Status armor;
    public Status evasion;                      //闪避性能
    public Status damage;
    public Status damagesSpeed;
    public int weaponAttack = 0;
    
    private Player player => GetComponent<Player>();
    
    public int currentHealth;
    [HideInInspector] public UnityEvent onHealthChange;
    
    public void Start()
    {
        currentHealth = health.getValue() + extraHealth.getValue();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;       //计算生命值
        onHealthChange?.Invoke();       //触发订阅事件（更新血条
        if (currentHealth < 0) 
            Die();
        
        player.DamageEffect();
    }

    public void DoDamage(EnemyStatus enemyStatus)
    {
        int totalDamage = Mathf.Clamp(weaponAttack + damage.getValue() - enemyStatus.armor.getValue(), 0, int.MaxValue); //护甲值过大会导致伤害为负数
        enemyStatus.TakeDamage(totalDamage);
    }

    public void Die()
    {
        player.Die();
    }
}
