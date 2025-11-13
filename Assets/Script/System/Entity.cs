using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    
    #region Components
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public EntityFX fx { get; private set; }
    public SpriteRenderer sr { get; private set; }
    public PlayerStatus playerStatus { get; private set; }
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

    public System.Action onFlipped = () => { };
    
    protected virtual void Awake() {}
    
    protected virtual void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponentInChildren<Rigidbody2D>();
        fx = GetComponentInChildren<EntityFX>();
        playerStatus = GetComponent<PlayerStatus>();
        enemyStatus = GetComponent<EnemyStatus>();
        cd = GetComponent<CapsuleCollider2D>();
    }   
    
    protected virtual void Update() {}

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
    }
    #endregion
    
    #region Flip
    public virtual void Flip()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
        onFlipped();
    }

    public virtual void FlipController(float _xVelocity)
    {
        if (_xVelocity < 0 && facingRight || _xVelocity > 0 && !facingRight)
        {
            Flip();
        }
    }
    #endregion
    
    #region Velocity

    public void ZeroVelocity()
    {
        if (isKnocked)
        {
            return;
        }
        rb.velocity = new Vector2(0, 0);
    } 
    
    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (isKnocked)
        {
            return;
        }
        rb.velocity = new Vector2(xVelocity, yVelocity);
        FlipController(xVelocity);
    }
    
    #endregion

    public virtual void Die() {}
}
