using UnityEngine;

/// <summary>
/// 发射子弹攻击策略（如飞刀）
/// 向敌人方向发射子弹，子弹飞行并命中敌人
/// </summary>
[CreateAssetMenu(fileName = "NewProjectileAttack", menuName = "WeaponStrategy/ProjectileAttack")]
public class ProjectileAttackStrategy : AttackStrategySO
{
    [Header("子弹设置")]
    [Tooltip("子弹预制体")]
    [SerializeField]
    protected GameObject projectilePrefab;

    [Tooltip("子弹速度")]
    [SerializeField]
    protected float projectileSpeed = 10f;

    [Tooltip("子弹数量（同时发射）")]
    [SerializeField]
    protected int projectileCount = 1;

    [Tooltip("散射角度（度）")]
    [SerializeField]
    protected float spreadAngle = 30f;

    [Tooltip("子弹生命周期（秒）")]
    [SerializeField]
    protected float projectileLifetime = 3f;

    [Header("瞄准设置")]
    [Tooltip("是否自动瞄准最近敌人")]
    [SerializeField]
    protected bool autoAim = false;

    [Tooltip("瞄准范围")]
    [SerializeField]
    protected float aimRange = 10f;

    /// <summary>
    /// 执行子弹攻击
    /// </summary>
    public override void Attack(Transform owner, int weaponLevel, Transform target = null)
    {
        if (owner == null || projectilePrefab == null)
            return;

        // 确定攻击方向
        Vector3 attackDirection = DetermineAttackDirection(owner, target);

        // 发射子弹
        FireProjectiles(owner, attackDirection, weaponLevel);
    }

    /// <summary>
    /// 确定攻击方向
    /// </summary>
    protected virtual Vector3 DetermineAttackDirection(Transform owner, Transform target)
    {
        if (autoAim)
        {
            // 尝试寻找最近敌人
            Transform nearestEnemy = FindNearestEnemy(owner);
            if (nearestEnemy != null)
            {
                Vector3 direction = (nearestEnemy.position - owner.position).normalized;
                return new Vector3(direction.x, 0f, direction.z);
            }
        }

        // 默认面向前方
        return owner.forward;
    }

    /// <summary>
    /// 寻找最近敌人
    /// </summary>
    protected virtual Transform FindNearestEnemy(Transform owner)
    {
        Collider[] hitColliders = Physics.OverlapSphere(owner.position, aimRange, enemyLayer);

        Transform nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider collider in hitColliders)
        {
            float distance = Vector3.Distance(owner.position, collider.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = collider.transform;
            }
        }

        return nearestEnemy;
    }

    /// <summary>
    /// 发射子弹
    /// </summary>
    protected virtual void FireProjectiles(Transform owner, Vector3 direction, int weaponLevel)
    {
        int damage = GetBaseDamage(weaponLevel);

        // 计算散射角度范围
        float totalSpread = spreadAngle * (projectileCount - 1);
        float startAngle = -totalSpread / 2f;
        float angleStep = spreadAngle;

        for (int i = 0; i < projectileCount; i++)
        {
            // 计算当前子弹的角度
            float currentAngle = startAngle + angleStep * i;

            // 旋转方向
            Vector3 projectileDirection = Quaternion.Euler(0f, currentAngle, 0f) * direction;

            // 创建子弹
            GameObject projectile = CreateProjectile(owner, projectileDirection, damage);

            // 初始化子弹
            // Bullet bullet = projectile.GetComponent<Bullet>();
            // if (bullet != null)
            // {
            //     bullet.Initialize(damage, projectileSpeed, projectileLifetime, enemyLayer);
            // }
        }
    }

    /// <summary>
    /// 创建子弹
    /// </summary>
    protected virtual GameObject CreateProjectile(Transform owner, Vector3 direction, int damage)
    {
        // 计算发射位置（2.5D适配）
        Vector3 spawnPosition = Get2DPosition(owner, owner.position + owner.forward * 0.5f);

        GameObject projectile;

        // 使用对象池创建子弹
        if (ObjectPoolManager.Instance != null)
        {
            projectile = ObjectPoolManager.Instance.GetObject(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
        }
        else
        {
            // 如果对象池管理器不存在，直接实例化
            projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
        }

        // 设置子弹速度方向
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }

        return projectile;
    }
}
