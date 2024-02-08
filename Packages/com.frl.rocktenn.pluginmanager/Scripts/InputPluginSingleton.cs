using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using PluginHandle = System.IntPtr;

public class InputPluginSingleton : MonoBehaviour {

    // Properties
    static public PluginHandle c_handle { get { return singleton?._cHandle ?? IntPtr.Zero; } }
    static public InputPluginSingleton singleton {
        get {
            if (!_singleton) {
                GameObject new_go = new GameObject("InputPluginSingleton");
                new_go.AddComponent<InputPluginSingleton>();
                Debug.Assert(_singleton);
            }
            return _singleton;
        }
    }

    // Private fields
    static InputPluginSingleton _singleton = null;
    PluginHandle _cHandle = IntPtr.Zero;

    void Awake() {
        if (_singleton != null) {
            Debug.LogError("Attempted to instatiate second InputPluginSingleton. Only one is allowed.");
            Application.Quit();
        }
        _singleton = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
        _cHandle = PluginManagerAPI.GetPlugin(PluginManagerSingleton.c_handle, "InputPlugin", PluginManagerAPI.ANY_PLUGIN_VERSION);
    }
}
