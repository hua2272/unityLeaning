using UnityEngine;
using System.Collections;

public class BackpackManager : MonoBehaviour
{
    public static BackpackManager Instance { get; private set; }

    [Header("UI References")] [SerializeField]
    private GameObject backpackPanel;

    [SerializeField] private ManualBackpackManager manualBackpackManager;

    [Header("武器加载")] [SerializeField] private WeaponData[] initialWeapons;
    [SerializeField] private bool loadWeaponsOnStart = true;
    [SerializeField] private float loadDelay = 0.1f;

    // 玩家装备引用
    public PlayerEquipment playerEquipment;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        backpackPanel.SetActive(false);
    }

    private IEnumerator Start()
    {
        // 查找玩家装备
        playerEquipment = FindObjectOfType<PlayerEquipment>();
        
        // 注册事件
        if (manualBackpackManager != null)
        {
            manualBackpackManager.OnWeaponEquipped += HandleWeaponEquip;
        }
        
        // 延迟加载初始武器，确保所有系统已初始化
        yield return new WaitForSeconds(loadDelay);
        
        if (loadWeaponsOnStart)
        {
            LoadInitialWeapons();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleBackpack();
        }
    }

    // 加载初始武器
    public void LoadInitialWeapons()
    {
        if (initialWeapons == null || initialWeapons.Length == 0)
        {
            Debug.LogWarning("No initial weapons configured!");
            return;
        }

        // 为每个武器分配到指定格子
        for (int i = 0; i < initialWeapons.Length; i++)
        {
            if (i < manualBackpackManager.GetSlotCount())
            {
                manualBackpackManager.AddWeaponToSlot(i, initialWeapons[i], i == 0);
            }
            else
            {
                Debug.LogWarning($"Not enough slots for all initial weapons! Slot {i} is out of range.");
            }
        }

        Debug.Log($"Loaded {initialWeapons.Length} initial weapons");
    }

    // 从外部加载武器（替代WeaponLoader的功能）
    public void LoadWeapons(WeaponData[] weapons, bool clearExisting = false)
    {
        if (clearExisting)
        {
            manualBackpackManager.ClearAllSlots();
        }

        foreach (var weapon in weapons)
        {
            AddWeapon(weapon);
        }
    }

    // 切换背包显示
    public void ToggleBackpack()
    {
        bool isActive = !backpackPanel.activeSelf;
        backpackPanel.SetActive(isActive);

        // 可选：暂停游戏当背包打开
        Time.timeScale = isActive ? 0 : 1;
    }

    // 处理武器装备
    private void HandleWeaponEquip(int slotIndex, WeaponData weapon)
    {
        // 通知玩家装备系统
        if (playerEquipment != null)
        {
            playerEquipment.EquipWeapon(weapon);
        }
        else
        {
            Debug.LogError("PlayerEquipment not found!");
        }
    }

    // 添加新武器到背包
    public void AddWeapon(WeaponData weapon)
    {
        int emptySlot = manualBackpackManager.FindEmptySlot();
        if (emptySlot >= 0)
        {
            manualBackpackManager.AddWeaponToSlot(emptySlot, weapon);
        }
        else
        {
            Debug.LogWarning("No empty slots available!");
        }
    }

    // 从背包移除武器
    public void RemoveWeapon(int slotIndex)
    {
        manualBackpackManager.RemoveWeaponFromSlot(slotIndex);
    }

    // 获取指定格子的武器
    public WeaponData GetWeaponInSlot(int slotIndex)
    {
        return manualBackpackManager.GetWeaponInSlot(slotIndex);
    }

    // 检查背包是否打开
    public bool IsBackpackOpen()
    {
        return backpackPanel.activeSelf;
    }

    // 设置玩家装备引用
    public void SetPlayerEquipment(PlayerEquipment equipment)
    {
        playerEquipment = equipment;
    }

    // 清空背包
    public void ClearBackpack()
    {
        manualBackpackManager.ClearAllSlots();
    }
}