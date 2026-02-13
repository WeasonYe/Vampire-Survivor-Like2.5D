using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器管理器，负责管理玩家的武器
/// </summary>
public class WeaponManager : Singleton<WeaponManager>
{
    [Header("武器设置")]
    [Tooltip("武器预制体")]
    [SerializeField]
    private GameObject weaponPrefab;

    [Tooltip("武器挂点")]
    [SerializeField]
    private Transform weaponHolder;

    [Header("初始武器")]
    [Tooltip("初始攻击策略")]
    [SerializeField]
    private AttackStrategySO initialStrategy;

    [Tooltip("初始武器等级")]
    [SerializeField]
    private int initialWeaponLevel = 1;

    private List<WeaponBase> weapons = new List<WeaponBase>();
    private Transform player;

    /// <summary>
    /// 武器数量
    /// </summary>
    public int WeaponCount => weapons.Count;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // 查找玩家
        FindPlayer();

        // 初始化武器挂点
        InitializeWeaponHolder();

        // 添加初始武器
        if (initialStrategy != null)
        {
            AddWeapon(initialStrategy, initialWeaponLevel);
        }
    }

    /// <summary>
    /// 查找玩家
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    /// <summary>
    /// 初始化武器挂点
    /// </summary>
    private void InitializeWeaponHolder()
    {
        // 如果没有指定武器挂点，使用玩家对象
        if (weaponHolder == null)
        {
            if (player != null)
            {
                weaponHolder = player;
            }
            else
            {
                // 创建武器挂点对象
                GameObject holder = new GameObject("WeaponHolder");
                weaponHolder = holder.transform;
            }
        }
    }

    /// <summary>
    /// 添加武器
    /// </summary>
    /// <param name="strategy">攻击策略</param>
    /// <param name="startLevel">初始等级</param>
    /// <returns>创建的武器</returns>
    public WeaponBase AddWeapon(AttackStrategySO strategy, int startLevel = 1)
    {
        if (strategy == null)
        {
            Debug.LogError("[WeaponManager] Cannot add weapon with null strategy!");
            return null;
        }

        // 确保武器挂点存在
        if (weaponHolder == null)
        {
            InitializeWeaponHolder();
        }

        // 创建武器对象
        GameObject weaponObject;
        if (weaponPrefab != null)
        {
            weaponObject = Instantiate(weaponPrefab, weaponHolder);
        }
        else
        {
            // 如果没有指定武器预制体，创建空对象
            weaponObject = new GameObject("Weapon_" + weapons.Count);
            weaponObject.transform.SetParent(weaponHolder);
        }

        // 获取武器组件
        WeaponBase weapon = weaponObject.GetComponent<WeaponBase>();
        if (weapon == null)
        {
            weapon = weaponObject.AddComponent<WeaponBase>();
        }

        // 设置策略和等级
        weapon.SetStrategy(strategy);
        weapon.SetLevel(startLevel);

        // 添加到武器列表
        weapons.Add(weapon);

        Debug.Log("[WeaponManager] Added weapon: " + strategy.name + " at level " + startLevel);

        return weapon;
    }

    /// <summary>
    /// 移除武器
    /// </summary>
    /// <param name="weapon">要移除的武器</param>
    public void RemoveWeapon(WeaponBase weapon)
    {
        if (weapon == null || !weapons.Contains(weapon))
            return;

        // 从列表中移除
        weapons.Remove(weapon);

        // 销毁武器对象
        Destroy(weapon.gameObject);

        Debug.Log("[WeaponManager] Removed weapon: " + (weapon.Strategy != null ? weapon.Strategy.name : "null"));
    }

    /// <summary>
    /// 移除指定索引的武器
    /// </summary>
    /// <param name="index">武器索引</param>
    public void RemoveWeaponAt(int index)
    {
        if (index < 0 || index >= weapons.Count)
            return;

        RemoveWeapon(weapons[index]);
    }

    /// <summary>
    /// 升级指定索引的武器
    /// </summary>
    /// <param name="index">武器索引</param>
    public void UpgradeWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count)
            return;

        weapons[index].Upgrade();
    }

    /// <summary>
    /// 升级所有武器
    /// </summary>
    public void UpgradeAllWeapons()
    {
        foreach (WeaponBase weapon in weapons)
        {
            weapon.Upgrade();
        }
    }

    /// <summary>
    /// 设置指定索引武器的策略（用于武器进化）
    /// </summary>
    /// <param name="index">武器索引</param>
    /// <param name="newStrategy">新策略</param>
    public void EvolveWeapon(int index, AttackStrategySO newStrategy)
    {
        if (index < 0 || index >= weapons.Count || newStrategy == null)
            return;

        weapons[index].SetStrategy(newStrategy);
        Debug.Log("[WeaponManager] Evolved weapon at index " + index + " to " + newStrategy.name);
    }

    /// <summary>
    /// 获取指定索引的武器
    /// </summary>
    /// <param name="index">武器索引</param>
    /// <returns>武器对象</returns>
    public WeaponBase GetWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count)
            return null;

        return weapons[index];
    }

    /// <summary>
    /// 获取所有武器
    /// </summary>
    /// <returns>武器列表</returns>
    public List<WeaponBase> GetAllWeapons()
    {
        return new List<WeaponBase>(weapons);
    }

    /// <summary>
    /// 清空所有武器
    /// </summary>
    public void ClearAllWeapons()
    {
        foreach (WeaponBase weapon in weapons)
        {
            if (weapon != null)
            {
                Destroy(weapon.gameObject);
            }
        }

        weapons.Clear();

        Debug.Log("[WeaponManager] Cleared all weapons");
    }

    /// <summary>
    /// 设置所有武器的自动攻击状态
    /// </summary>
    /// <param name="enable">是否启用</param>
    public void SetAllWeaponsAutoAttack(bool enable)
    {
        foreach (WeaponBase weapon in weapons)
        {
            weapon.SetAutoAttack(enable);
        }
    }

    /// <summary>
    /// 立即执行所有武器的攻击（忽略冷却时间）
    /// </summary>
    public void ForceAllWeaponsAttack()
    {
        foreach (WeaponBase weapon in weapons)
        {
            weapon.ForceAttack();
        }
    }
}
