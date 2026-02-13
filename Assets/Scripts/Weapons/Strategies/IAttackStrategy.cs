using UnityEngine;

/// <summary>
/// 攻击策略接口，所有攻击策略都必须实现此接口
/// </summary>
public interface IAttackStrategy
{
    /// <summary>
    /// 执行攻击
    /// </summary>
    /// <param name="owner">攻击发起者（通常是玩家）的Transform</param>
    /// <param name="weaponLevel">武器等级（影响伤害、数量等）</param>
    /// <param name="target">可选的目标（用于自动瞄准）</param>
    void Attack(Transform owner, int weaponLevel, Transform target = null);
    
    /// <summary>
    /// 获取当前等级下的攻击间隔（秒/次）
    /// </summary>
    /// <param name="weaponLevel">武器等级</param>
    /// <returns>攻击间隔（秒）</returns>
    float GetAttackRate(int weaponLevel);
    
    /// <summary>
    /// 获取当前等级下的基础伤害
    /// </summary>
    /// <param name="weaponLevel">武器等级</param>
    /// <returns>基础伤害值</returns>
    int GetBaseDamage(int weaponLevel);
}
