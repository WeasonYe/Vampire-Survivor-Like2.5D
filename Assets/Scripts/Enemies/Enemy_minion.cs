using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 小怪敌人，继承自EnemyBase
/// 具有更快的移动速度，但生命值和伤害较低
/// </summary>
public class Enemy_minion : EnemyBase
{
    [Header("小怪特有设置")]
    [Tooltip("移动速度倍率")]
    [SerializeField]
    private float speedMultiplier = 1.5f;

    protected override void Start()
    {
        base.Start();
        
        // 应用速度倍率
        SetMoveSpeed(moveSpeed * speedMultiplier);
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void HandleAI()
    {
        base.HandleAI();
        
        // 小怪特有的AI行为
        // 可以在这里添加其他小怪特有的行为
    }

    protected override void Die()
    {
        base.Die();
        
        // 小怪死亡处理
    }
}
