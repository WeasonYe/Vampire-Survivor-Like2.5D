using UnityEngine;

/// <summary>
/// 持续范围攻击策略（如圣水）
/// 在指定位置生成持续数秒的伤害区域，周期性造成伤害
/// </summary>
[CreateAssetMenu(fileName = "NewAreaAttack", menuName = "WeaponStrategy/AreaAttack")]
public class AreaAttackStrategy : AttackStrategySO
{
    [Header("区域效果设置")]
    [Tooltip("区域效果预制体")]
    [SerializeField]
    protected GameObject areaEffectPrefab;

    [Tooltip("效果半径")]
    [SerializeField]
    protected float effectRadius = 2f;

    [Tooltip("效果持续时间（秒）")]
    [SerializeField]
    protected float effectDuration = 3f;

    [Tooltip("伤害总次数")]
    [SerializeField]
    protected int damageTickCount = 3;

    [Header("生成设置")]
    [Tooltip("是否在玩家前方生成")]
    [SerializeField]
    protected bool spawnInFront = true;

    [Tooltip("前方生成距离")]
    [SerializeField]
    protected float spawnDistance = 2f;

    /// <summary>
    /// 执行区域攻击
    /// </summary>
    public override void Attack(Transform owner, int weaponLevel, Transform target = null)
    {
        if (owner == null || areaEffectPrefab == null)
            return;

        // 计算生成位置
        Vector3 spawnPosition = CalculateSpawnPosition(owner);

        // 创建区域效果
        CreateAreaEffect(spawnPosition, weaponLevel);
    }

    /// <summary>
    /// 计算生成位置
    /// </summary>
    protected virtual Vector3 CalculateSpawnPosition(Transform owner)
    {
        Vector3 position;

        if (spawnInFront)
        {
            // 在玩家前方生成
            position = owner.position + owner.forward * spawnDistance;
        }
        else
        {
            // 在玩家位置生成
            position = owner.position;
        }

        // 2.5D适配
        return Get2DPosition(owner, position);
    }

    /// <summary>
    /// 创建区域效果
    /// </summary>
    protected virtual void CreateAreaEffect(Vector3 position, int weaponLevel)
    {
        GameObject areaEffect;

        // 使用对象池创建区域效果
        if (ObjectPoolManager.Instance != null)
        {
            areaEffect = ObjectPoolManager.Instance.GetObject(areaEffectPrefab, position, Quaternion.identity);
        }
        else
        {
            // 如果对象池管理器不存在，直接实例化
            areaEffect = Instantiate(areaEffectPrefab, position, Quaternion.identity);
        }

        // // 获取区域伤害控制器
        // AreaDamageController damageController = areaEffect.GetComponent<AreaDamageController>();
        // if (damageController != null)
        // {
        //     // 初始化区域伤害控制器
        //     int damage = GetBaseDamage(weaponLevel);
        //     damageController.Initialize(effectRadius, damage, damageTickCount, effectDuration, enemyLayer);
        // }
        // else
        // {
        //     // 如果没有区域伤害控制器，自动添加
        //     damageController = areaEffect.AddComponent<AreaDamageController>();
        //     int damage = GetBaseDamage(weaponLevel);
        //     damageController.Initialize(effectRadius, damage, damageTickCount, effectDuration, enemyLayer);
        // }
    }
}
