using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class AutoSaveTrigger : MonoBehaviour
{
    [Header("自动存档设置")]
    public string savePointName = "存档点";
    public float saveCooldown = 10f; // 存档冷却时间，防止频繁存档
    
    [Header("UI提示设置")]
    private UIManager uiManager;
    public float displayTime = 2f;
    public TextMeshProUGUI savePointText;
    public Image saveIcon;
    
    private bool canSave = true;
    
    private void Start()
    {
        uiManager = UIManager.instance;
        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;  // 确保Collider2D是触发器
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canSave)
        {
            TriggerAutoSave();
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canSave)
        {
            TriggerAutoSave();
        }
    }
    
    private void TriggerAutoSave()
    {
        if (!canSave) return;
        //GameSaveManager.instance.SaveGame();  // todo 执行存档
        Debug.Log($"在 {savePointName} 自动存档成功");
        uiManager.SetUIVisibility(UIGroup.AutoSaveInfo, true, 0.5f);
        //savePointText.text = $"已存档 - {savePointName}";
        StartCoroutine(SaveCooldown());// 进入冷却
    }
    
    
    private IEnumerator SaveCooldown()
    {
        canSave = false;
        yield return new WaitForSeconds(saveCooldown);
        canSave = true;
    }
    
    // 可视化调试
    private void OnDrawGizmos()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, collider.bounds.size);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
            
            // 显示存档点名称
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"存档点: {savePointName}");
            #endif
        }
    }
    
    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}