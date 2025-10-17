using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class ExperienceOrb : MonoBehaviour
{
    [Header("经验值设置")]
    public int expValue = 10; // 每个粒子的经验值
    
    [Header("物理设置")]
    public float bounceForce = 3f;
    public float randomForceRange = 2f;
    public float bounciness = 0.6f;
    
    [Header("视觉设置")]
    public Material glowMaterial;
    public Color orbColor = Color.yellow;
    
    [Header("吸收设置")]
    public float attractRange = 3f;
    public float attractSpeed = 8f;
    public float collectionRange = 0.5f;
    
    private Rigidbody2D rb;
    private Collider2D coll;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Player player;
    private bool isGrounded = false;
    private bool isAttracting = false;
    private PhysicsMaterial2D physicsMaterial;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        player = PlayerManager.instance.player;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        SetupPhysicsMaterial();
        SetupVisuals();
        Vector2 randomDirection = new Vector2(Random.Range(-randomForceRange, randomForceRange), Random.Range(1f, randomForceRange));       //随机弹跳力
        
        rb.AddForce(randomDirection * bounceForce, ForceMode2D.Impulse);
        
        StartCoroutine(CheckGrounded());        //开始检查地面状态
    }
    
    void Update()
    {
        if (isGrounded && !isAttracting)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= attractRange)
            {
                isAttracting = true;
                rb.simulated = false; //2D中禁用物理模拟
                coll.enabled = false;
            }
        }

        if (isAttracting)
        {
            // 向玩家移动
            transform.position = Vector2.MoveTowards(
                transform.position, 
                player.transform.position, 
                attractSpeed * Time.deltaTime
            );
            
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= collectionRange)
            {
                CollectExperience();
            }
        }
    }
    
    IEnumerator CheckGrounded()
    {
        yield return new WaitForSeconds(0.5f); // 等待初始弹跳
        
        int groundedCount = 0;
        const int requiredGroundedFrames = 3; // 需要连续几帧检测到地面才算真正落地
        
        while (!isGrounded)
        {
            // 使用射线检测判断是否落地
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.3f);
            if (hit.collider != null)
            {
                groundedCount++;
                if (groundedCount >= requiredGroundedFrames)
                {
                    isGrounded = true;
                    // 减小阻力，让物体可以继续弹跳
                    rb.drag = 0.3f;
                    rb.angularDrag = 0.5f;
                    break;
                }
            }
            else
            {
                groundedCount = 0; // 重置计数
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    void CollectExperience()
    {
        player.playerStatus.HandleExperienceGained(expValue);   //通知玩家获得经验值
        PlayCollectionEffect();                                 //播放收集效果
        Destroy(gameObject);                                    //销毁粒子
    }
    
    // todo 频繁创建销毁的对象考虑使用对象池优化
    void PlayCollectionEffect()
    {
        // 创建2D收集特效
        GameObject effect = new GameObject("CollectionEffect");
        effect.transform.position = transform.position;
        
        // 添加粒子爆发效果
        ParticleSystem burstParticles = effect.AddComponent<ParticleSystem>();
        var main = burstParticles.main;
        main.startSpeed = 5f;
        main.startLifetime = 0.5f;
        main.startSize = 0.1f;
        main.startColor = orbColor;
        
        // 自动销毁特效
        Destroy(effect, 1f);
    }
    
    // 可视化吸引范围（仅在编辑器中显示）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractRange);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
    }
    
    void SetupVisuals()
    {
        // 设置精灵颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = orbColor;
        }
        // 设置材质
        if (glowMaterial != null)
        {
            spriteRenderer.material = glowMaterial;
        }
        //设置2D灯光颜色
        // Light light = GetComponent<Light>();
        // if (light != null)
        // {
        //     light.color = orbColor;
        // }
    }
    
    void SetupPhysicsMaterial()
    {
        physicsMaterial = new PhysicsMaterial2D();
        physicsMaterial.bounciness = bounciness;
        physicsMaterial.friction = 0.1f;
        
        // 将物理材质应用到碰撞体
        if (coll != null)
        {
            coll.sharedMaterial = physicsMaterial;
        }
    }
}