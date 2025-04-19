using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemText;

    public InventoryItem Item;

    public void UpdateSlot(InventoryItem _newItem)
    {
        Item = _newItem;
        itemImage.color = Color.white;
        if (Item != null)
        {
            itemImage.sprite = Item.data.icon;
            if (Item.stackSize > 1)
            {
                itemText.text = Item.stackSize.ToString();
            }
            else
            {
                itemText.text = "";
            }
        }
    }
}
