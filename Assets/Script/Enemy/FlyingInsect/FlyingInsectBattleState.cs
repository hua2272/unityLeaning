using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInsectBattleState : EnemyState
{
    private FlyingInsectEnemy enemy;
    private float playerLostTime;                                           // 玩家丢失的时间
    private const float playerLostThreshold = 5f;                           // 5秒阈值
    private List<EnemyState> attackStates = new List<EnemyState>();         // 存储所有攻击状态的列表
    private int lastAttackIndex = -1;                                       // 记录上一次使用的攻击状态索引，避免连续使用同一个
    
    public FlyingInsectBattleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, FlyingInsectEnemy enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("-----------进入battleState");
        playerLostTime = 0f;
        
        attackStates.Clear();// 初始化攻击状态列表
        attackStates.Add(enemy.laserState);
        attackStates.Add(enemy.attackState);
    }
    
    public override void Exit()
    {
        base.Exit();
        playerLostTime = 0f;
    }
    
    // 随机选择一个攻击状态（避免连续使用同一个）
    private EnemyState GetRandomAttackState()
    {
        if (attackStates.Count == 0)
        {
            Debug.LogWarning("没有可用的攻击状态！");
            return enemy.laserState; // 返回默认状态
        }
        
        // 如果只有一个攻击状态，直接返回
        if (attackStates.Count == 1)
            return attackStates[0];
        
        // 随机选择一个不同的攻击状态
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, attackStates.Count);
        } 
        while (randomIndex == lastAttackIndex && attackStates.Count > 1);
        
        lastAttackIndex = randomIndex;
        return attackStates[randomIndex];
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
            stateMachine.ChangeState(GetRandomAttackState());
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