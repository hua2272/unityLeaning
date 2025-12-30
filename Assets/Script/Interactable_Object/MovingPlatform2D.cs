using UnityEngine;

public class PianoKeyPlatform : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("平台下降速度")]
    public float downSpeed = 5f;
    
    [Tooltip("平台抬起速度")]
    public float upSpeed = 8f;
    
    [Tooltip("平台按下深度")]
    public float pressDepth = 0.3f;
    
    [Header("钢琴音效设置")]
    [Tooltip("钢琴音符音频片段")]
    public AudioClip pianoNote;
    
    [Tooltip("音符基础音量")]
    [Range(0f, 1f)]
    public float baseVolume = 0.8f;
    
    [Tooltip("最大触键力度对应的额外音量")]
    [Range(0f, 0.5f)]
    public float velocityVolume = 0.3f;
    
    [Tooltip("延音时间（秒）")]
    public float sustainTime = 2.0f;
    
    [Tooltip("声音衰减曲线 - 控制音量的衰减速度")]
    public AnimationCurve volumeDecayCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("物理模拟")]
    [Tooltip("平台回弹力度")]
    public float reboundForce = 2f;
    
    [Tooltip("阻尼系数")]
    public float damping = 0.95f;
    
    [Header("检测设置")]
    [Tooltip("检测玩家站立的区域")]
    public Vector2 detectionSize = new Vector2(1f, 0.1f);
    
    [Tooltip("检测区域的Y轴偏移")]
    public float detectionOffsetY = 0.5f;
    
    [Tooltip("玩家标签")]
    public string playerTag = "Player";
    
    // 私有变量
    private Vector2 originalPosition;
    private Vector2 pressedPosition;
    private Rigidbody2D rb;
    private BoxCollider2D platformCollider;
    
    // 钢琴模拟状态
    private enum PianoKeyState { Idle, Pressing, Sustaining, Releasing, Rebouncing }
    private PianoKeyState currentState = PianoKeyState.Idle;
    
    // 音频控制
    private AudioSource audioSource;
    private float noteStartTime;
    private float currentVelocity; // 按下速度，影响音量和音色
    private float sustainProgress; // 延音进度
    private bool soundPlaying = false;
    
    // 物理模拟
    private float currentVerticalVelocity;
    private float targetYPosition;
    
    void Start()
    {
        // 记录初始位置
        originalPosition = transform.position;
        pressedPosition = originalPosition + Vector2.down * pressDepth;
        
        // 获取或添加必要的组件
        platformCollider = GetComponent<BoxCollider2D>();
        
        // 为平台添加刚体（如果还没有的话）
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        
        // 添加音频源组件
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D音效
        
        // 初始化状态
        targetYPosition = originalPosition.y;
    }
    
    void Update()
    {
        // 检测玩家是否站在平台上
        bool playerDetected = CheckForPlayer();
        
        // 状态机处理
        HandleStateMachine(playerDetected);
        
        // 平滑移动平台
        SmoothMovePlatform();
        
        // 更新音频效果
        UpdateAudioEffect();
    }
    
    void HandleStateMachine(bool playerDetected)
    {
        switch (currentState)
        {
            case PianoKeyState.Idle:
                if (playerDetected)
                {
                    // 开始按下
                    currentState = PianoKeyState.Pressing;
                    currentVelocity = downSpeed; // 记录按下速度
                    targetYPosition = pressedPosition.y;
                    OnKeyPress();
                }
                break;
                
            case PianoKeyState.Pressing:
                if (Mathf.Abs(transform.position.y - pressedPosition.y) < 0.01f)
                {
                    // 到达按下位置，进入延音状态
                    currentState = PianoKeyState.Sustaining;
                    sustainProgress = 0f;
                }
                break;
                
            case PianoKeyState.Sustaining:
                if (!playerDetected)
                {
                    // 玩家离开，开始释放
                    currentState = PianoKeyState.Releasing;
                    OnKeyRelease();
                }
                else
                {
                    // 更新延音进度
                    sustainProgress += Time.deltaTime / sustainTime;
                    if (sustainProgress >= 1f)
                    {
                        currentState = PianoKeyState.Releasing;
                        OnKeyRelease();
                    }
                }
                break;
                
            case PianoKeyState.Releasing:
                if (!playerDetected)
                {
                    targetYPosition = originalPosition.y;
                    if (Mathf.Abs(transform.position.y - originalPosition.y) < 0.01f)
                    {
                        // 到达原始位置，进入回弹状态
                        currentState = PianoKeyState.Rebouncing;
                        currentVerticalVelocity = -reboundForce; // 轻微回弹
                    }
                }
                else
                {
                    // 玩家又站上来了，重新按下
                    currentState = PianoKeyState.Pressing;
                    currentVelocity = downSpeed;
                    targetYPosition = pressedPosition.y;
                    OnKeyPress();
                }
                break;
                
            case PianoKeyState.Rebouncing:
                // 模拟阻尼回弹
                currentVerticalVelocity *= damping;
                targetYPosition = originalPosition.y;
                
                if (Mathf.Abs(currentVerticalVelocity) < 0.01f)
                {
                    currentState = PianoKeyState.Idle;
                }
                break;
        }
    }
    
    void SmoothMovePlatform()
    {
        // 根据状态决定移动速度
        float moveSpeed;
        switch (currentState)
        {
            case PianoKeyState.Pressing:
                moveSpeed = downSpeed;
                break;
            case PianoKeyState.Releasing:
                moveSpeed = upSpeed;
                break;
            case PianoKeyState.Rebouncing:
                // 回弹时使用物理模拟
                currentVerticalVelocity *= damping;
                float newY = Mathf.Clamp(
                    transform.position.y + currentVerticalVelocity * Time.deltaTime,
                    pressedPosition.y,
                    originalPosition.y + 0.1f // 允许轻微超出
                );
                Vector2 newPosition = new Vector2(transform.position.x, newY);
                rb.MovePosition(newPosition);
                return;
            default:
                moveSpeed = upSpeed;
                break;
        }
        
        // 平滑移动到目标位置
        float newYPos = Mathf.MoveTowards(
            transform.position.y, 
            targetYPosition, 
            moveSpeed * Time.deltaTime
        );
        
        Vector2 pos = new Vector2(transform.position.x, newYPos);
        rb.MovePosition(pos);
    }
    
    void OnKeyPress()
    {
        if (pianoNote == null || audioSource == null) return;
        
        // 计算最终音量（基于基础音量和速度力度）
        float velocityFactor = Mathf.Clamp01(currentVelocity / downSpeed);
        float finalVolume = baseVolume + (velocityVolume * velocityFactor);
        
        // 播放音符
        audioSource.clip = pianoNote;
        audioSource.volume = finalVolume;
        audioSource.Play();
        
        noteStartTime = Time.time;
        soundPlaying = true;
        
        Debug.Log($"钢琴键按下 - 力度: {velocityFactor:F2}, 音量: {finalVolume:F2}");
    }
    
    void OnKeyRelease()
    {
        Debug.Log("钢琴键释放");
        // 释放时不需要立即停止音频，让音频自然衰减
    }
    
    void UpdateAudioEffect()
    {
        if (!soundPlaying || !audioSource.isPlaying) return;
        
        float timeSinceNoteStart = Time.time - noteStartTime;
        
        // 根据时间和状态调整音量
        float volumeMultiplier = 1f;
        
        if (currentState == PianoKeyState.Sustaining)
        {
            // 延音阶段：使用衰减曲线
            float decayFactor = volumeDecayCurve.Evaluate(Mathf.Clamp01(timeSinceNoteStart / sustainTime));
            volumeMultiplier = decayFactor;
        }
        else if (currentState == PianoKeyState.Releasing)
        {
            // 释放阶段：快速衰减
            float releaseProgress = Mathf.Clamp01((transform.position.y - pressedPosition.y) / pressDepth);
            volumeMultiplier = Mathf.Lerp(1f, 0f, releaseProgress * 2f); // 快速衰减
        }
        else if (currentState == PianoKeyState.Rebouncing)
        {
            // 回弹阶段：几乎静音
            volumeMultiplier = 0.1f;
        }
        
        // 应用音量调整
        audioSource.volume = Mathf.Clamp(baseVolume * volumeMultiplier, 0f, 1f);
        
        // 如果音量接近0，停止播放
        if (audioSource.volume < 0.01f)
        {
            audioSource.Stop();
            soundPlaying = false;
        }
    }
    
    bool CheckForPlayer()
    {
        // 创建一个检测区域（在平台上方一点的位置）
        Vector2 detectionCenter = (Vector2)transform.position + Vector2.up * detectionOffsetY;
        
        // 检测该区域内是否有玩家
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            detectionCenter, 
            detectionSize, 
            0f
        );
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(playerTag))
            {
                if (IsPlayerOnTop(collider))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    bool IsPlayerOnTop(Collider2D playerCollider)
    {
        Bounds playerBounds = playerCollider.bounds;
        float playerBottom = playerBounds.min.y;
        
        Bounds platformBounds = platformCollider.bounds;
        float platformTop = platformBounds.max.y;
        
        float distance = playerBottom - platformTop;
        return Mathf.Abs(distance) < 0.1f && playerBottom > platformTop;
    }
    
    // 在编辑器中可视化
    void OnDrawGizmosSelected()
    {
        // 原始位置
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(originalPosition, new Vector3(1f, 0.1f, 1f));
        
        // 按下位置
        Gizmos.color = Color.red;
        Vector3 downPosition = transform.position + Vector3.down * pressDepth;
        Gizmos.DrawWireCube(downPosition, new Vector3(1f, 0.1f, 1f));
        Gizmos.DrawLine(transform.position, downPosition);
        
        // 当前状态显示
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, 
                                $"State: {currentState}\nVolume: {(audioSource != null ? audioSource.volume.ToString("F2") : "0")}",
                                style);
        #endif
    }
    
    // 添加一个可调用的方法来测试钢琴键
    [ContextMenu("测试钢琴键按下")]
    public void TestKeyPress()
    {
        if (currentState != PianoKeyState.Idle) return;
        
        currentState = PianoKeyState.Pressing;
        currentVelocity = downSpeed;
        targetYPosition = pressedPosition.y;
        OnKeyPress();
    }
}