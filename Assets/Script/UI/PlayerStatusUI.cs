using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    private PlayerStatus playerStatus;
    private Slider healthSlider;
    private Slider staminaSlider;
    
    private void Start()
    {
        playerStatus = PlayerManager.instance.playerStatus;
        Slider[] sliders = GetComponentsInChildren<Slider>();
        healthSlider = sliders[0];
        staminaSlider = sliders[1];
        playerStatus.onHealthChange.AddListener(UpdateHealthUI); //更新血量放到update里会造成不必要的资源消耗，故采用事件
        playerStatus.onStaminaChange.AddListener(UpdateStaminaUI);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        healthSlider.maxValue = playerStatus.health.getValue() + playerStatus.extraHealth.getValue();
        healthSlider.value = playerStatus.currentHealth;
    }
    
    private void UpdateStaminaUI()
    {
        staminaSlider.maxValue = playerStatus.stamina.getValue() + playerStatus.extraStamina.getValue();
        staminaSlider.value = playerStatus.currentStamina;
    }
    
    private void OnDestroy()
    {
        playerStatus.onHealthChange.RemoveListener(UpdateHealthUI);
        playerStatus.onHealthChange.RemoveListener(UpdateStaminaUI);
    }
}
