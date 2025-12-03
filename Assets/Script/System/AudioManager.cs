using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    [Header("音频源")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource; // 独立的UI音频源
    
    [Header("常用音频")] //仅常用音频片段通过该脚本管理
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip uiNavigate;
    
    private AudioClip currentMusic;
    
    [Header("音频混合器")]
    public AudioMixer audioMixer;
    
    [Header("参数名称")]
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";
    
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    
    private void Awake()
    {
        Debug.Log("<color=#FF0000>-------AudioManager instance-------</color>");
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
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
        // 从PlayerPrefs加载设置，没有则使用默认值 todo 读取存档的音量数据需要修改
        int savedMusicVol = PlayerPrefs.GetInt(MUSIC_VOLUME_KEY, 5);
        int savedSFXVol = PlayerPrefs.GetInt(SFX_VOLUME_KEY, 5);
        
        SetMusicVolume(savedMusicVol);
        SetSFXVolume(savedSFXVol);
    }
    
    public void SetMusicVolume(int volumeLevel)
    {
        volumeLevel = Mathf.Clamp(volumeLevel, 1, 10);
        float volumeDB = VolumeToDB(volumeLevel);
        
        audioMixer.SetFloat(musicVolumeParam, volumeDB);
        PlayerPrefs.SetInt(MUSIC_VOLUME_KEY, volumeLevel);
        PlayerPrefs.Save();
    }
    
    public void SetSFXVolume(int volumeLevel)
    {
        volumeLevel = Mathf.Clamp(volumeLevel, 1, 10);
        float volumeDB = VolumeToDB(volumeLevel);
        
        audioMixer.SetFloat(sfxVolumeParam, volumeDB);
        PlayerPrefs.SetInt(SFX_VOLUME_KEY, volumeLevel);
        PlayerPrefs.Save();
    }
    
    public void PlayBackgroundMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource != null && musicClip != null)
        {
            // 如果已经在播放同一首音乐，则不重复播放
            if (currentMusic == musicClip && musicSource.isPlaying)
                return;
            
            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.Play();
            currentMusic = musicClip;
        }
    }
    
    /// 停止背景音乐
    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            currentMusic = null;
        }
    }
    
    /// 暂停背景音乐
    public void PauseBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }
    
    /// 恢复背景音乐
    public void ResumeBackgroundMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && currentMusic != null)
        {
            musicSource.Play();
        }
    }
    
    
    /// 切换背景音乐（带淡入淡出效果）
    /// <param name="newMusicClip">新的音乐片段</param>
    /// <param name="fadeDuration">淡入淡出时间</param>
    /// <param name="loop">是否循环</param>
    public void SwitchBackgroundMusic(AudioClip newMusicClip, float fadeDuration = 1f, bool loop = true)
    {
        StartCoroutine(FadeSwitchMusic(newMusicClip, fadeDuration, loop));
    }
    
    private System.Collections.IEnumerator FadeSwitchMusic(AudioClip newMusicClip, float fadeDuration, bool loop)
    {
        // 淡出现有音乐
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }
            musicSource.Stop();
            musicSource.volume = startVolume;
        }
        PlayBackgroundMusic(newMusicClip, loop);// 播放新音乐
        // 淡入新音乐（如果需要）
        if (newMusicClip != null)
        {
            float originalVolume = musicSource.volume;
            musicSource.volume = 0;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, originalVolume, t / fadeDuration);
                yield return null;
            }
            musicSource.volume = originalVolume;
        }
    }
    
    /// 通用的播放方法
    public void PlaySFX(AudioClip clip, AudioSourceType sourceType = AudioSourceType.SFX)
    {
        AudioSource source = GetAudioSource(sourceType);
        if (source != null && clip != null)
        {
            source.PlayOneShot(clip);
        }
    }
    
    // UI特定音效（便捷方法）
    public void PlayUISound(UISoundType type)
    {
        AudioClip clip = null;
        
        switch(type)
        {
            case UISoundType.Click:
                clip = uiClick;
                break;
            case UISoundType.Navigate:
                clip = uiNavigate;
                break;
        }
        
        PlaySFX(clip, AudioSourceType.UI);
    }
    
    private AudioSource GetAudioSource(AudioSourceType type)
    {
        switch(type)
        {
            case AudioSourceType.Music: return musicSource;
            case AudioSourceType.SFX: return sfxSource;
            case AudioSourceType.UI: return uiSource;
            default: return sfxSource;
        }
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
        if (volumeLevel <= 1) return -80f; // 静音
        float normalizedVolume = (volumeLevel - 1) / 9.0f;// 将1-10转换为0-1的线性值
        
        // 使用对数映射：0-1 -> -80dB到0dB
        // 避免完全静音（-80dB），给一个最小音量
        return Mathf.Lerp(-40f, 0f, normalizedVolume);
    }

    /// <summary>
    /// 将分贝值转换为音量级别(1-10)
    /// </summary>
    /// <param name="dbValue">分贝值</param>
    /// <returns>音量级别 1-10</returns>
    private int DBToVolumeLevel(float dbValue)
    {
        float normalizedVolume = Mathf.InverseLerp(-40f, 0f, dbValue);// 将分贝值转换为0-1的标准化值
        return Mathf.Clamp(Mathf.RoundToInt(normalizedVolume * 9 + 1), 1, 10);// 转换为1-10的整数
    }
}

public enum AudioSourceType
{
    Music,
    SFX,
    UI
}

public enum UISoundType
{
    Click,
    Navigate,
    Confirm,
    Cancel
}