using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public ObjectiveTypewriter objectiveTypewriter;
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

    void Start()
    {
        foreach (GameObject g in BoardText)
        {
            g.SetActive(false);
        }
    
        StartCoroutine(TextColorPingPop());

        playButton.onClick.AddListener(() => StartCoroutine(PlayButtonClickEvent()));
    }


    IEnumerator PlayButtonClickEvent()
    {
        Invoke(nameof(Type), textTimeDelay);
        Invoke(nameof(BoardTextContainer), textTimeDelay + 2);
        playButton.transform.localScale = Vector3.zero;
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime * transitionTime;

            currrentCanvasPosition.localPosition = Vector3.Lerp(currrentCanvasPosition.localPosition, blackBoardPosition.localPosition, t);
            currrentCanvasPosition.localRotation = Quaternion.Lerp(currrentCanvasPosition.localRotation, blackBoardPosition.localRotation, t);

            yield return null;
                
        }


    }

    public void BoardTextContainer()
    {
        StopAllCoroutines();
        StartCoroutine(OnBoardTest());
    }

    IEnumerator OnBoardTest()
    {
        foreach (GameObject g in BoardText)
        {
            g.SetActive(true);
            yield return new WaitForSeconds(textTimeDelay + 3f);
        }
        Invoke(nameof(ExploreButtonON), 3f);
       
    }

    public void ExploreButtonON()
    {
        exploreBtn.gameObject.SetActive(true);
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

                playText.color = new Color(1, 1, 1, Mathf.Lerp(from, to, t));
                yield return null;
            }

            goingUp = !goingUp;
        }

    }

}
