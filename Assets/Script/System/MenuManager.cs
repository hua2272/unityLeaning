using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance { get; private set; }
    
    [SerializeField] private GameObject panel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        panel.SetActive(false);
    }

    private void Update()
    {
        if (PlayerInputManager.instance.GetButtonDown("Menu"))
        {
            bool isActive = !panel.activeSelf;
            panel.SetActive(isActive);
            GetComponent<Canvas>().sortingOrder = 100;      //提高渲染层级
            Time.timeScale = isActive ? 0 : 1;              //暂停游戏 TODO 需要停止按键检测
        }
    }
}