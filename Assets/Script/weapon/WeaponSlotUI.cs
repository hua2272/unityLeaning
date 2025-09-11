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
    [SerializeField] private int slotIndex = 0; // 手动设置的格子编号

    [Header("事件")]
    public UnityEvent<int> OnSlotClicked;
    public UnityEvent<int> OnSlotHovered;

    // 当前武器和状态
    private WeaponData currentWeapon;
    private bool isEquipped = false;
    
    public void Initialize()
    {
        ClearSlot();
        // 按钮添加点击事件监听
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => OnSlotClicked.Invoke(slotIndex));
    }

    // 设置武器到格子 - 从WeaponData自动获取所有信息
    public void SetWeapon(WeaponData weapon, bool equipped = false)
    {
        if (weapon == null)
        {
            ClearSlot();
            return;
        }
        currentWeapon = weapon;
        isEquipped = equipped;

        // 从WeaponData自动设置所有UI内容
        UpdateUIFromWeaponData();
        Debug.Log($"Set weapon {weapon.weaponName} to slot {slotIndex}");
    }

    // 从WeaponData更新UI内容
    private void UpdateUIFromWeaponData()
    {
        // 设置武器图标
        if (iconImage != null && currentWeapon.icon != null)
        {
            iconImage.sprite = currentWeapon.icon;
            iconImage.gameObject.SetActive(true);
            iconImage.preserveAspect = true;
        }

        // 设置武器名称
        if (nameText != null)
        {
            nameText.text = currentWeapon.weaponName;
            nameText.gameObject.SetActive(true);
        }

        // 设置武器描述
        if (descriptionText != null)
        {
            descriptionText.text = currentWeapon.description;
            descriptionText.gameObject.SetActive(false); // 默认隐藏，悬停时显示
        }

        // 隐藏空指示器
        if (emptyIndicator != null)
            emptyIndicator.SetActive(false);

        // 显示或隐藏装备指示器
        if (equippedIndicator != null)
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
        OnSlotHovered.Invoke(slotIndex);
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
    public WeaponData GetWeapon()
    {
        return currentWeapon;
    }

    // 获取格子索引
    public int GetSlotIndex()
    {
        return slotIndex;
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