using System.Collections;
using TMPro;
using UnityEngine;

public class TextWriter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private AudioSource typingSound;

    [Header("Typing Settings")]
    [TextArea(3, 10)]
    [SerializeField] private string richTextToWrite;
    [SerializeField] private float typingSpeed = 0.04f;


    public void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(TypeRichText(richTextToWrite));
    }

    IEnumerator TypeRichText(string fullText)
    {
      objectiveText.gameObject.SetActive(true); 
        objectiveText.text = "";

        typingSound.Play();

        int i = 0;
        while (i < fullText.Length)
        {
            // Handle rich text tag (skip typing each character in tag)
            if (fullText[i] == '<')
            {
                int tagCloseIndex = fullText.IndexOf('>', i);
                if (tagCloseIndex != -1)
                {
                    string tag = fullText.Substring(i, tagCloseIndex - i + 1);
                    objectiveText.text += tag;
                    i = tagCloseIndex + 1;
                    continue;
                }
            }

            // Normal character
            objectiveText.text += fullText[i];
            i++;

            yield return new WaitForSeconds(typingSpeed);
        }

        typingSound.Stop();
    }
}
