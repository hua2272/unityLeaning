using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{


    public PlayerGroundedState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        if (playerInputManager.GetButton("Skill_1"))
        {
            stateMachine.ChangeState(player.counterAttack);
        }
        if (!player.isGroundDetected())
        {
            stateMachine.ChangeState(player.airState);
        }
        if (playerInputManager.GetButton("Jump") && player.isGroundDetected())
        {
            rb.velocity = new Vector2(rb.velocity.x, player.jumpForce);
        }
        if (playerInputManager.GetButtonDown("Attack_1"))
        {
            stateMachine.ChangeState(player.primaryAttack);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}