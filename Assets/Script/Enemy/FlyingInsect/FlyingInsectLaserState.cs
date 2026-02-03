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
        Debug.Log("进入激光攻击状态，重置所有参数");
        enemy.lockedPlayer = null;
        enemy.laserStateTimer = 0f;
        enemy.currentLaserDirection = enemy.transform.right;
        enemy.targetLockPosition = Vector2.zero;
        currentPhase = LaserPhase.Scanning;
        phaseTimer = 0f;
        damageTimer = 0f;
        scanAngle = 0f;
        
        enemy.ZeroVelocity();
        isLaserActive = true;                                                   // 标记激光为激活状态
        laserCoroutine = enemy.StartCoroutine(LaserAttackRoutine());            // 开始激光攻击协程
    }
    
    public override void Exit()
    {
        base.Exit();
        Debug.Log("退出激光攻击状态");
        isLaserActive = false;                                                  // 标记激光为非激活状态
        if (laserCoroutine != null)
        {
            enemy.StopCoroutine(laserCoroutine);                                // 停止激光协程
            laserCoroutine = null;
        }
        
        enemy.laserLineRenderer.enabled = false;                                // 清理激光效果
        enemy.lockedPlayer = null;
        enemy.currentLaserDirection = Vector2.zero;
    }
    
    public override void Update()
    {
        base.Update();
        if (!isLaserActive) return;
        enemy.laserStateTimer += Time.deltaTime;                                // 更新激光状态计时器
    }
    
    private IEnumerator LaserAttackRoutine()                                    // 激光攻击主协程
    {
        yield return enemy.StartCoroutine(PerformScanningPhase());       // 第一阶段：扇形扫描（寻找目标）
        yield return new WaitForSeconds(2f);
        
        if (enemy.lockedPlayer == null)
        {
            Debug.Log("扫描阶段未找到目标，结束激光攻击");
            stateMachine.ChangeState(enemy.patrolState);                        // 如果扫描阶段没有找到目标，直接退出
            yield break;
        }
        
        yield return enemy.StartCoroutine(PerformLockingPhase());         // 第二阶段：锁定跟踪
        yield return new WaitForSeconds(2f);
        yield return enemy.StartCoroutine(PerformDamagingPhase());        // 第三阶段：伤害判定
        Debug.Log("激光攻击完成");
        if (isLaserActive)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
    
    
    private IEnumerator PerformScanningPhase()                                  // 扫描阶段
    {
        Debug.Log("----扫描阶段");
        currentPhase = LaserPhase.Scanning;
        phaseTimer = 0f;
        
        enemy.laserLineRenderer.enabled = true;                                 // 启用激光显示
        enemy.laserLineRenderer.startColor = Color.yellow;
        enemy.laserLineRenderer.endColor = Color.yellow;
        float scanDuration = 3f;                                                // 扫描持续时间
        while (phaseTimer < scanDuration && isLaserActive)
        {
            phaseTimer += Time.deltaTime;
            scanAngle = Mathf.PingPong(phaseTimer * scanSpeed, enemy.laserSectorAngle) - enemy.laserSectorAngle / 2;    // 计算扫描角度（来回扫描）
            Vector2 scanDir = Quaternion.Euler(0, 0, scanAngle) * new Vector2(enemy.facingDir, 0);                        // 计算扫描方向
            UpdateLaserVisualization(scanDir, Color.yellow);                                                                 // 更新激光可视化
            
            if (DetectPlayerInSector(scanDir)) yield break;                                                                     // 检测到玩家后提前结束扫描
            yield return null;
        }
        enemy.lockedPlayer = null;                                                                                              // 扫描结束未发现玩家
    }
    
    private IEnumerator PerformLockingPhase()                                                                                   // 锁定阶段
    {
        Debug.Log("----锁定阶段");
        currentPhase = LaserPhase.Locking;
        phaseTimer = 0f;
        enemy.laserLineRenderer.startColor = Color.red;
        enemy.laserLineRenderer.endColor = Color.red;
        
        while (phaseTimer < enemy.followDuration && isLaserActive)                                                              // 第一阶段：跟随玩家1秒
        {
            phaseTimer += Time.deltaTime;
            Vector2 dirToPlayer = ((Vector2)enemy.lockedPlayer.position - (Vector2)enemy.laserOrigin.position).normalized;      // 计算指向玩家的方向
            enemy.currentLaserDirection = dirToPlayer;
            enemy.targetLockPosition = enemy.lockedPlayer.position;
            UpdateLaserVisualization(enemy.currentLaserDirection, Color.red);
            if (!IsPlayerStillInSight())                                                                                        // 检查玩家是否还在视野内
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
            UpdateLaserVisualization(enemy.currentLaserDirection, Color.red);                                               // 保持锁定最后的方向
            yield return null;
        }
        Debug.Log("锁定阶段结束");
    }
    
    private IEnumerator PerformDamagingPhase()                                                                                  // 伤害判定阶段
    {
        Debug.Log("----伤害阶段");
        currentPhase = LaserPhase.Damaging;
        phaseTimer = 0f;
        damageTimer = 0f;
        enemy.laserLineRenderer.startColor = Color.cyan;
        enemy.laserLineRenderer.endColor = Color.cyan;
        
        while (phaseTimer < enemy.damageDuration && isLaserActive)
        {
            phaseTimer += Time.deltaTime;
            damageTimer += Time.deltaTime;
            UpdateLaserVisualization(enemy.currentLaserDirection, Color.cyan);// 更新激光显示
            
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
    
    private void UpdateLaserVisualization(Vector2 direction, Color color)                               // 更新激光可视化
    {
        if (enemy.laserLineRenderer == null || enemy.laserOrigin == null) return;
        Vector2 origin = enemy.laserOrigin.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, enemy.laserSectorRadius, enemy.playerMask | enemy.obstacleMask);
        
        float drawLength = hit.collider != null ? hit.distance : enemy.laserSectorRadius;
        enemy.laserLineRenderer.SetPosition(0, origin);                                         // 更新LineRenderer
        enemy.laserLineRenderer.SetPosition(1, origin + direction * drawLength);
        enemy.laserLineRenderer.startColor = color;                                                     // 更新颜色
        enemy.laserLineRenderer.endColor = color;
    }
    
    
    private bool DetectPlayerInSector(Vector2 scanDirection)                                                                // 检测扇形区域内玩家
    {
        Vector2 origin = enemy.laserOrigin.position;
        Collider2D[] players = Physics2D.OverlapCircleAll(origin, enemy.laserSectorRadius, enemy.playerMask);           // 使用OverlapCircle检测所有玩家
        
        foreach (Collider2D player in players)
        {
            Vector2 dirToPlayer = ((Vector2)player.transform.position - origin).normalized;
            float angleToPlayer = Vector2.Angle(scanDirection, dirToPlayer);                                                // 计算角度是否在扇形内
            if (angleToPlayer <= 5f)                                                                                        // 使用较小的角度容差
            {
                float distance = Vector2.Distance(origin, player.transform.position);                                   // 检查是否有障碍物阻挡
                RaycastHit2D hit = Physics2D.Raycast(origin, dirToPlayer, distance, enemy.obstacleMask);
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
    
    
    private bool IsPlayerStillInSight()                                                                        // 检查玩家是否仍在视野内
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
        RaycastHit2D hit = Physics2D.Raycast(origin, dirToPlayer, distance, enemy.obstacleMask);
        return hit.collider == null;
    }
    
    // 应用激光伤害
    private void ApplyLaserDamage()
    {
        Vector2 origin = enemy.laserOrigin.position;
        
        // 检测射线上的所有玩家
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, enemy.currentLaserDirection, enemy.laserSectorRadius, enemy.playerMask);
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                // 检查是否有障碍物
                float distance = Vector2.Distance(origin, hit.point);
                RaycastHit2D obstacleCheck = Physics2D.Raycast(origin, enemy.currentLaserDirection, distance, enemy.obstacleMask);
                
                if (obstacleCheck.collider == null)
                {
                    // TODO 对玩家造成伤害
                }
            }
        }
    }
}