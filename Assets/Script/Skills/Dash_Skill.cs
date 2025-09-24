using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash_Skill : Skill
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            player.dashDir = player.dashDir == 0 ? player.facingDir : Input.GetAxisRaw("Horizontal");
            player.stateMachine.ChangeState(player.dashState);
        }
    }
}