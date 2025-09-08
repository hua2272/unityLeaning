using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clone_Skill : Skill
{
    [Header("Clone Info")] 
    [SerializeField] private float cloneDuration;
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private bool canAttack;
    
    private SpriteRenderer sr;
    private Animator anim;
    [SerializeField] private float colorLosingSpeed;
    private float cloneTimer;

    [SerializeField] private Transform attackCheck;
    [SerializeField] private float attackCheckRadius = 0.8f;
    private Transform closestEnemy;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("SpriteRenderer组件未找到，脚本将禁用。", this);
            this.enabled = false; // 禁用脚本
            return;
        }
        anim = GetComponent<Animator>();
    }
    

    public void SetupClone(Transform _newTransform)
    {
        GameObject newClone = Instantiate(clonePrefab);
        if (canAttack)
        {
            anim.SetInteger("AttackNumber", Random.Range(1, 4));
        }

        transform.position = _newTransform.position;
        cloneTimer = cloneDuration;
        FaceClosetTarget();
        
        cloneTimer -= Time.deltaTime;
        if (cloneTimer < 0)
        {
            sr.color = new Color(1, 1, 1, sr.color.a - colorLosingSpeed * Time.deltaTime); //分身变透明
            if (sr.color.a <= 0)
            {
                Destroy(gameObject); //销毁已透明的分身（可以放到CreateClone方法中，无需频繁检测）
            }
        }
    }

    private void AnimationTrigger()
    {
        cloneTimer = -1;
    }

    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCheck.position, attackCheckRadius);
        foreach (Collider2D hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                hit.GetComponent<Enemy>().DamageEffect();
            }
        }
    }

    private void FaceClosetTarget() //分身面向敌人
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 25);
        float closestDistance = Mathf.Infinity;
        foreach (Collider2D hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                float distance2Enemy = Vector2.Distance(transform.position, hit.transform.position);
                if (distance2Enemy < closestDistance)
                {
                    closestDistance = distance2Enemy;
                    closestEnemy = hit.transform;
                }
            }
        }

        if (closestEnemy != null)
        {
            if (transform.position.x > closestEnemy.position.x)
            {
                transform.Rotate(0, 180, 0);
            }
        }
    }

    // public void CreateClone(Transform _clonePosition)
    // {
    //     GameObject newClone = Instantiate(clonePrefab);
    //     newClone.GetComponent<Clone_Skill_Controller>().SetupClone(_clonePosition, cloneDuration, canAttack);
    // }
}
