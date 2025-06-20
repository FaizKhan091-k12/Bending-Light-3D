using System.Collections;
using FirstPersonMobileTools.DynamicFirstPerson;
using TMPro;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using UnityEngine.UI;

public class OpeningSceneController : MonoBehaviour
{
    
    [Header("Debug Options")]
    [SerializeField] private bool playOpeningScene = true;
    
    [Space(10)]
    [Header("UI References")] 
    [SerializeField] Toggle crouchToggle;
    [SerializeField] ProceduralImage imageToFadeIn;
    [SerializeField] private GameObject joyStickCanvas;
    [SerializeField] float fadeDuration = 2f;

    [Header("Audio References")]
    [SerializeField] private AudioSource schoolPublicAudio;
    [SerializeField] private AudioSource openingBackgroundAudio;

    [Header("Player References")]
    [SerializeField] private CameraLook cameraLook;
    [SerializeField] private nonMobileInput nonMobileInput;
    [SerializeField] private ObjectiveTypewriter objectiveTypewriter;
    
    [Header("Dialogue References")]
    [SerializeField] private DialogueData[] dialogueDatabase; 
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject subtitlePanel;


   

    
    [System.Serializable]
    public class DialogueLine
    {
        public AudioClip voiceClip;
        [TextArea] public string subtitleText;
        public float displayDuration = 3f;
    }
    
    public enum DialogueType
    {
        Opening,
        GlassPickup,
        LaserPickup,
        
       
    }
    [System.Serializable]
    public class DialogueData
    {
        public DialogueType type;
        public DialogueLine[] lines;
    }


    private void Start()
    {
   //     Application.targetFrameRate = 60;
        if (playOpeningScene)
        {
        
            StartCoroutine(InitiateOpeningScene());
            
        }
        else
        {
            return;
        }
    }

    IEnumerator InitiateOpeningScene()
    {
        imageToFadeIn.color = Color.black;
        joyStickCanvas.SetActive(false);
        crouchToggle.isOn = true;
        cameraLook.enabled = false;
        nonMobileInput.enabled = false;

      
        if (openingBackgroundAudio) openingBackgroundAudio.Play();
        if (schoolPublicAudio) schoolPublicAudio.Play();

      
        Color color = imageToFadeIn.color;
        float t = 0f;

        // yield return new WaitForSeconds(0.2f);
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            imageToFadeIn.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        imageToFadeIn.gameObject.SetActive(false);

        PlayDialogue(DialogueType.Opening);


    }
    IEnumerator PlayDialogueSequence(DialogueLine[] lines, bool zoomAfter = false)
    {
        subtitlePanel.SetActive(true);

        foreach (DialogueLine line in lines)
        {
            if (line.voiceClip != null)
            {
                voiceSource.clip = line.voiceClip;
                voiceSource.Play();
            }

            yield return StartCoroutine(TypeSubtitle(line.subtitleText, 0.04f));
            yield return new WaitForSeconds(line.displayDuration);
        }

        subtitlePanel.SetActive(false);
        subtitleText.text = "";

        if (zoomAfter)
            StartCoroutine(CameraZoomEffect());
    }

    public void PlayDialogue(DialogueType type)
    {
        DialogueData data = System.Array.Find(dialogueDatabase, d => d.type == type);

        if (data != null)
        {
            bool isOpening = (type == DialogueType.Opening);
            StartCoroutine(PlayDialogueSequence(data.lines, isOpening));
        }
        else
        {
            Debug.LogWarning($"Dialogue for type {type} not found.");
        }
    }

  
    IEnumerator TypeSubtitle(string fullText, float typingSpeed)
    {
        subtitleText.text = "Player: ";
        foreach (char c in fullText)
        {
            subtitleText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }


    IEnumerator  CameraZoomEffect()
    {
        float defaultCameraFOV = 60f;
        float tempCameraFOV = 20f;
        
        Camera cam = Camera.main;

        float t = 0f;
        while (t < 1)
        {
            t+= Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(defaultCameraFOV, tempCameraFOV, t);
            yield return null;
            
        }
     
        objectiveTypewriter.StartTyping();
        
        
    }
    
    IEnumerator  CameraZoomEffectReset()
    {
        float defaultCameraFOV = 60f;
        float tempCameraFOV = 20f;
        
        Camera cam = Camera.main;

        float t = 0f;
        while (t < 1)
        {
            t+= Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(tempCameraFOV, defaultCameraFOV, t);
            yield return null;
            
        }
     
        
        
    }

    public void OpeningSceneFinised()
    {
        cameraLook.enabled = true;
        nonMobileInput.enabled = true;
        joyStickCanvas.SetActive(true);
        crouchToggle.isOn = false;
        StartCoroutine(CameraZoomEffectReset());

    }
    
 



    
}