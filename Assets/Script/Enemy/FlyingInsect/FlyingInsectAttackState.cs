using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInsectAttackState : EnemyState
{
    private FlyingInsectEnemy enemy;
    
    // 冲撞相关参数
    private Vector3 targetPosition;
    private Vector3 startChargePosition; // 实际开始冲撞的位置
    private bool isCharging = false;
    private bool hasHit = false;
    private bool isPreparing = true; // 准备阶段标志
    private float chargeSpeed = 15f; // 冲撞速度
    private float rotationSpeed = 10f; // 旋转速度
    private float attackRange = 10f; // 攻击范围
    
    // 冲撞计时器
    private float chargeDuration = 1f; // 冲撞持续时间
    private float chargeTimer = 0f;
    public Vector3 startPosition;
    
    // 准备阶段参数
    private float prepareDuration = 1f; // 准备阶段持续时间
    private float prepareTimer = 0f;
    private Vector3 prepareTargetPosition; // 准备阶段的目标位置
    private float prepareDistance = 10f; // 拉开的距离
    private float angleVariation = 60f; // 角度变化范围（正负值）
    private float prepareSpeed = 5f; // 准备阶段的移动速度
    
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
        isPreparing = true;
        chargeTimer = chargeDuration;
        prepareTimer = 0f;
        
        startPosition = enemy.transform.position;
        targetPosition = player.position;
        
        // 计算准备阶段的目标位置（拉开距离）
        CalculatePreparePosition();
        
        // 设置冲撞参数
        chargeSpeed = enemy.attackSpeed;
        attackRange = enemy.attackRange;
        prepareSpeed = chargeSpeed * 0.5f; // 准备阶段速度为冲撞速度的一半
        
        Debug.Log($"锁定目标位置: {targetPosition}");
        Debug.Log($"准备位置: {prepareTargetPosition}");
    }
    
    /// <summary>
    /// 计算准备阶段的位置（拉开距离）
    /// </summary>
    private void CalculatePreparePosition()
    {
        // 计算从目标指向当前位置的方向
        Vector3 toEnemy = (startPosition - targetPosition).normalized;
        
        // 添加随机角度偏移
        float randomAngle = Random.Range(-angleVariation, angleVariation);
        Quaternion randomRotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
        Vector3 prepareDirection = randomRotation * toEnemy;
        
        // 计算准备位置
        prepareTargetPosition = targetPosition + prepareDirection * prepareDistance;
        
        // 确保准备位置不会太靠近目标
        if (Vector3.Distance(prepareTargetPosition, targetPosition) < prepareDistance * 0.5f)
        {
            // 如果太近，重新计算
            CalculatePreparePosition();
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        Debug.Log("退出冲撞攻击状态");
        isCharging = false;
        hasHit = false;
        isPreparing = false;
    }

    public override void Update()
    {
        base.Update();
        
        if (isPreparing)
        {
            HandlePreparationPhase();
        }
        else if (!isCharging)
        {
            HandleAimingPhase();
        }
        else
        {
            HandleChargingPhase();
        }
    }
    
    /// <summary>
    /// 处理准备阶段（拉开距离）
    /// </summary>
    private void HandlePreparationPhase()
    {
        // 移动向准备位置
        Vector3 direction = (prepareTargetPosition - enemy.transform.position).normalized;
        Vector3 movement = direction * prepareSpeed * Time.deltaTime;
        
        // 移动敌人
        enemy.transform.position += movement;
        
        // 转向移动方向
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // 更新准备计时器
        prepareTimer += Time.deltaTime;
        
        // 检查是否到达准备位置或超时
        float distanceToPrepareTarget = Vector3.Distance(enemy.transform.position, prepareTargetPosition);
        
        if (distanceToPrepareTarget < 0.5f || prepareTimer >= prepareDuration)
        {
            // 准备阶段结束
            isPreparing = false;
            startChargePosition = enemy.transform.position; // 记录开始冲撞的位置
            Debug.Log("准备阶段结束，开始瞄准目标");
        }
    }
    
    /// <summary>
    /// 处理瞄准阶段
    /// </summary>
    private void HandleAimingPhase()
    {
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
        if (angleToTarget < 5f || stateTimer > 1f)
        {
            isCharging = true;
            Debug.Log("开始冲撞攻击！");
        }
    }
    
    /// <summary>
    /// 处理冲撞阶段
    /// </summary>
    private void HandleChargingPhase()
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
        
        // 检查是否超出范围（从实际开始冲撞的位置计算）
        if (Vector3.Distance(startChargePosition, enemy.transform.position) > attackRange * 2f)
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
            
            // 绘制准备位置
            if (isPreparing)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(prepareTargetPosition, 0.3f);
                Gizmos.DrawLine(enemy.transform.position, prepareTargetPosition);
            }
            
            // 绘制冲撞路径
            if (isCharging)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(enemy.transform.position, targetPosition);
            }
        }
    }
}