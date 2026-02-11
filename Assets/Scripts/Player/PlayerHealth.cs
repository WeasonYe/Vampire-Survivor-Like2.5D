using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家生命值管理脚本
/// </summary>
public class PlayerHealth : Singleton<PlayerHealth>
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
    private float invulnerabilityDuration = 2f;

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
    }

    private void Start()
    {
        // 初始化生命值
        currentHealth = maxHealth;

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

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        if (isInvulnerable || IsDead)
            return;

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

    /// <summary>
    /// 治疗
    /// </summary>
    /// <param name="healAmount">治疗量</param>
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
}
