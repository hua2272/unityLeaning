using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DropItem
{
    public GameObject itemPrefab; // 要掉落的物品预制体
    [Range(0f, 1f)] public float dropChance = 0.5f; // 掉落几率(0-1)
    public int minAmount = 1; // 最小掉落数量
    public int maxAmount = 1; // 最大掉落数量
}

public class ItemDropper : MonoBehaviour
{
    [Header("掉落设置")]
    [SerializeField] private List<DropItem> dropTable = new List<DropItem>(); // 掉落物品表
    [SerializeField] private Vector2 dropOffset = new Vector2(0, 0.5f); // 掉落位置偏移
    [SerializeField] private float dropRadius = 1f; // 物品散落半径

    // 当敌人死亡时调用此方法
    public void DropItemsOnDeath()
    {
        foreach (var dropItem in dropTable)
        {
            // 根据掉落几率决定是否掉落
            if (Random.value <= dropItem.dropChance)
            {
                // 确定掉落数量
                int amount = Random.Range(dropItem.minAmount, dropItem.maxAmount + 1);
                
                for (int i = 0; i < amount; i++)
                {
                    // 计算随机掉落位置
                    Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
                    Vector3 dropPosition = transform.position + new Vector3(
                        dropOffset.x + randomOffset.x,
                        dropOffset.y + randomOffset.y,
                        0
                    );
                    
                    // 实例化物品
                    Instantiate(dropItem.itemPrefab, dropPosition, Quaternion.identity);
                }
            }
        }
    }
}