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
        FixedDirection,// 固定方向
        Damaging       // 伤害判定
    }
    
    private LaserPhase currentPhase = LaserPhase.Scanning;
    private float phaseTimer = 0f;
    private float damageTimer = 0f;
    private float scanAngle = 0f;
    private float scanSpeed = 180f;
    
    public FlyingInsectLaserState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, FlyingInsectEnemy enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        // 初始化激光
        enemy.InitializeLaser();
        
        // 重置参数
        enemy.lockedPlayer = null;
        enemy.laserStateTimer = 0f;
        enemy.currentLaserDirection = enemyBase.transform.right;
        
        // 开始激光攻击协程
        if (laserCoroutine != null) 
            enemyBase.StopCoroutine(laserCoroutine);
        
        laserCoroutine = enemyBase.StartCoroutine(LaserAttackRoutine());
        
        Debug.Log("进入激光攻击状态");
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 停止激光协程
        if (laserCoroutine != null)
        {
            enemyBase.StopCoroutine(laserCoroutine);
            laserCoroutine = null;
        }
        
        // 隐藏激光
        if (enemy.laserLineRenderer != null)
            enemy.laserLineRenderer.enabled = false;
        
        Debug.Log("退出激光攻击状态");
    }
    
    public override void Update()
    {
        base.Update();
        
        // 更新状态计时器
        enemy.laserStateTimer += Time.deltaTime;
    }
    
    // 激光攻击主协程
    private IEnumerator LaserAttackRoutine()
    {
        // 第一阶段：扇形扫描
        yield return StartCoroutine(ScanningPhase());
        
        // 第二阶段：锁定跟踪
        yield return StartCoroutine(LockingPhase());
        
        // 第三阶段：固定方向
        yield return StartCoroutine(FixedDirectionPhase());
        
        // 第四阶段：伤害判定
        yield return StartCoroutine(DamagingPhase());
        
        // 激光攻击结束，返回空闲状态
        stateMachine.ChangeState(enemy.patrolState);
    }
    
    // 第一阶段：扇形扫描
    private IEnumerator ScanningPhase()
    {
        Debug.Log("开始扇形扫描");
        currentPhase = LaserPhase.Scanning;
        phaseTimer = 0f;
        
        // 启用激光显示
        enemy.laserLineRenderer.enabled = true;
        enemy.laserLineRenderer.startColor = Color.yellow;
        enemy.laserLineRenderer.endColor = Color.yellow;
        
        // 扫描持续时间（可配置，这里用3秒）
        float scanDuration = 3f;
        
        while (phaseTimer < scanDuration)
        {
            phaseTimer += Time.deltaTime;
            
            // 计算扫描角度
            scanAngle = Mathf.PingPong(phaseTimer * scanSpeed, enemy.laserSectorAngle) 
                      - enemy.laserSectorAngle / 2;
            
            // 计算扫描方向
            Vector2 scanDir = Quaternion.Euler(0, 0, scanAngle) * enemyBase.transform.right;
            
            // 更新激光显示
            UpdateLaserVisualization(scanDir, Color.yellow);
            
            // 检测玩家
            if (DetectPlayerInSector(scanDir))
            {
                Debug.Log("检测到玩家，开始锁定");
                yield break;
            }
            
            yield return null;
        }
        
        Debug.Log("未检测到玩家，结束激光攻击");
        // 扫描结束未发现玩家，直接结束状态
        stateMachine.ChangeState(enemy.patrolState);
    }
    
    // 第二阶段：锁定跟踪
    private IEnumerator LockingPhase()
    {
        Debug.Log("开始锁定跟踪");
        currentPhase = LaserPhase.Locking;
        phaseTimer = 0f;
        
        // 设置激光颜色为红色
        enemy.laserLineRenderer.startColor = Color.red;
        enemy.laserLineRenderer.endColor = Color.red;
        
        // 锁定阶段总时长
        while (phaseTimer < enemy.lockDuration && enemy.lockedPlayer != null)
        {
            phaseTimer += Time.deltaTime;
            
            // 前followDuration秒跟随玩家
            if (phaseTimer < enemy.followDuration)
            {
                Vector2 dirToPlayer = ((Vector2)enemy.lockedPlayer.position - (Vector2)enemy.laserOrigin.position).normalized;
                enemy.currentLaserDirection = dirToPlayer;
                enemy.targetLockPosition = enemy.lockedPlayer.position;
            }
            // 1秒后锁定最后位置
            else if (phaseTimer >= enemy.followDuration && phaseTimer < enemy.lockDuration)
            {
                // 记录最后方向
                if (Mathf.Approximately(phaseTimer - Time.deltaTime, enemy.followDuration))
                {
                    enemy.currentLaserDirection = ((Vector2)enemy.targetLockPosition - (Vector2)enemy.laserOrigin.position).normalized;
                    Debug.Log($"锁定玩家最后位置: {enemy.targetLockPosition}");
                }
            }
            
            // 更新激光显示
            UpdateLaserVisualization(enemy.currentLaserDirection, Color.red);
            
            // 检查玩家是否还在视野内
            if (!IsPlayerStillInSight())
            {
                Debug.Log("玩家丢失，提前结束锁定");
                yield break;
            }
            
            yield return null;
        }
        
        Debug.Log("锁定阶段结束");
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
        
        // 角度检查
        float angleToPlayer = Vector2.Angle(enemyBase.transform.right, dirToPlayer);
        if (angleToPlayer > enemy.laserSectorAngle / 2) return false;
        
        // 障碍物检查
        RaycastHit2D hit = Physics2D.Raycast(origin, dirToPlayer, distance, enemy.obstacleLayer);
        return hit.collider == null;
    }
    
    // 第三阶段：固定方向（短暂停留）
    private IEnumerator FixedDirectionPhase()
    {
        Debug.Log("固定方向阶段");
        currentPhase = LaserPhase.FixedDirection;
        phaseTimer = 0f;
        
        // 短暂停留0.5秒
        float fixedDuration = 0.5f;
        
        while (phaseTimer < fixedDuration)
        {
            phaseTimer += Time.deltaTime;
            
            // 更新激光显示（紫色）
            UpdateLaserVisualization(enemy.currentLaserDirection, new Color(1, 0, 1)); // 洋红色
            
            yield return null;
        }
        
        Debug.Log("固定方向阶段结束");
    }
    
    // 第四阶段：伤害判定
    private IEnumerator DamagingPhase()
    {
        Debug.Log("开始伤害判定阶段");
        currentPhase = LaserPhase.Damaging;
        phaseTimer = 0f;
        damageTimer = 0f;
        
        // 设置激光颜色为红色（伤害颜色）
        enemy.laserLineRenderer.startColor = Color.red;
        enemy.laserLineRenderer.endColor = Color.red;
        
        while (phaseTimer < enemy.damageDuration)
        {
            phaseTimer += Time.deltaTime;
            damageTimer += Time.deltaTime;
            
            // 更新激光显示
            UpdateLaserVisualization(enemy.currentLaserDirection, Color.red);
            
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
                    // todo 对玩家造成伤害
                    
                }
            }
        }
    }
    
    // 辅助方法：扇形区域内检测玩家（更精确的方法）
    private bool DetectPlayerInSector(Vector2 scanDirection)
    {
        Vector2 origin = enemy.laserOrigin.position;
        
        // 使用OverlapCircle检测所有玩家
        Collider2D[] players = Physics2D.OverlapCircleAll(origin, enemy.laserSectorRadius, enemy.playerLayer);
        
        foreach (Collider2D player in players)
        {
            Vector2 dirToPlayer = ((Vector2)player.transform.position - origin).normalized;
            
            // 计算角度是否在扇形内
            float angleToPlayer = Vector2.Angle(enemyBase.transform.right, dirToPlayer);
            
            if (angleToPlayer <= enemy.laserSectorAngle / 2)
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
}