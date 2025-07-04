using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectiveTypewriter : MonoBehaviour
{

    [SerializeField] private OpeningSceneController openingSceneController;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("Typing Settings")]
    [TextArea]
    [SerializeField] private string fullObjectiveText = "1. Find the laser\n2. Find the glass block\n3. Place both items on the experiment table";

    [SerializeField] private string bendingLightText;

    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] private AudioSource typingSound;
    [SerializeField] private GameObject objectiveTextGM;


    private void Start()
    {
        objectiveTextGM.SetActive(false);
    }

    public void StartTyping()
    {
        if (objectiveText != null)
        {
            StartCoroutine(TypeText());
        }
    }

    IEnumerator TypeText()
    {
        objectiveTextGM.SetActive(true);
        objectiveText.text = "";

        typingSound.Play();
        foreach (char c in fullObjectiveText)
        {
            objectiveText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingSound.Stop();
        openingSceneController.OpeningSceneFinised();
    }

    public void StartTypingBlackBoard()
    {
        StartCoroutine(BlackBoardInfoTyping());
    }


    IEnumerator BlackBoardInfoTyping()
    {
        objectiveTextGM.SetActive(true);
        

        typingSound.Play();
        foreach (char c in bendingLightText)
        {
            objectiveText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingSound.Stop();



    }

  

}