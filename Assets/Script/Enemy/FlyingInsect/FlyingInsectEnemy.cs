using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInsectEnemy : Enemy
{
    public FlyingInsectPatrolState patrolState { get; private set; }
    
    [Header("巡逻设置")]
    [SerializeField] public Transform[] patrolPoints; // 巡逻点
    [SerializeField] public float patrolSpeed = 3f;
    [SerializeField] public float patrolWaitTime = 1f; // 到达巡逻点后的等待时间
    [SerializeField] public float patrolHeight = 5f; // 巡逻飞行高度
    public int currentPatrolIndex = 0;
    
    [Header("视野设置")]
    [SerializeField] public float detectionRange = 10f;
    [SerializeField] public float attackRange = 7f;
    [SerializeField] public float minAttackDistance = 3f; // 最小攻击距离
    [SerializeField] public float visionAngle = 90f; // 视野角度
    [SerializeField] public LayerMask playerLayer;
    [SerializeField] public LayerMask obstacleLayer;

    protected override void Awake()
    {
        base.Awake();
        patrolState = new FlyingInsectPatrolState(this, stateMachine, "Patrol", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(patrolState);
    }
    
    protected override void Update()
    {
        base.Update();
    }
    
}
