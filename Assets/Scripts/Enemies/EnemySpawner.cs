using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人生成器，负责循环生成敌人
/// </summary>
public class EnemySpawner : Singleton<EnemySpawner>
{
    [Header("生成设置")]
    [Tooltip("敌人预制体列表")]
    [SerializeField]
    private List<GameObject> enemyPrefabs = new List<GameObject>();

    [Tooltip("初始生成延迟")]
    [SerializeField]
    private float initialSpawnDelay = 2f;

    [Tooltip("生成间隔")]
    [SerializeField]
    private float spawnInterval = 1.5f;

    [Tooltip("最大同时存在的敌人数量")]
    [SerializeField]
    private int maxEnemies = 30;

    [Header("生成范围设置")]
    [Tooltip("生成范围半径")]
    [SerializeField]
    private float spawnRadius = 50f;

    [Tooltip("最小生成距离（离玩家）")]
    [SerializeField]
    private float minSpawnDistance = 10f;

    [Header("地图边界设置")]
    [Tooltip("是否使用地图边界")]
    [SerializeField]
    private bool useMapBounds = false;

    [Tooltip("地图最小边界")]
    [SerializeField]
    private Vector3 mapMinBounds = new Vector3(-100f, 0f, -100f);

    [Tooltip("地图最大边界")]
    [SerializeField]
    private Vector3 mapMaxBounds = new Vector3(100f, 0f, 100f);

    private Transform player;
    private bool isSpawning = true;

    /// <summary>
    /// 生成的敌人总数
    /// </summary>
    public int TotalEnemiesSpawned { get; private set; } = 0;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // 查找玩家
        FindPlayer();
        
        // 开始生成敌人
        StartCoroutine(SpawnEnemies());
    }

    private void Update()
    {
        // 检查玩家是否存在
        if (player == null)
        {
            FindPlayer();
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
    /// 生成敌人的协程
    /// </summary>
    private IEnumerator SpawnEnemies()
    {
        // 初始延迟
        yield return new WaitForSeconds(initialSpawnDelay);

        while (isSpawning)
        {
            // 检查是否可以生成敌人
            if (CanSpawnEnemy())
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 生成敌人
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0 || player == null)
            return;

        // 选择一个随机的敌人预制体
        int randomEnemyIndex = Random.Range(0, enemyPrefabs.Count);
        GameObject enemyPrefab = enemyPrefabs[randomEnemyIndex];

        // 计算生成位置
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // 从对象池获取敌人
        GameObject enemy = ObjectPoolManager.Instance.GetObject(enemyPrefab, spawnPosition, Quaternion.identity);

        // 设置敌人父对象
        enemy.transform.SetParent(transform);

        // 设置敌人预制体引用
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            enemyBase.EnemyPrefab = enemyPrefab;
        }

        // 增加生成计数
        TotalEnemiesSpawned++;
    }

    /// <summary>
    /// 获取随机生成位置
    /// </summary>
    /// <returns>生成位置</returns>
    private Vector3 GetRandomSpawnPosition()
    {
        if (player == null)
        {
            return transform.position;
        }

        Vector3 spawnPosition;
        int maxAttempts = 10;
        int attempt = 0;

        do
        {
            // 随机角度
            float angle = Random.Range(0f, Mathf.PI * 2f);
            
            // 随机距离（在最小距离和生成半径之间）
            float distance = Random.Range(minSpawnDistance, spawnRadius);
            
            // 计算位置
            float x = player.position.x + Mathf.Cos(angle) * distance;
            float z = player.position.z + Mathf.Sin(angle) * distance;
            float y = player.position.y;
            
            // 检查地图边界
            if (useMapBounds)
            {
                x = Mathf.Clamp(x, mapMinBounds.x, mapMaxBounds.x);
                z = Mathf.Clamp(z, mapMinBounds.z, mapMaxBounds.z);
            }
            
            // 检查地面高度
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, y + 50f, z), Vector3.down, out hit, 100f))
            {
                y = hit.point.y;
            }
            
            spawnPosition = new Vector3(x, y, z);
            attempt++;
        } while (!IsPositionValid(spawnPosition) && attempt < maxAttempts);
        
        return spawnPosition;
    }

    /// <summary>
    /// 检查位置是否有效
    /// </summary>
    /// <param name="position">要检查的位置</param>
    /// <returns>位置是否有效</returns>
    private bool IsPositionValid(Vector3 position)
    {
        // 检查是否在地图边界内
        if (useMapBounds)
        {
            if (position.x < mapMinBounds.x || position.x > mapMaxBounds.x)
                return false;
            if (position.z < mapMinBounds.z || position.z > mapMaxBounds.z)
                return false;
        }

        // 检查是否在玩家最小距离外
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(position, player.position);
            if (distanceToPlayer < minSpawnDistance)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 检查是否可以生成敌人
    /// </summary>
    /// <returns>是否可以生成敌人</returns>
    private bool CanSpawnEnemy()
    {
        // 检查当前敌人数量
        int currentEnemyCount = GetEnemyCount();
        return currentEnemyCount < maxEnemies;
    }

    /// <summary>
    /// 获取当前场景中的敌人数量
    /// </summary>
    /// <returns>敌人数量</returns>
    private int GetEnemyCount()
    {
        return FindObjectsOfType<EnemyBase>().Length;
    }

    /// <summary>
    /// 停止生成敌人
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
    }

    /// <summary>
    /// 开始生成敌人
    /// </summary>
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnEnemies());
        }
    }

    /// <summary>
    /// 设置生成间隔
    /// </summary>
    /// <param name="interval">生成间隔</param>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = Mathf.Max(0.1f, interval);
    }

    /// <summary>
    /// 设置最大敌人数量
    /// </summary>
    /// <param name="count">最大敌人数量</param>
    public void SetMaxEnemies(int count)
    {
        maxEnemies = Mathf.Max(1, count);
    }

    /// <summary>
    /// 设置地图边界
    /// </summary>
    /// <param name="min">最小边界</param>
    /// <param name="max">最大边界</param>
    public void SetMapBounds(Vector3 min, Vector3 max)
    {
        mapMinBounds = min;
        mapMaxBounds = max;
        useMapBounds = true;
    }

    /// <summary>
    /// 启用或禁用地图边界
    /// </summary>
    /// <param name="enable">是否启用</param>
    public void EnableMapBounds(bool enable)
    {
        useMapBounds = enable;
    }

    /// <summary>
    /// 立即生成一个敌人
    /// </summary>
    public void SpawnSingleEnemy()
    {
        if (CanSpawnEnemy())
        {
            SpawnEnemy();
        }
    }
}
