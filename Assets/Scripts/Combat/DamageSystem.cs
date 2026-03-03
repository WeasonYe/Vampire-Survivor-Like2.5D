using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 伤害类型枚举
/// </summary>
public enum DamageType
{
    Physical,
    Magic,
    TrueDamage
}

/// <summary>
/// 伤害系统，负责处理游戏中的伤害计算和应用
/// </summary>
public class DamageSystem : Singleton<DamageSystem>
{
    [Header("伤害设置")]
    [Tooltip("暴击倍率")]
    [SerializeField]
    private int criticalMultiplier = 2;

    [Tooltip("伤害浮动范围（0-1）")]
    [SerializeField]
    private float damageVariance = 0.1f;

    [Tooltip("是否启用伤害数字显示")]
    [SerializeField]
    private bool showDamageNumbers = true;

    [Tooltip("伤害数字预制体")]
    [SerializeField]
    private GameObject damageNumberPrefab;

    [Tooltip("伤害数字显示持续时间")]
    [SerializeField]
    private float damageNumberDuration = 1f;

    [Tooltip("伤害数字上升速度")]
    [SerializeField]
    private float damageNumberRiseSpeed = 1f;

    /// <summary>
    /// 暴击倍率
    /// </summary>
    public float CriticalMultiplier => criticalMultiplier;

    /// <summary>
    /// 伤害浮动范围
    /// </summary>
    public float DamageVariance => damageVariance;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // 注册事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.EnemyDamaged, OnEnemyDamaged);
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.PlayerDamaged, OnPlayerDamaged);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 注销事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.EnemyDamaged, OnEnemyDamaged);
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.PlayerDamaged, OnPlayerDamaged);
        }
    }

    /// <summary>
    /// 对目标造成伤害
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="baseDamage">基础伤害</param>
    /// <param name="damageType">伤害类型</param>
    /// <param name="isCritical">是否暴击</param>
    /// <param name="weapon">造成伤害的武器</param>
    /// <param name="attacker">攻击者</param>
    /// <returns>实际造成的伤害</returns>
    public int DealDamage(GameObject target, int baseDamage, DamageType damageType = DamageType.Physical, bool isCritical = false, object weapon = null, GameObject attacker = null)
    {
        if (target == null)
            return 0;

        // 计算浮动伤害
        float variance = Random.Range(-damageVariance, damageVariance);
        float calculatedDamage = baseDamage * (1f + variance);

        // 应用暴击倍率
        if (isCritical)
        {
            calculatedDamage *= criticalMultiplier;
        }

        // 转换为整数
        int finalDamage = Mathf.RoundToInt(calculatedDamage);

        // 应用伤害
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(finalDamage, weapon);
        }

        // 显示伤害数字
        if (showDamageNumbers)
        {
            ShowDamageNumber(target.transform.position, finalDamage, isCritical);
        }

        return finalDamage;
    }

    /// <summary>
    /// 对目标造成伤害（使用Transform）
    /// </summary>
    /// <param name="target">目标Transform</param>
    /// <param name="baseDamage">基础伤害</param>
    /// <param name="damageType">伤害类型</param>
    /// <param name="isCritical">是否暴击</param>
    /// <param name="weapon">造成伤害的武器</param>
    /// <param name="attacker">攻击者</param>
    /// <returns>实际造成的伤害</returns>
    public int DealDamage(Transform target, int baseDamage, DamageType damageType = DamageType.Physical, bool isCritical = false, object weapon = null, GameObject attacker = null)
    {
        if (target == null)
            return 0;

        return DealDamage(target.gameObject, baseDamage, damageType, isCritical, weapon, attacker);
    }

    /// <summary>
    /// 对多个目标造成范围伤害
    /// </summary>
    /// <param name="position">中心位置</param>
    /// <param name="radius">伤害半径</param>
    /// <param name="baseDamage">基础伤害</param>
    /// <param name="damageType">伤害类型</param>
    /// <param name="layerMask">层级遮罩</param>
    /// <param name="weapon">造成伤害的武器</param>
    /// <param name="attacker">攻击者</param>
    /// <returns>总伤害</returns>
    public int DealAreaDamage(Vector3 position, float radius, int baseDamage, DamageType damageType = DamageType.Physical, LayerMask layerMask = default, object weapon = null, GameObject attacker = null)
    {
        int totalDamage = 0;

        // 检测范围内的所有碰撞体
        Collider[] hitColliders = Physics.OverlapSphere(position, radius, layerMask);

        foreach (Collider collider in hitColliders)
        {
            // 跳过攻击者
            if (attacker != null && collider.gameObject == attacker)
                continue;

            // 对每个目标造成伤害
            int damage = DealDamage(collider.gameObject, baseDamage, damageType, false, weapon, attacker);
            totalDamage += damage;
        }

        return totalDamage;
    }

    /// <summary>
    /// 计算暴击
    /// </summary>
    /// <param name="criticalChance">暴击几率（0-1）</param>
    /// <returns>是否暴击</returns>
    public bool IsCritical(float criticalChance)
    {
        return Random.value < criticalChance;
    }

    /// <summary>
    /// 显示伤害数字
    /// </summary>
    /// <param name="position">显示位置</param>
    /// <param name="damage">伤害值</param>
    /// <param name="isCritical">是否暴击</param>
    private void ShowDamageNumber(Vector3 position, int damage, bool isCritical)
    {
        if (damageNumberPrefab == null)
            return;

        // 创建伤害数字对象
        GameObject damageNumberObject = Instantiate(damageNumberPrefab, position, Quaternion.identity);
        
        // 获取或添加伤害数字组件
        DamageNumber damageNumber = damageNumberObject.GetComponent<DamageNumber>();
        if (damageNumber == null)
        {
            damageNumber = damageNumberObject.AddComponent<DamageNumber>();
        }

        // 初始化伤害数字
        damageNumber.Initialize(damage, isCritical, damageNumberDuration, damageNumberRiseSpeed);
    }

    /// <summary>
    /// 敌人受伤事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnEnemyDamaged(object[] parameters)
    {
        // 可以在这里添加敌人受伤时的额外逻辑
        // 例如：播放受伤音效、触发受伤动画等
    }

    /// <summary>
    /// 玩家受伤事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnPlayerDamaged(object[] parameters)
    {
        // 可以在这里添加玩家受伤时的额外逻辑
        // 例如：屏幕震动、受伤特效等
    }

    /// <summary>
    /// 设置暴击倍率
    /// </summary>
    /// <param name="multiplier">新的暴击倍率</param>
    public void SetCriticalMultiplier(int multiplier)
    {
        criticalMultiplier = Mathf.Max(1, multiplier);
    }

    /// <summary>
    /// 设置伤害浮动范围
    /// </summary>
    /// <param name="variance">新的伤害浮动范围（0-1）</param>
    public void SetDamageVariance(float variance)
    {
        damageVariance = Mathf.Clamp01(variance);
    }

    /// <summary>
    /// 设置是否显示伤害数字
    /// </summary>
    /// <param name="show">是否显示</param>
    public void SetShowDamageNumbers(bool show)
    {
        showDamageNumbers = show;
    }
}
