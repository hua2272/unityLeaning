using UnityEngine;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance { get; private set; }
    [SerializeField] private GameObject panel;
    
    public delegate void DialogueEvent();
    public delegate void NodeEvent(DialogueNode node);
    
    public event DialogueEvent OnDialogueStart;
    public event DialogueEvent OnDialogueEnd;
    public event NodeEvent OnNodeUpdate;

    private DialogueLoader dialogueLoader;
    private Player player ;
    
    private int currentNpcId;
    private DialogueNode currentNode;
    private Stack<DialogueNode> nodeStack = new Stack<DialogueNode>();
    
    void Awake()
    {
        Debug.Log("<color=#FF0000>-------DialogueManager instance-------</color>");
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        panel.SetActive(false);
        dialogueLoader = GetComponent<DialogueLoader>();
        player = PlayerManager.instance.player;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            bool isActive = !panel.activeSelf;
            panel.SetActive(isActive);
            Time.timeScale = isActive ? 0 : 1;//暂停游戏
            
            var closestNPC = player.GetClosestVisibleNPC();
            if (closestNPC != null)
            {
                Debug.Log($"与最近的NPC交互 ID: {closestNPC.npcId}");
                currentNpcId = closestNPC.npcId;
                currentNode = dialogueLoader.LoadDialogueNode(closestNPC.npcId);
                nodeStack.Clear();
                OnDialogueStart?.Invoke();
                OnNodeUpdate?.Invoke(currentNode);
            }
        }
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

    public void EndDialogue()
    {
        OnDialogueEnd?.Invoke();
    }
}