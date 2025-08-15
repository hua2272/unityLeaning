using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss Settings")]
    public Transform player;
    public float phase1AttackRange = 3f;
    public float phase2MeleeRange = 3f;
    public float phase2RangedRange = 5f;
    public float moveSpeed = 2f;
    public int maxHealth = 1000;
    
    [Header("Attack Settings")]
    public float phase1AttackDelay = 0.5f;
    public float hammerAttackDelay = 0.7f;
    public float magicAttackDelay = 1.2f;
    public GameObject magicProjectilePrefab;
    public Transform projectileSpawnPoint;
    
    [Header("State")]
    public int currentHealth;
    public bool isFacingRight = true;
    public bool isAttacking = false;
    public BossState currentState = BossState.Phase1;
    
    public Animator animator;
    private Rigidbody2D rb;
    
    public enum BossState { Phase1, Phase2 }
    
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        UpdatePhaseVisual();
    }
    
    void Update()
    {
        UpdatePhase();
        UpdateAnimations();
    }
    
    void UpdatePhase()
    {
        if (currentHealth <= maxHealth / 2 && currentState != BossState.Phase2)
        {
            currentState = BossState.Phase2;
            UpdatePhaseVisual();
        }
    }
    
    void UpdatePhaseVisual()
    {
        animator.SetInteger("Phase", currentState == BossState.Phase1 ? 1 : 2);
    }
    
    void UpdateAnimations()
    {
        animator.SetBool("Walking", rb.velocity.magnitude > 0.1f);
    }
    
    public void MoveTowardsPlayer()
    {
        if (isAttacking) return;
        
        float direction = player.position.x > transform.position.x ? 1 : -1;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        
        if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
        {
            Flip();
        }
    }
    
    public void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    public void FacePlayer()
    {
        if ((player.position.x > transform.position.x && !isFacingRight) || 
            (player.position.x < transform.position.x && isFacingRight))
        {
            Flip();
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        // 死亡处理
        Destroy(gameObject, 1f);
    }
}