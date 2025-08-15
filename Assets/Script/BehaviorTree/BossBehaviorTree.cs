using System.Collections.Generic;
using UnityEngine;

public class BossBehaviorTree : BehaviorTree
{
    private BossController bossController;
    
    protected override void Start()
    {
        bossController = GetComponent<BossController>();
        base.Start();
    }
    
    protected override BTNode SetupTree()
    {
        // 获取Blackboard引用
        Blackboard bb = GetBlackboard();
        bb.SetValue("Boss", bossController);
        
        // 定义条件节点
        Condition isPhase1 = new Condition(() => bossController.currentState == BossController.BossState.Phase1);
        Condition isPhase2 = new Condition(() => bossController.currentState == BossController.BossState.Phase2);
        Condition playerInMeleeRangePhase1 = new Condition(() => 
            Vector2.Distance(transform.position, bossController.player.position) < bossController.phase1AttackRange);
        Condition playerInMeleeRangePhase2 = new Condition(() => 
            Vector2.Distance(transform.position, bossController.player.position) < bossController.phase2MeleeRange);
        Condition playerInRangedRangePhase2 = new Condition(() => 
            Vector2.Distance(transform.position, bossController.player.position) > bossController.phase2RangedRange);
        Condition playerOutOfMeleeRangePhase1 = new Condition(() => 
            Vector2.Distance(transform.position, bossController.player.position) > bossController.phase1AttackRange);
        Condition isNotAttacking = new Condition(() => !bossController.isAttacking);
        
        // 定义行为节点
        ActionNode phase1ComboAttack = new ActionNode(() => {
            bossController.StartCoroutine(Phase1ComboAttack());
            return BTNode.NodeState.Success;
        });
        
        ActionNode moveToPlayer = new ActionNode(() => {
            bossController.MoveTowardsPlayer();
            return BTNode.NodeState.Running;
        });
        
        ActionNode hammerAttack = new ActionNode(() => {
            bossController.StartCoroutine(Phase2HammerAttack());
            return BTNode.NodeState.Success;
        });
        
        ActionNode magicAttack = new ActionNode(() => {
            bossController.StartCoroutine(Phase2MagicAttack());
            return BTNode.NodeState.Success;
        });
        
        // 构建行为树
        BTNode phase1Behavior = new Selector(new List<BTNode> {
            new Sequence(new List<BTNode> {
                playerInMeleeRangePhase1,
                isNotAttacking,
                phase1ComboAttack
            }),
            new Sequence(new List<BTNode> {
                playerOutOfMeleeRangePhase1,
                moveToPlayer
            })
        });
        
        BTNode phase2Behavior = new Selector(new List<BTNode> {
            new Sequence(new List<BTNode> {
                playerInMeleeRangePhase2,
                isNotAttacking,
                hammerAttack
            }),
            new Sequence(new List<BTNode> {
                playerInRangedRangePhase2,
                isNotAttacking,
                magicAttack
            }),
            moveToPlayer
        });
        
        return new Selector(new List<BTNode> {
            new Sequence(new List<BTNode> {
                isPhase1,
                phase1Behavior
            }),
            new Sequence(new List<BTNode> {
                isPhase2,
                phase2Behavior
            })
        });
    }
    
    private System.Collections.IEnumerator Phase1ComboAttack()
    {
        bossController.isAttacking = true;
        bossController.animator.SetInteger("AttackType", 0);
        bossController.animator.SetBool("Attacking", true);
        
        for (int i = 0; i < 3; i++)
        {
            bossController.FacePlayer();
            yield return new WaitForSeconds(bossController.phase1AttackDelay);
        }
        
        bossController.animator.SetBool("Attacking", false);
        bossController.isAttacking = false;
    }
    
    private System.Collections.IEnumerator Phase2HammerAttack()
    {
        bossController.isAttacking = true;
        bossController.animator.SetInteger("AttackType", 1);
        bossController.animator.SetBool("Attacking", true);
        
        bossController.FacePlayer();
        yield return new WaitForSeconds(bossController.hammerAttackDelay);
        
        bossController.animator.SetBool("Attacking", false);
        bossController.isAttacking = false;
    }
    
    private System.Collections.IEnumerator Phase2MagicAttack()
    {
        bossController.isAttacking = true;
        bossController.animator.SetInteger("AttackType", 2);
        bossController.animator.SetBool("Attacking", true);
        
        bossController.FacePlayer();
        yield return new WaitForSeconds(bossController.magicAttackDelay / 2);
        
        if (bossController.magicProjectilePrefab && bossController.projectileSpawnPoint)
        {
            Instantiate(bossController.magicProjectilePrefab, 
                       bossController.projectileSpawnPoint.position, 
                       Quaternion.identity);
        }
        
        yield return new WaitForSeconds(bossController.magicAttackDelay / 2);
        
        bossController.animator.SetBool("Attacking", false);
        bossController.isAttacking = false;
    }
}