using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ManualBackpackManager : MonoBehaviour
{
    // 所有格子的引用
    private Dictionary<int, WeaponSlotUI> slots = new Dictionary<int, WeaponSlotUI>();
    private bool isInitialized = false;

    // 当前装备的武器索引
    private int equippedSlotIndex = -1;

    // 事件
    public System.Action<int, WeaponData> OnWeaponEquipped;

    private void Awake()
    {
        InitializeSlots();
    }

    // 初始化所有格子
    private void InitializeSlots()
    {
        if (isInitialized) return;
        
        slots.Clear();

        // 获取所有子对象中的WeaponSlotUI组件
        WeaponSlotUI[] slotComponents = GetComponentsInChildren<WeaponSlotUI>(true);
        
        Debug.Log($"Found {slotComponents.Length} weapon slot UI components in {gameObject.name}");

        foreach (WeaponSlotUI slot in slotComponents)
        {
            int index = slot.GetSlotIndex();

            // 检查索引是否重复
            if (slots.ContainsKey(index))
            {
                Debug.LogError($"Duplicate slot index found: {index}");
                continue;
            }

            // 初始化格子
            slot.Initialize();
            slot.OnSlotClicked.AddListener(HandleSlotClick);
            slot.OnSlotHovered.AddListener(HandleSlotHover);

            slots.Add(index, slot);

            Debug.Log($"Initialized slot {index}: {slot.gameObject.name}");
        }

        Debug.Log($"Total slots initialized: {slots.Count}");
        isInitialized = true;
    }
    
    private void OnDestroy()
    {
        // 取消事件订阅，防止内存泄漏
        foreach (var slot in slots.Values)
        {
            if (slot != null)
            {
                slot.OnSlotClicked.RemoveListener(HandleSlotClick);
                slot.OnSlotHovered.RemoveListener(HandleSlotHover);
            }
        }
    }

    // 添加武器到指定格子
    public bool AddWeaponToSlot(int slotIndex, WeaponData weapon, bool equip = false)
    {
        if (!slots.ContainsKey(slotIndex))
        {
            Debug.LogWarning($"Slot {slotIndex} not found!");
            return false;
        }

        if (!slots[slotIndex].IsEmpty())
        {
            Debug.LogWarning($"Slot {slotIndex} is already occupied!");
            return false;
        }

        // 设置武器到格子
        slots[slotIndex].SetWeapon(weapon, equip);

        // 如果是装备状态，更新装备索引
        if (equip)
        {
            equippedSlotIndex = slotIndex;
        }

        return true;
    }

    // 从格子移除武器
    public bool RemoveWeaponFromSlot(int slotIndex)
    {
        if (!slots.ContainsKey(slotIndex) || slots[slotIndex].IsEmpty())
            return false;
            
        slots[slotIndex].ClearSlot();

        // 如果移除的是当前装备的武器
        if (slotIndex == equippedSlotIndex)
        {
            equippedSlotIndex = -1;
        }
        return true;
    }

    // 获取指定格子的武器
    public WeaponData GetWeaponInSlot(int slotIndex)
    {
        if (!slots.ContainsKey(slotIndex))
            return null;

        return slots[slotIndex].GetWeapon();
    }

    // 获取所有武器
    public Dictionary<int, WeaponData> GetAllWeapons()
    {
        Dictionary<int, WeaponData> weapons = new Dictionary<int, WeaponData>();

        foreach (var pair in slots)
        {
            if (!pair.Value.IsEmpty())
            {
                weapons.Add(pair.Key, pair.Value.GetWeapon());
            }
        }

        return weapons;
    }

    // 获取空格子索引
    public int FindEmptySlot()
    {
        foreach (var pair in slots)
        {
            if (pair.Value.IsEmpty())
            {
                return pair.Key;
            }
        }
        return -1;
    }

    // 获取所有格子数量
    public int GetSlotCount()
    {
        // 如果尚未初始化，先初始化
        if (!isInitialized)
        {
            InitializeSlots();
        }
        return slots.Count;
    }

    // 事件处理
    private void HandleSlotClick(int slotIndex)
    {
        if (slots.ContainsKey(slotIndex) && !slots[slotIndex].IsEmpty())
        {
            // 装备武器
            EquipWeapon(slotIndex, slots[slotIndex].GetWeapon());
            OnWeaponEquipped?.Invoke(slotIndex, slots[slotIndex].GetWeapon());
        }
    }

    private void HandleSlotHover(int slotIndex)
    {
        // 处理悬停逻辑
        if (slotIndex >= 0 && slots.ContainsKey(slotIndex) && !slots[slotIndex].IsEmpty())
        {
            // 显示武器信息
            Debug.Log($"Hovering over weapon: {slots[slotIndex].GetWeapon().weaponName}");
        }
    }

    // 装备武器
    private void EquipWeapon(int slotIndex, WeaponData weapon)
    {
        // 取消之前装备的武器
        if (equippedSlotIndex >= 0 && equippedSlotIndex != slotIndex)
        {
            slots[equippedSlotIndex].UpdateEquippedState(false);
        }

        // 装备新武器
        equippedSlotIndex = slotIndex;
        slots[slotIndex].UpdateEquippedState(true);

        // 通知玩家装备系统
        PlayerEquipment equipment = FindObjectOfType<PlayerEquipment>();
        if (equipment != null)
        {
            equipment.EquipWeapon(weapon);
        }
    }

    // 清空所有格子
    public void ClearAllSlots()
    {
        foreach (var pair in slots)
        {
            pair.Value.ClearSlot();
        }

        equippedSlotIndex = -1;
    }

    // 获取指定格子的UI组件
    public WeaponSlotUI GetSlotUI(int slotIndex)
    {
        return slots.ContainsKey(slotIndex) ? slots[slotIndex] : null;
    }
    
    // 用于更新特定格子的装备状态
    public void UpdateSlotEquippedState(int slotIndex, bool equipped)
    {
        if (slots.ContainsKey(slotIndex))
        {
            slots[slotIndex].UpdateEquippedState(equipped);
        }
    }
}