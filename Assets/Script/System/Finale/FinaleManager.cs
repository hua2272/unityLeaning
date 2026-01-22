using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Rendering;


public class FinaleManager : MonoBehaviour
{
    [Header("Content Settings")] 
    [SerializeField] private Sprite[] content;
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

    [Header("Events")] public UnityEvent onCreditsStart;
    public UnityEvent onCreditsComplete;

    private Coroutine creditsCoroutine;
    private bool isPlaying = false;

    private void Start()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
            if (!useBackgroundImage) backgroundImage.sprite = null;
        }

        if (autoStartOnEnable)
        {
            if (isPlaying) return;
            StopAllCoroutines();
            creditsCoroutine = StartCoroutine(PlayCreditsSequence());
        }
    }

    private IEnumerator PlayCreditsSequence()
    {
        isPlaying = true;
        onCreditsStart?.Invoke();
        yield return StartCoroutine(FadeBackground(0f, 1f, backgroundFadeDuration));// 淡入背景
        
        do
        {
            for (int i = 0; i < content.Length; i++)
            {
                yield return StartCoroutine(DisplayContent(content[i], i));
                yield return new WaitForSeconds(intervalBetweenItems);
            }
        } while (loopCredits && isPlaying);
        
        yield return StartCoroutine(FadeBackground(1f, 0f, backgroundFadeDuration));// 淡出背景

        isPlaying = false;
        onCreditsComplete?.Invoke();
    }

    private IEnumerator DisplayContent(Sprite item, int index)
    {
        // 获取UI组件
        Image itemImage = item.GetComponent<Image>();
        CanvasGroup itemCanvasGroup = item.GetComponent<CanvasGroup>();

        if (itemCanvasGroup != null) itemCanvasGroup.alpha = 0f;
        
        itemImage.sprite = item.GetComponent<SpriteRenderer>().sprite;
        itemImage.gameObject.SetActive(true);
        
        //保持图片比例
        float aspectRatio = item.rect.width / item.rect.height;
        RectTransform rect = itemImage.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.y * aspectRatio, rect.sizeDelta.y);

        // 淡入
        if (itemCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(itemCanvasGroup, 0f, 1f, fadeInDuration / 2f));
        }
        yield return new WaitForSeconds(1f);

        // 淡出
        if (itemCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(itemCanvasGroup, 1f, 0f, fadeOutDuration / 2f));
        }
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