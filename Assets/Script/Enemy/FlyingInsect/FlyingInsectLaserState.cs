using System.Collections;
using UnityEngine;

public class FlyingInsectLaserState : EnemyState
{
    private FlyingInsectEnemy enemy;
    private Coroutine laserCoroutine;
    private Coroutine laserVisualizationCoroutine; // 新增：专门处理激光可视化的协程
    
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
    private Vector2 currentLaserEndPoint; // 当前激光终点位置
    private Transform lockedPlayer;
    private Vector2 currentLaserDirection;

    public FlyingInsectLaserState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, FlyingInsectEnemy enemy) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("进入激光攻击状态，重置所有参数");
        lockedPlayer = null;
        enemy.laserStateTimer = 0f;
        currentLaserDirection = enemy.transform.right;
        enemy.targetLockPosition = Vector2.zero;
        currentPhase = LaserPhase.Scanning;
        phaseTimer = 0f;
        damageTimer = 0f;
        scanAngle = 0f;
        
        enemy.ZeroVelocity();
        isLaserActive = true;                                                            // 标记激光为激活状态
        enemy.laserLineRenderer.enabled = true;
        laserCoroutine = enemy.StartCoroutine(LaserAttackRoutine());                     // 启动两个协程：一个处理状态逻辑，一个专门处理可视化更新
        laserVisualizationCoroutine = enemy.StartCoroutine(LaserVisualizationRoutine());
    }
    
    public override void Exit()
    {
        base.Exit();
        Debug.Log("退出激光攻击状态");
        isLaserActive = false;                                                  // 标记激光为非激活状态
        if (laserCoroutine != null)
        {
            enemy.StopCoroutine(laserCoroutine);
            laserCoroutine = null;
        }
        if (laserVisualizationCoroutine != null)
        {
            enemy.StopCoroutine(laserVisualizationCoroutine);
            laserVisualizationCoroutine = null;
        }
        enemy.laserLineRenderer.enabled = false;                                // 清理激光效果
        lockedPlayer = null;
        currentLaserDirection = Vector2.zero;
    }
    
    public override void Update()
    {
        base.Update();
        if (!isLaserActive) return;
        enemy.laserStateTimer += Time.deltaTime;                                // 更新激光状态计时器
    }
    
    // 激光可视化更新的协程，确保每帧都更新
    private IEnumerator LaserVisualizationRoutine()
    {
        while (isLaserActive)
        {
            Vector2 origin = enemy.laserOrigin.position;
            Vector2 direction = currentLaserDirection.normalized;
        
            // 执行射线检测，找到激光终点
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, enemy.laserSectorRadius, enemy.obstacleMask);
        
            // 优先使用障碍物碰撞点
            if (hit.collider != null)
            {
                currentLaserEndPoint = hit.point;
            }
            else
            {
                // 如果没有障碍物，检测玩家
                RaycastHit2D playerHit = Physics2D.Raycast(origin, direction, enemy.laserSectorRadius, enemy.playerMask);
                if (playerHit.collider != null)
                {
                    currentLaserEndPoint = playerHit.point;
                }
                else
                {
                    currentLaserEndPoint = origin + direction * enemy.laserSectorRadius;            // 如果都没有，使用最大距离的点
                }
            }
            // 更新LineRenderer
            enemy.laserLineRenderer.SetPosition(0, origin);
            enemy.laserLineRenderer.SetPosition(1, currentLaserEndPoint);
            Debug.DrawLine(origin, currentLaserEndPoint, GetLaserColor(), Time.deltaTime);      // 调试：绘制激光线
            yield return null;
        }
    }
    
    private IEnumerator LaserAttackRoutine()                                    // 激光攻击主协程
    {
        yield return enemy.StartCoroutine(PerformScanningPhase());       // 第一阶段：扇形扫描（寻找目标）
        yield return new WaitForSeconds(2f);
        
        if (lockedPlayer == null)
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
        
        enemy.laserLineRenderer.startColor = Color.yellow;
        enemy.laserLineRenderer.endColor = Color.yellow;
        
        float scanDuration = 3f;                                                // 扫描持续时间
        while (phaseTimer < scanDuration && isLaserActive)
        {
            phaseTimer += Time.deltaTime;
            // 计算扫描角度（来回扫描）
            scanAngle = Mathf.PingPong(phaseTimer * scanSpeed, enemy.laserSectorAngle) - enemy.laserSectorAngle / 2;
            // 计算扫描方向
            currentLaserDirection = Quaternion.Euler(0, 0, scanAngle) * new Vector2(enemy.facingDir, 0);
            
            if (DetectPlayerInSector(currentLaserDirection)) 
            {
                Debug.Log("扫描阶段发现玩家！");
                yield break;                                                    // 检测到玩家后提前结束扫描
            }
            yield return null;
        }
        lockedPlayer = null;                                              // 扫描结束未发现玩家
    }
    
    private IEnumerator PerformLockingPhase()                                   // 锁定阶段
    {
        Debug.Log("----锁定阶段");
        currentPhase = LaserPhase.Locking;
        phaseTimer = 0f;
        enemy.laserLineRenderer.startColor = Color.red;
        enemy.laserLineRenderer.endColor = Color.red;
        
        // 第一阶段：跟随玩家
        float followTime = 0f;
        while (followTime < enemy.followDuration && isLaserActive && lockedPlayer != null)
        {
            followTime += Time.deltaTime;
            
            // 计算指向玩家的方向
            Vector2 dirToPlayer = ((Vector2)lockedPlayer.position - (Vector2)enemy.laserOrigin.position).normalized;
            currentLaserDirection = dirToPlayer;
            enemy.targetLockPosition = lockedPlayer.position;
            
            if (!IsPlayerStillInSight())                                        // 检查玩家是否还在视野内
            {
                Debug.Log("玩家丢失，提前结束锁定");
                lockedPlayer = null;
                yield break;
            }
            yield return null;
        }
        
        // 第二阶段：锁定最后位置
        float lockTime = 0f;
        while (lockTime < enemy.lockDuration && isLaserActive)
        {
            lockTime += Time.deltaTime;
            
            // 如果玩家还存在，继续跟踪
            if (lockedPlayer != null && IsPlayerStillInSight())
            {
                Vector2 dirToPlayer = ((Vector2)lockedPlayer.position - (Vector2)enemy.laserOrigin.position).normalized;
                currentLaserDirection = dirToPlayer;
                enemy.targetLockPosition = lockedPlayer.position;
            }
            // 否则保持当前方向
            // 注意：激光终点会通过激光可视化协程每帧更新
            
            yield return null;
        }
        Debug.Log("锁定阶段结束");
    }
    
    private IEnumerator PerformDamagingPhase()                                  // 伤害判定阶段
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
    
    private Color GetLaserColor()       // 根据当前阶段返回激光颜色
    {
        switch (currentPhase)
        {
            case LaserPhase.Scanning:
                return Color.yellow;
            case LaserPhase.Locking:
                return Color.red;
            case LaserPhase.Damaging:
                return Color.cyan;
            default:
                return Color.white;
        }
    }
    
    private bool DetectPlayerInSector(Vector2 scanDirection)
    {
        Vector2 origin = enemy.laserOrigin.position;
        
        // 使用射线检测，而不是OverlapCircle，这样更精确
        RaycastHit2D hit = Physics2D.Raycast(origin, scanDirection, enemy.laserSectorRadius, enemy.playerMask);
        
        if (hit.collider != null)
        {
            // 检查是否有障碍物阻挡
            float distance = Vector2.Distance(origin, hit.point);
            RaycastHit2D obstacleCheck = Physics2D.Raycast(origin, scanDirection, distance, enemy.obstacleMask);
            
            if (obstacleCheck.collider == null)
            {
                lockedPlayer = hit.collider.transform;
                enemy.targetLockPosition = hit.point;
                return true;
            }
        }
        return false;
    }
    
    private bool IsPlayerStillInSight()
    {
        if (lockedPlayer == null) return false;
        
        Vector2 origin = enemy.laserOrigin.position;
        Vector2 dirToPlayer = ((Vector2)lockedPlayer.position - origin).normalized;
        float distance = Vector2.Distance(origin, lockedPlayer.position);
        
        // 距离检查
        if (distance > enemy.laserSectorRadius) return false;
        
        // 角度检查
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
        Vector2 direction = currentLaserDirection.normalized;
        
        // 检测射线上的所有玩家
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, enemy.laserSectorRadius, enemy.playerMask);
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                // 检查是否有障碍物阻挡
                float distance = Vector2.Distance(origin, hit.point);
                RaycastHit2D obstacleCheck = Physics2D.Raycast(origin, direction, distance, enemy.obstacleMask);
                
                if (obstacleCheck.collider == null)
                {
                    // TODO 对玩家造成伤害
                    Debug.Log($"对玩家 {hit.collider.name} 造成激光伤害");
                }
            }
        }
    }
}