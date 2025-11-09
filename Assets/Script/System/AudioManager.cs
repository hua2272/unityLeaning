using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    [Header("音频混合器")]
    public AudioMixer audioMixer;
    
    [Header("参数名称")]
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";
    
    private void Awake()
    {
        Debug.Log("-------ScreenController instance-------");
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Start()
    {
        // 初始化音量设置
        InitializeAudioSettings();
    }

    /// <summary>
    /// 初始化音频设置
    /// </summary>
    private void InitializeAudioSettings()
    {
        // 设置默认音量（中等音量）
        SetMusicVolume(5);
        SetSFXVolume(5);
    }

    /// <summary>
    /// 设置音乐音量
    /// </summary>
    /// <param name="volumeLevel">音量级别 1-10</param>
    public void SetMusicVolume(int volumeLevel)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioMixer未分配！");
            return;
        }

        // 确保输入值在1-10范围内
        volumeLevel = Mathf.Clamp(volumeLevel, 1, 10);
        
        // 将1-10转换为分贝值（0到-80分贝范围）
        float volumeDB = VolumeToDB(volumeLevel);
        
        // 设置音频混合器参数
        audioMixer.SetFloat(musicVolumeParam, volumeDB);
        
        Debug.Log($"音乐音量设置为: {volumeLevel}/10 ({volumeDB:F1}dB)");
    }

    /// <summary>
    /// 设置音效音量
    /// </summary>
    /// <param name="volumeLevel">音量级别 1-10</param>
    public void SetSFXVolume(int volumeLevel)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioMixer未分配！");
            return;
        }

        // 确保输入值在1-10范围内
        volumeLevel = Mathf.Clamp(volumeLevel, 1, 10);
        
        // 将1-10转换为分贝值
        float volumeDB = VolumeToDB(volumeLevel);
        
        // 设置音频混合器参数
        audioMixer.SetFloat(sfxVolumeParam, volumeDB);
        
        Debug.Log($"音效音量设置为: {volumeLevel}/10 ({volumeDB:F1}dB)");
    }

    /// <summary>
    /// 将音量级别(1-10)转换为分贝值
    /// </summary>
    /// <param name="volumeLevel">音量级别 1-10</param>
    /// <returns>对应的分贝值</returns>
    private float VolumeToDB(int volumeLevel)
    {
        // 音量映射关系：
        // 1 = -80dB (几乎静音)
        // 10 = 0dB (最大音量)
        // 使用对数曲线，符合人耳听觉特性
        
        if (volumeLevel <= 1)
            return -80f; // 静音
        
        // 将1-10转换为0-1的线性值
        float normalizedVolume = (volumeLevel - 1) / 9.0f;
        
        // 使用对数映射：0-1 -> -80dB到0dB
        // 避免完全静音（-80dB），给一个最小音量
        return Mathf.Lerp(-40f, 0f, normalizedVolume);
        
        // 或者使用更精确的对数计算：
        // return Mathf.Log10(normalizedVolume) * 20f;
    }

    /// <summary>
    /// 获取当前音乐音量级别
    /// </summary>
    /// <returns>音量级别 1-10</returns>
    public int GetMusicVolumeLevel()
    {
        if (audioMixer != null && audioMixer.GetFloat(musicVolumeParam, out float currentDB))
        {
            return DBToVolumeLevel(currentDB);
        }
        return 5; // 默认值
    }

    /// <summary>
    /// 获取当前音效音量级别
    /// </summary>
    /// <returns>音量级别 1-10</returns>
    public int GetSFXVolumeLevel()
    {
        if (audioMixer != null && audioMixer.GetFloat(sfxVolumeParam, out float currentDB))
        {
            return DBToVolumeLevel(currentDB);
        }
        return 5; // 默认值
    }

    /// <summary>
    /// 将分贝值转换为音量级别(1-10)
    /// </summary>
    /// <param name="dbValue">分贝值</param>
    /// <returns>音量级别 1-10</returns>
    private int DBToVolumeLevel(float dbValue)
    {
        // 将分贝值转换为0-1的标准化值
        float normalizedVolume = Mathf.InverseLerp(-40f, 0f, dbValue);
        
        // 转换为1-10的整数
        return Mathf.Clamp(Mathf.RoundToInt(normalizedVolume * 9 + 1), 1, 10);
    }
}