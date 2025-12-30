using UnityEngine;
using System.Collections;

[System.Serializable] public class DestructibleTileData
{
    public string tileId;
    public bool isDestroyed;
    public Vector3 position;
    public string sceneName; // 添加场景信息，支持多场景
}

public class DestructibleTileController : MonoBehaviour
{
    [Header("伤害设置")]
    public int maxHealth = 1;
    public int currentHealth;
    
    [Header("标识设置")]
    public string tileId; // 唯一标识，用于存档
    
    [Header("组件引用")]
    private Animator animator;
    private Collider2D tileCollider;
    private SpriteRenderer spriteRenderer;
    
    [Header("存档设置")]
    public bool shouldSaveState = true; // 是否需要存档记录
    private GameSaveManager gameSaveManager;
    
    private bool isDestroyed = false;
    
    void Start()
    {
        gameSaveManager = GameSaveManager.instance;
        // 自动获取组件
        if (animator == null) animator = GetComponent<Animator>();
        if (tileCollider == null) tileCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentHealth = maxHealth;
        if (string.IsNullOrEmpty(tileId)) tileId = GenerateTileId();// 生成唯一ID（如果没有设置）
        CheckSavedState();// 检查存档状态
    }
    
    string GenerateTileId()
    {
        // 使用位置和场景信息生成相对唯一的ID
        return $"tile_{transform.position.x}_{transform.position.y}_{gameObject.name}";
    }
    
    public void TakeDamage(int damage)
    {
        Debug.Log("test---------->>>>>>>>>damage: " + damage);
        if (isDestroyed) return;
        currentHealth -= damage;
        
        if (animator != null)
        {
            animator.SetTrigger("Hit");// 播放受击动画
        }
        if (currentHealth <= 0)
        {
            isDestroyed = true;
            if (animator != null)
            {
                animator.SetTrigger("Destroy");// 播放销毁动画
            }
            if (tileCollider != null)
            {
                tileCollider.enabled = false;// 禁用碰撞体
            }
            if (shouldSaveState)
            {
                SaveDestroyedState();// 保存状态到存档
            }
            StartCoroutine(ProcessAfterAnimation());// 动画播放完成后处理对象
        }
    }
    
    IEnumerator ProcessAfterAnimation()
    {
        yield return new WaitForSeconds(1f); // 等待动画播放完成// 根据实际动画长度调整
        if (!shouldSaveState)
        {
            Destroy(gameObject);// 如果不需存档，直接销毁；如需存档，保持对象但隐藏
        }
        else
        {
            gameObject.SetActive(false);// 对于需要存档的对象，保持存在但设置为不可见
        }
    }
    
    void CheckSavedState()
    {
        if (!shouldSaveState) return;
        bool wasDestroyed = gameSaveManager.IsTileDestroyed(tileId);// 从存档系统检查此Tile是否已被摧毁
        if (wasDestroyed)
        {
            isDestroyed = true;
            gameObject.SetActive(false);// 如果存档中记录为已摧毁，直接应用摧毁状态
        }
    }

    void SaveDestroyedState()
    {
        if (!shouldSaveState) return;
        DestructibleTileData tileData = new DestructibleTileData // 保存到存档系统
        {
            tileId = this.tileId,
            isDestroyed = true,
            position = transform.position
        };
        gameSaveManager.SaveTileState(tileData);
    }
}