using UnityEngine;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    public delegate void DialogueEvent();
    public delegate void NodeEvent(DialogueNode node);
    
    public event DialogueEvent OnDialogueStart;
    public event DialogueEvent OnDialogueEnd;
    public event NodeEvent OnNodeUpdate;

    private DialogueLoader dialogueLoader;
    //private InventorySystem inventory;
    //private QuestSystem questSystem;
    
    private int currentNpcId;
    private DialogueNode currentNode;
    private Stack<DialogueNode> nodeStack = new Stack<DialogueNode>();

    // void Start()
    // {
    //     dialogueLoader = FindObjectOfType<DialogueLoader>();
    //     //inventory = FindObjectOfType<InventorySystem>();
    //     //questSystem = FindObjectOfType<QuestSystem>();
    // }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        if (dialogueLoader == null)
        {
            dialogueLoader = GetComponent<DialogueLoader>();
            if (dialogueLoader == null)
            {
                Debug.LogError("DialogueLoader not found on DialogueManager GameObject!");
            }
        }
    }

    public void StartDialogue(int npcId)
    {
        currentNpcId = npcId;
        currentNode = dialogueLoader.LoadDialogueNode(npcId);
        nodeStack.Clear();
        OnDialogueStart?.Invoke();
        OnNodeUpdate?.Invoke(currentNode);
    }

    public bool CheckOptionConditions(DialogueOption option)
    {
        // if (!string.IsNullOrEmpty(option.requiredItemId) && !inventory.HasItem(option.requiredItemId))
        // {
        //     return false;
        // }
        // if (option.requiredQuestProgress > 0 && !questSystem.CheckQuestProgress(option.requiredQuestProgress))
        // {
        //     return false;
        // }
        return true;
    }

    public void SelectOption(DialogueOption option)
    {
        nodeStack.Push(currentNode);
        if (option.nextNodeId <= 0)
        {
            EndDialogue();
            return;
        }

        currentNode = dialogueLoader.LoadDialogueNode(currentNpcId, option.nextNodeId);
        OnNodeUpdate?.Invoke(currentNode);
    }

    public void GoBack()
    {
        if (nodeStack.Count > 0)
        {
            currentNode = nodeStack.Pop();
            OnNodeUpdate?.Invoke(currentNode);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        OnDialogueEnd?.Invoke();
    }
}