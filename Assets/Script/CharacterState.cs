using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterState : MonoBehaviour
{
    public Status strength;
    public Status damage;
    public Status maxHealth;
    [SerializeField] private int currentHealth;

    protected virtual void Start()
    {
        currentHealth = maxHealth.getValue();
    }

    public virtual void DoDamage(CharacterState _targetState)
    {
        int totalDamage = damage.getValue() + strength.getValue();
        _targetState.TakeDamage(totalDamage);
    }

    public virtual void TakeDamage(int _damage)
    {
        currentHealth -= _damage;
        if (currentHealth < 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
    }
}
