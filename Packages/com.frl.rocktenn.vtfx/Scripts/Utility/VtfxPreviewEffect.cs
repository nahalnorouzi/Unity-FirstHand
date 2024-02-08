using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using EffectHandle = System.IntPtr;


public class VtfxPreviewEffect : MonoBehaviour {

    // Inspector fields
    public VtfxEffectsLibrary _library;
    public List<string> _loadedEffects = new List<string>();
    public int _previewEffect = 0;


    // Private fields
    EffectHandle _previewHandle = VtfxLocalPluginAPI.kInvalidHandle;

    // Unity methods
    void Update() {
        _library.GetEffectNames(_loadedEffects);
    }

    void OnDisable() {
        Stop();
    }

    void OnApplicationQuit() {
        Stop();
    }


    // Methods
    public void Preview() {
        if (_library && _previewEffect >= 0 && _previewEffect < _loadedEffects.Count) {
            // Stop old preview
            Stop();

            // Play new effect
            string effectName = _loadedEffects[_previewEffect];
            _previewHandle = _library.Play(effectName);
        }
    }

    public void Stop() {
        if (_previewHandle != VtfxLocalPluginAPI.kInvalidHandle) {
            _library.Vtfx_ReleaseEffect(_previewHandle);
            _previewHandle = VtfxLocalPluginAPI.kInvalidHandle;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(VtfxPreviewEffect))]
public class VtfxPreviewEffectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VtfxPreviewEffect vpe = (VtfxPreviewEffect)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Preview"))
                vpe.Preview();
            if (GUILayout.Button("Stop"))
                vpe.Stop();
        }
    }
}
#endif
