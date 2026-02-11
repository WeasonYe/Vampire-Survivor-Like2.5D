using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家属性管理脚本
/// </summary>
public class PlayerStats : Singleton<PlayerStats>
{
    [Header("基础属性")]
    [Tooltip("当前得分")]
    private int score = 0;

    [Tooltip("当前等级")]
    private int level = 1;

    [Tooltip("当前经验值")]
    private int currentExperience = 0;

    [Tooltip("升级所需经验值")]
    private int experienceToNextLevel = 100;

    [Tooltip("经验值增长系数")]
    [SerializeField]
    private float experienceGrowthFactor = 1.5f;

    [Header("能力属性")]
    [Tooltip("伤害倍率")]
    [SerializeField]
    private float damageMultiplier = 1.0f;

    [Tooltip("攻击速度倍率")]
    [SerializeField]
    private float attackSpeedMultiplier = 1.0f;

    [Tooltip("移动速度倍率")]
    [SerializeField]
    private float moveSpeedMultiplier = 1.0f;

    [Tooltip("幸运值")]
    [SerializeField]
    private float luck = 1.0f;

    /// <summary>
    /// 当前得分
    /// </summary>
    public int Score => score;

    /// <summary>
    /// 当前等级
    /// </summary>
    public int Level => level;

    /// <summary>
    /// 当前经验值
    /// </summary>
    public int CurrentExperience => currentExperience;

    /// <summary>
    /// 升级所需经验值
    /// </summary>
    public int ExperienceToNextLevel => experienceToNextLevel;

    /// <summary>
    /// 经验值百分比
    /// </summary>
    public float ExperiencePercentage => (float)currentExperience / experienceToNextLevel;

    /// <summary>
    /// 伤害倍率
    /// </summary>
    public float DamageMultiplier => damageMultiplier;

    /// <summary>
    /// 攻击速度倍率
    /// </summary>
    public float AttackSpeedMultiplier => attackSpeedMultiplier;

    /// <summary>
    /// 移动速度倍率
    /// </summary>
    public float MoveSpeedMultiplier => moveSpeedMultiplier;

    /// <summary>
    /// 幸运值
    /// </summary>
    public float Luck => luck;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // 初始化属性
        ResetStats();

        // 注册事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.EnemyKilled, OnEnemyKilled);
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.GameStart, OnGameStart);
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.GameRestart, OnGameRestart);
        }
    }

    private void OnDestroy()
    {
        // 注销事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.EnemyKilled, OnEnemyKilled);
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.GameStart, OnGameStart);
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.GameRestart, OnGameRestart);
        }
    }

    /// <summary>
    /// 增加得分
    /// </summary>
    /// <param name="amount">增加的分数</param>
    public void AddScore(int amount)
    {
        score += amount;

        // 触发得分增加事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.TriggerEvent(GameEventsManager.EventTypes.PlayerLevelUp, score, level);
        }
    }

    /// <summary>
    /// 增加经验值
    /// </summary>
    /// <param name="amount">增加的经验值</param>
    public void AddExperience(int amount)
    {
        currentExperience += amount;

        // 检查是否升级
        while (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }

        // 触发经验值增加事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.TriggerEvent("ExperienceGained", currentExperience, experienceToNextLevel, level);
        }
    }

    /// <summary>
    /// 升级
    /// </summary>
    private void LevelUp()
    {
        // 扣除升级所需经验值
        currentExperience -= experienceToNextLevel;
        
        // 增加等级
        level++;
        
        // 计算下一级所需经验值
        experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * experienceGrowthFactor);
        
        // 触发升级事件
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.TriggerEvent(GameEventsManager.EventTypes.PlayerLevelUp, level, experienceToNextLevel);
        }
        
        Debug.Log($"Player leveled up to {level}!");
    }

    /// <summary>
    /// 设置伤害倍率
    /// </summary>
    /// <param name="multiplier">伤害倍率</param>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0.1f, multiplier);
    }

    /// <summary>
    /// 增加伤害倍率
    /// </summary>
    /// <param name="amount">增加的倍率</param>
    public void AddDamageMultiplier(float amount)
    {
        damageMultiplier += amount;
    }

    /// <summary>
    /// 设置攻击速度倍率
    /// </summary>
    /// <param name="multiplier">攻击速度倍率</param>
    public void SetAttackSpeedMultiplier(float multiplier)
    {
        attackSpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    /// <summary>
    /// 增加攻击速度倍率
    /// </summary>
    /// <param name="amount">增加的倍率</param>
    public void AddAttackSpeedMultiplier(float amount)
    {
        attackSpeedMultiplier += amount;
    }

    /// <summary>
    /// 设置移动速度倍率
    /// </summary>
    /// <param name="multiplier">移动速度倍率</param>
    public void SetMoveSpeedMultiplier(float multiplier)
    {
        moveSpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    /// <summary>
    /// 增加移动速度倍率
    /// </summary>
    /// <param name="amount">增加的倍率</param>
    public void AddMoveSpeedMultiplier(float amount)
    {
        moveSpeedMultiplier += amount;
    }

    /// <summary>
    /// 设置幸运值
    /// </summary>
    /// <param name="value">幸运值</param>
    public void SetLuck(float value)
    {
        luck = Mathf.Max(0.1f, value);
    }

    /// <summary>
    /// 增加幸运值
    /// </summary>
    /// <param name="amount">增加的幸运值</param>
    public void AddLuck(float amount)
    {
        luck += amount;
    }

    /// <summary>
    /// 重置属性
    /// </summary>
    public void ResetStats()
    {
        score = 0;
        level = 1;
        currentExperience = 0;
        experienceToNextLevel = 100;
        damageMultiplier = 1.0f;
        attackSpeedMultiplier = 1.0f;
        moveSpeedMultiplier = 1.0f;
        luck = 1.0f;
    }

    /// <summary>
    /// 敌人死亡事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnEnemyKilled(object[] parameters)
    {
        // 假设参数中包含敌人名称和得分
        if (parameters.Length >= 2)
        {
            string enemyName = (string)parameters[0];
            int enemyScore = (int)parameters[1];
            
            // 增加得分和经验值
            AddScore(enemyScore);
            AddExperience(enemyScore / 2); // 经验值为得分的一半
        }
    }

    /// <summary>
    /// 游戏开始事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnGameStart(object[] parameters)
    {
        ResetStats();
    }

    /// <summary>
    /// 游戏重启事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnGameRestart(object[] parameters)
    {
        ResetStats();
    }
}
