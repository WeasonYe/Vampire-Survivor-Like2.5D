using UnityEngine;

/// <summary>
/// 攻击策略ScriptableObject基类，所有具体的攻击策略都继承此类
/// </summary>
[CreateAssetMenu(fileName = "NewAttackStrategy", menuName = "WeaponStrategy/AttackStrategy")]
public abstract class AttackStrategySO : ScriptableObject, IAttackStrategy
{
    [Header("基础参数")]
    [Tooltip("基础伤害")]
    [SerializeField]
    protected int baseDamage = 10;

    [Tooltip("基础攻击间隔（秒）")]
    [SerializeField]
    protected float baseAttackRate = 1f;

    [Header("成长曲线")]
    [Tooltip("伤害成长曲线（X轴：等级，Y轴：伤害倍数）")]
    [SerializeField]
    protected AnimationCurve damageCurve = AnimationCurve.Linear(0, 1, 7, 3);

    [Tooltip("攻击速度成长曲线（X轴：等级，Y轴：攻速倍数）")]
    [SerializeField]
    protected AnimationCurve rateCurve = AnimationCurve.Linear(0, 1, 7, 1.5f);

    [Header("2.5D适配")]
    [Tooltip("武器模型Y轴偏移（相对于玩家）")]
    [SerializeField]
    protected float yOffset = 0.5f;

    [Tooltip("敌人层级")]
    [SerializeField]
    protected LayerMask enemyLayer = 1 << 6; // 默认为第6层

    /// <summary>
    /// 获取当前等级下的基础伤害
    /// </summary>
    /// <param name="weaponLevel">武器等级</param>
    /// <returns>基础伤害值</returns>
    public virtual int GetBaseDamage(int weaponLevel)
    {
        float multiplier = damageCurve.Evaluate(weaponLevel);
        return Mathf.RoundToInt(baseDamage * multiplier);
    }

    /// <summary>
    /// 获取当前等级下的攻击间隔（秒/次）
    /// </summary>
    /// <param name="weaponLevel">武器等级</param>
    /// <returns>攻击间隔（秒）</returns>
    public virtual float GetAttackRate(int weaponLevel)
    {
        float multiplier = rateCurve.Evaluate(weaponLevel);
        return baseAttackRate / multiplier;
    }

    /// <summary>
    /// 执行攻击（抽象方法，由子类实现）
    /// </summary>
    /// <param name="owner">攻击发起者</param>
    /// <param name="weaponLevel">武器等级</param>
    /// <param name="target">目标（可选）</param>
    public abstract void Attack(Transform owner, int weaponLevel, Transform target = null);

    /// <summary>
    /// 获取2.5D适配后的位置（固定Y轴）
    /// </summary>
    /// <param name="owner">所有者Transform</param>
    /// <param name="position">目标位置</param>
    /// <returns>适配后的位置</returns>
    protected Vector3 Get2DPosition(Transform owner, Vector3 position)
    {
        return new Vector3(position.x, owner.position.y + yOffset, position.z);
    }

    /// <summary>
    /// 检查目标是否在敌人层级
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <returns>是否是敌人</returns>
    protected bool IsEnemy(GameObject target)
    {
        return (enemyLayer.value & (1 << target.layer)) != 0;
    }
}
