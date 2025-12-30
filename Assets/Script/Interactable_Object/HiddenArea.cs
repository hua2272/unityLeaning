using UnityEngine;

public class HiddenArea : MonoBehaviour
{
    [Header("视觉设置")]
    [SerializeField] private Sprite distantSprite; // 远处显示的精灵
    [SerializeField] private Sprite closeSprite;   // 近处显示的实际精灵
    [SerializeField] private Color distantColor = new Color(1, 1, 1, 0.8f); // 远处颜色（半透明）
    [SerializeField] private Color closeColor = Color.white; // 近处颜色（正常）
    
    [Header("光照效果")]
    [SerializeField] private Material distantMaterial; // 远处材质（特殊反射效果）
    [SerializeField] private Material closeMaterial;   // 近处材质（正常材质）
    [SerializeField] private float glowIntensity = 1.2f; // 发光强度
    [SerializeField] private float glowSpeed = 2f; // 发光闪烁速度
    
    [Header("探测范围")]
    [SerializeField] private float detectionRadius = 5f; // 玩家探测半径
    [SerializeField] private LayerMask playerLayer; // 玩家层级
    
    [Header("平滑过渡")]
    [SerializeField] private float transitionSpeed = 3f; // 过渡速度
    
    // 组件引用
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    
    // 状态变量
    private bool playerIsClose = false;
    private float glowTimer = 0f;
    private Color targetColor;
    private Material targetMaterial;
    
    void Start()
    {
        // 获取组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 寻找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // 初始化状态
        InitializeDistantState();
    }
    
    void Update()
    {
        // 检测玩家距离
        CheckPlayerDistance();
        
        // 更新视觉效果
        UpdateVisuals();
        
        // 如果玩家在远处，添加发光效果
        if (!playerIsClose)
        {
            ApplyGlowEffect();
        }
    }
    
    void InitializeDistantState()
    {
        // 设置远处状态的精灵和颜色
        if (distantSprite != null)
        {
            spriteRenderer.sprite = distantSprite;
        }
        
        spriteRenderer.color = distantColor;
        
        // 设置远处材质（特殊光照反射）
        if (distantMaterial != null)
        {
            spriteRenderer.material = distantMaterial;
        }
        else
        {
            // 如果没有指定材质，创建一个基本的发光材质
            CreateDefaultDistantMaterial();
        }
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        // 计算玩家距离
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        // 检查玩家是否在探测范围内
        bool wasClose = playerIsClose;
        playerIsClose = distance <= detectionRadius;
        
        // 如果状态改变，切换显示
        if (wasClose != playerIsClose)
        {
            OnStateChanged();
        }
    }
    
    void OnStateChanged()
    {
        if (playerIsClose)
        {
            // 切换到近处状态
            SwitchToCloseState();
        }
        else
        {
            // 切换到远处状态
            SwitchToDistantState();
        }
    }
    
    void SwitchToCloseState()
    {
        // 切换到实际画面
        if (closeSprite != null)
        {
            spriteRenderer.sprite = closeSprite;
        }
        
        targetColor = closeColor;
        
        // 切换到正常材质
        if (closeMaterial != null)
        {
            targetMaterial = closeMaterial;
        }
        else
        {
            spriteRenderer.material = null; // 使用默认材质
        }
        
        // 触发事件（可用于音效、粒子效果等）
        OnAreaRevealed();
    }
    
    void SwitchToDistantState()
    {
        // 切换回远处提示画面
        if (distantSprite != null)
        {
            spriteRenderer.sprite = distantSprite;
        }
        
        targetColor = distantColor;
        targetMaterial = distantMaterial;
    }
    
    void UpdateVisuals()
    {
        // 平滑过渡颜色
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * transitionSpeed);
        
        // 平滑过渡材质（如果需要）
        if (targetMaterial != null && spriteRenderer.material != targetMaterial)
        {
            spriteRenderer.material.Lerp(spriteRenderer.material, targetMaterial, Time.deltaTime * transitionSpeed);
        }
    }
    
    void ApplyGlowEffect()
    {
        // 计算发光强度（使用正弦波创造脉动效果）
        glowTimer += Time.deltaTime * glowSpeed;
        float glow = Mathf.Sin(glowTimer) * 0.5f + 0.5f; // 0到1之间
        
        // 应用发光效果
        if (spriteRenderer.material != null && spriteRenderer.material.HasProperty("_EmissionColor"))
        {
            Color emissionColor = Color.white * glowIntensity * (1 + glow * 0.3f);
            spriteRenderer.material.SetColor("_EmissionColor", emissionColor);
        }
    }
    
    void CreateDefaultDistantMaterial()
    {
        // 创建一个基本的发光材质
        Material glowMat = new Material(Shader.Find("Sprites/Default"));
        
        // 如果使用URP或HDRP，你可能需要不同的shader
        // Material glowMat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        
        // 添加一些基本的发光效果
        glowMat.EnableKeyword("_EMISSION");
        spriteRenderer.material = glowMat;
        distantMaterial = glowMat;
    }
    
    void OnAreaRevealed()
    {
        // 这里可以添加显示隐藏区域时的特效
        // 例如：播放音效、粒子效果、屏幕抖动等
        
        Debug.Log("隐藏区域被发现！");
        
        // 示例：播放一个简单的粒子效果
        PlayRevealEffect();
    }
    
    void PlayRevealEffect()
    {
        // 创建一个简单的粒子效果
        GameObject effect = new GameObject("RevealEffect");
        effect.transform.position = transform.position;
        
        ParticleSystem ps = effect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.5f;
        main.startSpeed = 2f;
        main.startLifetime = 1f;
        main.startColor = Color.yellow;
        
        var emission = ps.emission;
        emission.rateOverTime = 20f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        
        // 自动销毁
        Destroy(effect, 1f);
    }
    
    // 在编辑器中可视化探测范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = playerIsClose ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // 绘制连接线（如果玩家存在）
        if (playerTransform != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
    
    // 公开方法，供其他脚本调用
    public void RevealArea()
    {
        // 强制显示隐藏区域
        SwitchToCloseState();
    }
    
    public void HideArea()
    {
        // 强制隐藏区域
        SwitchToDistantState();
    }
    
    public bool IsPlayerInRange()
    {
        return playerIsClose;
    }
}