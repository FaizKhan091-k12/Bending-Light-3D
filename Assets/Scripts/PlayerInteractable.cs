using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInteractable : MonoBehaviour
{
    [Header("Debug Options")] [SerializeField]
    private bool lockMouseState;
    
    [Space(10)]
    public OpeningSceneController openingSceneController;

    [Header("RayCast Refrences")] 
    [SerializeField] Camera cam;
    [SerializeField] float rayMaxDistance;
    
    [Header("Pickup Refrences")]
    [SerializeField] Button pickupButton;
    [SerializeField] Transform pickupLocations;
    [SerializeField] Transform GlassDropLocation;
    [SerializeField] Transform laserDropLocation;

    [Header("Item Setting")]
    [SerializeField] bool itemInHand = false;
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
    
    Interactable currentInteractable;
    float t = 0f;
    bool glassHasPickedUp = false;
    bool laserHasPickedUp = false;
    

   private void Start()
   {
   
       InstructionPanel.SetActive(false);
       pickupButton.gameObject.SetActive(false);
    
   }

   private void Update()
   {
       MouseStateController();
       if (!itemInHand)
       {
           RayCastItems();
       }
       else
       {
           CalculateDistancebtwTableNPlayer();
           InstructionPanel.SetActive(false);
       }

   }

   private void CalculateDistancebtwTableNPlayer()
   {
       float distance = Vector3.Distance(player.position, experimentTable.position);
       if (distance < itemDropDistance)
       {
           pickupButton.gameObject.SetActive(true);
           Interact(currentInteractable);
       }
       else
       {
           pickupButton.gameObject.SetActive(false);

       }
   }

   private void MouseStateController()
   {
       if(!lockMouseState) return;
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

   private void RayCastItems()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        RaycastHit hitInfo;

       
        Debug.DrawRay(ray.origin, ray.direction * rayMaxDistance, Color.red);

        if (Physics.Raycast(ray, out hitInfo, rayMaxDistance))
        {
           
            Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
            
            if (interactable != null)
            {
                pickupButton.gameObject.SetActive(true);
                currentInteractable = interactable;
                 Interact(interactable);
                 if (!itemInHand)
                 {
                     InstructionPanel.SetActive(true);
                 }
                
                

            }
            else
            {
                
                pickupButton.gameObject.SetActive(false);  
                InstructionPanel.SetActive(false);
            }
        }
        else
        {
            pickupButton.gameObject.SetActive(false);
            InstructionPanel.SetActive(false);

        }

    }

    public void Interact(Interactable interactable)
    {
        pickupButton.onClick.RemoveAllListeners();
        pickupButton.onClick.AddListener(delegate
        {
            if (interactable != null)
            {
                ObjectPickAndDrop(interactable);
            }
        });

        if (Input.GetKeyDown(pickupKey))
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
            pickupButton.gameObject.SetActive(false);
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
            pickupButton.gameObject.SetActive(false);
            itemInHand = false;

            if (interactable.isGlass)
            {
                if (objectiveComplete1.activeInHierarchy && objectiveComplete2.activeInHierarchy)
                {
                    objectiveComplete3.SetActive(true);
                }
                interactable.gameObject.transform.SetParent(GlassDropLocation);
                // interactable.gameObject.transform.localPosition = Vector3.zero;
                // interactable.gameObject.transform.localRotation = Quaternion.identity;
                StartCoroutine(GlassPickUpDropDuration(interactable,Quaternion.identity));
                interactable.GetComponent<BoxCollider>().enabled = false;

            }
            else if (interactable.isLaser)
            {
                if (objectiveComplete1.activeInHierarchy && objectiveComplete2.activeInHierarchy)
                {
                    objectiveComplete3.SetActive(true);
                }
                interactable.gameObject.transform.SetParent(laserDropLocation);
                // interactable.gameObject.transform.localPosition = Vector3.zero;
                // interactable.gameObject.transform.localRotation = Quaternion.identity;
                StartCoroutine(GlassPickUpDropDuration(interactable,Quaternion.identity));
                interactable.GetComponent<BoxCollider>().enabled = false;
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
  

    

    
}
