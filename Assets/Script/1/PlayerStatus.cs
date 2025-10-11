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
    
    [Header("Health Recovery")]
    public int currentHealth;
    [HideInInspector] public UnityEvent onHealthChange;
    
    [Header("Stamina Recovery")]
    public int currentStamina;
    public float staminaRecoveryRate = 5f;                      //耐力恢复速率（每秒恢复量）
    public float staminaRecoveryDelay = 2f;                     //停止消耗耐力后开始恢复的延迟时间
    private float lastStaminaUseTime;                           //最后一次使用耐力的时间
    private bool isRecoveringStamina = false;                   //是否正在恢复耐力
    [HideInInspector] public UnityEvent onStaminaChange;
    
    public void Start()
    {
        currentHealth = health.getValue() + extraHealth.getValue();
        StartCoroutine(StaminaRecoveryRoutine());
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
    
    
    private IEnumerator StaminaRecoveryRoutine()                                                //耐力恢复协程
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);                                              //每0.1秒检查一次
            
            if (currentStamina < stamina.getValue())
            {
                if (Time.time - lastStaminaUseTime >= staminaRecoveryDelay)                     //检查是否过了恢复延迟时间
                {
                    isRecoveringStamina = true;
                    float recoveryAmount = staminaRecoveryRate * 0.1f;                          //计划回复量
                    int staminaDeficit = stamina.getValue() - currentStamina;                   //当前精力值与最大精力值的差值
                    if (staminaDeficit > 0)
                    {
                        int amountToRecover = Mathf.Min((int)recoveryAmount, staminaDeficit);   //取差值和计划回复量的较小值
                        currentStamina = stamina.getValue() + amountToRecover;
                    }
                    onStaminaChange?.Invoke();
                }
            }
            else
            {
                isRecoveringStamina = false;
            }
        }
    }
}
