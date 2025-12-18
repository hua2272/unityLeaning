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
        // player.anim.SetBool("SuccessfulCounterAttack", false);
        // player.anim.SetBool("FailCounterAttack", false);
        player.anim.SetFloat("CounterAttackState", 0);
    }

    public override void Update()
    {
        base.Update();
        player.ZeroVelocity();
    
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius);
    
        bool foundCounterableTarget = false;
    
        foreach (Collider2D hit in colliders)
        {
            // 检查敌人反击
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && enemy.ActiveCounterImage())
            {
                foundCounterableTarget = true;
                if (!counterWindowStarted)
                {
                    counterWindowStarted = true;
                    counterTimer = 0f;
                }
            
                if (counterTimer <= 0.5f && playerInputManager.GetButtonDown("Attack_1") && enemy.canBeStunned)
                {
                    player.anim.SetFloat("CounterAttackState", 1);
                    enemy.EnterStunnedState();
                    hasCountered = true;
                    break;
                }
            }
            
            // 新增：检查炮弹反击
            ParabolicProjectile projectile = hit.GetComponent<ParabolicProjectile>();
            if (projectile != null && !projectile.isReflected) // 确保没有被反弹过
            {
                foundCounterableTarget = true;
                if (!counterWindowStarted)
                {
                    counterWindowStarted = true;
                    counterTimer = 0f;
                }
                
                if (counterTimer <= 0.5f && playerInputManager.GetButtonDown("Attack_1"))
                {
                    player.anim.SetFloat("CounterAttackState", 1);
                    ReflectProjectile(projectile);
                    hasCountered = true;
                    break;
                }
            }
        }
    
        // 更新计时器
        if (counterWindowStarted)
        {
            counterTimer += Time.deltaTime;
        
            if (counterTimer > 0.5f && !hasCountered && !hasFailed && foundCounterableTarget)
            {
                player.anim.SetFloat("CounterAttackState", 2);
                hasFailed = true;
            }
        }
    
        // 不在反击窗口期内按下Attack_1直接切换状态
        if (playerInputManager.GetButtonDown("Attack_1") && 
            (!counterWindowStarted || counterTimer > 0.5f) && 
            !hasCountered)
        {
            stateMachine.ChangeState(player.primaryAttack);
            return;
        }
    
        if (triggerCalled || playerInputManager.GetButtonUp("Skill_1"))
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
    
    // 新增：反弹炮弹方法
    private void ReflectProjectile(ParabolicProjectile projectile)
    {
        // 计算反弹方向（水平方向，基于玩家面向方向）
        Vector3 reflectDirection = player.facingDir == 1 ? Vector3.right : Vector3.left;
        
        // 可以添加一些随机偏移让反弹更有趣
        float randomOffset = Random.Range(-0.2f, 0.2f);
        reflectDirection += Vector3.up * randomOffset;
        
        // 调用炮弹的反弹方法
        projectile.ReflectProjectile(reflectDirection);
        
        // 可以在这里添加反击成功的音效或视觉效果
        Debug.Log("成功反弹炮弹！");
    }

    public override void Exit()
    {
        base.Exit();
    }
}