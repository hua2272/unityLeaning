using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References - 这些需要在编辑器中设置")]
    [SerializeField] private Image iconImage;                   //武器图标
    [SerializeField] private TextMeshProUGUI nameText;          //武器名称
    [SerializeField] private TextMeshProUGUI descriptionText;   //武器描述
    [SerializeField] private GameObject emptyIndicator;         //空格子指示器
    [SerializeField] private GameObject equippedIndicator;      //装备状态指示器

    [Header("格子设置")]
    [SerializeField] private int slotId = 0;                    //手动设置的格子编号

    [Header("事件")]
    public UnityEvent<int> OnSlotClicked;
    public UnityEvent<int> OnSlotHovered;

    
    private WeaponData currentWeapon;                           //当前格子的武器
    private bool isEquipped = false;                            //当前格子的武器是否装备

    //从WeaponData获取武器信息并设置到格子
    public void SetWeapon(WeaponData weapon, bool equipped = false)
    {
        if (weapon == null)
        {
            ClearSlot();
            return;
        }
        currentWeapon = weapon;
        isEquipped = equipped;
        
        if (iconImage != null && currentWeapon.icon != null)         //设置武器图标
        {
            iconImage.sprite = currentWeapon.icon;
            iconImage.gameObject.SetActive(true);
            iconImage.preserveAspect = true;
        }
        if (nameText != null)
        {
            nameText.text = currentWeapon.weaponName;                      //设置武器名称
            nameText.gameObject.SetActive(true);
        }
        if (descriptionText != null)                                    //设置武器描述（默认隐藏，悬停时显示
        {
            descriptionText.text = currentWeapon.description;
            descriptionText.gameObject.SetActive(false);
        }
        if (emptyIndicator != null)                                    //隐藏空指示器
            emptyIndicator.SetActive(false);
        if (equippedIndicator != null)                                 //显示或隐藏装备指示器
            equippedIndicator.SetActive(isEquipped);
    }

    // 清空格子
    public void ClearSlot()
    {
        currentWeapon = null;
        isEquipped = false;

        // 隐藏所有武器信息
        if (iconImage != null)
            iconImage.gameObject.SetActive(false);

        if (nameText != null)
            nameText.gameObject.SetActive(false);

        if (descriptionText != null)
            descriptionText.gameObject.SetActive(false);

        // 显示空指示器
        if (emptyIndicator != null)
            emptyIndicator.SetActive(true);

        // 隐藏装备指示器
        if (equippedIndicator != null)
            equippedIndicator.SetActive(false);
    }

    // 悬停事件 - 显示描述
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentWeapon != null && descriptionText != null)
        {
            descriptionText.gameObject.SetActive(true);
        }
        OnSlotHovered.Invoke(slotId);
    }

    // 悬停结束 - 隐藏描述
    public void OnPointerExit(PointerEventData eventData)
    {
        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(false);
        }
        OnSlotHovered.Invoke(-1);
    }

    // 获取当前武器
    public WeaponData GetCurrentWeapon()
    {
        return currentWeapon;
    }

    // 获取格子索引
    public int GetSlotId()
    {
        return slotId;
    }

    // 检查是否为空
    public bool IsEmpty()
    {
        return currentWeapon == null;
    }
    
    // 更新装备状态
    public void UpdateEquippedState(bool equipped)
    {
        isEquipped = equipped;
        if (equippedIndicator != null)
        {
            equippedIndicator.SetActive(equipped);
        }
    }
}