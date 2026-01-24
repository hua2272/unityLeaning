using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Finale : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image displayImage; // 用于显示图片的Image组件
    [SerializeField] private CanvasGroup imageCanvasGroup; // 显示图片的CanvasGroup

    [Header("Content Settings")] 
    [SerializeField] private Sprite[] content; // 使用Sprite数组存储图片
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float intervalBetweenItems = 0.5f;
    [SerializeField] private bool autoStartOnEnable = true;
    [SerializeField] private bool loopCredits = false;

    [Header("Background Settings")] 
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color backgroundColor = Color.black;
    [SerializeField] private bool useBackgroundImage = false;
    [SerializeField] private float backgroundFadeDuration = 2f;

    [Header("Events")] 
    public UnityEvent onCreditsStart;
    public UnityEvent onCreditsComplete;

    private AudioManager audioManager;
    public AudioClip background;
    private GameStateManager gameStateManager;
    
    private Coroutine creditsCoroutine;
    private bool isPlaying = false;

    private void Start()
    {
        audioManager = AudioManager.instance;
        gameStateManager = GameStateManager.instance;
        gameStateManager.CurrentState = GameState.Finale;
        audioManager.PlayBackgroundMusic(background, true);
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
            if (!useBackgroundImage) backgroundImage.sprite = null;
        }

        // 确保显示图片的CanvasGroup存在
        if (displayImage != null && imageCanvasGroup == null)
        {
            imageCanvasGroup = displayImage.GetComponent<CanvasGroup>();
            if (imageCanvasGroup == null)
            {
                imageCanvasGroup = displayImage.gameObject.AddComponent<CanvasGroup>();
            }
        }

        // 初始隐藏显示图片
        if (imageCanvasGroup != null)
        {
            imageCanvasGroup.alpha = 0f;
            displayImage.gameObject.SetActive(false);
        }

        if (autoStartOnEnable)
        {
            StartCredits();
        }
    }

    public void StartCredits()
    {
        if (isPlaying) return;
        
        StopAllCoroutines();
        creditsCoroutine = StartCoroutine(PlayCreditsSequence());
    }

    private IEnumerator PlayCreditsSequence()
    {
        isPlaying = true;
        onCreditsStart?.Invoke();
        
        // 淡入背景
        yield return StartCoroutine(FadeBackground(0f, 1f, backgroundFadeDuration));
        
        do
        {
            for (int i = 0; i < content.Length; i++)
            {
                yield return StartCoroutine(DisplayContent(content[i]));
                yield return new WaitForSeconds(intervalBetweenItems);
            }
        } while (loopCredits && isPlaying);
        
        // 淡出背景
        yield return StartCoroutine(FadeBackground(1f, 0f, backgroundFadeDuration));

        isPlaying = false;
        onCreditsComplete?.Invoke();
    }

    private IEnumerator DisplayContent(Sprite sprite)
    {
        if (displayImage == null || sprite == null) yield break;
        
        // 设置要显示的图片
        displayImage.sprite = sprite;
        displayImage.gameObject.SetActive(true);
        
        // 保持图片比例
        if (displayImage.preserveAspect)
        {
            // 如果设置了保持比例，自动处理
            displayImage.SetNativeSize();
        }
        else
        {
            // 手动计算比例并设置大小
            float aspectRatio = sprite.rect.width / sprite.rect.height;
            RectTransform rectTransform = displayImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 保持高度不变，按比例调整宽度
                float newWidth = rectTransform.sizeDelta.y * aspectRatio;
                rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
            }
        }

        // 淡入
        yield return StartCoroutine(FadeCanvasGroup(imageCanvasGroup, 0f, 1f, fadeInDuration));
        
        // 等待显示时间（这里固定1秒，你可以根据需要调整）
        yield return new WaitForSeconds(1f);
        
        // 淡出
        yield return StartCoroutine(FadeCanvasGroup(imageCanvasGroup, 1f, 0f, fadeOutDuration));
        
        // 隐藏图片
        displayImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        if (group == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        group.alpha = endAlpha;
    }

    private IEnumerator FadeBackground(float startAlpha, float endAlpha, float duration)
    {
        if (backgroundImage == null) yield break;

        Color startColor = backgroundColor;
        Color endColor = backgroundColor;
        startColor.a = startAlpha;
        endColor.a = endAlpha;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            backgroundImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        backgroundImage.color = endColor;
    }
}