using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class TouchIgnore : MonoBehaviour
{

    public Image[] ignoredUIImages;

    public static TouchIgnore Instance { get; private set; }

    private EventSystem eventSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        eventSystem = EventSystem.current;
    }

    public bool IsTouchOverIgnoredImage(Touch touch)
    {
        if (eventSystem == null || ignoredUIImages == null || ignoredUIImages.Length == 0)
            return false;

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = touch.position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            foreach (var img in ignoredUIImages)
            {
                if (result.gameObject == img.gameObject)
                    return true;
            }
        }

        return false;
    }
}


