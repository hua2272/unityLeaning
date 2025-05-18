using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    public int nodeId;
    public string speakerName;
    public string dialogueText;
    public int isPlayerDialogue;
    public List<DialogueOption> options;
}
