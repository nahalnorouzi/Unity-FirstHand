using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VtfxPlayEffectOnKeyPress : MonoBehaviour
{

    public VtfxEffectsLibrary _library;
    public string effect;
    public KeyCode key = KeyCode.Space;

    private System.IntPtr _effectHandle = VtfxLocalPluginAPI.kInvalidHandle;

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            if (_effectHandle == System.IntPtr.Zero)
            {
                _effectHandle = _library.Play(effect);
            }
            else
            {
                _library.Vtfx_ReleaseEffect(_effectHandle);
                _effectHandle = VtfxLocalPluginAPI.kInvalidHandle;
            }
        }

    }
}
