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
    //public TrailRenderer trailRenderer;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyLength;
    private float startTime;
    private float height;
    private bool isInitialized = false;
    
    void Update()
    {
        if (!isInitialized) return;
        
        // 计算移动进度
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        
        if (fractionOfJourney >= 1f)
        {
            // 到达目标位置
            ReachDestination();
            return;
        }
        
        // 计算当前位置（包含抛物线高度）
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
        float parabola = 1f - 4f * (fractionOfJourney - 0.5f) * (fractionOfJourney - 0.5f);
        currentPos.y += parabola * height;
        
        // 更新位置和朝向
        transform.position = currentPos;
        UpdateRotation();
        
        // 生命周期检查
        if (Time.time - startTime > lifetime)
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
    
    void UpdateRotation()
    {
        // 根据移动方向更新旋转
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
    
    void ReachDestination()
    {
        // 在目标位置创建爆炸效果
        if (hitEffect != null)
        {
            Instantiate(hitEffect, targetPosition, Quaternion.identity);
        }
        
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, targetPosition);
        }
        
        // 检查目标位置的碰撞
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(targetPosition, 1f, collisionLayers);
        foreach (Collider2D collider in hitColliders)
        {
            Player player = collider.GetComponent<Player>();
            if (player != null)
            {
                player.playerStatus.currentHealth -= damage;
            }
        }
        
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 中途碰撞检测
        if (((1 << collision.gameObject.layer) & collisionLayers) != 0)
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.playerStatus.currentHealth -= damage;
            }
            
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
    
    // 在Scene视图中绘制轨迹预览
    void OnDrawGizmosSelected()
    {
        if (!isInitialized) return;
        
        Gizmos.color = Color.yellow;
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