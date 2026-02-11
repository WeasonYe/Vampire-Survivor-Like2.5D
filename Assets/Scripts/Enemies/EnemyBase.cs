using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人基类，提供敌人的基础功能
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("生命值设置")]
    [Tooltip("最大生命值")]
    [SerializeField]
    protected float maxHealth = 100f;

    [Tooltip("当前生命值")]
    protected float currentHealth;

    [Header("移动设置")]
    [Tooltip("移动速度")]
    [SerializeField]
    protected float moveSpeed = 3f;

    [Tooltip("旋转速度")]
    [SerializeField]
    protected float rotateSpeed = 180f;

    [Header("检测设置")]
    [Tooltip("检测玩家范围")]
    [SerializeField]
    protected float detectionRange = 20f;

    [Header("伤害设置")]
    [Tooltip("接触伤害")]
    [SerializeField]
    protected float damage = 10f;

    protected Transform player;
    protected new Rigidbody rigidbody;
    protected bool isDead = false;
    
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
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 只在检测范围内移动
        if (distanceToPlayer <= detectionRange)
        {
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
    }

    // 不再需要UpdateAttackCooldown方法，怪物通过碰撞检测造成伤害，不需要冷却时间

    // 不再需要Attack方法，怪物通过碰撞检测造成伤害

    /// <summary>
    /// 对玩家造成伤害
    /// </summary>
    protected virtual void DealDamageToPlayer()
    {
        if (player == null || isDead)
            return;
        
        // 调用玩家的受伤方法
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"{gameObject.name} 对玩家造成 {damage} 点伤害");
        }
    }

    /// <summary>
    /// 碰撞检测，当怪物接触玩家时造成伤害
    /// </summary>
    /// <param name="collision">碰撞信息</param>
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (isDead)
            return;
        
        // 检查是否碰撞到玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            // 对玩家造成伤害
            DealDamageToPlayer();
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public virtual void TakeDamage(float damage)
    {
        if (isDead)
            return;
        
        currentHealth -= damage;
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡
    /// </summary>
    protected virtual void Die()
    {
        isDead = true;
        
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
    public void SetHealth(float health)
    {
        maxHealth = Mathf.Max(1f, health);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
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
    public void SetDamage(float dmg)
    {
        damage = Mathf.Max(0f, dmg);
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
}
