using System;
using System.Collections;
using System.Resources;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class LaserMovementController : MonoBehaviour
{
    public Outline outline;
    public float minOutline, maxOutline, outlineSpeed;
    public bool isReversed;
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

    [Header("UI References")] [SerializeField]
    private GameObject topLeftUI;
    [SerializeField] private GameObject topRightUI;
    [SerializeField] private GameObject bottomLeftUI;
    [SerializeField] GameObject bottomRightUI;

    [SerializeField] GameObject incidenceAngleImage;
    [SerializeField] GameObject incidenceRayText;
    [SerializeField] AudioSource laserSource;
    [SerializeField] AudioClip laserClip;
    bool firstTime = false;

    Color onColor = Color.green;
    Color offColor = Color.white;

    void OnEnable()
    {
        outline.OutlineWidth = 0f;
        Invoke(nameof(LaserONIndicator), 5);
    }

    public void LaserONIndicator()
    {
         StartCoroutine(StartOutline());
    }
    void Start()
    {
        incidenceAngleImage.SetActive(isLaserON);
        incidenceRayText.SetActive(isLaserON);
        bottomRightUI.transform.localScale = Vector3.zero;
        topLeftUI.transform.localScale = Vector3.zero;
        topRightUI.transform.localScale = Vector3.zero;
        bottomLeftUI.transform.localScale = Vector3.zero;

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
        targetAngle = angleSlider.value;
        angleText.text = angleSlider.value.ToString("F1") + "<sup> o</sup>";

    }




    void Update()
    {
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);
    }

    private void OnMouseDown()
    {


        //  GetComponent<BoxCollider>().enabled = false;
        if (!isLaserON)
        {
            isLaserON = true;
            laserSource.PlayOneShot(laserClip);
            incidenceAngleImage.SetActive(isLaserON);
            incidenceRayText.SetActive(isLaserON);
             FindFirstObjectByType<LaserBeamRenderer>().laserLength = 40f;
            outline.enabled = false;
            if (!firstTime)
            {
                firstTime = true;
                Invoke(nameof(LeftUI), 1f);
       

            }
        }
        else if (isLaserON)
        {
            isLaserON = false;
            incidenceAngleImage.SetActive(isLaserON);
            incidenceRayText.SetActive(isLaserON);
             FindFirstObjectByType<LaserBeamRenderer>().laserLength = 0f;
        }
        else
        {
            Debug.LogWarning("Something Fishy Going ON");
        }
        
    }

    public void LeftUI()
    {
        topLeftUI.transform.localScale = Vector3.zero;
        topLeftUI.transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
        Invoke(nameof(RightUI),.1f);
    }

    public void RightUI()
    {
        topRightUI.transform.localScale = Vector3.zero;
        topRightUI.transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
        Invoke(nameof(BottomLeftUI), .1f);
    }

    public void BottomLeftUI()
    {
        bottomLeftUI.transform.localScale = Vector3.zero;
        bottomLeftUI.transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
        Invoke(nameof(BottomRightUI), .1f);
    }
    public void BottomRightUI()
    {
        bottomRightUI.transform.localScale = Vector3.zero;
        bottomRightUI.transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
        Invoke(nameof(ImaginationVoidOpens), .5f);
    }

    public void ImaginationVoidOpens()
    {
        FindFirstObjectByType<OpeningSceneController>().PlayDialogue(OpeningSceneController.DialogueType.ImaginationBegin);
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


    IEnumerator StartOutline()
    {
        float t = 0f;
        while (true) 
        {
            // Normalize t to go from 0 to 1 over the duration of one full sine wave cycle
            t += Time.deltaTime * outlineSpeed;
            float sineValue = (Mathf.Sin(t * Mathf.PI * 2) + 1) / 2; // Converts sine wave from -1 to 1 to 0 to 1

            // Lerp between minOutline and maxOutline using the sine value
            outline.OutlineWidth = Mathf.Lerp(minOutline, maxOutline, sineValue);

            yield return null;
        }
    }
}
    
