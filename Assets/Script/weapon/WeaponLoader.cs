using UnityEngine;

public class WeaponLoader : MonoBehaviour
{
    [SerializeField] private WeaponData[] weapons;
    
    private void Start()
    {
        foreach(var weapon in weapons)
        {
            BackpackManager.Instance.AddWeaponToUI(weapon);
        }
    }
}