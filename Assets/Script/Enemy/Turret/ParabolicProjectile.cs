using UnityEngine;

public class ParabolicProjectile : MonoBehaviour
{
    [Header("抛物线炮弹设置")]
    public float speed = 10f;
    public float lifetime = 5f;
    public int damage = 15;
    
    [Header("碰撞检测")]
    public float collisionRadius = 0.5f; // 新增：碰撞检测半径
    
    [Header("视觉效果")]
    public GameObject hitEffect;
    public AudioClip hitSound;
    public AudioClip fireSound;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyLength;
    private float startTime;
    private float height;
    private bool isInitialized = false;
    
    // 新增：反击相关变量
    public bool isReflected = false;
    private Vector3 reflectDirection;
    private float reflectStartTime;

    void Update()
    {
        if (!isInitialized) return;
        
        if (isReflected)
        {
            UpdateReflectedMovement(); // 反弹后的直线飞行逻辑
            return;
        }
        UpdateParabolicMovement(); // 原有抛物线移动逻辑
    }
    
    void UpdateParabolicMovement()
    {
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
        float parabola = 1f - 4f * (fractionOfJourney - 0.5f) * (fractionOfJourney - 0.5f);
        currentPos.y += parabola * height;
        
        transform.position = currentPos;// 更新位置
        
        // 实时碰撞检测：以炮弹当前位置为圆心检测碰撞
        if (CheckCollisionAtCurrentPosition())
        {
            // 检测到碰撞，在当前位置爆炸并销毁
            CreateHitEffect(transform.position);
            Destroy(gameObject);
            return;
        }
        
        // 旋转逻辑保持不变
        if (Time.time - startTime > 0.1f)
        {
            Vector3 moveDirection = (transform.position - startPosition).normalized;
            if (moveDirection != Vector3.zero)
            {
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
        
        // 生命周期检查：如果炮弹飞得太久还没碰到任何东西，销毁它
        if (fractionOfJourney >= 1f || Time.time - startTime > lifetime)
        {
            // 如果到达目标位置还没有击中任何东西，在目标位置创建效果
            CreateHitEffect(targetPosition);
            Destroy(gameObject);
        }
    }
    
    void UpdateReflectedMovement()
    {
        Vector3 newPosition = transform.position + reflectDirection * speed * Time.deltaTime;       // 水平反弹飞行 
        
        // 更新位置
        transform.position = newPosition;
        // 反弹后的实时碰撞检测
        if (CheckCollisionAtCurrentPosition())
        {
            CreateHitEffect(transform.position);
            Destroy(gameObject);
            return;
        }
        
        // 更新旋转
        if (reflectDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(reflectDirection.y, reflectDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        if (Time.time - reflectStartTime > lifetime) Destroy(gameObject);       // 反弹后的生命周期检查
    }
    
    // 实时碰撞检测方法
    bool CheckCollisionAtCurrentPosition()
    {
        LayerMask checkLayerMask;   // 以炮弹当前位置为圆心，检测指定半径内的碰撞体
        if (isReflected)
            checkLayerMask = (1 << LayerMask.NameToLayer("Enemy")) | (1 << LayerMask.NameToLayer("Ground"));
        else
            checkLayerMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ground"));
        
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, collisionRadius, checkLayerMask);
        
        if (hitColliders.Length == 0) return false;         // 如果没有检测到碰撞体，返回false
        foreach (Collider2D collider in hitColliders)       // 对检测到的所有碰撞体应用伤害
        {
            ApplyDamage(collider);
        }
        
        return true; // 检测到碰撞，返回true
    }
    
    public void Initialize(Vector3 target, float projectileSpeed, float projectileHeight)
    {
        startPosition = transform.position;
        targetPosition = target;
        speed = projectileSpeed;
        height = projectileHeight;
        startTime = Time.time;
        
        journeyLength = Vector3.Distance(startPosition, targetPosition);
        isInitialized = true;
    }
    
    // 被弹反时调用的方法
    public void ReflectProjectile(Vector3 reflectDir)
    {
        if (isReflected) return; // 防止多次反弹
        
        isReflected = true;
        reflectDirection = reflectDir;
        reflectDirection.y = 0; // 确保水平反弹
        reflectDirection = reflectDirection.normalized;
        reflectStartTime = Time.time;
        
        gameObject.layer = LayerMask.NameToLayer("PlayerProjectile"); // 设置到玩家炮弹层
        
        if (TryGetComponent<TrailRenderer>(out TrailRenderer trail))// 可以在这里添加反弹视觉效果
        {
            trail.Clear(); // 清除原有轨迹
            Gradient gradient = new Gradient();
            gradient.colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.blue, 0f),
                new GradientColorKey(Color.cyan, 1f)
            };
            trail.colorGradient = gradient; // 改变轨迹颜色
        }
    }
    
    void CreateHitEffect(Vector3 position)
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, position, Quaternion.identity);
        }
        
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position);
        }
    }
    
    void ApplyDamage(Collider2D collider)
    {
        if (isReflected)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.enemyStatus.currentHealth -= damage*10;
                Debug.Log("now health: " + enemy.enemyStatus.currentHealth);
            }
        }
        else
        {
            Player player = collider.GetComponent<Player>();
            if (player != null)
            {
                player.playerStatus.currentHealth -= damage;
            }
        }
    }
    
    // 在Scene视图中绘制轨迹预览和碰撞检测范围
    void OnDrawGizmosSelected()
    {
        if (!isInitialized) return;
        
        Gizmos.color = isReflected ? Color.blue : Color.yellow;
        int segments = 20;
        Vector3 previousPoint = startPosition;
        
        for (int i = 1; i <= segments; i++)
        {
            float fraction = (float)i / segments;
            Vector3 point = Vector3.Lerp(startPosition, targetPosition, fraction);
            float parabola = 1f - 4f * (fraction - 0.5f) * (fraction - 0.5f);
            point.y += parabola * height;
            
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
        
        // 绘制碰撞检测范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
}