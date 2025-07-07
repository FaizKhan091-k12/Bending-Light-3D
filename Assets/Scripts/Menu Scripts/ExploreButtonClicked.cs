using UnityEngine;

public class ExploreButtonClicked : MonoBehaviour
{

    [SerializeField] bool onlyClickOnce = false;
    [SerializeField] GameObject exploreBtn;
    void OnMouseUpAsButton()
    {
        if (!onlyClickOnce)
        {

            onlyClickOnce = true;
            MainMenu.Instance.ExploreButtonClicked();
            Invoke(nameof(ExploreOFF), 1);
        }
    }

    public void ExploreOFF()
    {
          exploreBtn.gameObject.SetActive(false);
    }
}
