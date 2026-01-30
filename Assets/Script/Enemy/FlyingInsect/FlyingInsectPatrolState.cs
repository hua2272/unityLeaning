using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInsectPatrolState : EnemyState
{
    private FlyingInsectEnemy enemy;
    
    public FlyingInsectPatrolState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, FlyingInsectEnemy enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Update()
    {
        base.Update();
        if (enemy.patrolPoints.Length == 0) return;
        
        Vector2 targetPosition = enemy.patrolPoints[enemy.currentPatrolIndex].position;             //移动到当前巡逻点
        Vector2 direction = (targetPosition - (Vector2)enemy.transform.position).normalized;
        
        rb.velocity = direction * enemy.patrolSpeed;
        
        if (enemy.facingDir > 0)//更新朝向 todo 优化
        {
            enemy.Flip();
        }
        
        if (Vector2.Distance(enemy.transform.position, targetPosition) < 1f && stateTimer < 0)       //检查是否到达巡逻点，是否冷却
        {
            enemy.currentPatrolIndex = (enemy.currentPatrolIndex + 1) % enemy.patrolPoints.Length;      //切换到下一个巡逻点
            stateTimer = enemy.idleTime;
        }

        if (enemy.ConeCast(enemy.origin, enemy.maxRadius, enemy.direction, enemy.coneAngle).Length > 0)
        {
            stateMachine.ChangeState(enemy.attackState);
        }
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }
}
