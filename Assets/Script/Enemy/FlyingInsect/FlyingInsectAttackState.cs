using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInsectAttackState : EnemyState
{
    private FlyingInsectEnemy enemy;
    
    // 冲撞相关参数
    private Vector3 targetPosition;
    private bool isCharging = false;
    private bool hasHit = false;
    private float chargeSpeed = 15f; // 冲撞速度
    private float rotationSpeed = 10f; // 旋转速度
    private float attackRange = 10f; // 攻击范围
    
    // 冲撞计时器
    private float chargeDuration = 1f; // 冲撞持续时间
    private float chargeTimer = 0f;
    public Vector3 startPosition;
    
    public FlyingInsectAttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, FlyingInsectEnemy enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("进入冲撞攻击状态");
        // 重置状态
        isCharging = false;
        hasHit = false;
        chargeTimer = chargeDuration;

        startPosition = enemy.transform.position;
        targetPosition = player.position;   // 锁定玩家当前位置
        // 可以添加一些预测玩家移动的算法，例如：根据玩家速度和方向预测位置
        /*
        Rigidbody2D playerRb = enemy.player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            float predictionTime = Vector3.Distance(enemy.transform.position, targetPosition) / chargeSpeed;
            targetPosition += (Vector3)playerRb.velocity * predictionTime;
        }
        */
        Debug.Log($"锁定目标位置: {targetPosition}");
        // 设置冲撞参数
        chargeSpeed = enemy.attackSpeed;
        attackRange = enemy.attackRange;
        
        // 播放攻击动画或音效
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("退出冲撞攻击状态");
        isCharging = false;
        hasHit = false;
    }

    public override void Update()
    {
        base.Update();
        if (!isCharging)
        {
            // 准备阶段：转向目标并锁定位置
            // 计算朝向目标的向量
            Vector3 direction = targetPosition - enemy.transform.position;
        
            // 转向目标位置
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        
            // 检查是否已经朝向目标（可以设置一个角度阈值）
            float angleToTarget = Vector3.Angle(enemy.transform.up, direction.normalized);
        
            // 当转向基本完成或经过一定时间后，开始冲撞
            if (angleToTarget < 5f || stateTimer > 0.5f)
            {
                isCharging = true;
            }
        }
        else
        {
            // 朝目标位置移动
            Vector3 direction = (targetPosition - enemy.transform.position).normalized;
            Vector3 movement = direction * chargeSpeed * Time.deltaTime;
        
            // 移动敌人
            enemy.transform.position += movement;
        
            // 保持冲撞方向
            if (direction != Vector3.zero)
            {
                enemy.transform.up = direction;
            }
            CheckPlayerHit();// 检测是否击中玩家
            CheckObstacleHit();// 检测是否撞到墙壁或其他障碍物
        
            // 检查是否超出范围
            if (Vector3.Distance(startPosition, enemy.transform.position) > attackRange * 2f)
            {
                hasHit = true; // 视为冲撞结束 todo 添加后摇状态
            }
            
            // 检查冲撞是否结束
            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0f || hasHit)
            {
                stateMachine.ChangeState(enemy.battleState);
            }
        }
    }

    /// <summary>
    /// 检查是否击中玩家
    /// </summary>
    private void CheckPlayerHit()
    {
        if (hasHit) return;
        
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, player.position);
        
        if (distanceToPlayer < enemy.hitRadius)
        {
            // todo 对玩家造成伤害
            hasHit = true;
        }
    }

    /// <summary>
    /// 检查是否撞到障碍物
    /// </summary>
    private void CheckObstacleHit()
    {
        // 使用射线检测前方是否有障碍物
        RaycastHit2D hit = Physics2D.Raycast(enemy.transform.position, enemy.transform.up, 0.5f, enemy.obstacleMask);
        
        if (hit.collider != null)
        {
            Debug.Log($"撞到障碍物: {hit.collider.name}");
            hasHit = true;
            
            // 可以添加反弹效果
            // Vector3 reflectDirection = Vector3.Reflect(enemy.transform.up, hit.normal);
            // enemy.transform.up = reflectDirection;
        }
    }
    
    /// <summary>
    /// 绘制调试信息（可选）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (enemy != null && enemy.transform != null)
        {
            // 绘制冲撞方向
            Gizmos.color = Color.red;
            Gizmos.DrawLine(enemy.transform.position, enemy.transform.position + enemy.transform.up * 2f);
            
            // 绘制目标位置
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, 0.3f);
            
            // 绘制冲撞路径
            if (isCharging)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(enemy.transform.position, targetPosition);
            }
        }
    }
}