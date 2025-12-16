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
        // 移动到当前巡逻点
        Vector2 targetPosition = enemy.patrolPoints[enemy.currentPatrolIndex].position;
        Vector2 direction = (targetPosition - (Vector2)enemy.transform.position).normalized;
        
        // 保持巡逻高度
        float currentHeight = enemy.transform.position.y;
        float targetHeight = enemy.patrolHeight;
        
        // 垂直移动
        if (Mathf.Abs(currentHeight - targetHeight) > 0.1f)
        {
            float verticalDirection = Mathf.Sign(targetHeight - currentHeight);
            direction.y = verticalDirection * 0.5f; // 垂直移动速度较慢
        }
        
        // 移动
        rb.velocity = direction * enemy.patrolSpeed;
        Debug.Log("速度：" + rb.velocity);
        // enemy.SetVelocity(1,2);
        
        // 更新朝向
        if (direction.x != 0)
        {
            //spriteRenderer.flipX = direction.x < 0;
            enemy.transform.Rotate(0, 180, 0);
        }
        
        // 检查是否到达巡逻点
        if (Vector2.Distance(enemy.transform.position, targetPosition) < 0.5f)
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= enemy.patrolWaitTime)
            {
                // 切换到下一个巡逻点
                enemy.currentPatrolIndex = (enemy.currentPatrolIndex + 1) % enemy.patrolPoints.Length;
                stateTimer = 0f;
            }
        }
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter");
    }

    public override void Exist()
    {
        base.Exist();
    }
}
