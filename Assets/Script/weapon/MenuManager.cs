using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    
    [SerializeField] private GameObject panel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        panel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isActive = !panel.activeSelf;
            panel.SetActive(isActive);
            Time.timeScale = isActive ? 0 : 1;  //可选：暂停游戏当背包打开
        }
    }
}