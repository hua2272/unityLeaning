using UnityEngine;
using System.Collections;

public class BackpackManager : MonoBehaviour
{
    public static BackpackManager Instance { get; private set; }

    [Header("UI References")] [SerializeField]
    private GameObject backpackPanel;

    [SerializeField] private ManualBackpackManager manualBackpackManager;

    [Header("武器加载")] 
    [SerializeField] private WeaponData[] obtainedWeapons;
    [SerializeField] private bool loadWeaponsOnStart = true;

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
        // 注册事件
        if (manualBackpackManager != null)
        {
            manualBackpackManager.OnWeaponEquipped += HandleWeaponEquip;
        }
        // 延迟加载初始武器，确保所有系统已初始化
        yield return new WaitForSeconds(0.1f);
        if (loadWeaponsOnStart)
        {
            LoadInitialWeapons();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isActive = !backpackPanel.activeSelf;
            backpackPanel.SetActive(isActive);
            Time.timeScale = isActive ? 0 : 1;  //可选：暂停游戏当背包打开
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
            if (i < manualBackpackManager.GetSlotCount())
            {
                manualBackpackManager.AddWeaponToSlot(i, obtainedWeapons[i], i == 0);
            }
            else
            {
                Debug.LogWarning($"Not enough slots for all initial weapons! Slot {i} is out of range.");
            }
        }
        Debug.Log($"Loaded {obtainedWeapons.Length} initial weapons");
    }

    // 处理武器装备
    private void HandleWeaponEquip(int slotIndex, WeaponData weapon)
    {
        // 通知玩家装备系统
        if (playerEquipment != null)
        {
            playerEquipment.EquipWeapon(weapon);
        }
    }

    // 设置玩家装备引用
    public void SetPlayerEquipment(PlayerEquipment equipment)
    {
        playerEquipment = equipment;
    }
}