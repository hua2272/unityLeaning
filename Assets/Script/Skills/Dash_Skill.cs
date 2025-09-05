using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash_Skill : Skill
{
    private SpriteRenderer sr;
    private Animator anim;
    private Player player;

    void Start()
    {
        player = PlayerManager.instance.player;
    }
    
    void Update()
    {
        // if (player.isWallDetected())
        // {
        //     return;
        // }
        // SkillManager.instance.dash.CanUseSkill()
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            player.dashDir = player.dashDir == 0 ? player.facingDir : Input.GetAxisRaw("Horizontal");
            player.stateMachine.ChangeState(player.dashState);
        }
    }
}