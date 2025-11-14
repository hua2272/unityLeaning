using UnityEngine;
using System.Collections;

public class GroundSlamSkill : Skill
{
    [Header("技能设置")]
    public float slamSpeed = 25f;           // 下砸速度
    public float slamDamage = 30f;          // 伤害值
    public float detectionRadius = 2f;      // 伤害检测半径
    public LayerMask enemyLayer;            // 敌人层级
    public float verticalOffset = -0.5f;    // 伤害区域在玩家下方的垂直偏移
    
    [Header("输入设置")]
    public float doubleTapTime = 0.3f;      // 双击检测时间
    
    [Header("特效")]
    public GameObject slamEffect;           // 落地特效
    public AudioClip slamSound;             // 落地音效
    
    // 状态变量
    private bool isSlamming = false;
    private float lastSTapTime;
    private int sTapCount = 0;
    
    [SerializeField] private GameObject damageArea;
    
    void Start()
    {
        base.Start();
        //CreateDamageArea();
    }
    
    void Update()
    {
        DetectDoubleTapS();// 检测双击S键
        EndGroundSlamCheck();// 检测下砸结束
        //OnDrawGizmosSelected();
        //player.stateMachine.ChangeState();// 更新动画状态
    }
    
    void CreateDamageArea()
    {
        damageArea.transform.SetParent(player.transform);
        damageArea.transform.localPosition = new Vector3(0, verticalOffset, 0);
        
        CircleCollider2D collider = damageArea.GetComponent<CircleCollider2D>();
        collider.radius = detectionRadius;
        collider.isTrigger = true;
        damageArea.SetActive(false);
    }
    
    void EndGroundSlamCheck()
    {
        if (player.isGroundDetected() && isSlamming)    // 检测落地瞬间
        {
            isSlamming = false;// 停止下砸
            DetectAndDamageEnemies();// 检测并伤害敌人
            damageArea.SetActive(true);// 激活伤害区域（用于视觉效果）
            if (slamEffect != null)// 生成特效
            {
                Instantiate(slamEffect, damageArea.transform.position, Quaternion.identity);
            }
            if (slamSound != null)// 播放音效
            {
                AudioSource.PlayClipAtPoint(slamSound, damageArea.transform.position);
            }
            //StartCoroutine(CameraShake(0.2f, 0.3f));// 屏幕震动（可选）
            Debug.Log("下砸落地！造成伤害");
            StartCoroutine(DeactivateDamageArea());// 延迟关闭伤害区域
        }
    }
    
    // 检测并伤害敌人（只检测一次）
    void DetectAndDamageEnemies()
    {
        // 检测范围内的所有敌人
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(damageArea.transform.position, detectionRadius, enemyLayer);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            // 对敌人造成伤害
            //DealDamageToEnemy(enemy.gameObject);
        }
    }
    
    void DetectDoubleTapS()
    {
        if (player.isGroundDetected() || isSlamming) return;    //只能在空中且不在下砸状态时发动
        if (Input.GetKeyDown(KeyCode.S) )
        {
            if (Time.time - lastSTapTime < doubleTapTime)
            {
                sTapCount++;
                if (sTapCount >= 2)
                {
                    StartGroundSlam();// 触发下砸技能
                    sTapCount = 0;
                }
            }
            else
            {
                sTapCount = 1;
            }
            lastSTapTime = Time.time;
        }
        if (Time.time - lastSTapTime > doubleTapTime) 
            sTapCount = 0;// 重置计数（如果超过双击时间）
    }
    
    void StartGroundSlam()
    {
        isSlamming = true;
        player.SetVelocity(0, -slamSpeed); // 注意：向下应该是负值
        Debug.Log("发动下砸攻击！");
    }
    
    IEnumerator DeactivateDamageArea()
    {
        yield return new WaitForSeconds(0.2f);
        damageArea.SetActive(false);
    }
    
    void OnDrawGizmosSelected()// 可视化伤害范围（在Scene视图中显示）
    {
        if (damageArea != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(damageArea.transform.position, detectionRadius);
        }
    }
}