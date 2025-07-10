using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AutoOutline : MonoBehaviour
{
    [SerializeField] float from;
    [SerializeField] float to;
    [SerializeField] float speed;

    public Outline outline;
   public  Color outlineColor;


    public Outline.Mode Outlinemode;

    void Awake()
    {
        gameObject.AddComponent<Outline>();
   

    }
    void Start()
    {
        outline.OutlineColor = outlineColor;
        outline.OutlineMode = Outlinemode;
    }                                                                                                   
    void OnEnable()
    {
        outline = GetComponent<Outline>();
        StartCoroutine(StartOutline());
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        StopAllCoroutines();
        outline.OutlineWidth = 0f;
    }
    IEnumerator StartOutline()
    {

        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * speed;
            float sinTime = (Mathf.Sin(t) + 1) / 2f;

            outline.OutlineWidth = Mathf.Lerp(from, to, sinTime);
            yield return null; 


        }
    }


}
