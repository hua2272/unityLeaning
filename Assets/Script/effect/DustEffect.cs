using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustEffect : MonoBehaviour
{
    public Animator dustEffectsAnimator { get; private set; }
    private Player player;
    public Vector3 positionOffset;

    private void Start()
    {
        dustEffectsAnimator = GetComponent<Animator>();
        player = PlayerManager.instance.player;
    }
    
    private void Update()
    {
        // 计算偏移：水平偏移根据Player的朝向，垂直偏移不变
        Vector3 offset = new Vector3(positionOffset.x * player.facingDir, positionOffset.y, positionOffset.z);
        
        // 更新位置：Player的位置加上偏移
        transform.position = player.transform.position + offset;
        
        if (player.facingDir < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    public void StartDust()
    {
        dustEffectsAnimator.SetTrigger("StartDust");
    }
    
    public void StopDust()
    {
        dustEffectsAnimator.SetTrigger("StopDust");
    }
}
