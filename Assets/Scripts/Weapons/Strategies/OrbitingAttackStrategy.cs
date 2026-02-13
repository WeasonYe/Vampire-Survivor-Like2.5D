using System.Collections;
using UnityEngine;

/// <summary>
/// 环绕范围攻击策略（如斧头）
/// 武器模型围绕玩家旋转，每隔一定时间对范围内敌人造成伤害
/// </summary>
[CreateAssetMenu(fileName = "NewOrbitingAttack", menuName = "WeaponStrategy/OrbitingAttack")]
public class OrbitingAttackStrategy : AttackStrategySO
{
    [Header("环绕设置")]
    [Tooltip("环绕半径")]
    [SerializeField]
    protected float orbitRadius = 2f;

    [Tooltip("旋转速度（度/秒）")]
    [SerializeField]
    protected float orbitSpeed = 180f;

    [Tooltip("攻击范围")]
    [SerializeField]
    protected float attackRange = 1.5f;

    [Header("视觉效果")]
    [Tooltip("武器模型预制体")]
    [SerializeField]
    protected GameObject weaponVisualPrefab;

    protected GameObject weaponVisual;
    protected float currentAngle = 0f;

    /// <summary>
    /// 执行环绕攻击
    /// </summary>
    public override void Attack(Transform owner, int weaponLevel, Transform target = null)
    {
        // 确保武器视觉对象存在
        EnsureWeaponVisual(owner);

        // 执行范围伤害检测
        DealAreaDamage(owner);
    }

    /// <summary>
    /// 确保武器视觉对象存在
    /// </summary>
    protected virtual void EnsureWeaponVisual(Transform owner)
    {
        if (weaponVisualPrefab == null)
            return;

        if (weaponVisual == null)
        {
            // 创建武器视觉对象
            weaponVisual = Instantiate(weaponVisualPrefab, owner);
            weaponVisual.transform.localPosition = Vector3.zero;
        }

        // 更新武器位置
        UpdateWeaponPosition(owner);
    }

    /// <summary>
    /// 更新武器位置（环绕旋转）
    /// </summary>
    protected virtual void UpdateWeaponPosition(Transform owner)
    {
        if (weaponVisual == null || owner == null)
            return;

        // 更新旋转角度
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle >= 360f)
            currentAngle -= 360f;

        // 计算新位置
        float radians = currentAngle * Mathf.Deg2Rad;
        float x = Mathf.Cos(radians) * orbitRadius;
        float z = Mathf.Sin(radians) * orbitRadius;

        // 设置2.5D适配后的位置
        Vector3 position = Get2DPosition(owner, owner.position + new Vector3(x, 0f, z));
        weaponVisual.transform.position = position;

        // 面向旋转方向
        weaponVisual.transform.LookAt(position + new Vector3(x, 0f, z));
    }

    /// <summary>
    /// 执行范围伤害
    /// </summary>
    protected virtual void DealAreaDamage(Transform owner)
    {
        if (owner == null)
            return;

        // 计算武器当前位置
        Vector3 weaponPosition = weaponVisual != null ? weaponVisual.transform.position : owner.position;

        // 检测攻击范围内的敌人
        Collider[] hitColliders = Physics.OverlapSphere(weaponPosition, attackRange, enemyLayer);

        int damage = GetBaseDamage(owner.GetComponent<WeaponBase>()?.Level ?? 1);

        foreach (Collider collider in hitColliders)
        {
            // 检查是否是敌人
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"Orbiting attack dealt {damage} damage to {collider.gameObject.name}");
            }
        }
    }

    /// <summary>
    /// 清理武器视觉对象
    /// </summary>
    public virtual void Cleanup()
    {
        if (weaponVisual != null)
        {
            Destroy(weaponVisual);
            weaponVisual = null;
        }
    }
}
