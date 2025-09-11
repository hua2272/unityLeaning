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
    }

    // 装备武器（先卸下已经有装备
    public void EquipWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            Debug.LogWarning("Trying to equip null weapon!");
            return;
        }
        if (isWeaponEquipped)
        {
            UnequipWeapon();
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
            return;
        }
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
}