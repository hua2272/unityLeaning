using UnityEngine;

public class ScreenEffect : MonoBehaviour
{
    [Header("屏幕震动设置")]
    public float minShakeDuration = 0.1f;
    public float maxShakeDuration = 0.5f;
    public float minShakeMagnitude = 0.05f;
    public float maxShakeMagnitude = 0.2f;
    private CameraShake cameraShake;
    
    void Start()
    {
        cameraShake = Camera.main.GetComponent<CameraShake>();
    }
    
    public void CameraShakeEffect()
    {
        //float normalizedDamage = Mathf.Clamp01(damage / maxHealth);//计算震动强度和持续时间（基于伤害值）
        float normalizedDamage = 1f;
        float shakeDuration = Mathf.Lerp(minShakeDuration, maxShakeDuration, normalizedDamage);
        float shakeMagnitude = Mathf.Lerp(minShakeMagnitude, maxShakeMagnitude, normalizedDamage);
        
        if (cameraShake != null)
            cameraShake.TriggerShake(shakeDuration, shakeMagnitude);
    }
}
