using UnityEngine;

/// <summary>
/// 定点攻击策略（如鞭子）
/// 瞬间对玩家前方扇形区域内的敌人造成伤害
/// </summary>
[CreateAssetMenu(fileName = "NewPointBlankAttack", menuName = "WeaponStrategy/PointBlankAttack")]
public class PointBlankAttackStrategy : AttackStrategySO
{
    [Header("攻击范围")]
    [Tooltip("攻击距离")]
    [SerializeField]
    protected float attackRange = 3f;

    [Tooltip("扇形角度（度）")]
    [SerializeField]
    protected float attackAngle = 90f;

    [Header("视觉效果")]
    [Tooltip("攻击特效预制体")]
    [SerializeField]
    protected GameObject hitEffectPrefab;

    [Tooltip("特效持续时间（秒）")]
    [SerializeField]
    protected float effectDuration = 0.5f;

    /// <summary>
    /// 执行定点攻击
    /// </summary>
    public override void Attack(Transform owner, int weaponLevel, Transform target = null)
    {
        if (owner == null)
            return;

        // 执行扇形范围伤害
        DealSectorDamage(owner, weaponLevel);

        // 播放攻击特效
        SpawnHitEffect(owner);
    }

    /// <summary>
    /// 执行扇形范围伤害
    /// </summary>
    protected virtual void DealSectorDamage(Transform owner, int weaponLevel)
    {
        if (owner == null)
            return;

        // 获取伤害值
        int damage = GetBaseDamage(weaponLevel);

        // 检测攻击范围内的所有物体
        Collider[] hitColliders = Physics.OverlapSphere(owner.position, attackRange, enemyLayer);

        foreach (Collider collider in hitColliders)
        {
            // 检查是否在扇形范围内
            if (IsInSector(owner, collider.transform))
            {
                // 对敌人造成伤害
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    Debug.Log($"Point blank attack dealt {damage} damage to {collider.gameObject.name}");
                }
            }
        }
    }

    /// <summary>
    /// 检查目标是否在扇形范围内
    /// </summary>
    protected virtual bool IsInSector(Transform owner, Transform target)
    {
        if (owner == null || target == null)
            return false;

        // 计算到目标的方向
        Vector3 directionToTarget = target.position - owner.position;
        directionToTarget.y = 0f; // 忽略Y轴差异
        directionToTarget.Normalize();

        // 获取所有者的前方方向
        Vector3 ownerForward = owner.forward;
        ownerForward.y = 0f;
        ownerForward.Normalize();

        // 计算角度
        float angle = Vector3.Angle(ownerForward, directionToTarget);

        // 检查是否在扇形角度内
        return angle <= attackAngle / 2f;
    }

    /// <summary>
    /// 播放攻击特效
    /// </summary>
    protected virtual void SpawnHitEffect(Transform owner)
    {
        if (hitEffectPrefab == null || owner == null)
            return;

        // 计算特效位置（玩家前方）
        Vector3 effectPosition = Get2DPosition(owner, owner.position + owner.forward * (attackRange * 0.5f));

        // 创建特效
        GameObject effect = Instantiate(hitEffectPrefab, effectPosition, owner.rotation);

        // 自动销毁特效
        Destroy(effect, effectDuration);
    }
}
