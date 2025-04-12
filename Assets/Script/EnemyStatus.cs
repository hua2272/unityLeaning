using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : CharacterState
{

    private Enemy enemy;
    
    protected override void Start()
    {
        base.Start();
        enemy = GetComponent<Enemy>();
    }
    
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        enemy.DanmageEffect();
    }

    public override void Die()
    {
        base.Die();
        enemy.Die();
    }
}
