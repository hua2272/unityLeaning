using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Stunned Info")] 
    public float stunDuration;
    public Vector2 stunDirection;
    public bool canBeStunned;
    [SerializeField] protected GameObject counterImage;
    
    [Header("Move Info")]
    public float moveSpeed;
    public float idleTime;
    public float battleTime;
    
    [Header("Detect Info")]
    public float radius = 10;
    public float coneAngle = 60;

    [Header("Attack info")] 
    public float attackDistance;
    public float attackCooldown;
    [HideInInspector] public float lastTimeAttacked;
    
    [Header("攻击设置")]
    public Transform firePoint;
    public GameObject straightProjectilePrefab;
    public GameObject parabolicProjectilePrefab;
    
    [Header("目标设置")]
    public Transform playerTarget;
    
    [Header("直线炮弹设置")]
    public float straightFireRate = 2f;
    public float straightProjectileSpeed = 8f;
    
    [Header("抛物线炮弹设置")]
    public float parabolicFireRate = 3f;
    public float parabolicProjectileSpeed = 10f;
    public float parabolicHeight = 3f;
    
    public EnemyStateMachine stateMachine { get; private set; }
    public string lastAnimBoolName { get; private set; }
    
    #region Components
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public EntityFX fx { get; private set; }
    public EnemyStatus enemyStatus { get; private set; }
    public CapsuleCollider2D cd { get; private set; }
    #endregion
    
    [Header("Knockback Info")]
    [SerializeField] protected Vector2 knockbackDirection;
    [SerializeField] protected float knockbackDuration;
    protected bool isKnocked;
    
    [Header("Collision Info")]
    public Transform attackCheck;
    public float attackCheckRadius;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance;
    [SerializeField] protected LayerMask whatIsGround;
    
    [SerializeField] public int npcId;
    
    public int facingDir { get; private set; } = 1;
    protected bool facingRight = true;

    protected virtual void Awake()
    {
        stateMachine = new EnemyStateMachine();
    }

    protected virtual void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponentInChildren<Rigidbody2D>();
        fx = GetComponentInChildren<EntityFX>();
        enemyStatus = GetComponent<EnemyStatus>();
        cd = GetComponent<CapsuleCollider2D>();
    }

    protected virtual void Update()
    {
        stateMachine.currentState.Update();
    }
    
    public void FireStraightProjectile()
    {
        if (straightProjectilePrefab == null || firePoint == null) return;
        
        GameObject projectile = Instantiate(straightProjectilePrefab, firePoint.position, firePoint.rotation);
        StraightProjectile straightScript = projectile.GetComponent<StraightProjectile>();
        
        if (straightScript != null)
        {
            straightScript.Initialize(straightProjectileSpeed);
        }
    }
    
    public void FireParabolicProjectile()
    {
        if (parabolicProjectilePrefab == null || firePoint == null || playerTarget == null) return;
        
        GameObject projectile = Instantiate(parabolicProjectilePrefab, firePoint.position, Quaternion.identity);
        ParabolicProjectile parabolicScript = projectile.GetComponent<ParabolicProjectile>();
        
        if (parabolicScript != null)
        {
            parabolicScript.Initialize(playerTarget.position, parabolicProjectileSpeed, parabolicHeight);
        }
    }

    public virtual void AssignLastAnimName(string animBoolName)
    {
        lastAnimBoolName = animBoolName;
    }

    public virtual void OpenCounterAttackWindow()
    {
        canBeStunned = true;
        counterImage.SetActive(true);
    }

    public virtual void CloseCounterAttackWindow()
    {
        canBeStunned = false;
        counterImage.SetActive(false);
    }
    
    public virtual bool ActiveCounterImage()
    {
        return counterImage != null && counterImage.activeInHierarchy;
    }

    public virtual void EnterStunnedState()
    {
        CloseCounterAttackWindow();
    }
    
    public virtual void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    
    public virtual RaycastHit2D IsPlayerDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, 50, playerMask);

    public RaycastHit2D IsPlayerDetectedInCone(Transform center, Vector2 direction)
    {
        Vector2 centerPos = center.position;
        float halfAngle = coneAngle * 0.5f;
        float cosHalfAngle = Mathf.Cos(halfAngle * Mathf.Deg2Rad);
    
        // 1. 直接使用圆形检测（最简单）
        Collider2D[] results = new Collider2D[10];
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            centerPos,
            radius,
            results,
            playerMask
        );
    
        RaycastHit2D closestHit = new RaycastHit2D();
        float closestDistance = Mathf.Infinity;
    
        // 2. 快速筛选
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D playerCollider = results[i];
            Vector2 playerPos = playerCollider.transform.position;
            Vector2 toPlayer = (playerPos - centerPos);
        
            // 快速距离平方检查
            float sqrDistance = toPlayer.sqrMagnitude;
            if (sqrDistance > radius * radius) continue;
        
            // 快速角度检查（使用点积）
            toPlayer.Normalize();
            if (Vector2.Dot(direction, toPlayer) >= cosHalfAngle)
            {
                float distance = Mathf.Sqrt(sqrDistance);
            
                // 障碍物检测
                if (!Physics2D.Raycast(centerPos, toPlayer, distance, obstacleMask))
                {
                    // 最后的精确检测
                    RaycastHit2D hit = Physics2D.Raycast(
                        centerPos, 
                        toPlayer, 
                        distance, 
                        playerMask
                    );
                
                    if (hit.collider != null && distance < closestDistance)
                    {
                        closestHit = hit;
                        closestDistance = distance;
                    }
                }
            }
        }
    
        return closestHit;
    }

    
    public virtual void DamageEffect()
    {
        fx.StartCoroutine("FlashFX");
        StartCoroutine("HitKnockback");
        Debug.Log(gameObject.name + " was damaged !");
    }

    protected virtual IEnumerator HitKnockback()
    {
        isKnocked = true;
        rb.velocity = new Vector2(knockbackDirection.x * -facingDir, knockbackDirection.y);
        yield return new WaitForSeconds(knockbackDuration);
        isKnocked = false;
    }

    #region Collision
    public virtual bool isGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    public virtual bool isWallDetected() => Physics2D.Raycast(groundCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));
        Gizmos.DrawWireSphere(attackCheck.position, attackCheckRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance * facingDir, transform.position.y));
    }
    #endregion
    
    
    public virtual void Flip()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    public virtual void FlipController(float _xVelocity)
    {
        if (_xVelocity < 0 && facingRight || _xVelocity > 0 && !facingRight) Flip();
    }
    
    
    public void ZeroVelocity()
    {
        if (isKnocked) return;
        rb.velocity = new Vector2(0, 0);
    } 
    
    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (isKnocked) return;
        rb.velocity = new Vector2(xVelocity, yVelocity);
        FlipController(xVelocity);
    }

    public virtual void Die() {}
}
