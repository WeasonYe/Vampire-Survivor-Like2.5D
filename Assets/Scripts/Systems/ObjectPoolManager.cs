using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池管理器，负责管理游戏中的对象池
/// </summary>
public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    /// <summary>
    /// 对象池字典，键为预制体，值为对象池
    /// </summary>
    private Dictionary<GameObject, Queue<GameObject>> objectPools = new Dictionary<GameObject, Queue<GameObject>>();

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    /// <param name="prefab">要获取的对象预制体</param>
    /// <param name="position">对象的位置</param>
    /// <param name="rotation">对象的旋转</param>
    /// <returns>从对象池获取的对象</returns>
    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // 检查对象池是否存在
        if (!objectPools.ContainsKey(prefab))
        {
            // 创建新的对象池
            objectPools[prefab] = new Queue<GameObject>();
        }

        // 检查对象池是否有可用对象
        if (objectPools[prefab].Count > 0)
        {
            // 从对象池获取对象
            GameObject obj = objectPools[prefab].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 生成新对象
            GameObject obj = Instantiate(prefab, position, rotation);
            // 添加到对象池管理器的子节点
            obj.transform.SetParent(transform);
            return obj;
        }
    }

    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    /// <param name="prefab">对象的预制体</param>
    /// <param name="obj">要回收的对象</param>
    public void ReturnObject(GameObject prefab, GameObject obj)
    {
        // 检查对象池是否存在
        if (!objectPools.ContainsKey(prefab))
        {
            // 创建新的对象池
            objectPools[prefab] = new Queue<GameObject>();
        }

        // 禁用对象
        obj.SetActive(false);
        // 重置对象位置和旋转
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        // 回收对象到对象池
        objectPools[prefab].Enqueue(obj);
    }

    /// <summary>
    /// 预加载对象到对象池
    /// </summary>
    /// <param name="prefab">要预加载的对象预制体</param>
    /// <param name="count">预加载的数量</param>
    public void PreloadObjects(GameObject prefab, int count)
    {
        // 检查对象池是否存在
        if (!objectPools.ContainsKey(prefab))
        {
            // 创建新的对象池
            objectPools[prefab] = new Queue<GameObject>();
        }

        // 预加载指定数量的对象
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            objectPools[prefab].Enqueue(obj);
        }
    }

    /// <summary>
    /// 清空指定预制体的对象池
    /// </summary>
    /// <param name="prefab">要清空的预制体</param>
    public void ClearPool(GameObject prefab)
    {
        if (objectPools.ContainsKey(prefab))
        {
            // 销毁对象池中的所有对象
            foreach (GameObject obj in objectPools[prefab])
            {
                Destroy(obj);
            }
            // 清空对象池
            objectPools[prefab].Clear();
        }
    }

    /// <summary>
    /// 清空所有对象池
    /// </summary>
    public void ClearAllPools()
    {
        // 销毁所有对象池中的对象
        foreach (var pool in objectPools.Values)
        {
            foreach (GameObject obj in pool)
            {
                Destroy(obj);
            }
            pool.Clear();
        }
        // 清空对象池字典
        objectPools.Clear();
    }

    /// <summary>
    /// 获取对象池中的对象数量
    /// </summary>
    /// <param name="prefab">要查询的预制体</param>
    /// <returns>对象池中的对象数量</returns>
    public int GetPoolCount(GameObject prefab)
    {
        if (objectPools.ContainsKey(prefab))
        {
            return objectPools[prefab].Count;
        }
        return 0;
    }
}
