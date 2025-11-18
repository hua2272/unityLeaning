using UnityEngine;
using System.Collections;

public class GroundSlamSkill : Skill
{
    [Header("输入设置")]
    public float comboTimeWindow = 0.2f; // 组合键时间窗口

    private float lastSPressTime = -1f;
    private float lastJPressTime = -1f;
    [Header("技能设置")]
    public float slamSpeed = 25f;           // 下砸速度
    public float slamDamage = 30f;          // 伤害值
    public float detectionRadius = 2f;      // 伤害检测半径
    public LayerMask targetLayer;            // 敌人层级
    public float verticalOffset = -0.5f;    // 伤害区域在玩家下方的垂直偏移
    
    [Header("输入设置")]
    public float doubleTapTime = 0.3f;      // 双击检测时间
    
    [Header("特效")]
    public GameObject slamEffect;           // 落地特效
    public AudioClip slamSound;             // 落地音效
    
    // 状态变量
    public bool isSlamming = false;
    private float lastSTapTime;
    private int sTapCount = 0;
    
    [SerializeField] private GameObject damageArea;
    
    void Start()
    {
        base.Start();
        CreateDamageArea();
    }
    
    void Update()
    {
        DetectSJComboWithTimeWindow();// 检测双击S键
        EndGroundSlamCheck();// 检测下砸结束
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
            player.isSlamming = false;
            isSlamming = false;// 停止下砸
            DetectAndDamage();// 检测并伤害
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
    
    void DetectAndDamage()// 检测并伤害目标（只检测一次）
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(damageArea.transform.position, detectionRadius, targetLayer);
        foreach (Collider2D obj in targets)
        {
            if (obj.CompareTag("DestructibleTile"))
            {
                DestructibleTileController tileController = obj.GetComponentInParent<DestructibleTileController>();
                if (tileController == null)
                {
                    Debug.Log($"wei找到DestructibleTileController，对 {obj.name} 造成伤害");
                }
                tileController.TakeDamage(1);
            }
        }
    }
    
    void DetectSJComboWithTimeWindow()
    {
        if (player.isGroundDetected() || isSlamming) return;// 只能在空中且不在下砸状态时发动
        // 记录按键时间
        if (Input.GetKeyDown(KeyCode.S))
        {
            lastSPressTime = Time.time;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            lastJPressTime = Time.time;
        }
    
        // 检查是否在时间窗口内按下了两个键
        if (lastSPressTime > 0 && lastJPressTime > 0)
        {
            float timeDiff = Mathf.Abs(lastSPressTime - lastJPressTime);
            if (timeDiff <= comboTimeWindow)
            {
                Debug.Log("S+J组合键，发动下砸！");
                player.isSlamming = true;
                isSlamming = true;
                player.SetVelocity(0, -slamSpeed); // 注意：向下应该是负值
                lastSPressTime = -1f;// 重置时间，防止连续触发
                lastJPressTime = -1f;
            }
            else if (timeDiff > comboTimeWindow)
            {
                // 超过时间窗口，重置较旧的那个按键时间
                if (lastSPressTime < lastJPressTime)
                    lastSPressTime = -1f;
                else
                    lastJPressTime = -1f;
            }
        }
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