using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR

using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[System.Serializable]
struct JsonImportData
{
    public string description;
    public float duration;
    public bool looping;
    public float masterVolume;
}

[ScriptedImporter(1, "vtfx")]
public class VtfxImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var clip = ScriptableObject.CreateInstance<HapticClip>();
        clip.json = File.ReadAllText(ctx.assetPath);

        // TODO: use VTFX Runtime to do this (will require editor time plugin instance)
        JsonImportData data = JsonUtility.FromJson<JsonImportData>(clip.json);
        clip.description = data.description;
        clip.duration = data.duration;
        clip.looping = data.looping;
        clip.masterVolume = data.masterVolume;

        ctx.AddObjectToAsset("HapticClip", clip);
        ctx.SetMainObject(clip);
    }

    [MenuItem("Assets/Create/Haptics/Haptic Clip", false)]
    private static void MakeHapticClip()
    {
        // ensure that a VTFX project file exists in the root of Assets
        string[] s = Application.dataPath.Split('/');
        string projectName = s[s.Length - 2];
        string vtfxProjectPath = Application.dataPath + "/" + projectName + ".vtfx_project";
        if (!System.IO.File.Exists(vtfxProjectPath))
        {
            System.IO.File.WriteAllText(vtfxProjectPath, blankProject);
        }

        ProjectWindowUtil.CreateAssetWithContent(
            "HapticClip.vtfx",
            blankEffect,
            Resources.Load<Texture2D>("haptic_clip_icon"));
    }

    // TODO: create dynamically from VTFX Runtime
    private static string blankEffect = "{\"description\": \"\",\"duration\": 0,\"looping\": false,\"masterVolume\": 1,\"nodeTypeVersionNumbers\": {},\"nodes\": [],\"tunables\": [],\"version\": 24}";
    private static string blankProject = "{  \"dataPathRelative\": \"StreamingAssets\",  \"deviceSettings\": {  },  \"deviceTunables\": [  ],  \"globalTunables\": [],  \"version\": 24}";

}

#endif // UNITY_EDITOR
