using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Help : MonoBehaviour
{
    [SerializeField] GameObject helpText;

    [SerializeField] Button helpBtn;



    void Start()
    {
        helpBtn.onClick.AddListener(HelpBtnPressed);
    }
    public void HelpBtnPressed()
    {
        StopAllCoroutines();
        helpBtn.interactable = false;
        StartCoroutine(helpButtonText());
        Invoke(nameof(HelpTextBack), 2f);
    }

    IEnumerator helpButtonText()
    {

        float t = 0f;

        while (t < 1)
        {

            t += Time.deltaTime * .2f;

            helpText.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, Mathf.Lerp(0, 1, t));


            yield return null;


        }
    }

    public void HelpTextBack()
    {
        StopAllCoroutines();
        StartCoroutine(helpButtonTextAlphaBack());
    }
    IEnumerator helpButtonTextAlphaBack()
    {

        float t = 0f;

        while (t < 1)
        {

            t += Time.deltaTime * .2f;

            helpText.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, Mathf.Lerp(1, 0, t));


            yield return null;


        }
        helpBtn.interactable = true;
    }

}
