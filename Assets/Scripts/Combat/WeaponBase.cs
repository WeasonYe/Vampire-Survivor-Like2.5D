using UnityEngine;

/// <summary>
/// 武器基类，挂载在武器预制体上，负责攻击计时和策略调用
/// </summary>
public class WeaponBase : MonoBehaviour
{
    [Header("武器设置")]
    [Tooltip("攻击策略")]
    [SerializeField]
    protected AttackStrategySO strategy;

    [Tooltip("武器等级")]
    [SerializeField]
    protected int level = 1;

    [Tooltip("是否自动攻击")]
    [SerializeField]
    protected bool autoAttack = true;

    [Tooltip("攻击间隔覆盖（0表示使用策略默认值）")]
    [SerializeField]
    protected float attackIntervalOverride = 0f;

    protected float attackTimer = 0f;
    protected Transform owner;

    /// <summary>
    /// 当前武器等级
    /// </summary>
    public int Level => level;

    /// <summary>
    /// 当前攻击策略
    /// </summary>
    public AttackStrategySO Strategy => strategy;

    protected virtual void Awake()
    {
        // 查找所有者（玩家）
        FindOwner();
    }

    protected virtual void Start()
    {
        // 初始化攻击计时器
        attackTimer = GetAttackInterval();
    }

    protected virtual void Update()
    {
        if (strategy == null || owner == null)
            return;

        // 更新攻击计时器
        attackTimer -= Time.deltaTime;

        // 检查是否可以攻击
        if (attackTimer <= 0f && autoAttack)
        {
            PerformAttack();
            attackTimer = GetAttackInterval();
        }
    }

    /// <summary>
    /// 查找所有者
    /// </summary>
    protected virtual void FindOwner()
    {
        // 优先查找父对象
        if (transform.parent != null)
        {
            owner = transform.parent;
            return;
        }

        // 如果没有父对象，查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            owner = player.transform;
        }
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    protected virtual void PerformAttack()
    {
        if (strategy == null || owner == null)
            return;

        // 调用策略的攻击方法
        strategy.Attack(owner, level);
    }

    /// <summary>
    /// 获取攻击间隔
    /// </summary>
    /// <returns>攻击间隔（秒）</returns>
    protected virtual float GetAttackInterval()
    {
        if (attackIntervalOverride > 0f)
        {
            return attackIntervalOverride;
        }
        return strategy != null ? strategy.GetAttackRate(level) : 1f;
    }

    /// <summary>
    /// 升级武器
    /// </summary>
    public virtual void Upgrade()
    {
        level++;
        Debug.Log($"{gameObject.name} upgraded to level {level}");
    }

    /// <summary>
    /// 设置武器等级
    /// </summary>
    /// <param name="newLevel">新等级</param>
    public virtual void SetLevel(int newLevel)
    {
        level = Mathf.Max(1, newLevel);
    }

    /// <summary>
    /// 设置攻击策略
    /// </summary>
    /// <param name="newStrategy">新策略</param>
    public virtual void SetStrategy(AttackStrategySO newStrategy)
    {
        strategy = newStrategy;
        Debug.Log($"{gameObject.name} strategy changed to {newStrategy?.name}");
    }

    /// <summary>
    /// 设置攻击间隔覆盖
    /// </summary>
    /// <param name="interval">攻击间隔</param>
    public virtual void SetAttackInterval(float interval)
    {
        attackIntervalOverride = interval;
    }

    /// <summary>
    /// 设置自动攻击
    /// </summary>
    /// <param name="enable">是否启用</param>
    public virtual void SetAutoAttack(bool enable)
    {
        autoAttack = enable;
    }

    /// <summary>
    /// 立即执行一次攻击（忽略冷却时间）
    /// </summary>
    public virtual void ForceAttack()
    {
        PerformAttack();
        attackTimer = GetAttackInterval();
    }
}
