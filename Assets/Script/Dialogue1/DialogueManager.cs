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
    private PlayerManager playerManager;
    
    private int currentNpcId;
    private DialogueNode currentNode;
    private Stack<DialogueNode> nodeStack = new Stack<DialogueNode>();

    void Start()
    {
        dialogueLoader = GetComponent<DialogueLoader>();
        playerManager = PlayerManager.instance;
    }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
        panel.SetActive(false);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            bool isActive = !panel.activeSelf;
            panel.SetActive(isActive);
            Time.timeScale = isActive ? 0 : 1;//暂停游戏
            
            var closestNPC = playerManager.playerNpcDetector.GetClosestVisibleNPC();
            if (closestNPC != null)
            {
                Debug.Log($"与最近的NPC交互 ID: {closestNPC.npcId}");
                StartDialogue(closestNPC.npcId);
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