using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyStatus : MonoBehaviour
{
    [Header("Major state")]
    public Status health;
    public Status stamina;
    
    [Header("Defense / Offence")]
    public Status armor;
    public Status damage;
    public Status damagesSpeed;
    
    private Enemy enemy => GetComponent<Enemy>();
    
    [Header("Health Recovery")]
    public int currentHealth;
    [HideInInspector] public UnityEvent onHealthChange;
    
    [Header("经验值掉落")]
    public GameObject expOrbPrefab;
    public int expOrbCount = 3;                                     //掉落的粒子数量
    public int expPerOrb = 5;                                       //每个粒子的经验值
    
    public void Start()
    {
        currentHealth = health.getValue();
    }
    
    public void DoDamage(PlayerStatus playerStatus)
    {
        int totalDamage = Mathf.Clamp(damage.getValue() - playerStatus.armor.getValue(), 0, int.MaxValue);
        playerStatus.TakeDamage(totalDamage);
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        onHealthChange?.Invoke();
        enemy.DamageEffect();
        if (currentHealth < 0)
        {
            enemy.Die();
            DropExp();
        }
    }

    void DropExp()
    {
        if (expOrbPrefab ==null) return;
        for (int i = 0; i < expOrbCount; i++)
        {
            GameObject orb = Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
            ExperienceOrb expOrb = orb.GetComponent<ExperienceOrb>();
            if (expOrb != null)
            {
                expOrb.expValue = expPerOrb;
            }
        }
    }
}
