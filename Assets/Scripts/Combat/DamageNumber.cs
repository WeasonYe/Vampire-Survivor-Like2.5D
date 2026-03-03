using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 伤害数字显示组件
/// </summary>
public class DamageNumber : MonoBehaviour
{
    private TextMesh textMesh;
    private float duration;
    private float riseSpeed;
    private float timer;
    private Vector3 startPosition;

    /// <summary>
    /// 初始化伤害数字
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="isCritical">是否暴击</param>
    /// <param name="duration">显示持续时间</param>
    /// <param name="riseSpeed">上升速度</param>
    public void Initialize(int damage, bool isCritical, float duration, float riseSpeed)
    {
        // 获取或添加TextMesh组件
        textMesh = GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMesh>();
        }

        // 设置伤害文本
        textMesh.text = damage.ToString();

        // 设置颜色
        if (isCritical)
        {
            textMesh.color = Color.red;
            textMesh.fontSize = 20;
        }
        else
        {
            textMesh.color = Color.white;
            textMesh.fontSize = 14;
        }

        // 设置对齐方式
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;

        // 保存参数
        this.duration = duration;
        this.riseSpeed = riseSpeed;
        this.startPosition = transform.position;
        this.timer = 0f;

        // 确保朝向摄像机
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 上升效果
        transform.position = startPosition + Vector3.up * (riseSpeed * timer);

        // 逐渐消失
        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a = 1f - (timer / duration);
            textMesh.color = color;
        }

        // 持续时间结束，销毁对象
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}