using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using DG.Tweening;
using System.Diagnostics.Contracts;

public class LaserMenuWalkThrough : MonoBehaviour
{
    [SerializeField] float speed, minAlpha, maxAlpha;
    [SerializeField] ProceduralImage walkThroughImage;
    [SerializeField] float clickCount;

    [Header("Laser Settings Panel")]
    private Coroutine angleSliderRoutine;
    private Coroutine laserColorRoutine;
    [SerializeField] TextMeshProUGUI laserPanelText;
    [SerializeField] GameObject handIcon;
    [SerializeField] Slider angleSlider;
    [SerializeField] Slider laserColorSlider;
    [SerializeField] float SliderSpeed;

    [TextArea]
    public string texttoChange;

    [Header("Left Panel Settings")]
    public Coroutine leftPanelMovementRoutine;
    [SerializeField] TextMeshProUGUI leftPanelInfoText;
    [SerializeField] TextMeshProUGUI tapHereText;
    public bool first;

    [Header("Right Panel Settings")]
    [SerializeField] TextMeshProUGUI rightPanelInfoText;
    bool firstRight;


    [Header("Bottom Right Panel Settings")]
    [SerializeField] TextMeshProUGUI intensityInfoText;
    [SerializeField] TextMeshProUGUI happyLearning;

    void Awake()
    {
        laserPanelText.gameObject.SetActive(false);
        leftPanelInfoText.gameObject.SetActive(false);
        rightPanelInfoText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        StartCoroutine(ImageAlphaController());
        StartCoroutine(LaserPanelText());
        StartCoroutine(TapTextColorRoutine());
        handIcon.SetActive(true);
    }



    IEnumerator ImageAlphaController()
    {
        float t = 0f;

        while (true)
        {

            t += Time.deltaTime * speed;

            float sinTime = (Mathf.Sin(t) + 1) / 2f;
            walkThroughImage.color = new Color(0, 0, 0, Mathf.Lerp(minAlpha, maxAlpha, sinTime));

            yield return null;

        }
    }


    IEnumerator LaserPanelText()
    {
        laserPanelText.gameObject.SetActive(true);
        angleSliderRoutine = StartCoroutine(AngleSliderWalk());
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * speed;
            float sinTime = (Mathf.Sin(t) + 1) / 2f;
            Vector3 tempPos = laserPanelText.rectTransform.anchoredPosition;

            tempPos.y = Mathf.Lerp(0, 35, sinTime);
            laserPanelText.rectTransform.anchoredPosition = tempPos;

            yield return null;

        }

    }



    IEnumerator AngleSliderWalk()
    {

        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * SliderSpeed;

            float sinTime = Mathf.PingPong(t, 1f);

            angleSlider.value = Mathf.Lerp(angleSlider.minValue, angleSlider.maxValue, sinTime);



            yield return null;

        }

    }
    public void AngleSliderWalkComplete()
    {
        if (angleSliderRoutine != null)
        {
            StopCoroutine(angleSliderRoutine);
            angleSliderRoutine = null;
            Debug.Log("Slider walk stopped!");
        }

        laserColorRoutine = StartCoroutine(LasercolorWalk());
        laserPanelText.transform.localScale = Vector3.zero;
        laserPanelText.transform.DOScale(Vector3.one, 2f).SetEase(Ease.OutBack);
        laserPanelText.text = texttoChange;
    }


    IEnumerator LasercolorWalk()
    {

        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * SliderSpeed;

            float sinTime = Mathf.PingPong(t, 1f);

            laserColorSlider.value = Mathf.Lerp(laserColorSlider.minValue, laserColorSlider.maxValue, sinTime);



            yield return null;

        }

    }

    public void LaserColorWalkComplete()
    {
        if (laserColorRoutine != null)
        {
            StopCoroutine(laserColorRoutine);
            laserColorRoutine = null;
            Debug.Log("Laser walk stopped!");
        }

        laserPanelText.transform.localScale = Vector3.zero;
        laserPanelText.transform.DOScale(Vector3.one, 2f).SetEase(Ease.OutBack);
        laserPanelText.text = "Tap anywhere on the screen...";
    }

    public void ClickCounter()
    {
        Transform target = gameObject.transform;
        int index = target.GetSiblingIndex();

        if (index > 0)
        {
            target.SetSiblingIndex(index - 1);
        }
        clickCount++;
        switch (clickCount)
        {
            case 1:

                LeftPanelHasBeenActivated();

                break;
            case 2:

                Debug.Log(clickCount);
                tapHereText.gameObject.SetActive(false);
                walkThroughImage.raycastTarget = false;
                rightPanelInfoText.gameObject.SetActive(true);

                break;

            case 3:

                Debug.Log(clickCount);
                walkThroughImage.raycastTarget = false;
                tapHereText.gameObject.SetActive(false);
                intensityInfoText.gameObject.SetActive(true);
                StartCoroutine(IntensityTextUpDownRoutine());
                Invoke(nameof(WalkThroughImageRaycastON), 3f);

                break;
            case 4:
                Debug.Log(clickCount);
                tapHereText.gameObject.SetActive(false);
                intensityInfoText.gameObject.SetActive(false);
                walkThroughImage.raycastTarget = false;
                happyLearning.gameObject.SetActive(true);
                StartCoroutine(HappyFading());
                break;
            case 5:
                walkThroughImage.raycastTarget = false;
                tapHereText.gameObject.SetActive(false);
                Debug.Log(clickCount);
                break;
            case 6:

                Debug.Log(clickCount);
                break;
        }

    }


    public void LeftPanelHasBeenActivated()
    {

        laserPanelText.gameObject.SetActive(false);
        walkThroughImage.raycastTarget = false;
        leftPanelInfoText.gameObject.SetActive(true);

        leftPanelMovementRoutine = StartCoroutine(LeftPanelTextMovement());
    }


    IEnumerator LeftPanelTextMovement()
    {


        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * speed;
            float sinTime = (Mathf.Sin(t) + 1) / 2f;
            Vector3 tempPos = leftPanelInfoText.rectTransform.anchoredPosition;

            tempPos.y = Mathf.Lerp(0, -30, sinTime);
            leftPanelInfoText.rectTransform.anchoredPosition = tempPos;

            yield return null;

        }
    }

    IEnumerator TapTextColorRoutine()
    {


        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * speed;
            float sinTime = (Mathf.Sin(t) + 1) / 2f;
            Color tempPos = tapHereText.color;

            tempPos.a = Mathf.Lerp(0.2f, 1, sinTime);
            tapHereText.color = tempPos;

            yield return null;

        }
    }

    public void LeftDropDownClicked(GameObject tapleftPanelTxt)
    {
        if (!first)
        {
            first = true;
            tapHereText.gameObject.SetActive(true);
            tapleftPanelTxt.SetActive(false);
            walkThroughImage.raycastTarget = true;

        }
        else
        {
            return;
        }
    }


    public void RightDownClicked()
    {
        if (!firstRight)
        {
            firstRight = true;
            rightPanelInfoText.gameObject.SetActive(false);
            tapHereText.gameObject.SetActive(true);
            walkThroughImage.raycastTarget = true;
        }

    }

    public void WalkThroughImageRaycastON()
    {
        walkThroughImage.raycastTarget = true;
        tapHereText.gameObject.SetActive(true);

    }


    IEnumerator IntensityTextUpDownRoutine()
    {


        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * speed;
            float sinTime = (Mathf.Sin(t) + 1) / 2f;
            Vector3 tempPos = intensityInfoText.rectTransform.anchoredPosition;

            tempPos.y = Mathf.Lerp(0, 30, sinTime);
            intensityInfoText.rectTransform.anchoredPosition = tempPos;

            yield return null;

        }
    }

    IEnumerator HappyFading()
    {
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * .55f;
            Color tempColor = happyLearning.color;

            tempColor.a = Mathf.Lerp(1, 0, t);
            happyLearning.color = tempColor;
            yield return null;
        }
        walkThroughImage.gameObject.SetActive(false);
    }

}
