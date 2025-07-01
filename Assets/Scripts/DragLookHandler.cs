using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragLookHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Look Settings")]
    public Transform playerBody;
    public Transform cameraTransform;
    public float sensitivityX = 2f;
    public float sensitivityY = 2f;
    public float verticalClamp = 80f;

    private bool isDragging = false;
    private float xRotation = 0f;

    public Slider slider;


    void Start()
    {
        slider.onValueChanged.AddListener(delegate { sensitivityX = slider.value; sensitivityY = slider.value; });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || cameraTransform == null || playerBody == null)
            return;

#if UNITY_WEBGL || UNITY_STANDALONE || UNITY_EDITOR
        float deltaX = eventData.delta.x;
        float deltaY = eventData.delta.y;
#else
        float deltaX = eventData.delta.x / Screen.width * 1000f;
        float deltaY = eventData.delta.y / Screen.height * 1000f;
#endif

        float mouseX = deltaX * sensitivityX * Time.deltaTime;
        float mouseY = deltaY * sensitivityY * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}