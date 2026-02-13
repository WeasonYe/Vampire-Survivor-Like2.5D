using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家控制器，负责处理玩家的移动输入和控制
/// 支持键盘、手柄和触屏操作
/// </summary>
public class PlayerController : Singleton<PlayerController>
{
    [Header("移动设置")]
    [Tooltip("移动速度")]
    [SerializeField]
    private float moveSpeed = 5f;

    [Tooltip("冲刺速度倍数")]
    [SerializeField]
    private float sprintMultiplier = 2f;

    [Tooltip("旋转速度")]
    [SerializeField]
    private float rotateSpeed = 180f;

    [Header("输入设置")]
    [Tooltip("是否启用键盘输入")]
    [SerializeField]
    private bool useKeyboardInput = true;

    [Tooltip("是否启用手柄输入")]
    [SerializeField]
    private bool useControllerInput = true;

    [Tooltip("是否启用触屏输入")]
    [SerializeField]
    private bool useTouchInput = true;

    [Header("触屏设置")]
    [Tooltip("触屏死区大小")]
    [SerializeField]
    private float touchDeadZone = 0.1f;

    [Tooltip("触屏移动灵敏度")]
    [SerializeField]
    private float touchSensitivity = 1f;

    [Header("物理设置")]
    [Tooltip("是否使用CharacterController组件")]
    [SerializeField]
    private bool useCharacterController = true;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private Vector2 touchStartPosition;
    private Vector2 touchCurrentPosition;
    private bool isTouching = false;
    private float currentSpeed;

    /// <summary>
    /// 玩家是否正在移动
    /// </summary>
    public bool IsMoving { get; private set; }

    /// <summary>
    /// 玩家是否正在冲刺
    /// </summary>
    public bool IsSprinting { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        
        // 获取CharacterController组件
        characterController = GetComponent<CharacterController>();
        
        // 初始化当前速度
        currentSpeed = moveSpeed;
    }

    void Start()
    {
        // 如果没有CharacterController，尝试获取或添加
        if (useCharacterController && characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
            }
        }
    }

    void Update()
    {
        // 处理输入
        HandleInput();
        
        // 处理移动
        HandleMovement();
    }

    /// <summary>
    /// 处理玩家输入
    /// </summary>
    private void HandleInput()
    {
        Vector3 inputDirection = Vector3.zero;
        
        // 处理键盘输入
        if (useKeyboardInput)
        {
            inputDirection += GetKeyboardInput();
        }
        
        // 处理手柄输入
        if (useControllerInput)
        {
            inputDirection += GetControllerInput();
        }
        
        // 处理触屏输入
        if (useTouchInput)
        {
            inputDirection += GetTouchInput();
        }
        
        // 归一化输入方向，确保各方向移动速度一致
        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }
        
        // 更新移动方向
        moveDirection = inputDirection;
        
        // 检查是否正在移动
        IsMoving = moveDirection.magnitude > 0.1f;
        
        // 处理冲刺输入
        HandleSprintInput();
    }

    /// <summary>
    /// 获取键盘输入
    /// </summary>
    /// <returns>输入方向向量</returns>
    private Vector3 GetKeyboardInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        return new Vector3(horizontal, 0f, vertical);
    }

    /// <summary>
    /// 获取手柄输入
    /// </summary>
    /// <returns>输入方向向量</returns>
    private Vector3 GetControllerInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // 检查是否是从手柄输入的
        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            return new Vector3(horizontal, 0f, vertical);
        }
        
        return Vector3.zero;
    }

    /// <summary>
    /// 获取触屏输入
    /// </summary>
    /// <returns>输入方向向量</returns>
    private Vector3 GetTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    isTouching = true;
                    break;
                    
                case TouchPhase.Moved:
                    touchCurrentPosition = touch.position;
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
            
            if (isTouching)
            {
                Vector2 touchDelta = (touchCurrentPosition - touchStartPosition) * touchSensitivity * 0.01f;
                
                // 检查是否超过死区
                if (touchDelta.magnitude > touchDeadZone)
                {
                    return new Vector3(touchDelta.x, 0f, touchDelta.y);
                }
            }
        }
        
        return Vector3.zero;
    }

    /// <summary>
    /// 处理冲刺输入
    /// </summary>
    private void HandleSprintInput()
    {
        // 检查冲刺输入
        bool sprintInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetAxisRaw("Fire3") > 0.5f;
        
        IsSprinting = sprintInput && IsMoving;
        
        // 更新当前速度
        if (IsSprinting)
        {
            currentSpeed = moveSpeed * sprintMultiplier;
        }
        else
        {
            currentSpeed = moveSpeed;
        }
    }

    /// <summary>
    /// 处理玩家移动
    /// </summary>
    private void HandleMovement()
    {
        if (moveDirection.magnitude < 0.1f)
        {
            return;
        }
        
        // 计算移动向量
        Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
        
        // 旋转玩家朝向移动方向
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        
        // 使用CharacterController移动
        if (useCharacterController && characterController != null)
        {
            characterController.Move(movement);
        }
        else
        {
            // 直接修改位置
            transform.position += movement;
        }
    }

    /// <summary>
    /// 设置移动速度
    /// </summary>
    /// <param name="speed">新的移动速度</param>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    /// <summary>
    /// 设置冲刺倍数
    /// </summary>
    /// <param name="multiplier">新的冲刺倍数</param>
    public void SetSprintMultiplier(float multiplier)
    {
        sprintMultiplier = Mathf.Max(1f, multiplier);
    }

    /// <summary>
    /// 启用或禁用特定输入类型
    /// </summary>
    /// <param name="keyboard">是否启用键盘输入</param>
    /// <param name="controller">是否启用手柄输入</param>
    /// <param name="touch">是否启用触屏输入</param>
    public void SetInputTypes(bool keyboard, bool controller, bool touch)
    {
        useKeyboardInput = keyboard;
        useControllerInput = controller;
        useTouchInput = touch;
    }

    /// <summary>
    /// 立即停止移动
    /// </summary>
    public void StopMovement()
    {
        moveDirection = Vector3.zero;
        IsMoving = false;
        IsSprinting = false;
    }

    /// <summary>
    /// 强制移动玩家到指定位置
    /// </summary>
    /// <param name="position">目标位置</param>
    public void TeleportTo(Vector3 position)
    {
        if (characterController != null)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }
        else
        {
            transform.position = position;
        }
    }
}
