using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using ADClientHandle = System.IntPtr;
using ADDeviceHandle = System.UInt64;

public class ADClientSingleton : MonoBehaviour {

    // Statics
    static public ADClientHandle c_handle { get { return singleton._cHandle; } }

    static public ADClientSingleton singleton {
        get {
            if (PluginManagerSingleton.destroyed)
            {
                // Open question: silent null or throw exception? Opinions welcome.
                return null;
            }
            
            if (_singleton == null) {
                GameObject new_go = new GameObject("ADClientSingleton");
                new_go.AddComponent<ADClientSingleton>();
                Debug.Assert(_singleton != null); // Awake should set
            }

            return _singleton;
        }
    }



    // Private Fields
    static ADClientSingleton _singleton = null;
    ADClientHandle _cHandle = System.IntPtr.Zero;


    // Unity Methods
    void Awake()
    {
        if (_singleton != null) {
            Debug.LogError("Attempted to instatiate second ADClientSingleton. Only one is allowed.");
            Application.Quit();
        }

        _singleton = this;
        GameObject.DontDestroyOnLoad(this.gameObject);

        // Initialize ADClientAPI
        _cHandle = ADClientAPI.Initialize(PluginManagerSingleton.c_handle);
        Debug.Log("ADClientSingleton.Start created ADClient");

        // Connect
        ADClientAPI.Connect(_cHandle, "127.0.0.1");
    }

    static public List<ADClientAPI.DeviceInfo> GetAllDevices()
    {
        List<ADClientAPI.DeviceInfo> devices = new List<ADClientAPI.DeviceInfo>();

        int numDevices = ADClientAPI.GetNumDeviceInfos(c_handle);
        for (int i = 0; i < numDevices; ++i)
        {
            var deviceInfo = ADClientAPI.GetDeviceInfoByIndex(c_handle, i);
            if (deviceInfo != null)
                devices.Add(deviceInfo);
        }

        return devices;
    }

    static public ADClientAPI.DeviceInfo FindDevice(string name)
    {
        var devices = GetAllDevices();
        foreach (var device in devices)
        {
            if (device.name.Contains(name))
                return device;
        }

        return null;
    }

    static public ADClientAPI.DeviceInfo FindDevice(string name, ADClientAPI.DeviceHandedness handedness)
    {
        var devices = GetAllDevices();
        foreach (var device in devices) {
            if (device.name.Contains(name) && device.handedness == handedness)
                return device;
        }

        return null;
    }
}
