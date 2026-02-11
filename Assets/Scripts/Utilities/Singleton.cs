using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity单例基类，提供线程安全的单例模式实现
/// </summary>
/// <typeparam name="T">单例类型，必须继承自MonoBehaviour</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object lockObject = new object();
    private static bool applicationIsQuitting = false;

    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Returning null.");
                return null;
            }

            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (FindObjectsOfType<T>().Length > 1)
                    {
                        Debug.LogError($"[Singleton] Multiple instances of {typeof(T)} found! The first one will be used.");
                        return instance;
                    }

                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject($"{typeof(T)} (Singleton)");
                        instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                        Debug.Log($"[Singleton] Instance '{typeof(T)}' was created because it didn't exist.");
                    }
                    else
                    {
                        Debug.Log($"[Singleton] Using existing instance of '{typeof(T)}'.");
                    }
                }

                return instance;
            }
        }
    }

    /// <summary>
    /// 初始化时调用，确保单例唯一性
    /// </summary>
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[Singleton] Instance of '{typeof(T)}' already exists. Destroying this instance.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 销毁时调用，标记应用程序正在退出
    /// </summary>
    protected virtual void OnDestroy()
    {
        applicationIsQuitting = true;
    }
}
