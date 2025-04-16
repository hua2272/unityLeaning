using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] //inspector中不可见时尝试序列化
public class InventoryItem
{
    public ItemData data;
    public int stackSize;

    public InventoryItem(ItemData _newItemData)
    {
        data = _newItemData;
        AddStack();
    }
    
    public void AddStack() => stackSize++;
    public void RemoveStack() => stackSize--;
}
