using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMoverState : PlayerGroundedState
{
    
    
    public PlayerMoverState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine,  _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }
    
    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        player.SetVelocity(xInput * player.moveSpeed, rb.velocity.y);
        playerEffectManager.dustEffect.StartDust(player.transform.position, player.facingDir);
        
        if (xInput == 0 || player.isWallDetected())
        {
            stateMachine.ChangeState(player.idleState);
            playerEffectManager.dustEffect.StopDust();
        }
    }
    
}
