using UnityEditor;
using UnityEngine;
using System.IO;

public class PrefabBundleBuilder
{
    [MenuItem("Faiz/Android/Build Prefab Bundle")]
    static void BuildAll()
    {
        string path = "Assets/StreamingAssets";

        // 🔥 Step 1: Delete all existing files in StreamingAssets
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (!file.EndsWith(".meta")) // avoid deleting .meta files
                {
                    File.Delete(file);
                }
            }
        }
        else
        {
            Directory.CreateDirectory(path);
        }

        // 🛠 Step 2: Build new AssetBundles
        BuildPipeline.BuildAssetBundles(
            path,
            BuildAssetBundleOptions.None,
            BuildTarget.WebGL
        );

        Debug.Log("✅ Prefab bundles built successfully and old files cleared!");
    }
}