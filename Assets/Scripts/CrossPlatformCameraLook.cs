using UnityEngine;
using UnityEngine.EventSystems;

public class CrossPlatformCameraLook : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float touchSensitivity = 0.1f;

    [Header("Clamp Settings")]
    [SerializeField] private float clampAngle = 80f;

    [Header("References")]
    [SerializeField] private Transform playerBody; // Assign to your player’s body (usually parent of camera)

    private float verticalRotation = 0f;
    private Vector2 touchDelta;

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
#if UNITY_WEBGL
        if (Input.GetMouseButtonDown(0)) // Required to lock pointer in WebGL
        {
            LockCursor();
        }
#endif

#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
        HandleTouchLook();
#else
        HandleMouseLook();
#endif
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        if (playerBody)
            playerBody.Rotate(Vector3.up * mouseX);
    }

    void HandleTouchLook()
    {
        if (Input.touchCount == 1 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition * touchSensitivity;

                verticalRotation -= delta.y;
                verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

                transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
                if (playerBody)
                    playerBody.Rotate(Vector3.up * delta.x);
            }
        }
    }

    public void LockCursor()
    {
#if UNITY_WEBGL
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#elif UNITY_STANDALONE || UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
