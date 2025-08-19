using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    private WeaponData currentWeapon;
    
    public void Setup(WeaponData weapon)
    {
        currentWeapon = weapon;
        icon.sprite = weapon.icon;
        descriptionText.text = weapon.description;
        descriptionText.gameObject.SetActive(false);
        
        // 确保有按钮组件
        Button button = GetComponent<Button>();
        if (button == null) button = gameObject.AddComponent<Button>();
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnItemClicked);
        
        // 确保有事件触发器
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger == null) trigger = gameObject.AddComponent<EventTrigger>();
        
        // 清除旧事件
        trigger.triggers.Clear();
        
        // 添加鼠标进入事件
        EventTrigger.Entry entryEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entryEnter.callback.AddListener((data) => { ShowDescription(); });
        trigger.triggers.Add(entryEnter);
        
        // 添加鼠标离开事件
        EventTrigger.Entry entryExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        entryExit.callback.AddListener((data) => { HideDescription(); });
        trigger.triggers.Add(entryExit);
    }
    
    private void OnItemClicked()
    {
        BackpackManager.Instance.EquipWeapon(currentWeapon);
    }   
    
    private void ShowDescription()
    {
        descriptionText.gameObject.SetActive(true);
    }
    
    private void HideDescription()
    {
        descriptionText.gameObject.SetActive(false);
    }
}