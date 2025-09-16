using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponSlotsManager : MonoBehaviour
{
    private Dictionary<int, WeaponSlotUI> slots = new Dictionary<int, WeaponSlotUI>();  //所有格子的引用
    private bool isInitialized = false;
    private int equippedSlotId = -1;                                                    //已装备的武器索引
    public CharacterState characterState;                                               //玩家数值
    public SlotEffectsManager slotEffects;                                              //格子特效
    
    [Header("武器加载")] 
    [SerializeField] private WeaponData[] obtainedWeapons;

    private void Awake()
    {
        InitializeSlots();
        LoadInitialWeapons();
    }
    
    // 初始化所有格子
    private void InitializeSlots()
    {
        if (isInitialized) return;
        slots.Clear();
        WeaponSlotUI[] slotComponents = GetComponentsInChildren<WeaponSlotUI>(true);

        foreach (WeaponSlotUI slot in slotComponents)
        {
            int index = slot.GetSlotId();
            if (slots.ContainsKey(index))
            {
                Debug.LogError($"Duplicate slot index found: {index}");  //跳过重复的id
                continue;
            }
            // 初始化格子
            slot.ClearSlot();
            slot.OnSlotClicked.AddListener(HandleSlotClick);
            slot.OnSlotHovered.AddListener(HandleSlotHover);
            slots.Add(index, slot);
        }
        isInitialized = true;
    }
    
    // 初始化背包武器，为每个武器分配到指定格子
    public void LoadInitialWeapons()
    {
        if (obtainedWeapons == null || obtainedWeapons.Length == 0)
        {
            return;
        }
        for (int i = 0; i < obtainedWeapons.Length; i++)
        {
            if (i < slots.Count)
            {
                AddWeaponToSlot(i, obtainedWeapons[i], i == 0);
            }
        }
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
    public void AddWeaponToSlot(int slotIndex, WeaponData weapon, bool equip = false)
    {
        if (!slots.ContainsKey(slotIndex)) return;
        if (!slots[slotIndex].IsEmpty()) return;
        
        slots[slotIndex].SetWeapon(weapon, equip);                          // 设置武器到格子
        if (equip)                                                          // 如果是装备状态，更新装备索引
            equippedSlotId = slotIndex;
    }

    // 事件处理
    private void HandleSlotClick(int slotId)
    {
        if (slots.ContainsKey(slotId) && !slots[slotId].IsEmpty())
        {
            if (equippedSlotId >= 0 && equippedSlotId != slotId)
            {
                slots[equippedSlotId].UpdateEquippedState(false);           //卸下之前的武器
            }
            slots[slotId].UpdateEquippedState(true);                        //装备现在的武器
            WeaponData currentWeapon = slots[slotId].GetCurrentWeapon();
            characterState.SetWeaponAttack(currentWeapon.damage);
            equippedSlotId = slotId;
            slotEffects.PlaySound();                                        //目前声音播放不会覆盖前面未结束的声音
            Debug.LogError("当前武器伤害：" + currentWeapon.damage);
        }
    }

    private void HandleSlotHover(int slotIndex)
    {
        // 处理悬停逻辑
        if (slotIndex >= 0 && slots.ContainsKey(slotIndex) && !slots[slotIndex].IsEmpty())
        {
            // 显示武器信息
            Debug.Log($"Hovering over weapon: {slots[slotIndex].GetCurrentWeapon().weaponName}");
        }
    }
    
    // 添加新武器到背包 TODO 校验武器是否重复，重复则不拾起
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

    public string[] GetObtainedWeaponsName()
    {
        List<string> weaponNameList = new List<string>();
        foreach (WeaponData weapon in obtainedWeapons)
        {
            if (weapon != null)
            {
                weaponNameList.Add(weapon.weaponName);
            }
        }
        return weaponNameList.ToArray();
    }
    
    private void EquipWeaponWithName(String weaponName)
    {
        foreach (WeaponData weapon in obtainedWeapons)
        {
            if (weapon.weaponName.Equals(weaponName))
            {
                
            }
        }
    }
}