using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;
using Range = UnityEngine.SocialPlatforms.Range;

public class CharacterState : MonoBehaviour
{
    public static CharacterState instance { get; private set; }
    
    [Header("Major state")]
    public Status health;
    public Status stamina;                      //耐力
    public Status extraHealth;
    public Status extraStamina;
    
    [Header("Defense / Offence")]
    public Status armor;
    public Status evasion;                      //闪避性能
    public Status damage;
    public Status damagesSpeed;
    public int weaponAttack = 0;
    
    public int currentHealth;
    public Action onHealthChange = () => { };    //事件未被订阅时为Null会报错，该写法可省略Null判断

    protected virtual void Start()
    {
        currentHealth = GetMaxHealthValue();
    }
    
    public void SetWeaponAttack(int value)
    {
        weaponAttack = value;
    }

    public virtual void DoDamage(CharacterState _targetState)
    {
        int totalDamage = Mathf.Clamp(weaponAttack + damage.getValue() - _targetState.armor.getValue(), 0, int.MaxValue); //护甲值过大会导致伤害为负数
        _targetState.TakeDamage(totalDamage);
    }

    public virtual void TakeDamage(int _damage)
    {
        currentHealth -= _damage;       //计算生命值
        onHealthChange?.Invoke();       //触发订阅事件（更新血条
        if (currentHealth < 0) 
            Die();
    }

    public virtual void Die()
    {
    }

    public int GetMaxHealthValue() => health.getValue() + extraHealth.getValue();
}
