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
    private Player player; // 添加玩家引用

    public void Initialize(FeiLeiShen_Skill skillRef, Player playerRef)
    {
        skill = skillRef;
        player = playerRef;
        startPosition = transform.position;
        Debug.Log($"飞镖初始化: 技能引用={(skill != null ? "有效" : "无效")}, 玩家引用={(player != null ? "有效" : "无效")}");
    }

    void Start()
    {
        // 设置初始方向
        float direction = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(direction, 1, 1);
    }

    //飞镖未命中处理
    void Update()
    {
        if (hasHit) return;
        //飞镖移动
        transform.Translate(Vector3.right * speed * Time.deltaTime * Mathf.Sign(transform.localScale.x));
        //超出最大飞行距离则执行瞬移
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Debug.Log("飞镖未命中目标");
            skill.TriggerTeleport(transform.position);
            Destroy(gameObject);
        }
    }

    //飞镖命中处理
    void OnTriggerEnter2D(Collider2D other)
    {
        //如果已经命中过，则直接返回，防止多次命中
        if (hasHit) return;
        Debug.Log($"飞镖碰撞: {other.gameObject.name}");
        
        // 检查敌人碰撞
        if (skill != null && ((1 << other.gameObject.layer) & skill.enemyLayer) != 0)
        {
            hasHit = true;
            Debug.Log($"命中敌人: {other.gameObject.name}");
            // 计算敌人身后的位置
            Vector3 behindPosition = CalculateBehindPosition(other.transform);
            Debug.Log($"计算瞬移位置: {behindPosition}");
        
            // 触发玩家瞬移
            skill.TriggerTeleport(behindPosition);
            Destroy(gameObject);
        }
    }

    private Vector3 CalculateBehindPosition(Transform enemy)
    {
        // 根据敌人朝向计算身后位置
        float enemyDirection = Mathf.Sign(enemy.localScale.x);
        Vector3 behindOffset = Vector3.left * enemyDirection * 1.2f;
        return enemy.position + behindOffset;
    }
}