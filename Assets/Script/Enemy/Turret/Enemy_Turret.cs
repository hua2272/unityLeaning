using UnityEngine;
using System.Collections;

public class Enemy_Turret : Enemy
{
    private float straightFireTimer;
    private float parabolicFireTimer;
    
    
    void Start()
    {
        playerTarget = PlayerManager.instance.player.transform;
        straightFireTimer = straightFireRate;
        parabolicFireTimer = parabolicFireRate;
    }
    
    void Update()
    {
        if (playerTarget == null) return;
        
        // 更新发射计时器
        straightFireTimer -= Time.deltaTime;
        parabolicFireTimer -= Time.deltaTime;
        
        /*if (straightFireTimer <= 0f)                // 发射直线炮弹
        { 
            FireStraightProjectile();
            straightFireTimer = straightFireRate;
        }*/
        
        if (parabolicFireTimer <= 0f)               // 发射抛物线炮弹
        {
            FireParabolicProjectile();
            parabolicFireTimer = parabolicFireRate;
        }
    }
    
    void OnDrawGizmosSelected()                     // 在Scene视图中显示发射点和攻击范围
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 2f);
        }
    }
}