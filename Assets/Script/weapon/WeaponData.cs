using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Inventory/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite icon;
    [TextArea] public string description;

    public int damage = 10;
    public float attackSpeed = 1.0f;
}