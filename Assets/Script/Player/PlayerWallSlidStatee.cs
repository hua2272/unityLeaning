using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlideState : PlayerState
{
    public PlayerWallSlideState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            stateMachine.ChangeState(player.wallJump); //change之后仍然会继续执行该脚本剩余部分代码，所以需要return
            return;
        }
        
        rb.velocity = yInput < 0 ? new Vector2(0, rb.velocity.y * 1.1f) : new Vector2(0, rb.velocity.y * 0.7f);
        
        if (xInput != 0 && player.facingDir != xInput || player.isGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exist()
    {
        base.Exist();
    }
}
