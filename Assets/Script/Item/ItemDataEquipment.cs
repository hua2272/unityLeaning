using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType {Weapon, Armor, Amulet, Flask}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Data/Equipment")]
public class ItemDataEquipment : ItemData
{
    public EquipmentType equipmentType;

    [Header("Major stats")]
    public int strength;

    public void AddModifiers()
    {
        PlayerStatus playerStatus = PlayerManager.instance.player.GetComponent<PlayerStatus>();
        playerStatus.strength.AddModifier(strength);
    }
    
    public void RemoveModifiers()
    {
        PlayerStatus playerStatus = PlayerManager.instance.player.GetComponent<PlayerStatus>();
        playerStatus.strength.RemoveModifier(strength);
    }
}
