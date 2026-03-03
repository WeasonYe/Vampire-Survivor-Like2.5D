using System.Collections;
using UnityEngine;

/// <summary>
/// 投射物基类，用于处理子弹、飞刀等投射物
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("投射物设置")]
    [Tooltip("伤害值")]
    [SerializeField]
    protected int damage = 10;

    [Tooltip("投射物速度")]
    [SerializeField]
    protected float speed = 10f;

    [Tooltip("生命周期（秒）")]
    [SerializeField]
    protected float lifetime = 3f;

    [Tooltip("是否穿透敌人")]
    [SerializeField]
    protected bool pierce = false;

    [Tooltip("穿透数量")]
    [SerializeField]
    protected int pierceCount = 1;

    [Tooltip("伤害类型")]
    [SerializeField]
    protected DamageType damageType = DamageType.Physical;

    [Tooltip("是否造成暴击")]
    [SerializeField]
    protected bool isCritical = false;

    [Tooltip("敌人层级")]
    [SerializeField]
    protected LayerMask enemyLayer;

    [Header("效果设置")]
    [Tooltip("是否启用轨迹效果")]
    [SerializeField]
    protected bool enableTrail = false;

    [Tooltip("轨迹渲染器")]
    [SerializeField]
    protected TrailRenderer trailRenderer;

    [Tooltip("是否启用粒子效果")]
    [SerializeField]
    protected bool enableParticles = false;

    [Tooltip("粒子系统")]
    [SerializeField]
    protected ParticleSystem particleSystem;

    [Tooltip("碰撞特效预制体")]
    [SerializeField]
    protected GameObject hitEffectPrefab;

    protected Rigidbody rigidbody;
    protected Collider collider;
    protected int currentPierceCount;
    protected bool isInitialized = false;
    protected GameObject projectilePrefab;

    /// <summary>
    /// 投射物预制体引用（用于对象池回收）
    /// </summary>
    public GameObject ProjectilePrefab
    {
        get => projectilePrefab;
        set => projectilePrefab = value;
    }

    protected virtual void Awake()
    {
        // 获取组件
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        // 如果没有Rigidbody，添加一个
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        // 如果没有Collider，添加一个
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
            SphereCollider sphereCollider = collider as SphereCollider;
            if (sphereCollider != null)
            {
                sphereCollider.radius = 0.2f;
            }
        }

        // 配置Rigidbody
        rigidbody.useGravity = false;
        rigidbody.isKinematic = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        // 配置Collider
        collider.isTrigger = true;
    }

    protected virtual void OnEnable()
    {
        // 重置状态
        ResetProjectile();
    }

    protected virtual void OnDisable()
    {
        // 停止协程
        StopAllCoroutines();

        // 禁用轨迹
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        // 停止粒子
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }

    /// <summary>
    /// 初始化投射物
    /// </summary>
    /// <param name="damageValue">伤害值</param>
    /// <param name="speedValue">速度</param>
    /// <param name="lifetimeValue">生命周期</param>
    /// <param name="damageTypeEnum">伤害类型</param>
    /// <param name="critical">是否暴击</param>
    /// <param name="enemyLayerMask">敌人层级</param>
    /// <param name="prefab">投射物预制体</param>
    public virtual void Initialize(int damageValue, float speedValue, float lifetimeValue, DamageType damageTypeEnum = DamageType.Physical, bool critical = false, LayerMask enemyLayerMask = default, GameObject prefab = null)
    {
        damage = damageValue;
        speed = speedValue;
        lifetime = lifetimeValue;
        damageType = damageTypeEnum;
        isCritical = critical;
        enemyLayer = enemyLayerMask;
        projectilePrefab = prefab;

        isInitialized = true;

        // 启用轨迹
        if (enableTrail && trailRenderer != null)
        {
            trailRenderer.enabled = true;
            trailRenderer.Clear();
        }

        // 启用粒子
        if (enableParticles && particleSystem != null)
        {
            particleSystem.Play();
        }

        // 设置速度
        if (rigidbody != null)
        {
            rigidbody.velocity = transform.forward * speed;
        }

        // 开始生命周期
        StartCoroutine(LifetimeCoroutine());
    }

    /// <summary>
    /// 重置投射物状态
    /// </summary>
    protected virtual void ResetProjectile()
    {
        currentPierceCount = 0;
        isInitialized = false;

        // 启用碰撞体
        if (collider != null)
        {
            collider.enabled = true;
        }

        // 启用Rigidbody
        if (rigidbody != null)
        {
            rigidbody.isKinematic = false;
        }
    }

    /// <summary>
    /// 生命周期协程
    /// </summary>
    protected virtual IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(lifetime);

        // 生命周期结束，回收投射物
        ReturnToPool();
    }

    /// <summary>
    /// 碰撞检测
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!isInitialized)
            return;

        // 检查是否命中敌人
        if (IsEnemy(other))
        {
            // 对敌人造成伤害
            DealDamage(other.gameObject);

            // 创建碰撞特效
            SpawnHitEffect(other.transform.position);

            // 检查是否穿透
            if (pierce && currentPierceCount < pierceCount)
            {
                currentPierceCount++;
                return;
            }

            // 不穿透，回收投射物
            ReturnToPool();
        }
        else if (!IsProjectile(other))
        {
            // 碰撞到其他物体，回收投射物
            SpawnHitEffect(transform.position);
            ReturnToPool();
        }
    }

    /// <summary>
    /// 检查是否是敌人
    /// </summary>
    protected virtual bool IsEnemy(Collider collider)
    {
        // 检查层级
        if (enemyLayer != 0 && ((1 << collider.gameObject.layer) & enemyLayer) != 0)
        {
            return true;
        }

        // 检查是否有IDamageable组件
        IDamageable damageable = collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查是否是投射物
    /// </summary>
    protected virtual bool IsProjectile(Collider collider)
    {
        return collider.GetComponent<Projectile>() != null;
    }

    /// <summary>
    /// 对目标造成伤害
    /// </summary>
    protected virtual void DealDamage(GameObject target)
    {
        if (DamageSystem.Instance != null)
        {
            DamageSystem.Instance.DealDamage(target, damage, damageType, isCritical);
        }
        else
        {
            // 如果DamageSystem不存在，直接调用TakeDamage
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    /// <summary>
    /// 生成碰撞特效
    /// </summary>
    protected virtual void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab == null)
            return;

        GameObject hitEffect = Instantiate(hitEffectPrefab, position, Quaternion.identity);

        // 自动销毁特效
        ParticleSystem particles = hitEffect.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            Destroy(hitEffect, particles.main.duration);
        }
        else
        {
            Destroy(hitEffect, 1f);
        }
    }

    /// <summary>
    /// 回收投射物到对象池
    /// </summary>
    protected virtual void ReturnToPool()
    {
        // 禁用碰撞体
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 禁用Rigidbody
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
        }

        // 使用对象池返回
        if (ObjectPoolManager.Instance != null && projectilePrefab != null)
        {
            ObjectPoolManager.Instance.ReturnObject(projectilePrefab, gameObject);
        }
        else
        {
            // 如果对象池管理器不存在，销毁对象
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 设置投射物速度
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rigidbody != null)
        {
            rigidbody.velocity = transform.forward * speed;
        }
    }

    /// <summary>
    /// 设置投射物方向
    /// </summary>
    public void SetDirection(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction);
        if (rigidbody != null)
        {
            rigidbody.velocity = direction * speed;
        }
    }

    /// <summary>
    /// 设置伤害值
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    /// <summary>
    /// 设置是否暴击
    /// </summary>
    public void SetCritical(bool critical)
    {
        isCritical = critical;
    }

    /// <summary>
    /// 设置穿透
    /// </summary>
    public void SetPierce(bool enable, int count = 1)
    {
        pierce = enable;
        pierceCount = count;
    }
}
