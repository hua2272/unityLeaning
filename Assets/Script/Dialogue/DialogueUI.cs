using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Transform optionsPanel;
    public GameObject optionButtonPrefab;

    private DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = DialogueManager.instance;
        if (dialogueManager == null) 
        {
            Debug.LogError("DialogueManager not found in scene!");
        }
        dialogueManager.OnDialogueStart += ShowDialogue;
        dialogueManager.OnDialogueEnd += HideDialogue;
        dialogueManager.OnNodeUpdate += UpdateUI;
        dialoguePanel.SetActive(false);
    }

    void ShowDialogue()
    {
        dialoguePanel.SetActive(true);
    }

    void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    void UpdateUI(DialogueNode node)
    {
        speakerText.text = node.speakerName;
        dialogueText.text = node.dialogueText;
        
        foreach (Transform child in optionsPanel)           //清除旧选项
        {
            Destroy(child.gameObject);
        }
        foreach (var option in node.options)                //创建新选项按钮
        {
            if (dialogueManager.CheckOptionConditions(option))
            {
                GameObject buttonObj = Instantiate(optionButtonPrefab, optionsPanel);
                Button button = buttonObj.GetComponent<Button>();
                button.GetComponentInChildren<TextMeshProUGUI>().text = option.optionText;
                button.onClick.AddListener(() => dialogueManager.SelectOption(option));
            }
        }
    }
}