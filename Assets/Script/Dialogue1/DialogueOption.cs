using UnityEngine;

[System.Serializable]
public class DialogueOption
{
    public int optionId;
    public string optionText;
    public int nextNodeId;
    public int requiredItemId;
    public int requiredQuestProgress;
}