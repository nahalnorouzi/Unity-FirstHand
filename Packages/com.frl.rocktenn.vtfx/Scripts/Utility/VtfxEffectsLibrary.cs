using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

using VtfxRuntimeContextHandle = System.IntPtr;
using EffectHandle = System.IntPtr;


public class VtfxEffectsLibrary : MonoBehaviour
{
    // Inspector fields
    [System.Obsolete("LibraryPath deprecated. Please upgrade to LibraryJsonAsset + LibraryDataRoot instead", false)]
    public string _libraryPath;

    [Tooltip("Json file containing effects")]
    public TextAsset _libraryJsonAsset;
    [Tooltip("StreamingAssets-relative path to data root for effects file")]
    public string _libraryDataRoot;

    public int _numLoadedEffects = 0;


    // Private fields
    List<EffectEntry> _effectEntries = new List<EffectEntry>();


    // Methods
    void Start() {
        LoadEffects();
    }

    public void LoadEffects()
    {
        _effectEntries.Clear();

#if !UNITY_EDITOR && UNITY_ANDROID
        // Android *only* supports text asset
        Vtfx_LoadEffectsJson(_libraryJsonAsset.text, _libraryDataRoot);
#else
        if (_libraryJsonAsset) {
            Vtfx_LoadEffectsJson(_libraryJsonAsset.text, _libraryDataRoot);
        }
        else {
            Debug.LogError("Attempted to use old effect loading code. Need to upgrade.");
#if false // This is the old version that loads effects from a single json. Don't use it, switch to using individual .vtfx assets.
            var finalPath = Osiris.UnityStreamingAssetsHelper.FindStreamingAsset(_libraryPath) ?? _libraryPath;
            try {
                var jsonData = System.IO.File.ReadAllText(finalPath);
                Vtfx_LoadEffectsJson(jsonData, _libraryDataRoot);
            } catch(System.Exception e) {
                Debug.LogError($"VtfxEffectsLibrary::LoadEffects unable to load effects from _libraryPath [{_libraryPath}]. Exception: [{e.Message}]");
            }
#endif
        }
#endif

            _numLoadedEffects = Vtfx_GetNumEffects();
        for (int i = 0; i < _numLoadedEffects; ++i)
        {
            EffectHandle effect = Vtfx_GetEffectByIndex(i);
            if (effect == VtfxLocalPluginAPI.kInvalidHandle)
                continue;

            string effectName = Vtfx_Effect_GetName(effect);

            _effectEntries.Add(new EffectEntry(effectName, effect));
        }
    }

    public EffectHandle Play(int index)
    {
        if (index < 0 || index >= _effectEntries.Count)
        {
            return VtfxLocalPluginAPI.kInvalidHandle;
        }

        var entry = _effectEntries[index];
        var effectName = entry.name;
        EffectHandle newEffect = Vtfx_Effect_Clone(entry.handle, effectName + " (clone)");
        Vtfx_Effect_SetPlaying(newEffect, true);
        return newEffect;
    }

    public EffectHandle Play(string effectName)
    {
        for (int i = 0; i < _effectEntries.Count; ++i)
        {
            var entry = _effectEntries[i];
            if (entry.name == effectName)
                return Play(i);
        }

        return VtfxLocalPluginAPI.kInvalidHandle;
    }

    public void GetEffectNames(List<string> names)
    {
        if (names == null)
            names = new List<string>();
        names.Clear();
        names.Capacity = _effectEntries.Count;

        foreach (var entry in _effectEntries)
        {
            names.Add(entry.name);
        }
    }

    // Helpers
    class EffectEntry
    {
        public string name;
        public EffectHandle handle;

        public EffectEntry(string name, EffectHandle handle)
        {
            this.name = name;
            this.handle = handle;
        }
    }

    // Short-term hacky wrappers around VTFXPluginAPI
    // Will soon be replaced with unified API. - Forrest (1/22/2021)
    VtfxRuntimeContextHandle vtfx {
        get {
            return VtfxLocalSingleton.c_handle;
        }
    }

    void Vtfx_LoadEffects(string path) {
        VtfxLocalPluginAPI.LoadEffects(vtfx, path);
    }

    void Vtfx_LoadEffectsJson(string json, string dataPathRoot) {
        VtfxLocalPluginAPI.LoadEffectsJson(vtfx, json, dataPathRoot);
    }

    int Vtfx_GetNumEffects() {
        return VtfxLocalPluginAPI.GetNumEffects(vtfx);
    }

    EffectHandle Vtfx_GetEffectByIndex(int index) {
        return VtfxLocalPluginAPI.GetEffectByIndex(vtfx, index);
    }

    string Vtfx_Effect_GetName(EffectHandle effect) {
        return VtfxLocalPluginAPI.Effect_GetName(vtfx, effect);
    }

    EffectHandle Vtfx_Effect_Clone(EffectHandle effect, string name) {
        return VtfxLocalPluginAPI.Effect_Clone(vtfx, effect, name);
    }

    void Vtfx_Effect_SetPlaying(EffectHandle effect, bool playing) {
        VtfxLocalPluginAPI.Effect_SetPlaying(vtfx, effect, playing);
    }

    public void Vtfx_ReleaseEffect(EffectHandle effect) {
        if (effect != VtfxLocalPluginAPI.kInvalidHandle) {
            VtfxLocalPluginAPI.ReleaseEffect(vtfx, effect);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(VtfxEffectsLibrary))]
public class VtfxEffectsLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VtfxEffectsLibrary vel = (VtfxEffectsLibrary)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Reload Effects")) {
                vel.LoadEffects();
            }
        }
    }
}
#endif
