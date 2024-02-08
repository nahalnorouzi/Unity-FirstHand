using UnityEngine;

using VtfxDeviceHandle = System.IntPtr;

public class HapticDeviceListenerPose : MonoBehaviour
{
    public string deviceName;

    private string _cachedDeviceName;
    private VtfxDeviceHandle _cachedDeviceHandle;

    void Update()
    {
        // cache the device handle for as long as the name is unchanged
        if (_cachedDeviceName != deviceName) {
            VtfxLocalSingleton.singleton.UnregisterExcludedPoseDevice(_cachedDeviceHandle);
            var vtfx = VtfxLocalSingleton.c_handle;
            _cachedDeviceHandle = VtfxLocalPluginAPI.FindDeviceByName(vtfx, deviceName);
            _cachedDeviceName = deviceName;
            VtfxLocalSingleton.singleton.RegisterExcludedPoseDevice(_cachedDeviceHandle);
        }

        VtfxLocalSingleton.singleton.SetDeviceListenerPose(_cachedDeviceHandle, transform);
    }

    void OnDestroy()
    {
        VtfxLocalSingleton.singleton.UnregisterExcludedPoseDevice(_cachedDeviceHandle);
        _cachedDeviceHandle = VtfxLocalPluginAPI.kInvalidHandle;
        _cachedDeviceName = null;
    }
}
