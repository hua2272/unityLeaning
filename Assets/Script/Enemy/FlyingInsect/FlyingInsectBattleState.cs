using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInsectBattleState : EnemyState
{
    private FlyingInsectEnemy enemy;
    private float playerLostTime;                       // 玩家丢失的时间
    private const float playerLostThreshold = 5f;       // 5秒阈值
    
    public FlyingInsectBattleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, FlyingInsectEnemy enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("-----------进入battleState");
        playerLostTime = 0f;
    }
    
    public override void Exit()
    {
        base.Exit();
        playerLostTime = 0f;
    }
    
    
    public override void Update()
    {
        base.Update();
        // 检测玩家是否在锥形区域内
        RaycastHit2D playerHit = enemy.IsPlayerDetectedInCone(enemy.transform, new Vector2(enemy.facingDir, 0));
        if (playerHit.collider != null)
        {
            // 检测到玩家，重置计时器
            playerLostTime = 0f;
            stateMachine.ChangeState(enemy.laserState);
        }
        else
        {
            // 未检测到玩家，累加计时器
            playerLostTime += Time.deltaTime;
            
            // 如果超过5秒未检测到玩家，切换到巡逻状态
            if (playerLostTime >= playerLostThreshold)
            {
                stateMachine.ChangeState(enemy.patrolState);
            }
        }
    }
}
