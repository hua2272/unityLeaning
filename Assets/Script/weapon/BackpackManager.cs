using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackManager : MonoBehaviour
{
    public static BackpackManager Instance { get; private set; }
    
    [SerializeField] private GameObject backpackPanel;
    [SerializeField] private Transform weaponContainer;
    [SerializeField] private GameObject weaponItemPrefab;
    
    public PlayerEquipment PlayerEquip { get; set; }
    
    private void Awake()
    {
        Instance = this;
        backpackPanel.SetActive(false);
    }
    
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            backpackPanel.SetActive(!backpackPanel.activeSelf);
        }
    }
    
    public void AddWeaponToUI(WeaponData weapon)
    {
        var item = Instantiate(weaponItemPrefab, weaponContainer);
        item.GetComponent<Image>().sprite = weapon.icon;
        
        // 设置按钮点击事件
        item.GetComponent<Button>().onClick.AddListener(() => EquipWeapon(weapon));
        
        // 设置鼠标悬停事件
        var trigger = item.GetComponent<EventTrigger>();
        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((e) => ShowDescription(item, weapon.description));
        trigger.triggers.Add(enterEntry);
        
        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((e) => HideDescription(item));
        trigger.triggers.Add(exitEntry);
    }
    
    private void EquipWeapon(WeaponData weapon)
    {
        PlayerEquip?.EquipWeapon(weapon);
    }
    
    private void ShowDescription(GameObject item, string text)
    {
        item.transform.GetChild(0).gameObject.SetActive(true);
        item.transform.GetChild(0).GetComponent<Text>().text = text;
    }
    
    private void HideDescription(GameObject item)
    {
        item.transform.GetChild(0).gameObject.SetActive(false);
    }
}