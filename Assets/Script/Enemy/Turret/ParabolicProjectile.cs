using UnityEngine;

public class ParabolicProjectile : MonoBehaviour
{
    [Header("抛物线炮弹设置")]
    public float speed = 10f;
    public float lifetime = 5f;
    public int damage = 15;
    public LayerMask collisionLayers = -1;
    
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
            // 反弹后的直线飞行逻辑
            UpdateReflectedMovement();
            return;
        }
        
        // 原有抛物线移动逻辑
        UpdateParabolicMovement();
    }
    
    void UpdateParabolicMovement()
    {
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        
        if (fractionOfJourney >= 1f)
        {
            ReachDestination();
            return;
        }
        
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
        float parabola = 1f - 4f * (fractionOfJourney - 0.5f) * (fractionOfJourney - 0.5f);
        currentPos.y += parabola * height;
        
        transform.position = currentPos;
        UpdateRotation();
        
        if (Time.time - startTime > lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    void UpdateReflectedMovement()
    {
        // 水平反弹飞行 
        Vector3 newPosition = transform.position + reflectDirection * speed * Time.deltaTime;
        
        // 更新位置和旋转
        transform.position = newPosition;
        UpdateReflectedRotation();
        
        // 反弹后的生命周期检查
        if (Time.time - reflectStartTime > lifetime)
        {
            Destroy(gameObject);
        }
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
    
    // 新增：被反击时调用的方法
    public void ReflectProjectile(Vector3 reflectDir)
    {
        if (isReflected) return; // 防止多次反弹
        
        isReflected = true;
        reflectDirection = reflectDir;
        reflectDirection.y = 0; // 确保水平反弹
        reflectDirection = reflectDirection.normalized;
        reflectStartTime = Time.time;
        
        // 重置碰撞检测，确保反弹后能击中敌人
        gameObject.layer = LayerMask.NameToLayer("PlayerProjectile"); // 设置到玩家炮弹层
        
        // 可以在这里添加反弹视觉效果
        if (TryGetComponent<TrailRenderer>(out TrailRenderer trail))
        {
            trail.Clear(); // 清除原有轨迹
            trail.colorGradient = CreateReflectedTrailColor(); // 改变轨迹颜色
        }
    }
    
    Gradient CreateReflectedTrailColor()
    {
        Gradient gradient = new Gradient();
        gradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.blue, 0f),
            new GradientColorKey(Color.cyan, 1f)
        };
        return gradient;
    }
    
    void UpdateRotation()
    {
        if (Time.time - startTime > 0.1f)
        {
            Vector3 moveDirection = (transform.position - startPosition).normalized;
            if (moveDirection != Vector3.zero)
            {
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }
    
    void UpdateReflectedRotation()
    {
        // 反弹后的旋转，保持水平
        if (reflectDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(reflectDirection.y, reflectDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    void ReachDestination()
    {
        CreateHitEffect(targetPosition);
        CheckCollisionAtPosition(targetPosition);
        Destroy(gameObject);
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
    
    void CheckCollisionAtPosition(Vector3 position)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, 1f, collisionLayers);
        foreach (Collider2D collider in hitColliders)
        {
            ApplyDamage(collider);
        }
    }
    
    void ApplyDamage(Collider2D collider)
    {
        // 根据是否反弹应用不同的伤害逻辑
        if (isReflected)
        {
            // 反弹后击中敌人
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 这里假设Enemy有TakeDamage方法，根据你的实际代码调整
                //enemy.TakeDamage(damage * 2); // 反弹后伤害加倍
            }
        }
        else
        {
            // 原始状态击中玩家
            Player player = collider.GetComponent<Player>();
            if (player != null)
            {
                player.playerStatus.currentHealth -= damage;
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        int collisionLayer = 1 << collision.gameObject.layer;
        
        if ((collisionLayer & collisionLayers) != 0)
        {
            // 如果是反弹状态，只对敌人层做出反应
            if (isReflected)
            {
                Enemy enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    CreateHitEffect(transform.position);
                    ApplyDamage(collision);
                    Destroy(gameObject);
                }
            }
            else
            {
                // 原始状态对玩家层做出反应
                Player player = collision.GetComponent<Player>();
                if (player != null)
                {
                    CreateHitEffect(transform.position);
                    ApplyDamage(collision);
                    Destroy(gameObject);
                }
            }
        }
    }
    
    // 在Scene视图中绘制轨迹预览
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
    }
}