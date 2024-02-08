using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using PluginHandle = System.IntPtr;

public class HostAppPluginSingleton : MonoBehaviour {

    // Properties
    static public PluginHandle c_handle { get { return singleton?._cHandle ?? IntPtr.Zero; } }
    static public HostAppPluginSingleton singleton {
        get {
            if (!_singleton) {
                GameObject new_go = new GameObject("HostAppPluginSingleton");
                new_go.AddComponent<HostAppPluginSingleton>();
                Debug.Assert(_singleton);
            }
            return _singleton;
        }
    }

    // Private fields
    static HostAppPluginSingleton _singleton = null;
    PluginHandle _cHandle = IntPtr.Zero;

    void Awake() {
        if (_singleton != null) {
            Debug.LogError("Attempted to instatiate second HostAppPluginSingleton. Only one is allowed.");
            Application.Quit();
        }
        _singleton = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
        _cHandle = PluginManagerAPI.GetPlugin(PluginManagerSingleton.c_handle, "HostAppPlugin", PluginManagerAPI.ANY_PLUGIN_VERSION);
    }
}
