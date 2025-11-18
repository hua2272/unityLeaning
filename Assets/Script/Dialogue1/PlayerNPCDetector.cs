using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerNPCDetector : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask npcLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask; // 障碍物图层（如墙壁）
    [SerializeField] private bool showDebug = true;

    public Entity GetClosestVisibleNPC()
    {
        // 1. 先检测范围内的所有NPC
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, npcLayerMask);

        Entity closestNPC = null;
        float minDistance = float.MaxValue;

        // 2. 遍历所有NPC，找到最近的可见目标
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Entity>(out var npc))
            {
                Vector2 direction = npc.transform.position - transform.position;
                float distance = direction.sqrMagnitude; // 用平方距离优化计算

                // 3. 检查视线是否被阻挡
                RaycastHit2D obstacleCheck = Physics2D.Raycast(transform.position, direction.normalized, distance,obstacleLayerMask);
                // 无障碍物且距离更近
                if (obstacleCheck.collider == null && distance < minDistance)
                {
                    minDistance = distance;
                    closestNPC = npc;
                }
            }
        }

        // 调试绘制
        if (showDebug && closestNPC != null)
        {
            Debug.DrawLine(transform.position, closestNPC.transform.position, Color.green, 0.1f);
        }

        return closestNPC;
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
