using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public static SystemManager Instance { get; private set; }
    
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isActive = !panel.activeSelf;
            panel.SetActive(isActive);
            Time.timeScale = isActive ? 0 : 1;  //暂停游戏
        }
    }
}
