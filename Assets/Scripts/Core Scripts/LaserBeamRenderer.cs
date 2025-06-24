using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class LaserBeamRenderer : MonoBehaviour
{
    [Header("Laser Properties")]
    public Transform laserStart;
    public Vector3 laserDirection = Vector3.forward;
    public float laserLength = 10f;
    public float beamWidth = 0.05f;

    [Header("Color & Wavelength")]
    [Range(380f, 700f)] public float wavelength = 650f;
    public bool useWavelengthColor = true;

    [Header("Refraction Settings")]
    public LayerMask refractableLayer;
    public string leftMaterial = "air";
    public string rightMaterial = "glass"; // or "water"
    public int maxRefractions = 3;

    [Header("Debug Refraction Settings")]
    public float incidenceAngle;
    public float refractionAngle;
   

    [Header("UI Refraction Settings")]
    [SerializeField] TextMeshProUGUI incidenceAngleText;
    [SerializeField] TextMeshProUGUI refractionAngleText;
    //[SerializeField] TextMeshProUGUI leftSliderValue;
    //[SerializeField] TextMeshProUGUI rightSliderValue;
    [SerializeField] Slider leftRefractiveSlider;
    [SerializeField] Slider rightRefractiveSlider;
    [SerializeField] ProceduralImage refractionAngleImage;
    [SerializeField] ProceduralImage incidenceAngleImage;
    [SerializeField] TextMeshProUGUI leftIORText;
    [SerializeField] TextMeshProUGUI rightIORText;

    [SerializeField] TMP_Dropdown leftDropDown;
    [SerializeField] TMP_Dropdown rightDropDown;

    [SerializeField] Slider laserColorSlider;
    [SerializeField] TextMeshProUGUI laserColorValue;
    [SerializeField] ProceduralImage laserColorImage;
    private Material laserMaterial;
    private Color beamColor;

    // Dictionary to store dispersion ranges
    private readonly Dictionary<string, (float ior380, float ior700)> materialIORs = new Dictionary<string, (float, float)>()
    {
        { "glass", (1.519f, 1.499f) },
        { "water", (1.345f, 1.332f) },
        { "air",   (1.00029f, 1.00027f) },
        { "custom", (1.0f, 1.0f) } // fallback
    };

    void Start()
    {
        laserColorSlider.onValueChanged.AddListener(delegate
        {
            wavelength = laserColorSlider.value;
            laserColorValue.text = laserColorSlider.value + "nm";
            laserColorImage.color = beamColor;

        });
        leftDropDown.onValueChanged.AddListener(delegate
        {
              leftMaterial = leftDropDown.options[leftDropDown.value].text.ToLower();

        });
        rightDropDown.onValueChanged.AddListener(delegate
        {
            rightMaterial = rightDropDown.options[rightDropDown.value].text.ToLower();
        });
        Shader shader = Shader.Find("Unlit/Color");
        laserMaterial = new Material(shader);

        leftRefractiveSlider.onValueChanged.AddListener(delegate {
            materialIORs["custom"] = (leftRefractiveSlider.value, leftRefractiveSlider.value); // flat
            leftMaterial = "custom";
           // leftSliderValue.text = leftRefractiveSlider.value.ToString("0.000");
        });

        rightRefractiveSlider.onValueChanged.AddListener(delegate {
            materialIORs["custom"] = (rightRefractiveSlider.value + 0.019f, rightRefractiveSlider.value - 0.001f); // mock dispersion
            rightMaterial = "custom";
            //rightSliderValue.text = rightRefractiveSlider.value.ToString("0.000");
        });
    }

    void Update()
    {
        beamColor = useWavelengthColor ? WavelengthToRGB(wavelength) : Color.red;
        if (laserMaterial && laserMaterial.color != beamColor)
            laserMaterial.color = beamColor;
    }

    void OnRenderObject()
    {
        if (!laserStart || !laserMaterial) return;

        laserMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.QUADS);
        GL.Color(beamColor);

        Vector3 origin = laserStart.position;
        Vector3 direction = laserStart.TransformDirection(laserDirection.normalized);
        float remainingLength = laserLength;
        Vector3 lastPoint = origin;

        float currentIOR = GetIORFromMaterial(leftMaterial, wavelength);
        if (leftIORText) leftIORText.text = $"{currentIOR:F4}";
        bool insideMaterial = false;

        for (int i = 0; i < maxRefractions && remainingLength > 0; i++)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, remainingLength, refractableLayer))
            {
                Vector3 hitPoint = hit.point;
                Vector3 normal = hit.normal;

                DrawQuadBeam(lastPoint, hitPoint, beamWidth);
                lastPoint = hitPoint;

                incidenceAngle = Vector3.Angle(-direction, normal);
                incidenceAngleText.text = incidenceAngle.ToString("F1");
                incidenceAngleImage.fillAmount = (0.25f / 90f) * incidenceAngle;


                string nextMaterial = insideMaterial ? leftMaterial : rightMaterial;
                float nextIOR = GetIORFromMaterial(nextMaterial, wavelength);
                if (rightIORText) rightIORText.text = $"{nextIOR:F4}";
             //   Debug.Log($"λ: {wavelength}nm | Left IOR: {currentIOR:F4} ({leftMaterial}) | Right IOR: {nextIOR:F4} ({nextMaterial})");


                if (Mathf.Approximately(currentIOR, nextIOR))
                {
                    direction = direction.normalized;
                    origin = hitPoint + direction * 0.001f;
                    remainingLength -= Vector3.Distance(hitPoint, origin);
                    continue;
                }

                Vector3 reflectDir = Vector3.Reflect(direction, normal);
                Vector3 reflectEnd = hitPoint + reflectDir * 0.5f;
                DrawQuadBeam(hitPoint, reflectEnd, beamWidth * 0.5f);

                float thetaI_rad = incidenceAngle * Mathf.Deg2Rad;
                float sinThetaT = (currentIOR / nextIOR) * Mathf.Sin(thetaI_rad);

                float reflectance = 1f, transmittance = 0f;
                Vector3 refractedDir = Vector3.zero;

                if (sinThetaT < 1f)
                {
                    float cosThetaI = Mathf.Cos(thetaI_rad);
                    float cosThetaT = Mathf.Sqrt(1f - sinThetaT * sinThetaT);

                    float rs = (currentIOR * cosThetaI - nextIOR * cosThetaT) / (currentIOR * cosThetaI + nextIOR * cosThetaT);
                    float rp = (currentIOR * cosThetaT - nextIOR * cosThetaI) / (currentIOR * cosThetaT + nextIOR * cosThetaI);

                    reflectance = (rs * rs + rp * rp) * 0.5f;
                    transmittance = 1f - reflectance;

                    refractedDir = RefractRay(direction, normal, currentIOR, nextIOR);
                }

                if (refractedDir == Vector3.zero)
                {
                    origin = hitPoint + reflectDir * 0.001f;
                    direction = reflectDir;
                    remainingLength -= Vector3.Distance(hitPoint, origin);
                    continue;
                }

                refractionAngle = Vector3.Angle(refractedDir, -normal);
                refractionAngleText.text = refractionAngle.ToString("F1");
                refractionAngleImage.fillAmount = (0.25f / 90f) * refractionAngle;

                origin = hitPoint + refractedDir * 0.001f;
                direction = refractedDir;
                remainingLength -= Vector3.Distance(hitPoint, origin);
                insideMaterial = !insideMaterial;
                currentIOR = nextIOR;
            }
            else
            {
                Vector3 endPoint = origin + direction * remainingLength;
                DrawQuadBeam(lastPoint, endPoint, beamWidth);
                break;
            }
        }

        GL.End();
        GL.PopMatrix();
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
        Vector3 cameraDir = Camera.main.transform.forward;
        Vector3 side = Vector3.Cross((end - start).normalized, cameraDir).normalized * width;
        GL.Vertex(start - side);
        GL.Vertex(start + side);
        GL.Vertex(end + side);
        GL.Vertex(end - side);
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
