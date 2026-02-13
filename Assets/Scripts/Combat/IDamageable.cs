/// <summary>
/// 可伤害接口，所有可以受到伤害的对象都需要实现此接口
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    void TakeDamage(int damage);
}
