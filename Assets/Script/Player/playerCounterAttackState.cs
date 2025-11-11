using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCounterAttackState : PlayerState
{
    private float counterTimer = 0f;
    private bool counterWindowStarted = false;
    private bool hasCountered = false;
    private bool hasFailed = false;
    public playerCounterAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        counterTimer = 0f;
        counterWindowStarted = false;
        hasCountered = false;
        hasFailed = false;
        player.anim.SetBool("SuccessfulCounterAttack", false);
        player.anim.SetBool("FailCounterAttack", false);
    }

    public override void Update()
    {
        base.Update();
        player.ZeroVelocity();
    
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius);
        bool foundCounterableEnemy = false;
    
        foreach (Collider2D hit in colliders)
        {
            if (hit.GetComponent<Enemy>() == null) continue;
            if (!hit.GetComponent<Enemy>().ActiveCounterImage()) continue;
        
            foundCounterableEnemy = true;
        
            // 如果是第一次发现可反击的敌人，开始计时
            if (!counterWindowStarted)
            {
                counterWindowStarted = true;
                counterTimer = 0f;
            }
        
            // 如果计时器在0.5秒内且按下攻击键且敌人可被反击
            if (counterTimer <= 0.5f && playerInputManager.GetButtonDown("Attack_1") && hit.GetComponent<Enemy>().CanBeCounter())
            {
                player.anim.SetBool("SuccessfulCounterAttack", true);
                hit.GetComponent<Enemy>().EnterStunnedState();
                hasCountered = true;
                break; // 成功反击后跳出循环
            }
        }
    
        // 更新计时器（只有在反击窗口开始后才计时）
        if (counterWindowStarted)
        {
            counterTimer += Time.deltaTime;
        
            // 如果超过0.5秒且没有成功反击，触发失败
            if (counterTimer > 0.5f && !hasCountered && !hasFailed)
            {
                player.anim.SetBool("FailCounterAttack", true);
                hasFailed = true;
            }
        }
    
        // 新增功能：不在反击窗口期内按下Attack_1直接切换状态
        if (playerInputManager.GetButtonDown("Attack_1") && 
            (!counterWindowStarted || counterTimer > 0.5f) && 
            !hasCountered)
        {
            // 切换到攻击状态或其他你指定的状态
            stateMachine.ChangeState(player.primaryAttack);
            return; // 直接返回，避免执行后续逻辑
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
