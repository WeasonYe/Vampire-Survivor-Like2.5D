using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏事件管理器，负责管理游戏中的事件系统
/// </summary>
public class GameEventsManager : Singleton<GameEventsManager>
{
    /// <summary>
    /// 事件类型常量
    /// </summary>
    public static class EventTypes
    {
        // 玩家相关事件
        public const string PlayerSpawned = "PlayerSpawned";
        public const string PlayerDamaged = "PlayerDamaged";
        public const string PlayerHealed = "PlayerHealed";
        public const string PlayerDied = "PlayerDied";
        public const string PlayerLevelUp = "PlayerLevelUp";
        
        // 敌人相关事件
        public const string EnemySpawned = "EnemySpawned";
        public const string EnemyDamaged = "EnemyDamaged";
        public const string EnemyKilled = "EnemyKilled";
        
        // 游戏状态相关事件
        public const string GameStart = "GameStart";
        public const string GamePause = "GamePause";
        public const string GameResume = "GameResume";
        public const string GameOver = "GameOver";
        public const string GameRestart = "GameRestart";
        
        // 物品相关事件
        public const string ItemPickedUp = "ItemPickedUp";
        public const string ItemUsed = "ItemUsed";
        
        // 关卡相关事件
        public const string LevelStart = "LevelStart";
        public const string LevelComplete = "LevelComplete";
        public const string LevelFailed = "LevelFailed";
        
        // 经验值相关事件
        public const string ExperienceGained = "ExperienceGained";
    }

    /// <summary>
    /// 事件字典，键为事件名称，值为事件回调列表
    /// </summary>
    private Dictionary<string, System.Action<object[]>> eventDictionary = new Dictionary<string, System.Action<object[]>>();

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 注册事件监听器
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="listener">事件回调函数</param>
    public void RegisterListener(string eventName, System.Action<object[]> listener)
    {
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = null;
        }
        eventDictionary[eventName] += listener;
    }

    /// <summary>
    /// 注销事件监听器
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="listener">事件回调函数</param>
    public void UnregisterListener(string eventName, System.Action<object[]> listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= listener;
        }
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="parameters">事件参数</param>
    public void TriggerEvent(string eventName, params object[] parameters)
    {
        if (eventDictionary.ContainsKey(eventName) && eventDictionary[eventName] != null)
        {
            eventDictionary[eventName].Invoke(parameters);
        }
    }

    /// <summary>
    /// 清空指定事件的所有监听器
    /// </summary>
    /// <param name="eventName">事件名称</param>
    public void ClearEvent(string eventName)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = null;
        }
    }

    /// <summary>
    /// 清空所有事件的监听器
    /// </summary>
    public void ClearAllEvents()
    {
        eventDictionary.Clear();
    }

    /// <summary>
    /// 检查事件是否存在
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>事件是否存在</returns>
    public bool HasEvent(string eventName)
    {
        return eventDictionary.ContainsKey(eventName);
    }

    /// <summary>
    /// 获取事件的监听器数量
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>监听器数量</returns>
    public int GetListenerCount(string eventName)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            System.Delegate[] delegates = eventDictionary[eventName].GetInvocationList();
            return delegates.Length;
        }
        return 0;
    }
}
