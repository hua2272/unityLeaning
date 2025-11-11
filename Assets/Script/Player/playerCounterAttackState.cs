using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCounterAttackState : PlayerState
{
    public playerCounterAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.anim.SetBool("SuccessfulCounterAttack", false);
        player.anim.SetBool("FailCounterAttack", false);
    }

    public override void Update()
    {
        base.Update();
        player.ZeroVelocity();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius);
        foreach (Collider2D hit in colliders)
        {
            if (hit.GetComponent<Enemy>() ==null) continue;
            if (!hit.GetComponent<Enemy>().ActiveCounterImage()) continue;
            if (playerInputManager.GetButtonDown("Attack_1") && hit.GetComponent<Enemy>().CanBeCounter())
            {
                player.anim.SetBool("SuccessfulCounterAttack", true);
                hit.GetComponent<Enemy>().EnterStunnedState();
            }
            if (!playerInputManager.GetButtonDown("Attack_1"))
            {
                player.anim.SetBool("FailCounterAttack", true);
            }
        }
        if (triggerCalled || playerInputManager.GetButtonUp("Skill_1"))
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
