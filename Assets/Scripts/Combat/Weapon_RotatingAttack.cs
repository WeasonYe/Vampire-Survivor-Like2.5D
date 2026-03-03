using System.Collections;
using UnityEngine;

/// <summary>
/// 旋转攻击武器基类，提供旋转攻击的通用功能
/// 支持武器绕角色中心点（世界Y轴）旋转攻击
/// 所有需要旋转攻击的武器都可以继承此类
/// </summary>
public class Weapon_RotatingAttack : WeaponBase
{
    [Header("旋转攻击设置")]
    [Tooltip("旋转角度（度）")]
    [SerializeField]
    protected float rotationAngle = 360f;

    [Tooltip("旋转速度（度/秒）")]
    [SerializeField]
    protected float rotationSpeed = 180f;

    [Tooltip("旋转轴")]
    [SerializeField]
    protected Vector3 rotationAxis = Vector3.up;

    [Tooltip("武器模型Transform")]
    [SerializeField]
    protected Transform weaponModel;

    [Tooltip("是否使用动画（优先使用动画）")]
    [SerializeField]
    protected bool useAnimation = false;

    [Tooltip("攻击动画")]
    [SerializeField]
    protected Animation attackAnimation;

    [Tooltip("动画播放速度")]
    [SerializeField]
    protected float animationSpeed = 1f;

    [Header("绕角色旋转设置")]
    [Tooltip("是否绕角色中心点旋转攻击")]
    [SerializeField]
    protected bool orbitAroundOwner = true;

    [Tooltip("绕角色旋转半径")]
    [SerializeField]
    protected float orbitRadius = 1.5f;

    [Tooltip("绕角色旋转速度（度/秒）")]
    [SerializeField]
    protected float orbitSpeed = 180f;

    [Tooltip("攻击时旋转圈数")]
    [SerializeField]
    protected int attackOrbitCount = 1;

    [Tooltip("非攻击时的固定角度偏移（相对于角色前方）")]
    [SerializeField]
    protected float idleAngleOffset = -60f;

    [Header("攻击设置")]
    [Tooltip("攻击范围")]
    [SerializeField]
    protected float attackRange = 2f;

    [Tooltip("攻击角度（扇形）")]
    [SerializeField]
    protected float attackAngle = 90f;

    [Tooltip("攻击特效预制体")]
    [SerializeField]
    protected GameObject attackEffectPrefab;

    [Tooltip("特效持续时间")]
    [SerializeField]
    protected float effectDuration = 0.5f;

    [Header("攻击范围显示")]
    [Tooltip("是否显示攻击范围")]
    [SerializeField]
    protected bool showAttackRange = true;

    [Tooltip("攻击范围显示颜色")]
    [SerializeField]
    protected Color attackRangeColor = new Color(1f, 0.5f, 0f, 0.3f);

    [Tooltip("攻击范围线宽")]
    [SerializeField]
    protected float attackRangeLineWidth = 0.05f;

    protected bool isRotating = false;
    protected bool isOrbiting = false;
    protected float currentRotation = 0f;
    protected float currentOrbitAngle = 0f;
    protected float startOrbitAngle = 0f;
    protected Vector3 orbitOffset;
    protected float attackStartTime;
    protected float attackDuration;

    /// <summary>
    /// 更新：非攻击状态下保持剑相对角色的固定位置
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // 非攻击状态下，保持剑相对角色的固定位置
        if (!isRotating && orbitAroundOwner && owner != null)
        {
            UpdateIdlePosition();
        }
    }

    /// <summary>
    /// 更新非攻击状态下的位置（保持相对角色的固定位置）
    /// </summary>
    protected virtual void UpdateIdlePosition()
    {
        // 计算剑相对角色的固定位置
        float radians = idleAngleOffset * Mathf.Deg2Rad;
        
        Vector3 offset = new Vector3(
            Mathf.Sin(radians) * orbitRadius,
            1f,
            Mathf.Cos(radians) * orbitRadius
        );

        // 设置剑的位置（跟随角色移动，不受角色旋转影响）
        transform.position = owner.position + offset;

        // 剑的朝向：保持固定朝向，不跟随角色旋转
        // 剑始终面向角色前方偏移角度的方向
        Vector3 forwardDirection = new Vector3(
            Mathf.Sin(radians),
            -90f,
            Mathf.Cos(radians)
        );
        transform.rotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
    }

    /// <summary>
    /// 执行旋转攻击
    /// </summary>
    protected override void PerformAttack()
    {
        if (strategy == null || owner == null)
            return;

        base.PerformAttack();

        // 执行旋转攻击
        PerformRotatingAttack();
    }

    /// <summary>
    /// 执行旋转攻击
    /// </summary>
    protected virtual void PerformRotatingAttack()
    {
        if (isRotating)
            return;

        isRotating = true;
        currentRotation = 0f;
        
        // 记录当前角度作为起始角度（从当前位置开始旋转）
        startOrbitAngle = idleAngleOffset;
        currentOrbitAngle = startOrbitAngle;
        
        attackStartTime = Time.time;
        
        // 计算攻击持续时间
        float totalOrbitAngle = 360f * attackOrbitCount;
        attackDuration = totalOrbitAngle / orbitSpeed;

        // 开始旋转攻击协程
        StartCoroutine(RotateAndOrbitCoroutine());
    }

    /// <summary>
    /// 旋转和绕角色旋转的协程
    /// 武器绕着角色的中心点（世界Y轴）旋转
    /// </summary>
    protected virtual IEnumerator RotateAndOrbitCoroutine()
    {
        float totalOrbitAngle = 360f * attackOrbitCount;
        float elapsedTime = 0f;

        while (elapsedTime < attackDuration)
        {
            float t = elapsedTime / attackDuration;
            
            // 计算绕角色旋转角度（从起始角度开始，平滑旋转）
            currentOrbitAngle = startOrbitAngle + t * totalOrbitAngle;
            
            // 计算武器自身旋转角度
            currentRotation = t * rotationAngle;
            
            // 更新武器位置和旋转（绕着角色中心点旋转）
            UpdateOrbitPositionDuringAttack();
            
            // 执行伤害检测（持续检测）
            if (t > 0.1f) // 延迟一点开始检测，避免重复触发
            {
                DealDamage();
            }
            
            // 生成攻击特效（持续生成）
            if (attackEffectPrefab != null && Random.value < 0.3f) // 随机生成特效
            {
                SpawnAttackEffect();
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 旋转完成，更新非攻击状态的角度
        idleAngleOffset = currentOrbitAngle % 360f;
        
        isRotating = false;
    }

    /// <summary>
    /// 更新攻击时的位置（绕角色中心点旋转）
    /// </summary>
    protected virtual void UpdateOrbitPositionDuringAttack()
    {
        if (owner == null)
            return;

        // 获取角色的中心点位置
        Vector3 ownerCenter = owner.position;
        
        // 计算绕角色中心点的位置（在XZ平面做圆周运动）
        float radians = currentOrbitAngle * Mathf.Deg2Rad;
        
        // 计算新的位置：绕着角色中心点，在XZ平面旋转
        Vector3 newPosition = ownerCenter + new Vector3(
            Mathf.Sin(radians) * orbitRadius,
            1f,
            Mathf.Cos(radians) * orbitRadius
        );

        // 设置武器位置
        transform.position = newPosition;
        
        // 武器朝向：保持固定朝向，不跟随角色旋转
        Vector3 forwardDirection = new Vector3(
            Mathf.Sin(radians),
            -90f,
            Mathf.Cos(radians)
        );
        transform.rotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
    }

    /// <summary>
    /// 执行伤害检测
    /// </summary>
    protected virtual void DealDamage()
    {
        if (owner == null || strategy == null)
            return;

        int damage = strategy.GetBaseDamage(level);

        // 检测攻击范围内的所有物体
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);

        foreach (Collider collider in hitColliders)
        {
            // 忽略自己
            if (collider.gameObject == owner.gameObject)
                continue;
            
            // 检查是否在扇形范围内
            if (IsInSector(collider.transform))
            {
                // 对敌人造成伤害
                if (DamageSystem.Instance != null)
                {
                    DamageSystem.Instance.DealDamage(collider.gameObject, damage, DamageType.Physical, false, this, owner.gameObject);
                    Debug.Log($"Rotating attack dealt {damage} damage to {collider.gameObject.name}");
                }
                else
                {
                    IDamageable damageable = collider.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damage, this);
                        Debug.Log($"Rotating attack dealt {damage} damage to {collider.gameObject.name}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查目标是否在扇形范围内
    /// </summary>
    /// <param name="target">目标Transform</param>
    /// <returns>是否在扇形范围内</returns>
    protected virtual bool IsInSector(Transform target)
    {
        if (target == null || owner == null)
            return false;

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        return angle <= attackAngle / 2f;
    }

    /// <summary>
    /// 生成攻击特效
    /// </summary>
    protected virtual void SpawnAttackEffect()
    {
        if (attackEffectPrefab == null || owner == null)
            return;

        // 计算特效生成位置（武器位置）
        Vector3 spawnPosition = transform.position + transform.forward * (attackRange / 2f);

        // 生成特效
        GameObject effect = Instantiate(attackEffectPrefab, spawnPosition, transform.rotation);

        // 设置特效持续时间
        if (effectDuration > 0f)
        {
            Destroy(effect, effectDuration);
        }
    }

    /// <summary>
    /// 绘制攻击范围
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        if (!showAttackRange || owner == null)
            return;

        // 绘制攻击范围球体
        Gizmos.color = attackRangeColor;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 绘制扇形攻击角度
        if (attackAngle < 360f)
        {
            Vector3 forward = transform.forward;
            float halfAngle = attackAngle / 2f;
            
            // 计算扇形边界
            Quaternion leftRotation = Quaternion.AngleAxis(-halfAngle, Vector3.up);
            Quaternion rightRotation = Quaternion.AngleAxis(halfAngle, Vector3.up);
            Vector3 leftDirection = leftRotation * forward;
            Vector3 rightDirection = rightRotation * forward;
            
            // 绘制扇形边界线
            Gizmos.color = attackRangeColor;
            Gizmos.DrawLine(transform.position, transform.position + leftDirection * attackRange);
            Gizmos.DrawLine(transform.position, transform.position + rightDirection * attackRange);
        }

        // 绘制绕角色旋转轨迹
        if (orbitAroundOwner && owner != null)
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(owner.position, orbitRadius);
        }
    }

    /// <summary>
    /// 设置旋转角度
    /// </summary>
    /// <param name="angle">旋转角度</param>
    public virtual void SetRotationAngle(float angle)
    {
        rotationAngle = angle;
    }

    /// <summary>
    /// 设置旋转速度
    /// </summary>
    /// <param name="speed">旋转速度</param>
    public virtual void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    /// <summary>
    /// 设置旋转轴
    /// </summary>
    /// <param name="axis">旋转轴</param>
    public virtual void SetRotationAxis(Vector3 axis)
    {
        rotationAxis = axis;
    }

    /// <summary>
    /// 设置武器模型
    /// </summary>
    /// <param name="model">武器模型Transform</param>
    public virtual void SetWeaponModel(Transform model)
    {
        weaponModel = model;
    }

    /// <summary>
    /// 设置是否绕角色旋转
    /// </summary>
    /// <param name="orbit">是否绕角色旋转</param>
    public virtual void SetOrbitAroundOwner(bool orbit)
    {
        orbitAroundOwner = orbit;
    }

    /// <summary>
    /// 设置绕角色旋转半径
    /// </summary>
    /// <param name="radius">旋转半径</param>
    public virtual void SetOrbitRadius(float radius)
    {
        orbitRadius = Mathf.Max(0f, radius);
    }

    /// <summary>
    /// 设置绕角色旋转速度
    /// </summary>
    /// <param name="speed">旋转速度</param>
    public virtual void SetOrbitSpeed(float speed)
    {
        orbitSpeed = speed;
    }

    /// <summary>
    /// 设置攻击时旋转圈数
    /// </summary>
    /// <param name="count">旋转圈数</param>
    public virtual void SetAttackOrbitCount(int count)
    {
        attackOrbitCount = Mathf.Max(1, count);
    }

    /// <summary>
    /// 设置非攻击时的固定角度偏移
    /// </summary>
    /// <param name="angle">角度偏移</param>
    public virtual void SetIdleAngleOffset(float angle)
    {
        idleAngleOffset = angle;
    }

    /// <summary>
    /// 获取攻击范围
    /// </summary>
    /// <returns>攻击范围</returns>
    public virtual float GetAttackRange()
    {
        return attackRange;
    }

    /// <summary>
    /// 设置攻击范围
    /// </summary>
    /// <param name="range">攻击范围</param>
    public virtual void SetAttackRange(float range)
    {
        attackRange = Mathf.Max(0f, range);
    }

    /// <summary>
    /// 获取攻击角度
    /// </summary>
    /// <returns>攻击角度</returns>
    public virtual float GetAttackAngle()
    {
        return attackAngle;
    }

    /// <summary>
    /// 设置攻击角度
    /// </summary>
    /// <param name="angle">攻击角度</param>
    public virtual void SetAttackAngle(float angle)
    {
        attackAngle = Mathf.Clamp(angle, 0f, 360f);
    }

    /// <summary>
    /// 升级武器
    /// </summary>
    public override void Upgrade()
    {
        base.Upgrade();

        // 旋转攻击武器升级时增加攻击范围
        attackRange *= 1.1f;

        // 增加绕角色旋转半径
        orbitRadius *= 1.05f;

        Debug.Log($"{gameObject.name} upgraded to level {level}, attack range: {attackRange}, orbit radius: {orbitRadius}");
    }
}