using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ProximityPrompt : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float detectionRadius = 3f;
    //[SerializeField] private LayerMask playerLayer;
    private Transform playerTransform;
    
    [Header("UI提示设置")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image promptIcon;
    
    [Header("提示内容")]
    [SerializeField] private string promptMessage = "按 E 互动";
    [SerializeField] private Color textColor = Color.white;
    
    [Header("动画设置")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private CanvasGroup canvasGroup;
    private bool isPlayerNearby = false;
    private Coroutine fadeCoroutine;
    
    void Start()
    {
        canvasGroup = promptCanvas.GetComponent<CanvasGroup>();
        playerTransform = PlayerManager.instance.player.transform;
        canvasGroup.alpha = 0f;             // 初始隐藏UI
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        // 设置提示文本
        promptText.text = promptMessage;
        promptText.color = textColor;
        
    }
    
    void Update()
    {
        float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(playerTransform.position.x, playerTransform.position.y));
        
        bool wasPlayerNearby = isPlayerNearby;
        isPlayerNearby = distance <= detectionRadius;
        
        // 状态变化时触发动画
        if (isPlayerNearby && !wasPlayerNearby)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadePrompt(0f, 1f));
        }
        else if (!isPlayerNearby && wasPlayerNearby)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadePrompt(canvasGroup.alpha, 0f));
        }
    }
    
    IEnumerator FadePrompt(float startAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeDuration);
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            canvasGroup.alpha = currentAlpha;
            // 完全显示时启用交互
            if (currentAlpha > 0.9f)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            yield return null;
        }
        // 确保最终值准确
        canvasGroup.alpha = targetAlpha;
        if (targetAlpha < 0.1f)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        fadeCoroutine = null;
    }
    
    void OnDrawGizmosSelected()     // 可视化检测范围（在Scene视图中显示）
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, detectionRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}