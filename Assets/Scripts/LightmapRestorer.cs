using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LightmapRestorer : MonoBehaviour
{
    [Header("Lightmap Settings")]
    public Texture2D[] lightmapTextures;
    public Renderer[] renderers;
    public int[] lightmapIndices;
    public Vector4[] scaleOffsets;

    [Header("Lighting Environment")]
    public Material skyboxMaterial;     // 🎨 Assign your Skybox here
    public Light sunSource;            // ☀️ Assign your Sun Light here

#if UNITY_EDITOR
    [ContextMenu("Collect Lightmap Data")]
    void CollectLightmapData()
    {
        List<Renderer> rList = new List<Renderer>(GetComponentsInChildren<Renderer>());
        List<int> indexList = new List<int>();
        List<Vector4> offsetList = new List<Vector4>();

        foreach (var r in rList)
        {
            if (r.lightmapIndex >= 0)
            {
                indexList.Add(r.lightmapIndex);
                offsetList.Add(r.lightmapScaleOffset);
            }
        }

        renderers = rList.ToArray();
        lightmapIndices = indexList.ToArray();
        scaleOffsets = offsetList.ToArray();

        Debug.Log($"Lightmap data collected for {renderers.Length} renderers.");
    }
#endif

    void Start()
    {
        ApplyLightmaps();
        ApplySkybox();
        ApplySunSource();
    }

    private void ApplyLightmaps()
    {
        if (lightmapTextures == null || lightmapTextures.Length == 0) return;

        LightmapData[] newLightmaps = new LightmapData[lightmapTextures.Length];
        for (int i = 0; i < lightmapTextures.Length; i++)
        {
            newLightmaps[i] = new LightmapData();
            newLightmaps[i].lightmapColor = lightmapTextures[i];
        }
        LightmapSettings.lightmaps = newLightmaps;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].lightmapIndex = lightmapIndices[i];
                renderers[i].lightmapScaleOffset = scaleOffsets[i];
            }
        }
    }

    private void ApplySkybox()
    {
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment(); // 🌍 Required for skybox reflections
        }
    }

    private void ApplySunSource()
    {
        if (sunSource != null)
        {
            RenderSettings.sun = sunSource;
        }
    }
}
