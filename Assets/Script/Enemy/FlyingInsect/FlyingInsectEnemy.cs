using UnityEngine;
using UnityEngine.SceneManagement;

public class FlyingInsectEnemy : Enemy
{
    public FlyingInsectPatrolState patrolState { get; private set; }
    public FlyingInsectAttackState attackState { get; private set; }
    public FlyingInsectLaserState laserState { get; private set; }
    
    [Header("巡逻设置")]
    [SerializeField] public Transform[] patrolPoints; // 巡逻点
    [SerializeField] public float patrolSpeed = 3f;
    [SerializeField] public float patrolWaitTime = 1f; // 到达巡逻点后的等待时间
    [SerializeField] public float patrolHeight = 5f; // 巡逻飞行高度
    public int currentPatrolIndex = 0;
    public Vector2 origin;
    public float maxRadius;
    public Vector2 direction;
    public float coneAngle;
    
    [Header("视野设置")]
    [SerializeField] public float detectionRange = 10f;
    [SerializeField] public float attackRange = 7f;
    [SerializeField] public float minAttackDistance = 3f;
    [SerializeField] public float visionAngle = 90f;
    [SerializeField] public LayerMask playerLayer;
    [SerializeField] public LayerMask obstacleLayer;
    
    [Header("激光技能设置")]
    [SerializeField] public float laserSectorAngle = 60f; // 扇形角度
    [SerializeField] public float laserSectorRadius = 10f; // 扇形半径
    [SerializeField] public Vector2 laserOriginOffset = Vector2.zero; // 激光起点偏移
    [SerializeField] public float lockDuration = 2f; // 锁定总时长
    [SerializeField] public float followDuration = 1f; // 跟随玩家时长
    [SerializeField] public float damageDuration = 1f; // 伤害判定时长
    [SerializeField] public int laserDamage = 10; // 激光伤害
    [SerializeField] public float damageInterval = 0.5f; // 伤害间隔
    [SerializeField] public LineRenderer laserLineRenderer; // 激光渲染器
    [SerializeField] public Transform laserOrigin; // 激光起点
    
    // 激光相关公共变量
    [HideInInspector] public Vector2 currentLaserDirection;
    [HideInInspector] public Vector2 targetLockPosition;
    [HideInInspector] public Transform lockedPlayer;
    [HideInInspector] public float laserStateTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        patrolState = new FlyingInsectPatrolState(this, stateMachine, "Patrol", this);
        attackState = new FlyingInsectAttackState(this, stateMachine, "Attack", this);
        laserState = new FlyingInsectLaserState(this, stateMachine, "Laser", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(patrolState);
    }
    
    protected override void Update()
    {
        base.Update();
    }
    
    // 初始化激光
    public void InitializeLaser()
    {
        // 如果没有指定激光起点，创建一个
        if (laserOrigin == null)
        {
            GameObject originObj = new GameObject("LaserOrigin");
            originObj.transform.SetParent(transform);
            originObj.transform.localPosition = laserOriginOffset;
            laserOrigin = originObj.transform;
        }
        
        // 初始化LineRenderer
        if (laserLineRenderer == null)
        {
            laserLineRenderer = gameObject.AddComponent<LineRenderer>();
            laserLineRenderer.startWidth = 0.15f;
            laserLineRenderer.endWidth = 0.05f;
            laserLineRenderer.positionCount = 2;
            laserLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            laserLineRenderer.startColor = Color.yellow;
            laserLineRenderer.endColor = Color.yellow;
            laserLineRenderer.enabled = false;
        }
    }

    public override void Die()
    {
        base.Die();
        SceneManager.LoadSceneAsync("Finale");
    }
    
    // Gizmos绘制激光扇形
    void OnDrawGizmosSelected()
    {
        if (laserOrigin == null) return;
        
        Vector2 origin = laserOrigin.position;
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        
        Vector2 leftDir = Quaternion.Euler(0, 0, -laserSectorAngle / 2) * transform.right;
        Vector2 rightDir = Quaternion.Euler(0, 0, laserSectorAngle / 2) * transform.right;
        
        Gizmos.DrawLine(origin, origin + leftDir * laserSectorRadius);
        Gizmos.DrawLine(origin, origin + rightDir * laserSectorRadius);
        
        // 绘制扇形弧线
        int segments = 20;
        Vector2 prevPoint = origin + leftDir * laserSectorRadius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -laserSectorAngle / 2 + (laserSectorAngle / segments) * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * transform.right;
            Vector2 newPoint = origin + dir * laserSectorRadius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}