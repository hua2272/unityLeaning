using System.Collections;
using UnityEngine;

public class FeiLeiShen_Skill : Skill
{
    [Header("Skill Settings")]
    public int skillLevel;
    public GameObject dartPrefab;
    public Transform throwPoint;
    public float throwCooldown = 1.5f;
    public LayerMask enemyLayer;
    
    private bool canThrow = true;
    private bool isTeleporting = false;

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
        
        dart.transform.localScale = new Vector3(player.facingDir, 1, 1);
        
        DartProjectile dartScript = dart.GetComponent<DartProjectile>();
        if (dartScript != null)
        {
            dartScript.Initialize(this, player); // 传递玩家引用给飞镖
        }
        else
        {
            Debug.LogError("Dart prefab is missing DartProjectile component!");
        }
        Invoke(nameof(ResetThrow), throwCooldown);
    }

    public void TriggerTeleport(Vector3 targetPosition)
    {
        if (isTeleporting) return; //已在瞬移中，忽略请求
        
        isTeleporting = true;
        Debug.Log($"开始瞬移到位置: {targetPosition}");
        StartCoroutine(TeleportRoutine(targetPosition));
    }

    private IEnumerator TeleportRoutine(Vector3 target)
    {
        // 添加调试信息
        Debug.Log($"瞬移协程开始: 玩家当前位置 = {player.transform.position}");
        
        // 短暂延迟（视觉过渡）
        yield return new WaitForSeconds(0.1f);
        
        // 确保目标位置有效
        Vector3 finalPosition = CheckSafePosition(target);
        Debug.Log($"安全位置计算完成: {finalPosition}");
        
        // 关键修改：移动玩家对象而不是技能管理器
        player.transform.position = finalPosition;
        Debug.Log($"玩家已瞬移到新位置: {player.transform.position}");
        
        // 短暂无敌时间（可选）
        yield return new WaitForSeconds(0.3f);
        isTeleporting = false;
        
        Debug.Log("瞬移完成");
    }

    private Vector3 CheckSafePosition(Vector3 target)
    {
        // 防止瞬移到墙里
        Collider2D hit = Physics2D.OverlapCircle(target, 0.5f, LayerMask.GetMask("Ground"));
        if (hit == null) return target;
        
        Debug.Log($"目标位置被阻挡，寻找安全位置: {target}");
        
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
            {
                Debug.Log($"找到右侧安全位置: {rightCheck}");
                return rightCheck;
            }
            
            Vector3 leftCheck = origin + Vector3.left * i * 0.5f;
            if (!Physics2D.OverlapCircle(leftCheck, 0.4f, LayerMask.GetMask("Ground")))
            {
                Debug.Log($"找到左侧安全位置: {leftCheck}");
                return leftCheck;
            }
        }
        
        Debug.LogWarning("未找到安全位置，使用原点");
        return origin; // 作为后备
    }

    private void ResetThrow()
    {
        canThrow = true;
    }
}