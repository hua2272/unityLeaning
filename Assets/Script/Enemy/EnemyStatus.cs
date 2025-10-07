using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyStatus : MonoBehaviour
{
    [Header("Major state")]
    public Status health;
    public Status stamina;                      //耐力
    public Status experiencePoints;             //经验点
    
    [Header("Defense / Offence")]
    public Status armor;
    public Status damage;
    public Status damagesSpeed;
    
    private Enemy enemy => GetComponent<Enemy>();
    
    public int currentHealth;
    [HideInInspector] public UnityEvent onHealthChange;
    
    public void Start()
    {
        currentHealth = health.getValue();
    }
    
    public void DoDamage(PlayerStatus playerStatus)
    {
        int totalDamage = Mathf.Clamp(damage.getValue() - playerStatus.armor.getValue(), 0, int.MaxValue); //护甲值过大会导致伤害为负数
        playerStatus.TakeDamage(totalDamage);
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;       //计算生命值
        onHealthChange?.Invoke();       //触发订阅事件（更新血条
        if (currentHealth < 0) 
            Die();
        enemy.DamageEffect();
    }

    public void Die()
    {
        enemy.Die();
        // todo 掉落物品 GetComponent<ItemDropper>().DropItemsOnDeath();
    }
}
