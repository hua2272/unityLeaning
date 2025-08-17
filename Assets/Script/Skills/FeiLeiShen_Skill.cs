using System.Collections;
using UnityEngine;

public class FeiLeiShen_Skill : Skill
{
    [Header("Skill Settings")]
    public GameObject dartPrefab;
    public Transform throwPoint;
    public float throwCooldown = 1.5f;
    public LayerMask enemyLayer;
    
    private bool canThrow = true;
    private bool isTeleporting = false;
    //private SpriteRenderer spriteRenderer;
    private Player player;

    void Start()
    {
        player = GetComponentInParent<Player>();
        //spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canThrow && !isTeleporting)
        {
            ThrowDart();
        }
    }

    private void ThrowDart()
    {
        canThrow = false;
        GameObject dart = Instantiate(dartPrefab, throwPoint.position, Quaternion.identity);
        
        // 根据玩家朝向调整飞镖方向
        //dart.transform.localScale = new Vector3(player.facingDir, 1, 1);
        dart.transform.localScale = new Vector3(1, 1, 1);
        
        // 正确初始化飞镖
        DartProjectile dartScript = dart.GetComponent<DartProjectile>();
        if (dartScript != null)
        {
            dartScript.Initialize(this, player);
        }
        else
        {
            Debug.LogError("Dart prefab is missing DartProjectile component!");
        }
        
        Invoke(nameof(ResetThrow), throwCooldown);
    }

    public void TriggerTeleport(Vector3 targetPosition)
    {
        if (isTeleporting) return;
        
        isTeleporting = true;
        StartCoroutine(TeleportRoutine(targetPosition));
    }

    private IEnumerator TeleportRoutine(Vector3 target)
    {
        // 短暂延迟（视觉过渡）
        yield return new WaitForSeconds(0.1f);
        
        // 确保目标位置有效
        Vector3 finalPosition = CheckSafePosition(target);
        transform.position = finalPosition;
        
        // 短暂无敌时间（可选）
        yield return new WaitForSeconds(0.3f);
        isTeleporting = false;
    }

    private Vector3 CheckSafePosition(Vector3 target)
    {
        // 防止瞬移到墙里
        Collider2D hit = Physics2D.OverlapCircle(target, 0.5f, LayerMask.GetMask("Ground"));
        if (hit == null) return target;
        
        // 尝试寻找安全位置
        return FindNearestSafePosition(target);
    }

    private Vector3 FindNearestSafePosition(Vector3 origin)
    {
        // 简单实现：向两侧检测安全位置
        for (int i = 1; i <= 3; i++)
        {
            Vector3 rightCheck = origin + Vector3.right * i * 0.5f;
            if (!Physics2D.OverlapCircle(rightCheck, 0.4f, LayerMask.GetMask("Ground")))
                return rightCheck;
            
            Vector3 leftCheck = origin + Vector3.left * i * 0.5f;
            if (!Physics2D.OverlapCircle(leftCheck, 0.4f, LayerMask.GetMask("Ground")))
                return leftCheck;
        }
        return origin; // 作为后备
    }

    private void ResetThrow()
    {
        canThrow = true;
    }
}