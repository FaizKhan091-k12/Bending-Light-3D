using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.UI.ProceduralImage;

public class PlayerInteractable : MonoBehaviour
{

    public static PlayerInteractable Instance;
    [Header("Debug Options")][SerializeField]
    private bool lockMouseState;

    public bool bothObjectiveCompleted = false;

    [Space(10)]
    public OpeningSceneController openingSceneController;
    public InitiateExperiment initiateExperiment;

    [Header("RayCast Refrences")]
    [SerializeField] Camera cam;
    [SerializeField] float rayMaxDistance;

    [Header("Pickup Refrences")]
    [SerializeField] Button handIconPickupButton;
    [SerializeField] Button startExpButton;
    [SerializeField] Transform pickupLocations;
    [SerializeField] Transform GlassDropLocation;
    [SerializeField] Transform laserDropLocation;

    [Header("Item Setting")]
    [SerializeField] public bool itemInHand = false;
    [SerializeField] Transform player;
    [SerializeField] Transform experimentTable;
    [SerializeField] float itemDropDistance;
    [SerializeField] float pickAndDropSpeed;
    [SerializeField] KeyCode pickupKey = KeyCode.E;

    [Header("Objective Complete")]
    [SerializeField] GameObject objectiveComplete1;
    [SerializeField] GameObject objectiveComplete2;
    [SerializeField] GameObject objectiveComplete3;

    [SerializeField] private GameObject InstructionPanel;
    [SerializeField] TextMeshProUGUI instructionText;

    [Header("Start Experiment Refrences")]
    [SerializeField] ProceduralImage imageToFadeIn;
    [SerializeField] float fadeDuration;
    [SerializeField] GameObject experimentCam;
    [SerializeField] GameObject playerCam;
    [SerializeField] AudioSource[] startingAudioSources;
    [SerializeField] AudioSource experimentAudioSources;
    [SerializeField] GameObject City;
    [SerializeField] GameObject boardCanvas;

    [SerializeField] GameObject[] turnOffAllTheUnusedAssets;
    Interactable currentInteractable;
    float t = 0f;
    bool glassHasPickedUp = false;
    bool laserHasPickedUp = false;
    public bool startExperimentKeyIndicator = false;
    public Button backButton;
    public bool InLaserMode;



    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        backButton.onClick.AddListener(delegate { lockMouseState = true; });

        InstructionPanel.SetActive(false);
        handIconPickupButton.gameObject.SetActive(false);

    }

    private void Update()
    {
        MouseStateController();
        if (!startExperimentKeyIndicator)
        {
            if (!itemInHand && !bothObjectiveCompleted)
            {
                // RayCastItems();
                CheckNearbyInteractables();
            }
            else
            {

                if (bothObjectiveCompleted) return;
                CalculateDistancebtwTableNPlayer();
                //InstructionPanel.SetActive(false);
                startExpButton.gameObject.SetActive(false);


            }


        }
        else
        {
            //  InstructionPanel.SetActive(true);
            startExpButton.gameObject.SetActive(true);
            handIconPickupButton.gameObject.SetActive(false);
        }

        if (startExperimentKeyIndicator)
        {
            if (Input.GetKeyDown(pickupKey))
            {
                Debug.Log("Star");
                handIconPickupButton.interactable = false;
                StartExperiment();
                startExperimentKeyIndicator = false;
            }

        }

        if (InLaserMode)
        {
            lockMouseState = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        if (handIconPickupButton.gameObject.activeInHierarchy || startExpButton.gameObject.activeInHierarchy)
        {

            InstructionPanel.SetActive(true);

        }
        else
        {
            InstructionPanel.SetActive(false);

        }

#if UNITY_WEBGL || UNITY_STANDALONE || UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!InLaserMode)
            {

                lockMouseState = !lockMouseState;
            }

        }
#elif UNITY_ANDROID
    lockMouseState = false;
#endif



    }

    public void StartIndicatorFalse()
    {
        startExperimentKeyIndicator = false;
    }

    private void CalculateDistancebtwTableNPlayer()
    {
        float distance = Vector3.Distance(player.position, experimentTable.position);
        if (distance < itemDropDistance)
        {
            handIconPickupButton.gameObject.SetActive(true);
            instructionText.text = "Press E or tap on hand icon to drop item";
            //  InstructionPanel.SetActive(true);
            Interact(currentInteractable);
        }
        else
        {
            handIconPickupButton.gameObject.SetActive(false);

        }
    }

    private void MouseStateController()
    {
        if (!lockMouseState) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public bool IsItemNearby(Transform itemTransform)
    {
        float distance = Vector3.Distance(player.position, itemTransform.position);
        return distance < itemDropDistance;
    }

    public void OnItemClicked(Interactable interactable)
    {
        currentInteractable = interactable;
        Interact(interactable);  // Calls ObjectPickAndDrop
        Debug.Log("Pickup");
    }

    void CheckNearbyInteractables()
    {
        Interactable[] allInteractables = FindObjectsOfType<Interactable>();
        currentInteractable = null;

        foreach (var interactable in allInteractables)
        {
            float dist = Vector3.Distance(player.position, interactable.transform.position);


            if (interactable.isPickeUP == false)
            {
                if (dist < itemDropDistance)
                {

                    currentInteractable = interactable;
                    handIconPickupButton.gameObject.SetActive(true);
                    instructionText.text = "Tap the object to pick it up";
                    AutoOutline autoOutline = currentInteractable.transform.GetChild(0).GetComponent<AutoOutline>();
                    if (autoOutline != null)
                    {
                        autoOutline.gameObject.SetActive(false);
                    }


                    break;  // Pick the first nearby
                }
                else
                {
                    currentInteractable = interactable;
                    AutoOutline autoOutline = currentInteractable.transform.GetChild(0).GetComponent<AutoOutline>();
                    if (autoOutline != null && currentInteractable == null)
                    {
                        autoOutline.gameObject.SetActive(true);
                    }
                }
            }



        }

        if (currentInteractable == null)
        {
            handIconPickupButton.gameObject.SetActive(false);

        }

    }



    // private void RayCastItems()
    // {
    //     Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

    //     RaycastHit hitInfo;


    //     Debug.DrawRay(ray.origin, ray.direction * rayMaxDistance, Color.red);

    //     if (Physics.Raycast(ray, out hitInfo, rayMaxDistance))
    //     {

    //         Interactable interactable = hitInfo.collider.GetComponent<Interactable>();

    //         if (interactable != null)
    //         {
    //             handIconPickupButton.gameObject.SetActive(true);
    //             currentInteractable = interactable;
    //                  instructionText.text = "Press E or tap on hand icon to pick the item";
    //              Interact(interactable);
    //              if (!itemInHand)
    //              {
    //                //  InstructionPanel.SetActive(true);
    //              }



    //         }
    //         else
    //         {

    //             //handIconPickupButton.gameObject.SetActive(false);  
    //           //  InstructionPanel.SetActive(false);
    //            // Debug.Log("First");
    //         }
    //     }
    //     else
    //     {
    //         handIconPickupButton.gameObject.SetActive(false);
    //    //    InstructionPanel.SetActive(false);
    //       //  Debug.Log("Second");
    //     }

    // }

    public void Interact(Interactable interactable)
    {
       
        handIconPickupButton.onClick.RemoveAllListeners();
        handIconPickupButton.onClick.AddListener(delegate
        {
            if (!startExperimentKeyIndicator)
            {
                if (interactable != null)
                {
                    ObjectPickAndDrop(interactable);
                    Debug.Log("First");
                }
            }
            else
            {
                handIconPickupButton.interactable = false;
                StartExperiment();
                Debug.Log("Second");
            }

        });

        if (Input.GetKeyDown(pickupKey))
        {
            if (!startExperimentKeyIndicator)
            {
                if (interactable != null)
                {
                    ObjectPickAndDrop(interactable);
                }
            }
        }


        if (!startExperimentKeyIndicator)
        {
            if (interactable != null)
            {
                ObjectPickAndDrop(interactable);
            }
        }

    }


    public void  ObjectPickAndDrop(Interactable interactable)
    {
        //if we click pick button and object already is not in hand
 
        if (!itemInHand)
        {
            handIconPickupButton.gameObject.SetActive(false);
            itemInHand = true;
          
            if (interactable.isGlass)
            {
                if (!glassHasPickedUp)
                {
                    FindFirstObjectByType<OpeningSceneController>()
                        .PlayDialogue(OpeningSceneController.DialogueType.GlassPickup);

                    glassHasPickedUp = true;
                    objectiveComplete2.SetActive(true);
                }
               
                
                interactable.gameObject.transform.SetParent(pickupLocations);
                // interactable.gameObject.transform.localPosition = Vector3.zero;
                // interactable.gameObject.transform.localRotation = Quaternion.identity;
                StartCoroutine(GlassPickUpDropDuration(interactable,Quaternion.identity));
                
         

            }
            else if (interactable.isLaser)
            {
                if (!laserHasPickedUp)
                {
                    FindFirstObjectByType<OpeningSceneController>()
                        .PlayDialogue(OpeningSceneController.DialogueType.LaserPickup);

                    laserHasPickedUp = true;
                    objectiveComplete1.SetActive(true);
                }
                interactable.gameObject.transform.SetParent(pickupLocations);
                // interactable.gameObject.transform.localPosition = Vector3.zero;
                // interactable.gameObject.transform.localRotation = Quaternion.Euler(0,-90,90);
                StartCoroutine(GlassPickUpDropDuration(interactable,Quaternion.Euler(0,-90,90)));
            }
        }
        else
        {
            handIconPickupButton.gameObject.SetActive(false);
            itemInHand = false;

            if (interactable.isGlass)
            {
  
                if (objectiveComplete1.activeInHierarchy && objectiveComplete2.activeInHierarchy)
                {              
                                  Debug.Log("SCe");
                    objectiveComplete3.SetActive(true);
                    bothObjectiveCompleted = true;
                    FindFirstObjectByType<OpeningSceneController>()
                        .PlayDialogue(OpeningSceneController.DialogueType.StartExperiment);
                    Invoke(nameof(StartExperimentInstruction), 3f);

                }
                interactable.gameObject.transform.SetParent(GlassDropLocation);
                // interactable.gameObject.transform.localPosition = Vector3.zero;
                // interactable.gameObject.transform.localRotation = Quaternion.identity;
                StartCoroutine(GlassPickUpDropDuration(interactable, Quaternion.identity));
                interactable.GetComponent<BoxCollider>().enabled = false;
                Debug.Log("DropGlass");
            }
            else if (interactable.isLaser)
            {
                if (objectiveComplete1.activeInHierarchy && objectiveComplete2.activeInHierarchy)
                {
                    objectiveComplete3.SetActive(true);
                    bothObjectiveCompleted = true;
                    FindFirstObjectByType<OpeningSceneController>()
                        .PlayDialogue(OpeningSceneController.DialogueType.StartExperiment);
                    Invoke(nameof(StartExperimentInstruction), 3f);

                }
                interactable.gameObject.transform.SetParent(laserDropLocation);
                // interactable.gameObject.transform.localPosition = Vector3.zero;
                // interactable.gameObject.transform.localRotation = Quaternion.identity;
                StartCoroutine(GlassPickUpDropDuration(interactable, Quaternion.identity));
                interactable.GetComponent<BoxCollider>().enabled = false;
                Debug.Log("DropLaser");
            }
        }
     
    }

    IEnumerator GlassPickUpDropDuration(Interactable interactable,Quaternion rotation)
    {

        t = 0f;


        while (t < 1)
        {
            t += Time.deltaTime * pickAndDropSpeed;
            interactable.gameObject.transform.localPosition =
                Vector3.Lerp(interactable.gameObject.transform.localPosition, Vector3.zero, t);
            interactable.gameObject.transform.localRotation =
                Quaternion.Lerp(interactable.gameObject.transform.localRotation, rotation, t);
            yield return null;
        }
        
    }

    public void StartExperimentInstruction()
    {
        startExperimentKeyIndicator = true;
        InstructionPanel.SetActive(true);
        InstructionPanel.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Press E or tap on hand icon to start the Experiment";
        handIconPickupButton.gameObject.SetActive(true);
        bothObjectiveCompleted = false;
    }

    public void StartExperiment()
    {
        InLaserMode = true;
        boardCanvas.SetActive(false);
        StartCoroutine(StartExperimentCoRoutine());

        Invoke(nameof(UnUsedAssets), 5f);


    }

    public void UnUsedAssets()
{
    StartCoroutine(DestroyAssetsGradually());
}

IEnumerator DestroyAssetsGradually()
{
    int chunkSize = 3; // How many objects to disable/destroy per frame
    float delayBetweenChunks = 0.02f; // ~20ms between chunks

    for (int i = 0; i < turnOffAllTheUnusedAssets.Length; i++)
    {
        turnOffAllTheUnusedAssets[i].SetActive(false); // Disable first

        // Optionally: delay actual destruction a bit after disabling
        Destroy(turnOffAllTheUnusedAssets[i].gameObject, 2f);

        // Chunk: Every N objects, yield to next frame
        if ((i + 1) % chunkSize == 0)
            yield return new WaitForSeconds(delayBetweenChunks);
    }
}
    IEnumerator  StartExperimentCoRoutine()
    {
        imageToFadeIn.gameObject.SetActive(true);
        Color color = imageToFadeIn.color;
        float t = 0f;

        openingSceneController.cameraLook.enabled = false;
        openingSceneController.joyStickCanvas.gameObject.SetActive(false);
       
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(.25f, 1f, t / fadeDuration);
            imageToFadeIn.color = new Color(color.r, color.g, color.b, alpha);
            for (int i = 0; i < startingAudioSources.Length; i++)
            {

                startingAudioSources[i].volume = Mathf.Lerp(.3f, 0f, t / fadeDuration);
            }
            experimentCam.SetActive(true);
            openingSceneController.TurnOFFPlayerControls();
            playerCam.transform.SetParent(experimentCam.transform);
            playerCam.transform.localPosition =
                Vector3.Lerp(playerCam.transform.localPosition, Vector3.zero, t / fadeDuration);
            playerCam.transform.localRotation = Quaternion.Lerp(playerCam.transform.localRotation, Quaternion.identity, t / fadeDuration * .1f);
            yield return null;
        }
        
        
        handIconPickupButton.gameObject.SetActive(false);
        Debug.Log("OSsee");
      
        playerCam.SetActive(false);

        Invoke(nameof(ExperimentWindowStart),1f);

    }

    public void ExperimentWindowStart()
    {
        initiateExperiment.InitizeExperiment();
        StartCoroutine(ExperimentWindow());
        
    }
    IEnumerator ExperimentWindow()
    {
        imageToFadeIn.gameObject.SetActive(true);
        Color color = imageToFadeIn.color;
        float t = 0f;


        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            imageToFadeIn.color = new Color(color.r, color.g, color.b, alpha);
            experimentAudioSources.volume = Mathf.Lerp(0, .3f, t / fadeDuration);

            yield return null;
        }

        FindFirstObjectByType<OpeningSceneController>()
            .PlayDialogue(OpeningSceneController.DialogueType.LaserOn);

        City.SetActive(false);
            
    }





}
