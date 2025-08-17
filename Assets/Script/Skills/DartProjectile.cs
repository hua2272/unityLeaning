using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartProjectile : MonoBehaviour
{
[Header("Dart Settings")]
    public float speed = 25f;
    public float maxDistance = 8f;
    public int damage = 15;
    
    private Vector3 startPosition;
    private bool hasHit = false;
    private FeiLeiShen_Skill skill;
    private Player player;
    private SpriteRenderer spriteRenderer;

    public void Initialize(FeiLeiShen_Skill skillRef, Player playerRef)
    {
        skill = skillRef;
        player = playerRef;
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 设置初始方向
        float direction = transform.localScale.x > 0 ? 1 : -1;
        transform.localScale = new Vector3(direction, 1, 1);
    }

    void Update()
    {
        if (hasHit) return;
        
        // 飞镖移动
        transform.Translate(Vector3.right * speed * Time.deltaTime * Mathf.Sign(transform.localScale.x));
        
        // 检查飞行距离
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            HandleMiss();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // 检查敌人碰撞
        if (skill != null && ((1 << other.gameObject.layer) & skill.enemyLayer) != 0)
        {
            hasHit = true;
            HandleEnemyHit(other.transform);
        }
    }

    private void HandleEnemyHit(Transform enemy)
    {
        // todo 伤害敌人 

        // 计算敌人身后的位置
        Vector3 behindPosition = CalculateBehindPosition(enemy);
        
        // 触发玩家瞬移
        skill?.TriggerTeleport(behindPosition);
        Destroy(gameObject);
    }

    private void HandleMiss()
    {
        skill?.TriggerTeleport(transform.position);
        Destroy(gameObject);
    }

    private Vector3 CalculateBehindPosition(Transform enemy)
    {
        // 根据敌人朝向计算身后位置
        float enemyDirection = Mathf.Sign(enemy.localScale.x);
        Vector3 behindOffset = Vector3.left * enemyDirection * 1.2f;
        return enemy.position + behindOffset;
    }
}
