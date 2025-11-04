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
        if (Input.GetKeyDown(KeyCode.Mouse1) && HasNoSword())
        {
            stateMachine.ChangeState(player.aimSword);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            stateMachine.ChangeState(player.CounterAttack);
        }
        if (!player.isGroundDetected())
        {
            stateMachine.ChangeState(player.airState);
        }
        if (playerInputManager.GetButton("Jump") && player.isGroundDetected())
        {
            stateMachine.ChangeState(player.jumpState);
        }
        if (playerInputManager.GetButtonDown("Attack1"))
        {
            stateMachine.ChangeState(player.primaryAttack);
        }
    }

    private bool HasNoSword()
    {
        if (!player.sword)
        {
            return true;
        }
        player.sword.GetComponent<Sword_Skill_Controller>().ReturnSword();
        return false;
    }

    public override void Exist()
    {
        base.Exist();
    }
}