using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敌人血条UI组件，负责显示敌人的生命值
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("血条组件")]
    [Tooltip("血条背景")]
    [SerializeField]
    protected Image healthBarBackground;

    [Tooltip("血条填充")]
    [SerializeField]
    protected Image healthBarFill;

    [Tooltip("Canvas Group（用于渐隐）")]
    [SerializeField]
    protected CanvasGroup canvasGroup;

    [Header("显示设置")]
    [Tooltip("血条slider")]
    [SerializeField]
    protected Slider healthSlider;

    [Tooltip("血条偏移（相对于敌人）")]
    [SerializeField]
    protected Vector3 offset = new Vector3(0, 2f, 0);

    [Tooltip("血条显示距离")]
    [SerializeField]
    protected float showDistance = 15f;

    [Tooltip("血条渐隐速度")]
    [SerializeField]
    protected float fadeSpeed = 2f;

    [Tooltip("受伤后显示时间")]
    [SerializeField]
    protected float showAfterDamageTime = 2f;

    [Header("颜色设置")]
    [Tooltip("高血量颜色")]
    [SerializeField]
    protected Color highHealthColor = Color.green;

    [Tooltip("中血量颜色")]
    [SerializeField]
    protected Color mediumHealthColor = Color.yellow;

    [Tooltip("低血量颜色")]
    [SerializeField]
    protected Color lowHealthColor = Color.red;

    [Tooltip("血量阈值（中等）")]
    [SerializeField]
    protected float mediumHealthThreshold = 0.5f;

    [Tooltip("血量阈值（低）")]
    [SerializeField]
    protected float lowHealthThreshold = 0.25f;

    protected Transform targetEnemy;
    protected Camera mainCamera;
    protected float currentAlpha = 0f;
    protected float showTimer = 0f;
    protected bool isInitialized = false;

    /// <summary>
    /// 初始化血条
    /// </summary>
    /// <param name="enemy">敌人Transform</param>
    public virtual void Initialize(Transform enemy)
    {
        targetEnemy = enemy;
        mainCamera = Camera.main;
        isInitialized = true;
        // 初始化slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = 1;
        }

        // 初始隐藏
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// 更新血条显示
    /// </summary>
    /// <param name="currentHealth">当前生命值</param>
    /// <param name="maxHealth">最大生命值</param>
    public virtual void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (!isInitialized)
            return;

        // 计算血条填充比例
        float fillAmount = Mathf.Clamp01(currentHealth / maxHealth);

        // 更新血条填充
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = fillAmount;

            // 更新血条颜色
            UpdateHealthBarColor(fillAmount);
        }

        // 显示血条
        ShowHealthBar();
    }

    /// <summary>
    /// 更新血条颜色
    /// </summary>
    /// <param name="fillAmount">填充比例</param>
    protected virtual void UpdateHealthBarColor(float fillAmount)
    {
        if (healthBarFill == null)
            return;

        // 更新slider值
        if (healthSlider != null)
        {
            healthSlider.value = fillAmount;
        }

        // 根据血量比例设置颜色
        if (fillAmount > mediumHealthThreshold)
        {
            healthBarFill.color = highHealthColor;
        }
        else if (fillAmount > lowHealthThreshold)
        {
            healthBarFill.color = mediumHealthColor;
        }
        else
        {
            healthBarFill.color = lowHealthColor;
        }
    }

    /// <summary>
    /// 显示血条
    /// </summary>
    public virtual void ShowHealthBar()
    {
        showTimer = showAfterDamageTime;
        currentAlpha = 1f;
    }

    /// <summary>
    /// 隐藏血条
    /// </summary>
    public virtual void HideHealthBar()
    {
        currentAlpha = 0f;
    }

    protected virtual void Update()
    {
        if (!isInitialized || targetEnemy == null)
            return;

        // 更新血条位置
        UpdatePosition();

        // 更新血条显示状态
        UpdateVisibility();
    }

    /// <summary>
    /// 更新血条位置
    /// </summary>
    protected virtual void UpdatePosition()
    {
        // 计算血条目标位置
        Vector3 targetPosition = targetEnemy.position + offset;

        // 将血条面向摄像机
        if (mainCamera != null)
        {
            transform.position = targetPosition;
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }

    /// <summary>
    /// 更新血条可见性
    /// </summary>
    protected virtual void UpdateVisibility()
    {
        // 检查与玩家的距离
        if (mainCamera != null)
        {
            float distance = Vector3.Distance(transform.position, mainCamera.transform.position);

            // 如果距离太远，隐藏血条
            if (distance > showDistance)
            {
                currentAlpha = 0f;
            }
        }

        // 更新显示计时器
        if (showTimer > 0f)
        {
            showTimer -= Time.deltaTime;
        }
        else if (currentAlpha > 0f)
        {
            // 渐隐效果
            currentAlpha -= fadeSpeed * Time.deltaTime;
            currentAlpha = Mathf.Max(0f, currentAlpha);
        }

        // 应用透明度
        if (canvasGroup != null)
        {
            canvasGroup.alpha = currentAlpha;
        }
    }

    /// <summary>
    /// 重置血条
    /// </summary>
    public virtual void ResetHealthBar()
    {
        currentAlpha = 0f;
        showTimer = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
            healthBarFill.color = highHealthColor;
        }
    }

    /// <summary>
    /// 设置血条偏移
    /// </summary>
    /// <param name="newOffset">新的偏移量</param>
    public virtual void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    /// <summary>
    /// 设置血条显示距离
    /// </summary>
    /// <param name="distance">显示距离</param>
    public virtual void SetShowDistance(float distance)
    {
        showDistance = Mathf.Max(0f, distance);
    }

    /// <summary>
    /// 设置血条渐隐速度
    /// </summary>
    /// <param name="speed">渐隐速度</param>
    public virtual void SetFadeSpeed(float speed)
    {
        fadeSpeed = Mathf.Max(0f, speed);
    }

    /// <summary>
    /// 设置血条颜色
    /// </summary>
    /// <param name="highColor">高血量颜色</param>
    /// <param name="mediumColor">中血量颜色</param>
    /// <param name="lowColor">低血量颜色</param>
    public virtual void SetHealthBarColors(Color highColor, Color mediumColor, Color lowColor)
    {
        highHealthColor = highColor;
        mediumHealthColor = mediumColor;
        lowHealthColor = lowColor;
    }

    /// <summary>
    /// 获取当前透明度
    /// </summary>
    /// <returns>透明度</returns>
    public virtual float GetCurrentAlpha()
    {
        return currentAlpha;
    }

    /// <summary>
    /// 检查血条是否可见
    /// </summary>
    /// <returns>是否可见</returns>
    public virtual bool IsVisible()
    {
        return currentAlpha > 0f;
    }
}