using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using MirzaBeig.CinematicExplosionsFree;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;
    public ObjectiveTypewriter objectiveTypewriter;
    public OpeningSceneController openingSceneController;
    [Header("Play Menu")]
    [SerializeField] Button playButton;
    [SerializeField] TextMeshProUGUI playText;
    [SerializeField] float minAlpha;
    [SerializeField] float maxAlpha;
    [SerializeField] float alphasmoothTime;

    [Header("Board Menu")]
    [SerializeField] Transform currrentCanvasPosition;
    [SerializeField] Transform blackBoardPosition;
    [SerializeField] float transitionTime;
    [SerializeField] float textTimeDelay;

    [Header("Board Typing Text")]

    [SerializeField] GameObject[] BoardText;

    [SerializeField] Button exploreBtn;

    [SerializeField] GameObject inGameCanvas;
    [SerializeField] ProceduralImage fadingImage;
    [SerializeField] GameObject offSchoolAudio;
    [SerializeField] GameObject fpsCamera;
    [SerializeField] GameObject canvasCamera;
    [SerializeField] public bool[] boardTextComplete;
    [SerializeField] public bool allTextCompleted;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        inGameCanvas.SetActive(false);
        foreach (GameObject g in BoardText)
        {
            g.SetActive(false);
        }

        StartCoroutine(TextColorPingPop());

        playButton.onClick.AddListener(() => StartCoroutine(PlayButtonClickEvent()));
      //  exploreBtn.onClick.AddListener(ExploreButtonClicked);
    }

    public void ExploreButtonClicked()
    {
      
        StopAllCoroutines();
        inGameCanvas.SetActive(true);
        fadingImage.gameObject.SetActive(true);
        StartCoroutine(FadingImage());
    }

    private void Update()
    {
        if (allTextCompleted) return;
        if (boardTextComplete[0] == true)
        {
            BoardText[0].SetActive(true);
            StartCoroutine(ExploreButtonScalePoping());

        }
        if (boardTextComplete[1] == true)
        {
            BoardText[1].SetActive(true);
        }
        if (boardTextComplete[2] == true)
        {
            BoardText[2].SetActive(true);
        }
        if (boardTextComplete[3] == true)
        {
            BoardText[3].SetActive(true);
        }
        if (boardTextComplete[4] == true)
        {
            ExploreButtonON();
        }
        

      
        
        
    }

    IEnumerator FadingImage()
    {
        Color color = fadingImage.color;
        float t = 0f;

        // yield return new WaitForSeconds(0.2f);
        while (t < 1)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / 1);
            fadingImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        Debug.Log("Start");
        openingSceneController.enabled = true;
        offSchoolAudio.gameObject.SetActive(true);
        fpsCamera.SetActive(true);
        canvasCamera.SetActive(false);
    }

    IEnumerator PlayButtonClickEvent()
    {


        playButton.transform.localScale = Vector3.zero;

        float t = 0f;
        Vector3 startPos = currrentCanvasPosition.localPosition;
        Quaternion startRot = currrentCanvasPosition.localRotation;

        Vector3 endPos = blackBoardPosition.localPosition;
        Quaternion endRot = blackBoardPosition.localRotation;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionTime;

            currrentCanvasPosition.localPosition = Vector3.Lerp(startPos, endPos, t);
            currrentCanvasPosition.localRotation = Quaternion.Lerp(startRot, endRot, t);

            yield return new WaitForEndOfFrame();
        }
        Invoke(nameof(Type), textTimeDelay);
        

    }


    public void BoardTextContainer()
    {
        StopAllCoroutines();

    }

    void  OnBoardTest()
    {
       
        Invoke(nameof(ExploreButtonON), 3f);

    }



    public void ExploreButtonON()
    {
        allTextCompleted = true;
        exploreBtn.gameObject.SetActive(true);

      //  StopAllCoroutines();
       //StartCoroutine(ExploreButtonScalePoping());
    }
    public void Type()
    {
        objectiveTypewriter.StartTypingBlackBoard();
    }
    IEnumerator TextColorPingPop()
    {
        float t = 0f;
        bool goingUp = true;

        while (true)
        {
            t = 0f;

            float from = goingUp ? minAlpha : maxAlpha;
            float to = goingUp ? maxAlpha : minAlpha;

            while (t < 1f)
            {
                t += Time.deltaTime * alphasmoothTime;
 float smoothT = Mathf.SmoothStep(0, 1, t);
                playText.color = new Color(1, 1, 1, Mathf.Lerp(from, to, smoothT ));
                yield return null;
            }

            goingUp = !goingUp;
        }

    }

    IEnumerator ExploreButtonScalePoping()
    {
        float t = 0f;
        bool goingUp = true;
        
        while (true)
        {
            t = 0f;

            Vector3 from = goingUp ? new Vector3(.8f, .8f, .8f) : new Vector3(1.2f, 1.2f, 1.2f);
            Vector3 to = goingUp ? new Vector3(1.2f, 1.2f, 1.2f) : new Vector3(.8f, .8f, .8f);

            while (t < 1f)
            {
                t += Time.deltaTime * alphasmoothTime;
                       float smoothT = Mathf.SmoothStep(0, 1, t);

                exploreBtn.transform.localScale = Vector3.Lerp(from, to, smoothT);
                yield return null;
            }

            goingUp = !goingUp;
        }

    }
}
