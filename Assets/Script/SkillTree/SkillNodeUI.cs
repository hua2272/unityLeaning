using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SkillNodeUI : MonoBehaviour, IPointerClickHandler
{
    public string skillName;
    public string description;
    public int maxLevel = 4;                            // 最大等级
    public int currentLevel { get; set; } = 0;                        //当前等级
    public int requiredPoints = 1;                      // 每次升级需要的点数
    
    [Header("技能依赖")]
    //public List requiredSkills;                       // 需要先解锁的技能列表
    
    [HideInInspector] public UnityEvent OnNodeClicked;
    
    private void Start()
    {
        UpdateUI();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnNodeClicked?.Invoke();
    }
    
    // 更新UI显示
    public void UpdateUI()
    {
        if (currentLevel == 0)
        {
            transform.Find("skillIcon").gameObject.SetActive(true);
            transform.Find("skillUnlockedIcon").gameObject.SetActive(false);
        }
        else
        {
            transform.Find("skillUnlockedIcon").gameObject.SetActive(true);
            transform.Find("skillIcon").gameObject.SetActive(false);
        }
        for (int i = 1; i < currentLevel; i++)                                               //更新等级指示器
        {
            transform.Find("levelIcon/LV" + i).GetComponent<Image>().sprite = transform.Find("levelUnlockedIcon").GetComponent<Image>().sprite;
            Debug.Log("test");
        }
    }
}