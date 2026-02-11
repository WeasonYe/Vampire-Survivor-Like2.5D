using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 小怪敌人，继承自EnemyBase
/// 具有更快的移动速度，但生命值和伤害较低
/// </summary>
public class Enemy_minion : EnemyBase
{
    [Header("小怪特有设置")]
    [Tooltip("是否是群体行动")]
    [SerializeField]
    private bool isSwarm = true;

    [Tooltip("群体感知范围")]
    [SerializeField]
    private float swarmDetectionRange = 8f;

    [Tooltip("群体跟随距离")]
    [SerializeField]
    private float swarmFollowDistance = 3f;

    [Tooltip("是否会跳跃攻击")]
    [SerializeField]
    private bool canJumpAttack = true;

    [Tooltip("跳跃攻击冷却")]
    [SerializeField]
    private float jumpAttackCooldown = 3f;

    [Tooltip("跳跃攻击距离")]
    [SerializeField]
    private float jumpAttackRange = 5f;

    [Tooltip("跳跃攻击力")]
    [SerializeField]
    private float jumpForce = 5f;

    private List<Enemy_minion> nearbyMinions = new List<Enemy_minion>();
    private Enemy_minion swarmLeader = null;
    private float jumpAttackTimer = 0f;
    private bool isJumping = false;

    protected override void Awake()
    {
        base.Awake();
        
        // 小怪特有属性设置
        maxHealth = 30f;
        moveSpeed = 4.5f;
        damage = 5f;
        detectionRange = 15f;
        rotateSpeed = 250f;
    }

    protected override void Start()
    {
        base.Start();
        
        // 寻找群体
        if (isSwarm)
        {
            FindNearbyMinions();
        }
    }

    protected override void Update()
    {
        base.Update();
        
        if (isDead)
            return;
        
        // 更新跳跃攻击冷却
        UpdateJumpAttackCooldown();
        
        // 群体行为
        if (isSwarm)
        {
            HandleSwarmBehavior();
        }
        
        // 跳跃攻击
        if (canJumpAttack)
        {
            HandleJumpAttack();
        }
    }

    protected override void HandleAI()
    {
        base.HandleAI();
        
        // 小怪特有的AI行为
        if (isSwarm && swarmLeader != null)
        {
            // 跟随群体首领
            FollowSwarmLeader();
        }
    }

    /// <summary>
    /// 寻找附近的小怪
    /// </summary>
    private void FindNearbyMinions()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, swarmDetectionRange);
        nearbyMinions.Clear();
        
        foreach (Collider collider in colliders)
        {
            Enemy_minion minion = collider.GetComponent<Enemy_minion>();
            if (minion != null && minion != this && !minion.IsDead)
            {
                nearbyMinions.Add(minion);
            }
        }
        
        // 选择群体首领
        SelectSwarmLeader();
    }

    /// <summary>
    /// 选择群体首领
    /// </summary>
    private void SelectSwarmLeader()
    {
        if (nearbyMinions.Count > 0)
        {
            // 选择离玩家最近的作为首领
            Enemy_minion closestMinion = this;
            float closestDistance = GetDistanceToPlayer();
            
            foreach (Enemy_minion minion in nearbyMinions)
            {
                float distance = minion.GetDistanceToPlayer();
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestMinion = minion;
                }
            }
            
            swarmLeader = closestMinion;
        }
        else
        {
            swarmLeader = this;
        }
    }

    /// <summary>
    /// 处理群体行为
    /// </summary>
    private void HandleSwarmBehavior()
    {
        // 定期更新附近的小怪
        if (Time.frameCount % 60 == 0) // 每60帧更新一次
        {
            FindNearbyMinions();
        }
    }

    /// <summary>
    /// 跟随群体首领
    /// </summary>
    private void FollowSwarmLeader()
    {
        if (swarmLeader != null && swarmLeader != this && !swarmLeader.IsDead)
        {
            float distanceToLeader = Vector3.Distance(transform.position, swarmLeader.transform.position);
            
            if (distanceToLeader > swarmFollowDistance)
            {
                // 向首领移动
                Vector3 direction = (swarmLeader.transform.position - transform.position).normalized;
                direction.y = 0;
                
                // 计算移动向量
                Vector3 movement = direction * moveSpeed * Time.fixedDeltaTime * 0.7f; // 稍微慢一点
                rigidbody.MovePosition(rigidbody.position + movement);
                
                // 朝向首领
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
                }
            }
        }
    }

    /// <summary>
    /// 更新跳跃攻击冷却
    /// </summary>
    private void UpdateJumpAttackCooldown()
    {
        if (jumpAttackTimer > 0)
        {
            jumpAttackTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 处理跳跃攻击
    /// </summary>
    private void HandleJumpAttack()
    {
        if (player == null || isJumping || jumpAttackTimer > 0)
            return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 检查是否可以跳跃攻击
        if (distanceToPlayer <= jumpAttackRange)
        {
            JumpAttack();
        }
    }

    /// <summary>
    /// 跳跃攻击
    /// </summary>
    private void JumpAttack()
    {
        if (player == null || isJumping || jumpAttackTimer > 0)
            return;
        
        isJumping = true;
        jumpAttackTimer = jumpAttackCooldown;
        
        // 计算跳跃方向
        Vector3 jumpDirection = (player.position - transform.position).normalized;
        jumpDirection.y = 0.5f; // 添加向上的力
        
        // 应用跳跃力
        rigidbody.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
        
        // 重置跳跃状态
        StartCoroutine(ResetJumpState());
    }

    /// <summary>
    /// 重置跳跃状态
    /// </summary>
    private IEnumerator ResetJumpState()
    {
        yield return new WaitForSeconds(1f);
        isJumping = false;
    }

    protected override void Die()
    {
        base.Die();
        
        // 群体死亡处理
        if (isSwarm && swarmLeader == this)
        {
            // 重新选择首领
            foreach (Enemy_minion minion in nearbyMinions)
            {
                if (!minion.IsDead)
                {
                    minion.FindNearbyMinions();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 获取附近的小怪数量
    /// </summary>
    /// <returns>小怪数量</returns>
    public int GetNearbyMinionCount()
    {
        return nearbyMinions.Count;
    }

    /// <summary>
    /// 检查是否是群体首领
    /// </summary>
    /// <returns>是否是首领</returns>
    public bool IsSwarmLeader()
    {
        return swarmLeader == this;
    }
}
