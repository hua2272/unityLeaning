using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask npcLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask; // 障碍物图层（如墙壁）
    [SerializeField] private bool showDebug = true;

    public bool isBusy { get; private set; }
    public GameObject sword { get; private set; }
    [SerializeField] private DeathMenuController deathMenu;
    
    #region State
    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoverState moveState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerWallSlideState wallSlide { get; private set; }
    public PlayerWallJumpState wallJump { get; private set; }
    public PlayerPrimaryAttackState primaryAttack { get; private set; }
    public playerCounterAttackState counterAttack { get; private set; }
    public PlayerAimSwordState aimSword { get; private set; }
    public PlayerCatchSwordState catchSword { get; private set; }
    public PlayerDeadState deadState { get; private set; }
    #endregion

    [Header("Attack details")]
    public Vector2[] attackMovement;
    public float countAttackDuration = 0.5f;
    
    [Header("Move Info")]
    public float moveSpeed = 10f;
    public float jumpForce;
    public float swordReturnImpact;
    
    [Header("Dash Info")]
    public float dashSpeed;
    public float dashDuration;
    public float dashDir { get; set;}
    
    public bool isSlamming = false;
    
    #region Components
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public EntityFX fx { get; private set; }
    public PlayerStatus playerStatus { get; private set; }
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
    
    void Awake()
    {
        stateMachine = new PlayerStateMachine();
        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoverState(this, stateMachine, "Move");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallSlide = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJump = new PlayerWallJumpState(this, stateMachine, "Jump");
        primaryAttack = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        counterAttack = new playerCounterAttackState(this, stateMachine, "CounterAttack");
        aimSword = new PlayerAimSwordState(this, stateMachine, "AimSword");
        catchSword = new PlayerCatchSwordState(this, stateMachine, "CatchSword");
        deadState = new PlayerDeadState(this, stateMachine, "Die");
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponentInChildren<Rigidbody2D>();
        fx = GetComponentInChildren<EntityFX>();
        playerStatus = GetComponent<PlayerStatus>();
        cd = GetComponent<CapsuleCollider2D>();
        stateMachine.Initialize(idleState);
    }

    void Update()
    {
        stateMachine.currentState.Update();
    }

    public void AssignNewSword(GameObject _newSword)
    {
        sword = _newSword;
    }

    public void CatchTheSword()
    {
        stateMachine.ChangeState(catchSword);
        Destroy(sword);
    }

    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;
        yield return new WaitForSeconds(_seconds);
        isBusy = false;
    }

    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    public void Die()
    {
        stateMachine.ChangeState(deadState);
        //Time.timeScale = 0f; // TODO 清除页面
        deathMenu.ShowDeathMenu();
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
    
    public Enemy GetClosestVisibleNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, npcLayerMask);      //先检测范围内的所有NPC
        Enemy closestNPC = null;
        float minDistance = float.MaxValue;
        
        foreach (var hit in hits)                                                                            //遍历所有NPC，找到最近的可见目标
        {
            if (hit.TryGetComponent<Enemy>(out var npc))
            {
                Vector2 direction = npc.transform.position - transform.position;
                float distance = direction.sqrMagnitude;                                                                //用平方距离优化计算
                
                RaycastHit2D obstacleCheck = Physics2D.Raycast(transform.position, direction.normalized, distance,obstacleLayerMask);   //检查视线是否被阻挡
                if (obstacleCheck.collider == null && distance < minDistance)                                                                    //无障碍物且距离更近
                {
                    minDistance = distance;
                    closestNPC = npc;
                }
            }
        }
        if (showDebug && closestNPC != null)
            Debug.DrawLine(transform.position, closestNPC.transform.position, Color.green, 0.1f);
        return closestNPC;
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}