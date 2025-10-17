using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Status
{
    [SerializeField] private int baseValue;
    public List<int> modifiers;

    public int getValue()
    {
        return baseValue;
    }
    
    public void setValue(int value)
    {
        baseValue = value;
    }

    public void AddModifier(int _modifier)
    {
        modifiers.Add(_modifier);
    }

    public void RemoveModifier(int _modifier)
    {
        modifiers.Remove(_modifier);
    }
}
