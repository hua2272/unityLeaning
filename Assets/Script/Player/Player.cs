using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Entity
{

    public bool isBusy { get; private set; }
    public SkillManager skill { get; private set; }
    public GameObject sword { get; private set; }
    public DialogueManager dialogueManager;
    
    #region State
    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoverState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerWallSlideState wallSlide { get; private set; }
    public PlayerWallJumpState wallJump { get; private set; }
    public PlayerPrimaryAttackState primaryAttack { get; private set; }
    public playerCounterAttackState CounterAttack { get; private set; }
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
    public float dashDir { get; private set;}
    
    private PlayerNPCDetector npcDetector;
    
    protected override void Awake()
    {
        base.Awake();
        stateMachine = new PlayerStateMachine();
        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoverState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallSlide = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJump = new PlayerWallJumpState(this, stateMachine, "Jump");
        primaryAttack = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        CounterAttack = new playerCounterAttackState(this, stateMachine, "CounterAttack");
        aimSword = new PlayerAimSwordState(this, stateMachine, "AimSword");
        catchSword = new PlayerCatchSwordState(this, stateMachine, "CatchSword");
        deadState = new PlayerDeadState(this, stateMachine, "Die");
    }

    protected override void Start()
    {
        base.Start();
        skill = SkillManager.instance;
        stateMachine.Initialize(idleState);
        dialogueManager = DialogueManager.Instance;
        npcDetector = GetComponent<PlayerNPCDetector>();
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();
        Check4DashInput();
        Check4Talk();
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

    private void Check4DashInput()
    {
        if (isWallDetected())
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && SkillManager.instance.dash.CanUseSkill())
        {
            dashDir = dashDir == 0 ? facingDir : Input.GetAxisRaw("Horizontal");
            stateMachine.ChangeState(dashState);
        }
    }

    private void Check4Talk()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            var closestNPC = npcDetector.GetClosestVisibleNPC();
            if (closestNPC != null)
            {
                Debug.Log($"与最近的NPC交互 ID: {closestNPC.npcId}");
                if (dialogueManager != null)
                {
                    dialogueManager.StartDialogue(closestNPC.npcId);
                }
                else
                {
                    Debug.LogError("DialogueManager.Instance is null!");
                }
            }
        }
    }

    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
    }
}