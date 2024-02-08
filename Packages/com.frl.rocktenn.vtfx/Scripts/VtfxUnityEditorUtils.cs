using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

using VtfxRuntimeContextHandle = System.IntPtr;
using EffectHandle = System.IntPtr;
using TunableHandle = System.IntPtr;

#if UNITY_EDITOR
public static class VtfxUnityEditorUtils
{
    public static void BuildTunableProperties(VtfxRuntimeContextHandle vtfx, EffectHandle effect)
    {
        var numTunables = VtfxLocalPluginAPI.Effect_GetNumTunables(vtfx, effect);
        for (int i = 0; i < numTunables; ++i)
        {
            TunableHandle tunable = VtfxLocalPluginAPI.Effect_GetTunableAtIndex(vtfx, effect, i);

            if (tunable != VtfxLocalPluginAPI.kInvalidHandle)
            {
                float rangeStart = VtfxLocalPluginAPI.GetTunableRangeStart(vtfx, tunable);
                float rangeEnd = VtfxLocalPluginAPI.GetTunableRangeEnd(vtfx, tunable);
                int valueType = VtfxLocalPluginAPI.GetTunableType(vtfx, tunable);
                string name = VtfxLocalPluginAPI.GetTunableName(vtfx, tunable);
                float currentValue = VtfxLocalPluginAPI.GetTunableValue(vtfx, tunable);

                bool haveValidRange = (rangeStart != 0) && (rangeEnd != 0);
                if (haveValidRange)
                {
                    if (valueType == (int)VtfxLocalPluginAPI.TunableType.Float)
                    {
                        float val = EditorGUILayout.Slider(name, currentValue, rangeStart, rangeEnd);
                        if (val != currentValue)
                        {
                            VtfxLocalPluginAPI.SetTunableValue(vtfx, tunable, val);
                        }
                    }
                    else if (valueType == (int)VtfxLocalPluginAPI.TunableType.Int)
                    {
                        int val = EditorGUILayout.IntSlider(name, (int)currentValue, (int)rangeStart, (int)rangeEnd);
                        if ((float)val != currentValue)
                        {
                            VtfxLocalPluginAPI.SetTunableValue(vtfx, tunable, (float)val);
                        }
                    }
                }
                else
                {
                    if (valueType == (int)VtfxLocalPluginAPI.TunableType.Float)
                    {
                        float val = EditorGUILayout.FloatField(name, currentValue);
                        if (val != currentValue)
                        {
                            VtfxLocalPluginAPI.SetTunableValue(vtfx, tunable, val);
                        }
                    }
                    else if (valueType == (int)VtfxLocalPluginAPI.TunableType.Int)
                    {
                        int val = EditorGUILayout.IntField(name, (int)currentValue);
                        if ((float)val != currentValue)
                        {
                            VtfxLocalPluginAPI.SetTunableValue(vtfx, tunable, (float)val);
                        }
                    }
                }
            }
        }
    }
}
#endif
