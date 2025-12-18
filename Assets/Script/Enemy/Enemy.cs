using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected LayerMask whatIsPlayer;

    [Header("Stunned Info")] 
    public float stunDuration;
    public Vector2 stunDirection;
    public bool canBeStunned;
    [SerializeField] protected GameObject counterImage;
    
    [Header("Move Info")]
    public float moveSpeed;
    public float idleTime;
    public float battleTime;

    [Header("Attack info")] 
    public float attackDistance;
    public float attackCooldown;
    [HideInInspector] public float lastTimeAttacked;
    
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
    
    public virtual RaycastHit2D IsPlayerDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, 50, whatIsPlayer);

    public Collider2D[] ConeCast(Vector2 origin, float maxRadius, Vector2 direction, float coneAngle)
    {
        // 先获取圆形区域内的所有玩家碰撞体
        Collider2D[] allColliders = Physics2D.OverlapCircleAll(origin, maxRadius, whatIsPlayer);
    
        // 筛选在锥形角度内的碰撞体
        List<Collider2D> coneColliders = new List<Collider2D>();
    
        foreach (Collider2D collider in allColliders)
        {
            Vector2 toCollider = (Vector2)collider.transform.position - origin;
            float angle = Vector2.Angle(direction, toCollider);
        
            if (angle <= coneAngle / 2)
            {
                coneColliders.Add(collider);
            }
        }
        return coneColliders.ToArray();
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
