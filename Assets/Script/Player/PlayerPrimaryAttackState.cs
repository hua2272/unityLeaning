using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttackState : PlayerState
{
    
    private int comboCounter;
    private float lastTimeAttacked;
    private float comboWindow = 2;
    
    public PlayerPrimaryAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        comboCounter = comboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow ? 0 : comboCounter;
        player.anim.SetInteger("ComboCounter", comboCounter);
        float attackDir = xInput == 0 ? player.facingDir : xInput;
        player.SetVelocity(player.attackMovement[comboCounter].x * attackDir, player.attackMovement[comboCounter].y);
        stateTimer = 0.1f; //奔跑后攻击由于惯性攻击有延迟
        player.anim.speed = 1.2f;
    }

    public override void Update()
    {
        base.Update();
        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
        if (stateTimer < 0)
        {
            player.ZeroVelocity();
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.StartCoroutine("BusyFor", .15f);
        comboCounter++;
        lastTimeAttacked = Time.time;
        player.anim.speed = 1f;
    }
}
