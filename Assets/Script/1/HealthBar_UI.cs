using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar_UI : MonoBehaviour
{
    private Entity entity;
    private CharacterState characterState;
    private RectTransform transform;
    private Slider slider;
    
    private void Start()
    {
        transform = GetComponent<RectTransform>();
        entity = GetComponentInParent<Entity>();
        slider = GetComponentInChildren<Slider>();
        characterState = GetComponentInParent<CharacterState>();
        
        entity.onFlipped += FlipUI;
        characterState.onHealthChange += UpdateHealthUI; //更新血量放到update里会造成不必要的资源消耗，故采用事件
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        slider.maxValue = characterState.GetMaxHealthValue();
        slider.value = characterState.currentHealth;
    }

    private void FlipUI() => transform.Rotate(0, 180, 0); //角色翻转时防止血条翻转，所以再翻转一次233333

    private void OnDestroy() //取消事件订阅
    {
        entity.onFlipped -= FlipUI;
        characterState.onHealthChange -= UpdateHealthUI;
    }
}
