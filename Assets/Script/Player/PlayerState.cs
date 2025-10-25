using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected Player player;
    protected Rigidbody2D rb;
    protected PlayerEffectManager playerEffectManager;
    protected PlayerInputManager playerInputManager;
    
    protected float xInput;
    protected float yInput;
    private string animBoolName;
    
    protected float stateTimer;
    protected bool triggerCalled;

    public PlayerState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
    {
        this.player = _player;
        this.stateMachine = _stateMachine;
        this.animBoolName = _animBoolName;
    }

    public virtual void Enter()
    {
        player.anim.SetBool(animBoolName, true);
        rb = player.rb;
        triggerCalled = false;
        playerEffectManager = PlayerEffectManager.instance;
        playerInputManager = PlayerInputManager.instance;
    }
    
    public virtual void Update()
    {
        // Vector2 movement = player.customInputSystem.GetMovementAxis();
        Vector2 movement = playerInputManager.GetMovementAxis();
        xInput = movement.x;
        yInput = movement.y;
        stateTimer -= Time.deltaTime;
        // xInput = Input.GetAxisRaw("Horizontal");
        // yInput = Input.GetAxisRaw("Vertical");
        player.anim.SetFloat("yVelocity", rb.velocity.y);
    }
    
    public virtual void Exist()
    {
        player.anim.SetBool(animBoolName, false);
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
}