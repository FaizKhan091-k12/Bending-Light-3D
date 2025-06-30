using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class LaserMovementController : MonoBehaviour
{
    public bool isLaserON = false;
    public bool isDragging = false;
    public float rotationSpeed = 2.0f; 
    public float startAngle;
    public Vector3 dragStartPos;

    public float targetAngle = 45f;
    public float currentAngle = 45f;

    [SerializeField] private float smoothSpeed = 8f; 

    [SerializeField] Slider angleSlider;
    [SerializeField] TextMeshProUGUI angleText;
    [SerializeField] Toggle showAngleToggle;
    [SerializeField] Toggle showNormalsToggle;
    
    [SerializeField] Image showAngleToggleImage;
    [SerializeField] Image showNormalsToggleImage;

    [SerializeField] private GameObject normals;
    [SerializeField] private GameObject[] angles;
    

  
    

    
    Color onColor = Color.green;
    Color offColor = Color.white;
    
    void Start()
    {

        AngleSliderControlls();
        showAngleToggle.onValueChanged.AddListener(delegate
        {
            if (showAngleToggle.isOn)
            {
                showAngleToggleImage.color = onColor;
                foreach (GameObject angle in angles)
                {
                    angle.SetActive(true);
                }
            }
            else
            {

                foreach (GameObject angle in angles)
                {
                    angle.SetActive(false);
                }
                showAngleToggleImage.color = offColor;


            }
        });
        
        
        showNormalsToggle.onValueChanged.AddListener(delegate
        {
            if (showNormalsToggle.isOn)
            {
                normals.SetActive(true);
                showNormalsToggleImage.color = onColor;

            }
            else
            {
                normals.SetActive(false);
                showNormalsToggleImage.color = offColor;


            }
        });
        angleSlider.onValueChanged.AddListener(delegate { AngleSliderControlls(); });
        currentAngle = transform.eulerAngles.y;
        targetAngle = currentAngle;
    }

   
    private void AngleSliderControlls()
    {
        targetAngle = angleSlider.value; angleText.text = angleSlider.value.ToString("F1") + "<sup> o</sup>";
      
    }

    void LaserActivator()
    {
        if (!isLaserON)
        {
            
        }
    }
    
  
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
           
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            UpdateTargetAngle();
        }

        // Smoothly interpolate to the target angle
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);
    }

    void TryStartDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.GetComponentInParent<LaserMovementController>() == this)
            {
                
                dragStartPos = Input.mousePosition;
                startAngle = currentAngle; 
            }
        }
    }

    void UpdateTargetAngle()
    {
        Vector3 dragDelta = Input.mousePosition - dragStartPos;
        float angleDelta = dragDelta.x * rotationSpeed;
        targetAngle = startAngle + angleDelta;
    }
}