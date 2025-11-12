using UnityEngine;
using System.Collections;

public class Enemy_Turret : Enemy
{
    [Header("攻击设置")]
    public Transform firePoint;
    public GameObject straightProjectilePrefab;
    public GameObject parabolicProjectilePrefab;
    
    [Header("直线炮弹设置")]
    public float straightFireRate = 2f;
    public float straightProjectileSpeed = 8f;
    
    [Header("抛物线炮弹设置")]
    public float parabolicFireRate = 3f;
    public float parabolicProjectileSpeed = 10f;
    public float parabolicHeight = 3f;
    
    [Header("目标设置")]
    private Transform playerTarget;
    
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
        
        // 发射直线炮弹
        if (straightFireTimer <= 0f)
        {
            // FireStraightProjectile();
            // straightFireTimer = straightFireRate;
        }
        
        // 发射抛物线炮弹
        if (parabolicFireTimer <= 0f)
        {
            FireParabolicProjectile();
            parabolicFireTimer = parabolicFireRate;
        }
    }
    
    void FireStraightProjectile()
    {
        if (straightProjectilePrefab == null || firePoint == null) return;
        
        GameObject projectile = Instantiate(straightProjectilePrefab, firePoint.position, firePoint.rotation);
        StraightProjectile straightScript = projectile.GetComponent<StraightProjectile>();
        
        if (straightScript != null)
        {
            straightScript.Initialize(straightProjectileSpeed);
        }
    }
    
    void FireParabolicProjectile()
    {
        if (parabolicProjectilePrefab == null || firePoint == null || playerTarget == null) return;
        
        GameObject projectile = Instantiate(parabolicProjectilePrefab, firePoint.position, Quaternion.identity);
        ParabolicProjectile parabolicScript = projectile.GetComponent<ParabolicProjectile>();
        
        if (parabolicScript != null)
        {
            parabolicScript.Initialize(playerTarget.position, parabolicProjectileSpeed, parabolicHeight);
        }
    }
    
    // 在Scene视图中显示发射点和攻击范围
    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 2f);
        }
    }
}