using UnityEngine;

public class LaserRayDrawer : MonoBehaviour
{
    public Transform laserOrigin;         // Set this to the front of your laser
    public float maxDistance = 100f;
    public Color laserColor = Color.red;

    private static Material _lineMaterial;

    private void Awake()
    {
        CreateLineMaterial();
    }

    void CreateLineMaterial()
    {
        if (_lineMaterial == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            _lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void OnRenderObject()
    {
        if (!laserOrigin) return;

        Vector3 origin = laserOrigin.position;
        Vector3 direction = laserOrigin.forward;
        Vector3 endPoint = origin + direction * maxDistance;

        // Optional raycast
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
        {
            endPoint = hit.point;
        }

        _lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(laserColor);
        GL.Vertex(origin);
        GL.Vertex(endPoint);
        GL.End();
    }
}