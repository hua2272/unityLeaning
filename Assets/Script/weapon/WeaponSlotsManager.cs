using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponSlotsManager : MonoBehaviour
{
    public static WeaponSlotsManager Instance { get; private set; }
    // 所有格子的引用
    private Dictionary<int, WeaponSlotUI> slots = new Dictionary<int, WeaponSlotUI>();
    private bool isInitialized = false;

    // 当前装备的武器索引
    private int equippedSlotIndex = -1;

    // 事件
    public System.Action<int, WeaponData> OnWeaponEquipped = (index, weapon) => { };
    
    [Header("武器加载")] 
    [SerializeField] private WeaponData[] obtainedWeapons;
    [SerializeField] private bool loadWeaponsOnStart = true;
    
    public PlayerEquipment playerEquipment;

    private void Awake()
    {
        InitializeSlots();
        OnWeaponEquipped += HandleWeaponEquip;
        if (loadWeaponsOnStart)
        {
            LoadInitialWeapons();
        }
    }
    
    // 初始化背包武器
    public void LoadInitialWeapons()
    {
        if (obtainedWeapons == null || obtainedWeapons.Length == 0)
        {
            Debug.LogWarning("No initial weapons configured!");
            return;
        }
        // 为每个武器分配到指定格子
        for (int i = 0; i < obtainedWeapons.Length; i++)
        {
            if (i < slots.Count)
            {
                AddWeaponToSlot(i, obtainedWeapons[i], i == 0);
            }
            else
            {
                Debug.LogWarning($"Not enough slots for all initial weapons! Slot {i} is out of range.");
            }
        }
    }

    // 初始化所有格子
    private void InitializeSlots()
    {
        if (isInitialized) return;
        slots.Clear();
        WeaponSlotUI[] slotComponents = GetComponentsInChildren<WeaponSlotUI>(true);

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
        }
        isInitialized = true;
    }
    
    // 取消事件订阅，防止内存泄漏
    private void OnDestroy()
    {
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

    // 事件处理
    private void HandleSlotClick(int slotIndex)
    {
        if (slots.ContainsKey(slotIndex) && !slots[slotIndex].IsEmpty())
        {
            // 只触发事件，告知外界用户点击了哪个槽位，不处理实际装备逻辑
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
    }
    
    // 处理武器装备
    private void HandleWeaponEquip(int slotIndex, WeaponData weapon)
    {
        playerEquipment.EquipWeapon(weapon);
    }
    
    // 添加新武器到背包
    public void AddWeapon(WeaponData weapon)
    {
        foreach (var pair in slots)
        {
            if (pair.Value.IsEmpty())
            {
                AddWeaponToSlot(pair.Key, weapon);
            }
        }
    }
}