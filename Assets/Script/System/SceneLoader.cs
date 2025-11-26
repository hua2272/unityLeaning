using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private string nextSceneName;
    [SerializeField] private Vector3 spawnPosition;
    
    private bool playerInRange;
    private bool isTransitioning = false;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        isTransitioning = true;
        
        if (transitionAnimator != null)
            transitionAnimator.SetTrigger("Start");
        
        yield return new WaitForSeconds(transitionTime);
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
        isTransitioning = false;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = true;
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = false;
    }
    
    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F) && !isTransitioning)
        {
            LoadScene(nextSceneName);
        }
    }
}