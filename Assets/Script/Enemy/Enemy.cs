using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Entity
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

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();
    }

    protected override void Update()
    {
        base.Update();
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
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance * facingDir, transform.position.y));
    }
}
