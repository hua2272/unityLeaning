using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInsectAttackState : EnemyState
{
    private FlyingInsectEnemy enemy;
    public FlyingInsectAttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, FlyingInsectEnemy enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
