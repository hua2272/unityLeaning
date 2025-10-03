using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponSlotsManager : MonoBehaviour
{
    public static WeaponSlotsManager instance;
    private GameDataManager gameDataManager;
    
    private Dictionary<int, WeaponSlotUI> slots = new Dictionary<int, WeaponSlotUI>();  //所有格子的引用
    private bool isInitialized = false;
    //public int equippedSlotId { get; private set; } = 0;                                //已装备的武器索引
    public CharacterState characterState;                                               //玩家数值
    public SlotEffectsManager slotEffects;                                              //格子特效
    
    [Header("武器加载")] 
    [SerializeField] private WeaponData[] obtainedWeapons;
    
    private void Awake()
    {
        Debug.Log("-------WeaponSlotsManager instance------->");
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
        foreach (var slot in slots.Values)
        {
            if (slot != null)
            {
                slot.OnSlotClicked.RemoveListener(HandleSlotClick);
                slot.OnSlotHovered.RemoveListener(HandleSlotHover);
            }
        }
    }

    public void Start()
    {
        gameDataManager = GameDataManager.instance;
        // if (gameDataManager != null)
        // {
        //     equippedSlotId = gameDataManager.equippedSlotId;
        // }
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
                Debug.LogError($"Duplicate slot index found: {index}");                  //跳过重复的id
                continue;
            }
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
        if (obtainedWeapons == null || obtainedWeapons.Length == 0) return;
        for (int i = 0; i < obtainedWeapons.Length; i++)
        {
            if (i < slots.Count)
            {
                AddWeaponToSlot(i, obtainedWeapons[i], i == gameDataManager.equippedSlotId);
            }
        }
    }

    // 添加武器到指定格子
    public void AddWeaponToSlot(int slotId, WeaponData weapon, bool equip = false)
    {
        if (!slots.ContainsKey(slotId)) return;
        if (!slots[slotId].IsEmpty()) return;
        slots[slotId].SetWeapon(weapon, equip);
        if (equip) 
            gameDataManager.equippedSlotId = slotId;
    }

    // 事件处理
    public void HandleSlotClick(int slotId)
    {
        if (slots.ContainsKey(slotId) && !slots[slotId].IsEmpty())
        {
            if (gameDataManager.equippedSlotId >= 0 && gameDataManager.equippedSlotId != slotId)
            {
                slots[gameDataManager.equippedSlotId].UpdateEquippedState(false);           //卸下之前的武器
            }
            slots[slotId].UpdateEquippedState(true);                                        //装备现在的武器
            WeaponData currentWeapon = slots[slotId].GetCurrentWeapon();
            characterState.SetWeaponAttack(currentWeapon.damage);
            gameDataManager.equippedSlotId = slotId;
            Debug.LogError("当前武器伤害：" + currentWeapon.damage);
        }
        slotEffects.PlaySound();                                                            //目前声音播放不会覆盖前面未结束的声音
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
}