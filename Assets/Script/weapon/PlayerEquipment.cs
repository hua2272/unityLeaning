using UnityEngine;
using System.Collections;

public class PlayerEquipment : MonoBehaviour
{

    // 当前装备的武器
    private WeaponData currentWeapon;
    private bool isWeaponEquipped = false;

    // 事件
    public System.Action<WeaponData> OnWeaponEquipped;
    public System.Action OnWeaponUnequipped;
    
    private CharacterState characterState;

    private void Start()
    {
        characterState = GetComponent<CharacterState>();
        // 注册到BackpackManager
        if (BackpackManager.Instance != null)
        {
            BackpackManager.Instance.SetPlayerEquipment(this);
        }
        else
        {
            // 如果BackpackManager尚未初始化，等待并重试
            StartCoroutine(RegisterWithBackpackManager());
        }
    }
    
    // 使用协程等待BackpackManager初始化
    private IEnumerator RegisterWithBackpackManager()
    {
        int maxAttempts = 50;
        int attempts = 0;
        
        // 等待直到BackpackManager实例可用
        while (BackpackManager.Instance == null && attempts < maxAttempts)
        {
            attempts++;
            yield return null; // 等待下一帧
        }
        
        if (BackpackManager.Instance != null)
        {
            BackpackManager.Instance.SetPlayerEquipment(this);
            Debug.Log("PlayerEquipment registered with BackpackManager");
        }
        else
        {
            Debug.LogError("Failed to register with BackpackManager - instance not found after " + maxAttempts + " attempts");
        }
    }

    // 装备武器
    public void EquipWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            Debug.LogWarning("Trying to equip null weapon!");
            return;
        }
        if (isWeaponEquipped)
        {
            UnequipWeapon(); //如果已经有武器装备，先卸下
        }
        currentWeapon = weapon;
        // characterState.SetWeaponAttack(weapon.damage);
        characterState.SetWeaponAttack(weapon.damage); // 装备后更改攻击力
        Debug.LogError("当前武器伤害：" + weapon.damage);
        isWeaponEquipped = true;
        OnWeaponEquipped?.Invoke(weapon); //触发事件
        Debug.Log($"Equipped weapon: {weapon.weaponName}");
    }

    // 卸下武器
    public void UnequipWeapon()
    {
        if (!isWeaponEquipped)
        {
            return; // 没有装备武器，直接返回
        }
        // 触发事件
        OnWeaponUnequipped?.Invoke();
        Debug.Log($"Unequipped weapon: {currentWeapon.weaponName}");
        // 清除当前武器引用
        currentWeapon = null;
        isWeaponEquipped = false;
    }

    // 获取当前装备的武器
    public WeaponData GetCurrentWeapon()
    {
        //todo 从存档文件读取，放入内存后读取内存
        return currentWeapon;
    }

    // 检查是否有武器装备
    public bool IsWeaponEquipped()
    {
        return isWeaponEquipped;
    }

    // 切换武器装备状态
    public void ToggleWeapon()
    {
        if (isWeaponEquipped)
        {
            UnequipWeapon();
        }
        else if (currentWeapon != null)
        {
            EquipWeapon(currentWeapon);
        }
    }
    
    // 在PlayerEquipment中添加方法，用于处理武器切换
    public void SwitchWeapon(int slotIndex, WeaponData weapon)
    {
        // 通知BackpackManager更新装备状态
        if (BackpackManager.Instance != null)
        {
            // 这里需要获取ManualBackpackManager的引用
            ManualBackpackManager backpackManager = FindObjectOfType<ManualBackpackManager>();
            if (backpackManager != null)
            {
                backpackManager.UpdateSlotEquippedState(slotIndex, true);
            }
        }
        // 装备武器
        EquipWeapon(weapon);
    }
}