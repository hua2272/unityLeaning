using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;
using Range = UnityEngine.SocialPlatforms.Range;

public class CharacterState : MonoBehaviour
{
    [Header("Major stats")]
    public Status strength; //物伤加点
    public Status agility; //敏捷加点
    public Status intelligence; //法术加点
    public Status vitality; //生命值加点
    
    [Header("Defense stats")]
    public Status maxHealth;
    public Status armor;
    public Status evasion;
    
    [Header("Offence stats")]
    public Status damage;
    
    public int currentHealth;
    public System.Action onHealthChange;

    protected virtual void Start()
    {
        currentHealth = GetMaxHealthValue();
    }

    public virtual void DoDamage(CharacterState _targetState)
    {
        // int totalEvasion = _targetState.evasion.getValue() + _targetState.agility.getValue();
        // if (Random.Range(0, 100) < totalEvasion){}
        int totalDamage = Mathf.Clamp(damage.getValue() + strength.getValue() - _targetState.armor.getValue(), 0, int.MaxValue); //护甲值过大会导致伤害为负数
        _targetState.TakeDamage(totalDamage);
    }

    public virtual void TakeDamage(int _damage)
    {
        DecreaseHealth(_damage);
        if (currentHealth < 0)
        {
            Die();
        }
    }

    protected virtual void DecreaseHealth(int _damage)
    {
        currentHealth -= _damage;
        if (onHealthChange != null)
        {
            onHealthChange();
        }
    }

    public virtual void Die()
    {
    }

    public int GetMaxHealthValue() => maxHealth.getValue() + vitality.getValue();
}
