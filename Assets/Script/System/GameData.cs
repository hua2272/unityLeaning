using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class GameData
{
    public int playerLevel;
    public int playerHealth;
    public int equippedSlotId;
    public string scene;
    public Vector3 playerPosition;
    public string[] inventoryItems;
    public SerializableDictionary unlockedSkills; //JsonUtility无法解析字典类型
    public List<DestructibleTileData> destroyedTiles = new List<DestructibleTileData>();


    [Serializable] public class SerializableDictionary
    {
        [Serializable] public class KeyValuePair
        {
            public string key;
            public int value;
        }
    
        public List<KeyValuePair> items = new List<KeyValuePair>();
    
        // 添加或更新元素的方法
        public void AddOrUpdate(string key, int value)
        {
            var existingItem = items.Find(item => item.key == key);
            if (existingItem != null)
            {
                // 更新现有元素
                existingItem.value = value;
            }
            else
            {
                // 添加新元素
                items.Add(new KeyValuePair { key = key, value = value });
            }
        }
    
        // 获取元素值的方法
        public int GetValue(string key)
        {
            var item = items.Find(item => item.key == key);
            return item?.value ?? 0; // 如果不存在返回0
        }
        
        // 获取字典长度（元素个数）
        public int Count
        {
            get { return items.Count; }
        }
    
        // 检查键是否存在的方法
        public bool ContainsKey(string key)
        {
            return items.Exists(item => item.key == key);
        }
    
        // 删除元素的方法
        public void Remove(string key)
        {
            items.RemoveAll(item => item.key == key);
        }
    }
}