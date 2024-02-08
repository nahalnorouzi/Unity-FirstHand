using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VtfxHandle = System.IntPtr;
using VtfxRuntimeContextHandle = System.IntPtr;

public class VtfxDeviceList : MonoBehaviour
{
    public bool alwaysUpdate = true;
    public string[] devices;

    private VtfxRuntimeContextHandle _vtfx = System.IntPtr.Zero;

    // Start is called before the first frame update
    void Start()
    {
        _vtfx = VtfxLocalSingleton.c_handle;
        GenerateList();
    }

    // Update is called once per frame
    void Update()
    {
        if (alwaysUpdate)
            GenerateList();
    }

    private void GenerateList()
    {
        int n = VtfxLocalPluginAPI.GetNumDevices(_vtfx);
        VtfxHandle[] deviceHandles = new VtfxHandle[n];
        System.Array.Resize(ref devices, n);
        VtfxLocalPluginAPI.GetDevices(_vtfx, deviceHandles, n, out n);
        for (int i = 0; i < n; ++i)
        {
            var dev = deviceHandles[i];
            devices[i] = VtfxLocalPluginAPI.Device_GetName(_vtfx, dev);
        }
    }
}
