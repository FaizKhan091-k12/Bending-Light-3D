using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Collections.Generic;


public class LaserBeamRenderer : MonoBehaviour
{
     public Camera mainCam;
    [Header("Laser Properties")]
    public Transform laserStart;
    public Vector3 laserDirection = Vector3.forward;
    [Range(0f, 40f)] public float laserLength = 40f;
    public float beamWidth = 0.05f;
    public float reflectiveBeamWidht;

    [Header("Color & Wavelength")]
    [Range(380f, 700f)] public float wavelength = 650f;
    public bool useWavelengthColor = true;

    [Header("Refraction Settings")]
    public LayerMask refractableLayer;
    public string leftMaterial = "air";
    public string rightMaterial = "glass";
    public int maxRefractions = 3;

    [Header("Debug Refraction Settings")]
    public float incidenceAngle;
    public float refractionAngle;
    public float reflectionAngle;

    [Header("Intensity Debug")]
    public float reflectedSPercent;
    public float reflectedPPercent;
    public float transmittedSPercent;
    public float transmittedPPercent;
    public float transmittedAvgPercent;

    [Header("UI Refraction Settings")]
    [SerializeField] TextMeshProUGUI incidenceAngleText;
    [SerializeField] TextMeshProUGUI refractionAngleText;
    [SerializeField] TextMeshProUGUI reflectionAngleText;
    [SerializeField] Slider leftRefractiveSlider;
    [SerializeField] Slider rightRefractiveSlider;
    [SerializeField] ProceduralImage incidenceAngleImage;
    [SerializeField] ProceduralImage refractionAngleImage;
    [SerializeField] ProceduralImage reflectionAngleImage;
    [SerializeField] TextMeshProUGUI leftIORText;
    [SerializeField] TextMeshProUGUI rightIORText;
    [SerializeField] TMP_Dropdown leftDropDown;
    [SerializeField] TMP_Dropdown rightDropDown;
    [SerializeField] Slider laserColorSlider;
    [SerializeField] TextMeshProUGUI laserColorValue;
    [SerializeField] ProceduralImage laserColorImage;
    [SerializeField] private GameObject[] leftSideMaterialsArray;
    [SerializeField] private GameObject[] rightSideMaterialsArray;
    [SerializeField] TMP_Text incidenceAngleText3D;
    [SerializeField] TMP_Text refractionAngleText3D;
    [SerializeField] TMP_Text reflectionAngleText3D;

    [SerializeField] TextMeshProUGUI refelectiveIntensity;
    [SerializeField] TextMeshProUGUI transmittedIntensity;
    public Material laserMaterial;
    private Color beamColor;
    public bool didRefract = false;
    public bool didReflect = false;

    private readonly Dictionary<string, (float ior380, float ior700)> materialIORs = new()
    {
        { "glass", (1.519f, 1.499f) },
        { "water", (1.345f, 1.332f) },
        { "air", (1.00029f, 1.00027f) },
        { "custom", (1.0f, 1.0f) }
    };
// Add these new fields
private LineRenderer incidentLine;
private LineRenderer reflectedLine;
private LineRenderer refractedLine;

    void Awake()
    {
        incidentLine = CreateLineRenderer("IncidentLine", 0.01f);
        reflectedLine = CreateLineRenderer("ReflectedLine", 0.01f);
        refractedLine = CreateLineRenderer("RefractedLine", 0.005f);
    }
LineRenderer CreateLineRenderer(string name, float width)
{
    GameObject lineObj = new GameObject(name);
    lineObj.transform.parent = transform;
    var lr = lineObj.AddComponent<LineRenderer>();
    lr.material = laserMaterial;
    lr.widthMultiplier = width;
    lr.positionCount = 0;
    lr.useWorldSpace = true;
    lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    lr.receiveShadows = false;
    lr.alignment = LineAlignment.View;
    return lr;
}

    void Start()
    {
      
          MaterialInitilizer();
      
        laserColorSlider.onValueChanged.AddListener(_ =>
        {
            wavelength = laserColorSlider.value;
            laserColorValue.text = wavelength + "nm";
        });

        leftDropDown.onValueChanged.AddListener(_ =>
        {
            for (int i = 0; i < leftSideMaterialsArray.Length; i++)
                leftSideMaterialsArray[i].SetActive(i == leftDropDown.value);

            leftMaterial = leftDropDown.options[leftDropDown.value].text.ToLower();

        });

        rightDropDown.onValueChanged.AddListener(_ =>
        {
            for (int i = 0; i < rightSideMaterialsArray.Length; i++)
                rightSideMaterialsArray[i].SetActive(i == rightDropDown.value);

            rightMaterial = rightDropDown.options[rightDropDown.value].text.ToLower();
      
        });

        // Shader shader = Shader.Find("Unlit/Color");
        // laserMaterial = new Material(shader);

        leftRefractiveSlider.onValueChanged.AddListener(_ =>
        {
            materialIORs["custom"] = (leftRefractiveSlider.value, leftRefractiveSlider.value);
            leftMaterial = "custom";
        });

        rightRefractiveSlider.onValueChanged.AddListener(_ =>
        {
            materialIORs["custom"] = (rightRefractiveSlider.value + 0.019f, rightRefractiveSlider.value - 0.001f);
            rightMaterial = "custom";
        });
    }

    void MaterialInitilizer()
    {
        foreach (var mat in leftSideMaterialsArray) mat.SetActive(false);
        foreach (var mat in rightSideMaterialsArray) mat.SetActive(false);
        leftSideMaterialsArray[leftDropDown.value].SetActive(true);
        rightSideMaterialsArray[rightDropDown.value].SetActive(true);
    }

    void Update()
{
        UpdateLaserBeam();
        refractionAngleText3D.transform.parent.localEulerAngles = new Vector3(90, 0, incidenceAngle - refractionAngle);
        reflectionAngleText3D.transform.parent.localEulerAngles = new Vector3(90, 0, Mathf.Abs(incidenceAngle + reflectionAngle));

        beamColor = useWavelengthColor ? WavelengthToRGB(wavelength) : Color.red;
        if (laserMaterial.color != beamColor)
            laserMaterial.color = beamColor;

        laserColorImage.color = beamColor;

        bool sameMaterial = leftMaterial == rightMaterial;
        incidenceAngleText.text = $"{incidenceAngle:F1}<sup>o</sup>";
        refractionAngleText.text = $"{refractionAngle:F1}<sup>o</sup>";
        reflectionAngleText.text = $"{reflectionAngle:F1}<sup>o</sup>";

        incidenceAngleImage.fillAmount = (0.25f / 90f) * incidenceAngle;
        refractionAngleImage.fillAmount = (0.25f / 90f) * refractionAngle;
        if (reflectionAngleImage != null)
            reflectionAngleImage.fillAmount = (0.25f / 90f) * reflectionAngle;

        refractionAngleImage.gameObject.SetActive(didRefract || sameMaterial);
        refractionAngleText3D.transform.parent.gameObject.SetActive(didRefract || sameMaterial);
        reflectionAngleText3D.transform.parent?.gameObject.SetActive(didReflect);
        reflectionAngleImage?.gameObject.SetActive(didReflect);
        reflectionAngleText.gameObject.SetActive(didReflect);
       
    }






    // void OnRenderObject()
    // {

    //     didRefract = false;
    //     didReflect = false;

    //     if (!laserStart || !laserMaterial) return;

    //     laserMaterial.SetPass(0);
    //     GL.PushMatrix();
    //     GL.Begin(GL.TRIANGLES);
    //     GL.Color(beamColor);

    //     Vector3 origin = laserStart.position;
    //     Vector3 direction = laserStart.TransformDirection(laserDirection.normalized);
    //     float remainingLength = laserLength;
    //     Vector3 lastPoint = origin;

    //     float currentIOR = GetIORFromMaterial(leftMaterial, wavelength);
    //     leftIORText.text = $"{currentIOR:F3}";
    //     bool insideMaterial = false;

    //     for (int i = 0; i < maxRefractions && remainingLength > 0; i++)
    //     {
    //         if (Physics.Raycast(origin, direction, out RaycastHit hit, remainingLength, refractableLayer))
    //         {
    //             Vector3 hitPoint = hit.point;
    //             Vector3 normal = hit.normal.normalized;
    //             Vector3 incoming = direction.normalized;

    //             DrawQuadBeam(lastPoint, hitPoint, beamWidth);
    //             lastPoint = hitPoint;

    //             incidenceAngle = Vector3.Angle(normal, -incoming);
    //             string nextMaterial = insideMaterial ? leftMaterial : rightMaterial;
    //             float nextIOR = GetIORFromMaterial(nextMaterial, wavelength);
    //             rightIORText.text = $"{nextIOR:F3}";

    //             Vector3 reflectDir = Vector3.Reflect(incoming, normal);
    //             reflectionAngle = Vector3.Angle(reflectDir, normal);

    //             float thetaI_rad = incidenceAngle * Mathf.Deg2Rad;
    //             float sinThetaT = (currentIOR / nextIOR) * Mathf.Sin(thetaI_rad);
    //             Vector3 refractedDir = Vector3.zero;

    //             if (sinThetaT >= 1f)
    //             {
    //                 didReflect = true;
    //                 DrawQuadBeam(hitPoint, hitPoint + reflectDir * 0.5f, beamWidth * 0.5f);
    //                 Debug.Log("Total Internal Reflection");
    //             }
    //             else
    //             {
    //                 float cosThetaI = Mathf.Cos(thetaI_rad);
    //                 float cosThetaT = Mathf.Sqrt(1f - sinThetaT * sinThetaT);

    //                 float rs = (currentIOR * cosThetaI - nextIOR * cosThetaT) / (currentIOR * cosThetaI + nextIOR * cosThetaT);
    //                 float rp = (currentIOR * cosThetaT - nextIOR * cosThetaI) / (currentIOR * cosThetaT + nextIOR * cosThetaI);

    //                 reflectedSPercent = rs * rs * 100f;
    //                 reflectedPPercent = rp * rp * 100f;
    //                 transmittedSPercent = 100f - reflectedSPercent;
    //                 transmittedPPercent = 100f - reflectedPPercent;
    //                 transmittedAvgPercent = 0.5f * (transmittedSPercent + transmittedPPercent);

    //                 Debug.Log($"Rs: {reflectedSPercent:F2}% | Rp: {reflectedPPercent:F2}% | " +
    //                           $"Ts: {transmittedSPercent:F2}% | Tp: {transmittedPPercent:F2}% | AvgT: {transmittedAvgPercent:F2}%");

    //                 refelectiveIntensity.text = reflectedSPercent.ToString("F2") + "%";
    //                 transmittedIntensity.text = transmittedSPercent.ToString("F2") + "%";                    

    //                 if (reflectedSPercent > 0.5f)
    //                 {
    //                     didReflect = true;
    //                     DrawQuadBeam(hitPoint, hitPoint + reflectDir * 0.5f, reflectiveBeamWidht * reflectedSPercent / 100f);
    //                 }
    //                 else
    //                 {
    //                     Debug.Log("Reflectance too low, skipping reflective ray.");
    //                 }

    //                 refractedDir = RefractRay(incoming, normal, currentIOR, nextIOR);
    //                 refractionAngle = Vector3.Angle(-normal, refractedDir);
    //                 didRefract = true;

    //                 if (transmittedSPercent > 0.5f && refractedDir != Vector3.zero)
    //                 {
    //                     didRefract = true;
    //                     DrawQuadBeam(hitPoint, hitPoint + refractedDir * 10f, reflectiveBeamWidht * transmittedSPercent / 100f);
    //                 }

    //             }

    //             origin = (refractedDir == Vector3.zero) ? hitPoint + reflectDir * 0.001f : hitPoint + refractedDir * 0.001f;
    //             direction = (refractedDir == Vector3.zero) ? reflectDir : refractedDir;

    //             remainingLength -= Vector3.Distance(hitPoint, origin);
    //             insideMaterial = !insideMaterial;
    //             currentIOR = nextIOR;
    //         }
    //         else
    //         {
    //             Vector3 endPoint = origin + direction * remainingLength;
    //             DrawQuadBeam(lastPoint, endPoint, beamWidth);


    //             break;
    //         }
    //     }

    //     GL.End();
    //     GL.PopMatrix();
    // }
    void UpdateLaserBeam()
    {
        didRefract = false;
        didReflect = false;

        incidentLine.positionCount = 0;
        reflectedLine.positionCount = 0;
        refractedLine.positionCount = 0;

        if (!laserStart || !laserMaterial) return;

        beamColor = useWavelengthColor ? WavelengthToRGB(wavelength) : Color.red;
        laserMaterial.color = beamColor;

        // Set beam color to all line renderers
        incidentLine.startColor = beamColor; incidentLine.endColor = beamColor;
        reflectedLine.startColor = beamColor; reflectedLine.endColor = beamColor;
        refractedLine.startColor = beamColor; refractedLine.endColor = beamColor;

        Vector3 origin = laserStart.position;
        Vector3 direction = laserStart.TransformDirection(laserDirection.normalized);
        float remainingLength = laserLength;
        float currentIOR = GetIORFromMaterial(leftMaterial, wavelength);
        leftIORText.text = $"{currentIOR:F3}";

        bool insideMaterial = false;
        List<Vector3> incidentPoints = new() { origin };
        List<Vector3> reflectedPoints = new();
        List<Vector3> refractedPoints = new();

        for (int i = 0; i < maxRefractions && remainingLength > 0; i++)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, remainingLength, refractableLayer))
            {
                Vector3 hitPoint = hit.point;
                Vector3 normal = hit.normal.normalized;
                Vector3 incoming = direction.normalized;

                incidentPoints.Add(hitPoint);

                incidenceAngle = Vector3.Angle(normal, -incoming);
                string nextMaterial = insideMaterial ? leftMaterial : rightMaterial;
                float nextIOR = GetIORFromMaterial(nextMaterial, wavelength);
                rightIORText.text = $"{nextIOR:F3}";

                Vector3 reflectDir = Vector3.Reflect(incoming, normal);
                reflectionAngle = Vector3.Angle(reflectDir, normal);
                float thetaI_rad = incidenceAngle * Mathf.Deg2Rad;
                float sinThetaT = (currentIOR / nextIOR) * Mathf.Sin(thetaI_rad);
                Vector3 refractedDir = Vector3.zero;

                if (sinThetaT >= 1f)
                {
                    didReflect = true;
                    Vector3 reflectEnd = hitPoint + reflectDir * remainingLength;
                    reflectedPoints.Add(hitPoint);
                    reflectedPoints.Add(reflectEnd);
                    break;
                }
                else
                {
                    float cosThetaI = Mathf.Cos(thetaI_rad);
                    float cosThetaT = Mathf.Sqrt(1f - sinThetaT * sinThetaT);
                    float rs = (currentIOR * cosThetaI - nextIOR * cosThetaT) / (currentIOR * cosThetaI + nextIOR * cosThetaT);
                    float rp = (currentIOR * cosThetaT - nextIOR * cosThetaI) / (currentIOR * cosThetaT + nextIOR * cosThetaI);

                    reflectedSPercent = rs * rs * 100f;
                    reflectedPPercent = rp * rp * 100f;
                    transmittedSPercent = 100f - reflectedSPercent;
                    transmittedPPercent = 100f - reflectedPPercent;
                    transmittedAvgPercent = 0.5f * (transmittedSPercent + transmittedPPercent);

                    refelectiveIntensity.text = reflectedSPercent.ToString("F2") + "%";
                    transmittedIntensity.text = transmittedSPercent.ToString("F2") + "%";

                    if (reflectedSPercent > 0.5f)
                    {
                        didReflect = true;
                        Vector3 reflectEnd = hitPoint + reflectDir * remainingLength * (reflectedSPercent / 100f);
                        reflectedPoints.Add(hitPoint);
                        reflectedPoints.Add(reflectEnd);
                    }

                    refractedDir = RefractRay(incoming, normal, currentIOR, nextIOR);
                    refractionAngle = Vector3.Angle(-normal, refractedDir);

                    if (transmittedSPercent > 0.5f && refractedDir != Vector3.zero)
                    {
                        didRefract = true;
                        Vector3 refractEnd = hitPoint + refractedDir * remainingLength * (transmittedSPercent / 100f);
                        refractedPoints.Add(hitPoint);
                        refractedPoints.Add(refractEnd);
                    }
                }

                origin = (refractedDir == Vector3.zero) ? hitPoint + reflectDir * 0.001f : hitPoint + refractedDir * 0.001f;
                direction = (refractedDir == Vector3.zero) ? reflectDir : refractedDir;
                remainingLength -= Vector3.Distance(hit.point, origin);
                insideMaterial = !insideMaterial;
                currentIOR = nextIOR;
            }
            else
            {
                Vector3 endPoint = origin + direction * remainingLength;
                //incidentPoints.Add(endPoint);
                break;
            }
        }

        // Assign points to line renderers
        incidentLine.positionCount = incidentPoints.Count;
        incidentLine.SetPositions(incidentPoints.ToArray());

        if (didReflect)
        {
            reflectedLine.positionCount = reflectedPoints.Count;
            reflectedLine.widthMultiplier = reflectiveBeamWidht * (reflectedSPercent / 100f);
            reflectedLine.SetPositions(reflectedPoints.ToArray());
        }

        if (didRefract)
        {
            refractedLine.positionCount = refractedPoints.Count;
            refractedLine.widthMultiplier = reflectiveBeamWidht * (transmittedSPercent / 100f);
            refractedLine.SetPositions(refractedPoints.ToArray());
        }
        else
        {
            refractedLine.positionCount = 0;
        }
    }
    float GetIORFromMaterial(string material, float wavelengthNm)
    {
        if (!materialIORs.TryGetValue(material.ToLower(), out var range))
            range = materialIORs["custom"];

        float t = Mathf.InverseLerp(380f, 700f, Mathf.Clamp(wavelengthNm, 380f, 700f));
        return Mathf.Lerp(range.ior380, range.ior700, t);
    }

    Vector3 RefractRay(Vector3 incident, Vector3 normal, float n1, float n2)
    {
        incident = incident.normalized;
        normal = normal.normalized;
        float r = n1 / n2;
        float cosI = -Vector3.Dot(normal, incident);
        float sinT2 = r * r * (1f - cosI * cosI);
        if (sinT2 > 1f) return Vector3.zero;
        float cosT = Mathf.Sqrt(1f - sinT2);
        return r * incident + (r * cosI - cosT) * normal;
    }

    void DrawQuadBeam(Vector3 start, Vector3 end, float width)
    {
        Vector3 cameraDir = mainCam.transform.forward;
        Vector3 side = Vector3.Cross((end - start).normalized, cameraDir).normalized * width;

        Vector3 v1 = start - side;
        Vector3 v2 = start + side;
        Vector3 v3 = end + side;
        Vector3 v4 = end - side;

        // Triangle 1: v1, v2, v3
        GL.Vertex(v1);
        GL.Vertex(v2);
        GL.Vertex(v3);

        // Triangle 2: v3, v4, v1
        GL.Vertex(v3);
        GL.Vertex(v4);
        GL.Vertex(v1);
    }

    Color WavelengthToRGB(float wavelength)
    {
        if (wavelength < 440) return new Color((440 - wavelength) / 60f, 0f, 1f);
        else if (wavelength < 490) return new Color(0f, (wavelength - 440) / 50f, 1f);
        else if (wavelength < 510) return new Color(0f, 1f, (510 - wavelength) / 20f);
        else if (wavelength < 580) return new Color((wavelength - 510) / 70f, 1f, 0f);
        else if (wavelength < 645) return new Color(1f, (645 - wavelength) / 65f, 0f);
        else return new Color(1f, 0f, 0f);
    }
}
