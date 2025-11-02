using UnityEngine;

public class StraightProjectile : MonoBehaviour
{
    [Header("直线炮弹设置")]
    public float speed = 8f;
    public float lifetime = 5f;
    public int damage = 10;
    public LayerMask collisionLayers = -1;
    
    [Header("视觉效果")]
    public GameObject hitEffect;
    public AudioClip hitSound;
    public AudioClip fireSound;
    
    private Rigidbody2D rb;
    private float lifeTimer;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
    }
    
    public void Initialize(float projectileSpeed)
    {
        speed = projectileSpeed;
        lifeTimer = lifetime;
        
        // 设置初始速度
        if (rb != null)
        {
            rb.velocity = transform.right * speed;
        }
    }
    
    void Update()
    {
        // 手动移动（如果不用物理系统）
        // transform.position += transform.right * speed * Time.deltaTime;
        
        // 生命周期管理
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查碰撞层
        if (((1 << collision.gameObject.layer) & collisionLayers) != 0)
        {
            // 对玩家造成伤害
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.playerStatus.currentHealth -= damage;
            }
            
            // 播放命中效果
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }
            
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 物理碰撞处理
        if (((1 << collision.gameObject.layer) & collisionLayers) != 0)
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}