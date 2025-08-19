using TMPro;
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
        WeaponItemUI itemUI = item.GetComponent<WeaponItemUI>();
        if (itemUI != null)
        {
            itemUI.Setup(weapon);
        }
        else
        {
            Debug.LogError("武器预制体缺少WeaponItemUI组件", item);
        }
    }
    
    public void EquipWeapon(WeaponData weapon)
    {
        PlayerEquip?.EquipWeapon(weapon);
    }
    
    private void ShowDescription(GameObject item, string text)
    {
        item.transform.GetChild(0).gameObject.SetActive(true);
        item.transform.GetChild(0).GetComponent<TextMeshPro>().text = text;
    }
    
    private void HideDescription(GameObject item)
    {
        item.transform.GetChild(0).gameObject.SetActive(false);
    }
}