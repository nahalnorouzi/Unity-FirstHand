using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EffectHandle = System.IntPtr;
using Handedness = VtfxLocalPluginAPI.DeviceHandedness;

public class HapticListener : MonoBehaviour
{
    public Handedness handedness;

    void Awake()
    {
        listeners.Add(this);
    }

    void OnDestroy()
    {
        listeners.Remove(this);
    }

    internal static List<HapticListener> listeners = new List<HapticListener>();
}
