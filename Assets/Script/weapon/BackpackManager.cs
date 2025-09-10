using UnityEngine;
using System.Collections;

public class BackpackManager : MonoBehaviour
{
    public static BackpackManager Instance { get; private set; }

    [Header("UI References")] [SerializeField]
    private GameObject backpackPanel;

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
        backpackPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isActive = !backpackPanel.activeSelf;
            backpackPanel.SetActive(isActive);
            Time.timeScale = isActive ? 0 : 1;  //可选：暂停游戏当背包打开
        }
    }
}