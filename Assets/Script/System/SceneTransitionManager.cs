using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition Settings")]
    [SerializeField] private GameObject fadeCanvasPrefab;
    [SerializeField] private float fadeDuration = 1.0f;
    
    private Image fadeImage;
    private Canvas fadeCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeFadeCanvas();
    }

    private void InitializeFadeCanvas()
    {
        fadeCanvas = Instantiate(fadeCanvasPrefab).GetComponent<Canvas>();
        DontDestroyOnLoad(fadeCanvas.gameObject);
        fadeImage = fadeCanvas.GetComponentInChildren<Image>();
        fadeImage.gameObject.SetActive(false);
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        yield return StartCoroutine(FadeOut());                                                         //淡出
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield return StartCoroutine(FadeIn());                                                          //淡入
    }

    private IEnumerator FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1 - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        fadeImage.gameObject.SetActive(false);
    }
}