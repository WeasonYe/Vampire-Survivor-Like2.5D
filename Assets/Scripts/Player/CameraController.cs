using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    [Header("Target Settings")]
    [SerializeField]
    private Transform target;

    [Header("Camera Position Settings")]
    [SerializeField]
    private Vector3 offset = new Vector3(0f, 10f, -10f);

    [SerializeField]
    private float lookAtOffsetY = 1f;

    [Header("Movement Settings")]
    [SerializeField]
    private float smoothSpeed = 5f;

    [SerializeField]
    private bool useSmoothFollow = true;

    [Header("Zoom Settings")]
    [SerializeField]
    private float minZoom = 5f;

    [SerializeField]
    private float maxZoom = 20f;

    [SerializeField]
    private float zoomSpeed = 2f;

    [SerializeField]
    private float currentZoom = 10f;

    [Header("Camera Bounds")]
    [SerializeField]
    private bool useBounds = false;

    [SerializeField]
    private Vector2 minBounds = new Vector2(-50f, -50f);

    [SerializeField]
    private Vector2 maxBounds = new Vector2(50f, 50f);

    private Camera mainCamera;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Start()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        currentZoom = offset.magnitude;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        HandleZoom();
        FollowTarget();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            Vector3 direction = offset.normalized;
            offset = direction * currentZoom;
        }
    }

    private void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);
        }

        Vector3 smoothedPosition;

        if (useSmoothFollow)
        {
            smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            smoothedPosition = desiredPosition;
        }

        transform.position = smoothedPosition;
        transform.LookAt(target.position + Vector3.up * lookAtOffsetY);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetZoom(float zoomLevel)
    {
        currentZoom = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
        Vector3 direction = offset.normalized;
        offset = direction * currentZoom;
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        useBounds = true;
    }

    public void EnableBounds(bool enable)
    {
        useBounds = enable;
    }

    public void SetSmoothFollow(bool enable)
    {
        useSmoothFollow = enable;
    }

    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = Mathf.Max(0.1f, speed);
    }
}
