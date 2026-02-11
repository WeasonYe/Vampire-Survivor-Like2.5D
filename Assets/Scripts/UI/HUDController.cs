using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD控制器，负责显示玩家血条、状态、武器buff和关卡波次
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("血条设置")]
    [Tooltip("血条滑块")]
    [SerializeField]
    private Slider healthSlider;

    [Tooltip("血条文本")]
    [SerializeField]
    private Text healthText;

    [Header("状态设置")]
    [Tooltip("得分文本")]
    [SerializeField]
    private Text scoreText;

    [Tooltip("等级文本")]
    [SerializeField]
    private Text levelText;

    [Tooltip("经验值滑块")]
    [SerializeField]
    private Slider experienceSlider;

    [Tooltip("经验值文本")]
    [SerializeField]
    private Text experienceText;

    [Header("武器Buff设置")]
    [Tooltip("武器Buff容器")]
    [SerializeField]
    private Transform weaponBuffContainer;

    [Tooltip("武器Buff预制体")]
    [SerializeField]
    private GameObject weaponBuffPrefab;

    [Header("关卡波次设置")]
    [Tooltip("波次文本")]
    [SerializeField]
    private Text waveText;

    [Tooltip("波次倒计时文本")]
    [SerializeField]
    private Text waveCountdownText;

    // 武器Buff字典，键为武器名称，值为Buff实例
    private Dictionary<string, GameObject> weaponBuffs = new Dictionary<string, GameObject>();

    private void Start()
    {
        // 初始化UI
        InitializeUI();

        // 注册事件
        RegisterEvents();
    }

    private void OnDestroy()
    {
        // 注销事件
        UnregisterEvents();
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitializeUI()
    {
        // 初始化血条
        if (healthSlider != null)
        {
            healthSlider.value = 1.0f;
        }

        if (healthText != null)
        {
            healthText.text = "100/100";
        }

        // 初始化状态
        if (scoreText != null)
        {
            scoreText.text = "Score: 0";
        }

        if (levelText != null)
        {
            levelText.text = "Level: 1";
        }

        if (experienceSlider != null)
        {
            experienceSlider.value = 0.0f;
        }

        if (experienceText != null)
        {
            experienceText.text = "Exp: 0/100";
        }

        // 初始化波次
        if (waveText != null)
        {
            waveText.text = "Wave: 1";
        }

        if (waveCountdownText != null)
        {
            waveCountdownText.text = "Next: 0s";
        }

        // 清空武器Buff容器
        if (weaponBuffContainer != null)
        {
            foreach (Transform child in weaponBuffContainer)
            {
                Destroy(child.gameObject);
            }
        }

        weaponBuffs.Clear();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvents()
    {
        if (GameEventsManager.Instance != null)
        {
            // 玩家相关事件
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.PlayerDamaged, OnPlayerDamaged);
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.PlayerHealed, OnPlayerHealed);
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.PlayerLevelUp, OnPlayerLevelUp);
            
            // 游戏状态相关事件
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.GameStart, OnGameStart);
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.GameOver, OnGameOver);
            
            // 经验值相关事件
            GameEventsManager.Instance.RegisterListener(GameEventsManager.EventTypes.ExperienceGained, OnExperienceGained);
        }
    }

    /// <summary>
    /// 注销事件
    /// </summary>
    private void UnregisterEvents()
    {
        if (GameEventsManager.Instance != null)
        {
            // 玩家相关事件
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.PlayerDamaged, OnPlayerDamaged);
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.PlayerHealed, OnPlayerHealed);
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.PlayerLevelUp, OnPlayerLevelUp);
            
            // 游戏状态相关事件
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.GameStart, OnGameStart);
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.GameOver, OnGameOver);
            
            // 经验值相关事件
            GameEventsManager.Instance.UnregisterListener(GameEventsManager.EventTypes.ExperienceGained, OnExperienceGained);
        }
    }

    /// <summary>
    /// 更新血条
    /// </summary>
    /// <param name="currentHealth">当前生命值</param>
    /// <param name="maxHealth">最大生命值</param>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
        }
    }

    /// <summary>
    /// 更新得分
    /// </summary>
    /// <param name="score">得分</param>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    /// <summary>
    /// 更新等级
    /// </summary>
    /// <param name="level">等级</param>
    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }
    }

    /// <summary>
    /// 更新经验值
    /// </summary>
    /// <param name="currentExp">当前经验值</param>
    /// <param name="maxExp">最大经验值</param>
    public void UpdateExperience(int currentExp, int maxExp)
    {
        if (experienceSlider != null)
        {
            experienceSlider.value = (float)currentExp / maxExp;
        }

        if (experienceText != null)
        {
            experienceText.text = $"Exp: {currentExp}/{maxExp}";
        }
    }

    /// <summary>
    /// 更新波次
    /// </summary>
    /// <param name="wave">波次</param>
    public void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {wave}";
        }
    }

    /// <summary>
    /// 更新波次倒计时
    /// </summary>
    /// <param name="time">倒计时时间（秒）</param>
    public void UpdateWaveCountdown(float time)
    {
        if (waveCountdownText != null)
        {
            waveCountdownText.text = $"Next: {Mathf.CeilToInt(time)}s";
        }
    }

    /// <summary>
    /// 添加武器Buff
    /// </summary>
    /// <param name="weaponName">武器名称</param>
    /// <param name="level">武器等级</param>
    public void AddWeaponBuff(string weaponName, int level)
    {
        // 检查武器Buff是否已存在
        if (weaponBuffs.ContainsKey(weaponName))
        {
            // 更新现有Buff
            UpdateWeaponBuff(weaponName, level);
            return;
        }

        // 检查预制体和容器是否存在
        if (weaponBuffPrefab == null || weaponBuffContainer == null)
        {
            Debug.LogWarning("Weapon buff prefab or container is not assigned!");
            return;
        }

        // 创建新的Buff实例
        GameObject buffInstance = Instantiate(weaponBuffPrefab, weaponBuffContainer);
        
        // 设置Buff文本
        Text buffText = buffInstance.GetComponentInChildren<Text>();
        if (buffText != null)
        {
            buffText.text = $"{weaponName} Lv.{level}";
        }

        // 添加到字典
        weaponBuffs[weaponName] = buffInstance;
    }

    /// <summary>
    /// 更新武器Buff
    /// </summary>
    /// <param name="weaponName">武器名称</param>
    /// <param name="level">武器等级</param>
    public void UpdateWeaponBuff(string weaponName, int level)
    {
        if (weaponBuffs.ContainsKey(weaponName))
        {
            GameObject buffInstance = weaponBuffs[weaponName];
            Text buffText = buffInstance.GetComponentInChildren<Text>();
            if (buffText != null)
            {
                buffText.text = $"{weaponName} Lv.{level}";
            }
        }
    }

    /// <summary>
    /// 移除武器Buff
    /// </summary>
    /// <param name="weaponName">武器名称</param>
    public void RemoveWeaponBuff(string weaponName)
    {
        if (weaponBuffs.ContainsKey(weaponName))
        {
            Destroy(weaponBuffs[weaponName]);
            weaponBuffs.Remove(weaponName);
        }
    }

    /// <summary>
    /// 清空所有武器Buff
    /// </summary>
    public void ClearAllWeaponBuffs()
    {
        foreach (GameObject buff in weaponBuffs.Values)
        {
            Destroy(buff);
        }
        weaponBuffs.Clear();
    }

    /// <summary>
    /// 玩家受伤事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnPlayerDamaged(object[] parameters)
    {
        // 从PlayerHealth获取当前生命值
        if (FindObjectOfType<PlayerHealth>() != null)
        {
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    /// <summary>
    /// 玩家治疗事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnPlayerHealed(object[] parameters)
    {
        // 从PlayerHealth获取当前生命值
        if (FindObjectOfType<PlayerHealth>() != null)
        {
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    /// <summary>
    /// 玩家升级事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnPlayerLevelUp(object[] parameters)
    {
        // 从PlayerStats获取当前等级和得分
        if (PlayerStats.Instance != null)
        {
            UpdateLevel(PlayerStats.Instance.Level);
            UpdateScore(PlayerStats.Instance.Score);
            UpdateExperience(PlayerStats.Instance.CurrentExperience, PlayerStats.Instance.ExperienceToNextLevel);
        }
    }

    /// <summary>
    /// 经验值获得事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnExperienceGained(object[] parameters)
    {
        if (parameters.Length >= 3)
        {
            int currentExp = (int)parameters[0];
            int maxExp = (int)parameters[1];
            UpdateExperience(currentExp, maxExp);
        }
    }

    /// <summary>
    /// 游戏开始事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnGameStart(object[] parameters)
    {
        // 重置UI
        InitializeUI();
    }

    /// <summary>
    /// 游戏结束事件处理
    /// </summary>
    /// <param name="parameters">事件参数</param>
    private void OnGameOver(object[] parameters)
    {
        // 可以在这里添加游戏结束时的UI效果
    }
}
