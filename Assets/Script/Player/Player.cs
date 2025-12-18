using System.Collections;
using UnityEngine;

public class Player : Entity
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
    
    protected override void Awake()
    {
        base.Awake();
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

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
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

    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
        //Time.timeScale = 0f; // TODO 清除页面
        deathMenu.ShowDeathMenu();
    }
    
    public Entity GetClosestVisibleNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, npcLayerMask);      //先检测范围内的所有NPC
        Entity closestNPC = null;
        float minDistance = float.MaxValue;
        
        foreach (var hit in hits)                                                                            //遍历所有NPC，找到最近的可见目标
        {
            if (hit.TryGetComponent<Entity>(out var npc))
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