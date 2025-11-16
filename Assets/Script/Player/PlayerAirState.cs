public class PlayerAirState : PlayerState
{
    public PlayerAirState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        if (player.isSlamming)
        {
            player.anim.SetFloat("AirState", 2);
        }
        else
        {
            if (rb.velocity.y > 0)
            {
                player.anim.SetFloat("AirState", 0);
            }
            else
            {
                player.anim.SetFloat("AirState", 1);
            }
        }
        
        if (player.isGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }

        if (player.isWallDetected())
        {
            stateMachine.ChangeState(player.wallSlide);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
