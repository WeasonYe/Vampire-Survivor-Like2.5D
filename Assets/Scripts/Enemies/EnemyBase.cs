using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人基类，提供敌人的基础功能
/// </summary>
public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("生命值设置")]
    [Tooltip("最大生命值")]
    [SerializeField]
    protected int maxHealth = 100;

    [Tooltip("当前生命值")]
    protected int currentHealth;

    [Header("移动设置")]
    [Tooltip("移动速度")]
    [SerializeField]
    protected float moveSpeed = 3f;

    [Tooltip("旋转速度")]
    [SerializeField]
    protected float rotateSpeed = 180f;

    [Header("伤害设置")]
    [Tooltip("接触伤害")]
    [SerializeField]
    protected int damage = 10;

    [Header("血条设置")]
    [Tooltip("是否显示血条")]
    [SerializeField]
    protected bool showHealthBar = true;

    [Tooltip("血条预制体")]
    [SerializeField]
    protected GameObject healthBarPrefab;

    protected Transform player;
    protected new Rigidbody rigidbody;
    protected bool isDead = false;
    protected EnemyHealthBar healthBar;
    
    // 武器伤害冷却字典，存储每个武器的最后攻击时间
    protected Dictionary<object, float> weaponDamageCooldowns = new Dictionary<object, float>();

    /// <summary>
    /// 敌人预制体引用
    /// </summary>
    public GameObject EnemyPrefab { get; set; }

    /// <summary>
    /// 当前生命值
    /// </summary>
    public float CurrentHealth => currentHealth;

    /// <summary>
    /// 最大生命值
    /// </summary>
    public float MaxHealth => maxHealth;

    /// <summary>
    /// 是否死亡
    /// </summary>
    public bool IsDead => isDead;

    /// <summary>
    /// 接触伤害
    /// </summary>
    public int Damage => damage;

    protected virtual void Awake()
    {
        // 获取Rigidbody组件
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        // 设置Rigidbody约束
        rigidbody.freezeRotation = true;
        rigidbody.useGravity = true;
    }

    protected virtual void OnEnable()
    {
        // 重置状态
        ResetState();

        // 创建血条
        if (showHealthBar)
        {
            CreateHealthBar();
        }
    }

    protected virtual void OnDisable()
    {
        // 销毁血条
        if (healthBar != null)
        {
            if (ObjectPoolManager.Instance != null && healthBarPrefab != null)
            {
                ObjectPoolManager.Instance.ReturnObject(healthBarPrefab, healthBar.gameObject);
            }
            else
            {
                Destroy(healthBar.gameObject);
            }
            healthBar = null;
        }
    }

    protected virtual void Start()
    {
        // 初始化生命值
        currentHealth = maxHealth;

        // 查找玩家
        FindPlayer();
    }

    /// <summary>
    /// 重置敌人状态
    /// </summary>
    protected virtual void ResetState()
    {
        // 重置生命值
        currentHealth = maxHealth;

        // 重置死亡状态
        isDead = false;

        // 启用碰撞体
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        // 启用Rigidbody
        if (rigidbody != null)
        {
            rigidbody.isKinematic = false;
        }

        // 查找玩家
        FindPlayer();
    }

    protected virtual void Update()
    {
        if (isDead)
            return;

        // 处理AI行为
        HandleAI();
    }

    protected virtual void FixedUpdate()
    {
        if (isDead)
            return;

        // 处理移动
        HandleMovement();
    }

    /// <summary>
    /// 查找玩家
    /// </summary>
    protected virtual void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    /// <summary>
    /// 处理AI行为
    /// </summary>
    protected virtual void HandleAI()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // 怪物只需要靠近玩家，接触时会通过碰撞检测扣血
        // 不再需要攻击行为
    }

    /// <summary>
    /// 处理移动
    /// </summary>
    protected virtual void HandleMovement()
    {
        if (player == null)
            return;
            
        // 计算朝向玩家的方向
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // 保持在水平面上移动

        // 旋转朝向玩家
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
        }

        // 移动（一直向玩家移动，不考虑攻击范围）
        Vector3 movement = direction * moveSpeed * Time.fixedDeltaTime;
        rigidbody.MovePosition(rigidbody.position + movement);
    }

    /// <summary>
    /// 创建血条
    /// </summary>
    protected virtual void CreateHealthBar()
    {
        if (healthBarPrefab == null)
            return;

        // 使用对象池创建血条
        if (ObjectPoolManager.Instance != null)
        {
            GameObject healthBarObject = ObjectPoolManager.Instance.GetObject(healthBarPrefab, transform.position, Quaternion.identity);
            healthBar = healthBarObject.GetComponent<EnemyHealthBar>();
        }
        else
        {
            GameObject healthBarObject = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBar = healthBarObject.GetComponent<EnemyHealthBar>();
        }

        // 初始化血条
        if (healthBar != null)
        {
            healthBar.Initialize(transform);
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// 更新血条
    /// </summary>
    protected virtual void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// 受到伤害（IDamageable接口实现）
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="weapon">造成伤害的武器</param>
    public virtual void TakeDamage(int damage, object weapon = null)
    {
        if (isDead)
            return;

        // 检查武器伤害冷却
        if (weapon != null && IsWeaponInCooldown(weapon))
        {
            return; // 武器在冷却时间内，不处理伤害
        }

        currentHealth -= damage;

        // 更新血条
        UpdateHealthBar();

        // 触发受伤事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.TriggerEvent(GameEventsManager.EventTypes.EnemyDamaged, damage, currentHealth);
        }

        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
        else if (weapon != null)
        {
            // 更新武器的最后攻击时间
            UpdateWeaponCooldown(weapon);
        }
    }

    /// <summary>
    /// 检查武器是否在冷却时间内
    /// </summary>
    /// <param name="weapon">武器对象</param>
    /// <returns>是否在冷却时间内</returns>
    protected virtual bool IsWeaponInCooldown(object weapon)
    {
        if (weapon == null)
            return false;

        if (weaponDamageCooldowns.TryGetValue(weapon, out float lastAttackTime))
        {
            // 计算冷却时间（这里使用固定值，实际应该从武器获取攻击间隔）
            float cooldownTime = 0.5f; // 默认冷却时间
            
            // 如果武器是WeaponBase类型，使用其攻击间隔作为冷却时间
            if (weapon is WeaponBase weaponBase)
            {
                cooldownTime = weaponBase.GetAttackInterval();
            }
            
            return Time.time - lastAttackTime < cooldownTime;
        }
        
        return false;
    }

    /// <summary>
    /// 更新武器的冷却时间
    /// </summary>
    /// <param name="weapon">武器对象</param>
    protected virtual void UpdateWeaponCooldown(object weapon)
    {
        if (weapon == null)
            return;

        weaponDamageCooldowns[weapon] = Time.time;
    }

    /// <summary>
    /// 死亡
    /// </summary>
    protected virtual void Die()
    {
        isDead = true;

        // 隐藏血条
        if (healthBar != null)
        {
            healthBar.HideHealthBar();
        }

        // 禁用碰撞体
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // 禁用Rigidbody
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
        }

        // 延迟回收敌人到对象池
        StartCoroutine(ReturnToPool());
    }

    /// <summary>
    /// 将敌人回收回对象池
    /// </summary>
    /// <returns>协程</returns>
    protected virtual IEnumerator ReturnToPool()
    {
        // 等待2秒后回收
        yield return new WaitForSeconds(2f);

        // 检查对象池管理器是否存在
        if (ObjectPoolManager.Instance != null && EnemyPrefab != null)
        {
            // 回收敌人到对象池
            ObjectPoolManager.Instance.ReturnObject(EnemyPrefab, gameObject);
        }
        else
        {
            // 如果对象池管理器不存在，销毁敌人
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 设置生命值
    /// </summary>
    /// <param name="health">新的生命值</param>
    public void SetHealth(int health)
    {
        maxHealth = Mathf.Max(1, health);
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // 更新血条
        UpdateHealthBar();
    }

    /// <summary>
    /// 设置移动速度
    /// </summary>
    /// <param name="speed">新的移动速度</param>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    /// <summary>
    /// 设置伤害值
    /// </summary>
    /// <param name="dmg">新的伤害值</param>
    public void SetDamage(int dmg)
    {
        damage = Mathf.Max(0, dmg);
    }

    /// <summary>
    /// 获取与玩家的距离
    /// </summary>
    /// <returns>距离值</returns>
    public float GetDistanceToPlayer()
    {
        if (player == null)
            return float.MaxValue;

        return Vector3.Distance(transform.position, player.position);
    }

    /// <summary>
    /// 设置是否显示血条
    /// </summary>
    /// <param name="show">是否显示</param>
    public void SetShowHealthBar(bool show)
    {
        showHealthBar = show;

        if (show && healthBar == null && healthBarPrefab != null)
        {
            CreateHealthBar();
        }
        else if (!show && healthBar != null)
        {
            healthBar.HideHealthBar();
        }
    }

    /// <summary>
    /// 获取血条组件
    /// </summary>
    /// <returns>血条组件</returns>
    public EnemyHealthBar GetHealthBar()
    {
        return healthBar;
    }
}