using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家生命值管理脚本
/// </summary>
public class PlayerHealth : Singleton<PlayerHealth>, IDamageable
{
    [Header("生命值设置")]
    [Tooltip("最大生命值")]
    [SerializeField]
    private float maxHealth = 100f;

    [Tooltip("当前生命值")]
    private float currentHealth;

    [Tooltip("是否无敌")]
    private bool isInvulnerable = false;

    [Tooltip("无敌时间（秒）")]
    [SerializeField]
    private float invulnerabilityDuration = 1f;

    [Header("碰撞检测设置")]
    [Tooltip("胶囊体底部偏移（相对于玩家中心）")]
    [SerializeField]
    private Vector3 capsuleBottomOffset = new Vector3(0f, -0.5f, 0f);

    [Tooltip("胶囊体顶部偏移（相对于玩家中心）")]
    [SerializeField]
    private Vector3 capsuleTopOffset = new Vector3(0f, 0.5f, 0f);

    [Tooltip("胶囊体半径")]
    [SerializeField]
    private float capsuleRadius = 0.5f;

    [Tooltip("碰撞检测层级")]
    [SerializeField]
    private LayerMask enemyLayerMask;

    [Tooltip("最大碰撞检测数量")]
    [SerializeField]
    private int maxColliders = 10;

    private Collider[] colliderBuffer;
    private List<EnemyBase> currentCollidingEnemies = new List<EnemyBase>();

    /// <summary>
    /// 当前生命值
    /// </summary>
    public float CurrentHealth => currentHealth;

    /// <summary>
    /// 最大生命值
    /// </summary>
    public float MaxHealth => maxHealth;

    /// <summary>
    /// 生命值百分比
    /// </summary>
    public float HealthPercentage => currentHealth / maxHealth;

    /// <summary>
    /// 是否死亡
    /// </summary>
    public bool IsDead => currentHealth <= 0;

    protected override void Awake()
    {
        base.Awake();
        colliderBuffer = new Collider[maxColliders];
    }

    private void Start()
    {
        // 初始化生命值
        currentHealth = maxHealth;

        if (enemyLayerMask == 0)
        {
            enemyLayerMask = LayerMask.GetMask("Enemy");
        }

        // 注册事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.PlayerDamaged, OnPlayerDamaged);
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.PlayerHealed, OnPlayerHealed);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 注销事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.PlayerDamaged, OnPlayerDamaged);
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.PlayerHealed, OnPlayerHealed);
        }
    }

    private void Update()
    {
        if (isInvulnerable || IsDead)
            return;

        CheckEnemyCollision();
    }

    private void CheckEnemyCollision()
    {
        // 计算胶囊体的两个端点
        Vector3 point1 = transform.position + capsuleBottomOffset;
        Vector3 point2 = transform.position + capsuleTopOffset;
        
        // 使用 OverlapCapsuleNonAlloc 检测碰撞
        int colliderCount = Physics.OverlapCapsuleNonAlloc(point1, point2, capsuleRadius, colliderBuffer, enemyLayerMask);
        
        currentCollidingEnemies.Clear();
        
        for (int i = 0; i < colliderCount; i++)
        {
            EnemyBase enemy = colliderBuffer[i].GetComponent<EnemyBase>();
            if (enemy != null && !enemy.IsDead)
            {
                currentCollidingEnemies.Add(enemy);
            }
        }
        
        if (currentCollidingEnemies.Count > 0)
        {
            EnemyBase nearestEnemy = GetNearestEnemy();
            if (nearestEnemy != null)
            {
                if (DamageSystem.Instance != null)
                {
                    DamageSystem.Instance.DealDamage(gameObject, nearestEnemy.Damage, DamageType.Physical, false, nearestEnemy, nearestEnemy.gameObject);
                }
                else
                {
                    TakeDamage(nearestEnemy.Damage, nearestEnemy);
                }
            }
        }
    }

    private EnemyBase GetNearestEnemy()
    {
        EnemyBase nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
        foreach (EnemyBase enemy in currentCollidingEnemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }
        
        return nearestEnemy;
    }

    public void Heal(float healAmount)
    {
        if (IsDead)
            return;

        // 增加生命值
        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        float actualHeal = currentHealth - oldHealth;

        // 触发治疗事件
        if (GameEventsManager.Instance != null && actualHeal > 0)
        {
            GameEventsManager.Instance.TriggerEvent(GameEventsManager.EventTypes.PlayerHealed, actualHeal, currentHealth);
        }
    }

    /// <summary>
    /// 设置最大生命值
    /// </summary>
    /// <param name="newMaxHealth">新的最大生命值</param>
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    /// <summary>
    /// 直接设置生命值
    /// </summary>
    /// <param name="newHealth">新的生命值</param>
    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        // 检查是否死亡
        if (IsDead)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡
    /// </summary>
    private void Die()
    {
        // 触发死亡事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.TriggerEvent(GameEventsManager.EventTypes.PlayerDied);
        }

        // 这里可以添加死亡效果、游戏结束逻辑等
        Debug.Log("Player died!");
    }

    /// <summary>
    /// 无敌状态协程
    /// </summary>
    /// <returns>协程</returns>
    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        // 这里可以添加无敌状态的视觉效果，例如角色闪烁

        yield return new WaitForSeconds(invulnerabilityDuration);

        isInvulnerable = false;
    }

    /// <summary>
    /// 受伤事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnPlayerDamaged(object[] parameters)
    {
        // 可以在这里添加受伤时的额外逻辑
        Debug.Log($"Player took {parameters[0]} damage, current health: {parameters[1]}");
    }

    /// <summary>
    /// 治疗事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnPlayerHealed(object[] parameters)
    {
        // 可以在这里添加治疗时的额外逻辑
        Debug.Log($"Player healed {parameters[0]} health, current health: {parameters[1]}");
    }

    /// <summary>
    /// 重置生命值
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isInvulnerable = false;
    }

    /// <summary>
    /// 受到伤害（IDamageable接口实现）
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="weapon">造成伤害的武器</param>
    public void TakeDamage(int damage, object weapon = null)
    {
        if (isInvulnerable || IsDead)
            return;

        Debug.Log("触发受伤事件");
        // 减少生命值
        currentHealth = Mathf.Max(0, currentHealth - damage);

        // 触发受伤事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.TriggerEvent(GameEventsManager.EventTypes.PlayerDamaged, damage, currentHealth);
        }

        // 检查是否死亡
        if (IsDead)
        {
            Die();
        }
        else
        {
            // 进入无敌状态
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isInvulnerable || IsDead)
            return;
        
        EnemyBase enemy = hit.gameObject.GetComponent<EnemyBase>();
        if (enemy != null && !enemy.IsDead)
        {
            if (DamageSystem.Instance != null)
            {
                DamageSystem.Instance.DealDamage(gameObject, enemy.Damage, DamageType.Physical, false, enemy, enemy.gameObject);
            }
            else
            {
                TakeDamage(enemy.Damage, enemy);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 point1 = transform.position + capsuleBottomOffset;
        Vector3 point2 = transform.position + capsuleTopOffset;
        
        // 绘制胶囊体
        DrawWireCapsule(point1, point2, capsuleRadius);
    }
    
    private void DrawWireCapsule(Vector3 point1, Vector3 point2, float radius)
    {
        // 绘制两个端点的球体
        Gizmos.DrawWireSphere(point1, radius);
        Gizmos.DrawWireSphere(point2, radius);
        
        // 绘制连接线
        Vector3 direction = (point2 - point1).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized * radius;
        
        if (perpendicular == Vector3.zero)
        {
            perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * radius;
        }
        
        Gizmos.DrawLine(point1 + perpendicular, point2 + perpendicular);
        Gizmos.DrawLine(point1 - perpendicular, point2 - perpendicular);
        
        // 绘制其他方向的线
        Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular).normalized * radius;
        Gizmos.DrawLine(point1 + perpendicular2, point2 + perpendicular2);
        Gizmos.DrawLine(point1 - perpendicular2, point2 - perpendicular2);
    }
}
