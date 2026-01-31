using System.Collections;
using UnityEngine;

public class FlyingInsectLaserState : EnemyState
{
    private FlyingInsectEnemy enemy;
    private Coroutine laserCoroutine;
    
    // 激光状态
    private enum LaserPhase
    {
        Scanning,      // 扇形扫描
        Locking,       // 锁定跟踪
        Damaging       // 伤害判定
    }
    
    private LaserPhase currentPhase = LaserPhase.Scanning;
    private float phaseTimer = 0f;
    private float damageTimer = 0f;
    private float scanAngle = 0f;
    private float scanSpeed = 180f;
    private bool isLaserActive = false;

    public FlyingInsectLaserState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, FlyingInsectEnemy enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        Debug.Log("进入激光攻击状态");
        
        // 重置所有参数
        ResetLaserState();
        
        // 标记激光为激活状态
        isLaserActive = true;
        
        // 开始激光攻击协程
        laserCoroutine = enemy.StartCoroutine(LaserAttackRoutine());
    }
    
    public override void Exit()
    {
        base.Exit();
        
        Debug.Log("退出激光攻击状态");
        
        
        isLaserActive = false;// 标记激光为非激活状态
        rb.isKinematic = false;// 不受重力影响
        
        // 停止激光协程
        if (laserCoroutine != null)
        {
            enemy.StopCoroutine(laserCoroutine);
            laserCoroutine = null;
        }
        
        // 清理激光效果
        CleanupLaserEffects();
    }
    
    public override void Update()
    {
        base.Update();
        
        if (!isLaserActive) return;
        
        // 更新激光状态计时器
        enemy.laserStateTimer += Time.deltaTime;
    }
    
    // 重置激光状态参数
    private void ResetLaserState()
    {
        enemy.lockedPlayer = null;
        enemy.laserStateTimer = 0f;
        enemy.currentLaserDirection = enemy.transform.right;
        enemy.targetLockPosition = Vector2.zero;
        
        currentPhase = LaserPhase.Scanning;
        phaseTimer = 0f;
        damageTimer = 0f;
        scanAngle = 0f;
    }
    
    // 清理激光效果
    private void CleanupLaserEffects()
    {
        // 隐藏激光线
        if (enemy.laserLineRenderer != null)
        {
            enemy.laserLineRenderer.enabled = false;
        }
        
        // 重置所有相关变量
        enemy.lockedPlayer = null;
        enemy.currentLaserDirection = Vector2.zero;
    }

    // 激光攻击主协程
    private IEnumerator LaserAttackRoutine()
    {
        enemy.ZeroVelocity();
        rb.isKinematic = true;
        // 第一阶段：扇形扫描（寻找目标）
        yield return enemy.StartCoroutine(PerformScanningPhase());
        yield return new WaitForSeconds(2f);

        // 如果扫描阶段没有找到目标，直接退出
        if (enemy.lockedPlayer == null)
        {
            Debug.Log("扫描阶段未找到目标，结束激光攻击");
            stateMachine.ChangeState(enemy.patrolState);
            yield break;
        }
        
        // 第二阶段：锁定跟踪
        yield return enemy.StartCoroutine(PerformLockingPhase());
        yield return new WaitForSeconds(2f);
        
        // 第三阶段：伤害判定
        yield return enemy.StartCoroutine(PerformDamagingPhase());

        // 激光攻击结束
        Debug.Log("激光攻击完成");

        // 确保状态切换回巡逻
        if (isLaserActive)
        {
            stateMachine.ChangeState(enemy.patrolState);
        }
    }
    
    // 扫描阶段
    private IEnumerator PerformScanningPhase()
    {
        Debug.Log("开始扇形扫描");
        currentPhase = LaserPhase.Scanning;
        phaseTimer = 0f;
        
        // 启用激光显示
        enemy.laserLineRenderer.enabled = true;
        enemy.laserLineRenderer.startColor = Color.yellow;
        enemy.laserLineRenderer.endColor = Color.yellow;
        
        // 扫描持续时间
        float scanDuration = 3f;
        
        while (phaseTimer < scanDuration && isLaserActive)
        {
            phaseTimer += Time.deltaTime;
            
            // 计算扫描角度（来回扫描）
            scanAngle = Mathf.PingPong(phaseTimer * scanSpeed, enemy.laserSectorAngle) 
                      - enemy.laserSectorAngle / 2;
            
            // 计算扫描方向
            Vector2 scanDir = Quaternion.Euler(0, 0, scanAngle) * enemy.transform.right;
            
            // 更新激光可视化
            UpdateLaserVisualization(scanDir, Color.yellow);
            
            // 检测玩家
            if (DetectPlayerInSector(scanDir))
            {
                Debug.Log("检测到玩家，开始锁定");
                yield break; // 提前结束扫描
            }
            
            yield return null;
        }
        
        // 扫描结束未发现玩家
        enemy.lockedPlayer = null;
    }
    
    // 锁定阶段
    private IEnumerator PerformLockingPhase()
    {
        if (enemy.lockedPlayer == null) yield break;
        
        Debug.Log("开始锁定跟踪");
        currentPhase = LaserPhase.Locking;
        phaseTimer = 0f;
        
        // 设置激光颜色为红色
        enemy.laserLineRenderer.startColor = Color.red;
        enemy.laserLineRenderer.endColor = Color.red;
        
        // 第一阶段：跟随玩家1秒
        while (phaseTimer < enemy.followDuration && enemy.lockedPlayer != null && isLaserActive)
        {
            phaseTimer += Time.deltaTime;
            
            // 计算指向玩家的方向
            Vector2 dirToPlayer = ((Vector2)enemy.lockedPlayer.position - (Vector2)enemy.laserOrigin.position).normalized;
            enemy.currentLaserDirection = dirToPlayer;
            enemy.targetLockPosition = enemy.lockedPlayer.position;
            
            // 更新激光显示
            UpdateLaserVisualization(enemy.currentLaserDirection, Color.red);
            
            // 检查玩家是否还在视野内
            if (!IsPlayerStillInSight())
            {
                Debug.Log("玩家丢失，提前结束锁定");
                enemy.lockedPlayer = null;
                yield break;
            }
            
            yield return null;
        }
        
        // 第二阶段：锁定最后位置1秒
        float lockRemainingTime = enemy.lockDuration - enemy.followDuration;
        phaseTimer = 0f;
        
        while (phaseTimer < lockRemainingTime && isLaserActive)
        {
            phaseTimer += Time.deltaTime;
            
            // 保持锁定最后的方向
            UpdateLaserVisualization(enemy.currentLaserDirection, Color.red);
            
            yield return null;
        }
        
        Debug.Log("锁定阶段结束");
    }
    
    // 伤害判定阶段
    private IEnumerator PerformDamagingPhase()
    {
        Debug.Log("开始伤害判定阶段");
        currentPhase = LaserPhase.Damaging;
        phaseTimer = 0f;
        damageTimer = 0f;
        
        // 设置激光颜色为深红色（伤害颜色）
        Color damageColor = new Color(0.8f, 0.1f, 0.1f);
        enemy.laserLineRenderer.startColor = damageColor;
        enemy.laserLineRenderer.endColor = damageColor;
        
        while (phaseTimer < enemy.damageDuration && isLaserActive)
        {
            phaseTimer += Time.deltaTime;
            damageTimer += Time.deltaTime;
            
            // 更新激光显示
            UpdateLaserVisualization(enemy.currentLaserDirection, damageColor);
            
            // 定期造成伤害
            if (damageTimer >= enemy.damageInterval)
            {
                ApplyLaserDamage();
                damageTimer = 0f;
            }
            
            yield return null;
        }
        
        Debug.Log("伤害判定阶段结束");
    }
    
    // 更新激光可视化
    private void UpdateLaserVisualization(Vector2 direction, Color color)
    {
        if (enemy.laserLineRenderer == null || enemy.laserOrigin == null) return;
        
        Vector2 origin = enemy.laserOrigin.position;
        
        // 射线检测
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, enemy.laserSectorRadius, 
            enemy.playerLayer | enemy.obstacleLayer);
        
        float drawLength = hit.collider != null ? hit.distance : enemy.laserSectorRadius;
        
        // 更新LineRenderer
        enemy.laserLineRenderer.SetPosition(0, origin);
        enemy.laserLineRenderer.SetPosition(1, origin + direction * drawLength);
        
        // 更新颜色
        enemy.laserLineRenderer.startColor = color;
        enemy.laserLineRenderer.endColor = color;
    }
    
    // 检测扇形区域内玩家
    private bool DetectPlayerInSector(Vector2 scanDirection)
    {
        Vector2 origin = enemy.laserOrigin.position;
        
        // 使用OverlapCircle检测所有玩家
        Collider2D[] players = Physics2D.OverlapCircleAll(origin, enemy.laserSectorRadius, enemy.playerLayer);
        
        foreach (Collider2D player in players)
        {
            Vector2 dirToPlayer = ((Vector2)player.transform.position - origin).normalized;
            
            // 计算角度是否在扇形内
            float angleToPlayer = Vector2.Angle(scanDirection, dirToPlayer);
            
            if (angleToPlayer <= 5f) // 使用较小的角度容差
            {
                // 检查是否有障碍物阻挡
                float distance = Vector2.Distance(origin, player.transform.position);
                RaycastHit2D hit = Physics2D.Raycast(origin, dirToPlayer, distance, enemy.obstacleLayer);
                
                if (hit.collider == null)
                {
                    enemy.lockedPlayer = player.transform;
                    enemy.targetLockPosition = player.transform.position;
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // 检查玩家是否仍在视野内
    private bool IsPlayerStillInSight()
    {
        if (enemy.lockedPlayer == null) return false;
        
        Vector2 origin = enemy.laserOrigin.position;
        Vector2 dirToPlayer = ((Vector2)enemy.lockedPlayer.position - origin).normalized;
        float distance = Vector2.Distance(origin, enemy.lockedPlayer.position);
        
        // 距离检查
        if (distance > enemy.laserSectorRadius) return false;
        
        // 角度检查（允许稍微大一点的角度容差）
        float angleToPlayer = Vector2.Angle(enemy.transform.right, dirToPlayer);
        if (angleToPlayer > enemy.laserSectorAngle / 2 + 10f) return false;
        
        // 障碍物检查
        RaycastHit2D hit = Physics2D.Raycast(origin, dirToPlayer, distance, enemy.obstacleLayer);
        return hit.collider == null;
    }
    
    // 应用激光伤害
    private void ApplyLaserDamage()
    {
        Vector2 origin = enemy.laserOrigin.position;
        
        // 检测射线上的所有玩家
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, enemy.currentLaserDirection, 
            enemy.laserSectorRadius, enemy.playerLayer);
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                // 检查是否有障碍物
                float distance = Vector2.Distance(origin, hit.point);
                RaycastHit2D obstacleCheck = Physics2D.Raycast(origin, enemy.currentLaserDirection, 
                    distance, enemy.obstacleLayer);
                
                if (obstacleCheck.collider == null)
                {
                    // TODO 对玩家造成伤害
                }
            }
        }
    }
}