using System.Collections;
using UnityEngine;

/// <summary>
/// 剑类武器，继承自Weapon_RotatingAttack
/// 剑类武器使用旋转攻击，具有特定的攻击范围和攻击速度
/// </summary>
public class Weapon_Sword : Weapon_RotatingAttack
{
    [Header("剑类特有设置")]
    [Tooltip("是否连续攻击")]
    [SerializeField]
    protected bool canComboAttack = false;

    [Tooltip("连击时间窗口")]
    [SerializeField]
    protected float comboTimeWindow = 0.5f;

    [Tooltip("连击次数上限")]
    [SerializeField]
    protected int maxComboCount = 3;

    protected float comboTimer = 0f;
    protected int comboCount = 0;

    /// <summary>
    /// 执行剑类攻击
    /// </summary>
    protected override void PerformRotatingAttack()
    {
        // 处理连击
        if (canComboAttack && comboTimer > 0f)
        {
            comboCount++;
            comboTimer = comboTimeWindow;

            // 限制连击次数
            if (comboCount > maxComboCount)
            {
                comboCount = 1;
            }

            Debug.Log($"Combo attack! Combo count: {comboCount}");
        }
        else
        {
            comboCount = 0;
        }

        // 调用基类的旋转攻击
        base.PerformRotatingAttack();
    }

    /// <summary>
    /// 更新连击计时器
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // 更新连击计时器
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
        }
        else
        {
            comboCount = 0;
        }
    }

    /// <summary>
    /// 执行伤害检测（剑类特有的伤害计算）
    /// </summary>
    protected override void DealDamage()
    {
        // 剑类武器可以根据连击次数增加伤害
        if (canComboAttack && comboCount > 0)
        {
            // 临时增加攻击范围
            float originalRange = attackRange;
            attackRange *= (1f + comboCount * 0.05f);

            base.DealDamage();

            // 恢复攻击范围
            attackRange = originalRange;
        }
        else
        {
            base.DealDamage();
        }
    }

    /// <summary>
    /// 设置是否启用连击
    /// </summary>
    /// <param name="enable">是否启用</param>
    public virtual void SetComboAttack(bool enable)
    {
        canComboAttack = enable;
    }

    /// <summary>
    /// 设置连击时间窗口
    /// </summary>
    /// <param name="timeWindow">时间窗口</param>
    public virtual void SetComboTimeWindow(float timeWindow)
    {
        comboTimeWindow = Mathf.Max(0f, timeWindow);
    }

    /// <summary>
    /// 设置最大连击次数
    /// </summary>
    /// <param name="maxCount">最大连击次数</param>
    public virtual void SetMaxComboCount(int maxCount)
    {
        maxComboCount = Mathf.Max(1, maxCount);
    }

    /// <summary>
    /// 获取当前连击次数
    /// </summary>
    /// <returns>连击次数</returns>
    public virtual int GetComboCount()
    {
        return comboCount;
    }

    /// <summary>
    /// 升级武器
    /// </summary>
    public override void Upgrade()
    {
        base.Upgrade();

        // 剑类武器升级时增加连击次数上限
        if (canComboAttack)
        {
            maxComboCount = Mathf.Min(5, maxComboCount + 1);
            Debug.Log($"{gameObject.name} upgraded to level {level}, max combo count: {maxComboCount}");
        }
    }
}