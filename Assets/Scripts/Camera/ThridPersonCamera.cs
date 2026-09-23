using UnityEngine;
using UnityEngine.EventSystems;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private float targetHeight = 2f;

    [Header("Camera Movement")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float mouseSensitivity = 4f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 60f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private float collisionPadding = 0.15f;
    [SerializeField] private float minimumDistance = 0.6f;

    private float yaw;
    private float pitch = 15f;

    private void Start()
    {
        yaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        RotateCamera();
    }

    private void LateUpdate()
    {
        FollowTarget();
    }

    private void RotateCamera()
    {
        bool pointerOverUI =
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();

        if (Input.GetMouseButton(0) && !pointerOverUI)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }

    private void FollowTarget()
    {
        if (target == null)
        {
            return;
        }

        Vector3 focusPoint = target.position + Vector3.up * targetHeight;
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 directionFromTarget =
            -(cameraRotation * Vector3.forward);

        float currentDistance = GetCollisionDistance(
            focusPoint,
            directionFromTarget
        );

        transform.position =
            focusPoint + directionFromTarget * currentDistance;

        transform.rotation = cameraRotation;
    }

    private float GetCollisionDistance(
        Vector3 focusPoint,
        Vector3 directionFromTarget
    )
    {
        if (Physics.SphereCast(
            focusPoint,
            collisionRadius,
            directionFromTarget,
            out RaycastHit hit,
            distance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(
                hit.distance - collisionPadding,
                minimumDistance
            );
        }

        return distance;
    }
}