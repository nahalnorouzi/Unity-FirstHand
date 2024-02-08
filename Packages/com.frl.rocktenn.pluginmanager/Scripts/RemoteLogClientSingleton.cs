using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using PluginHandle = System.IntPtr;

public class RemoteLogClientSingleton : MonoBehaviour {

    // Properties
    static public PluginHandle c_handle { get { return singleton?._cHandle ?? IntPtr.Zero; } }
    static public RemoteLogClientSingleton singleton {
        get {
            if (!_singleton) {
                GameObject new_go = new GameObject("RemoteLogClientSingleton");
                new_go.AddComponent<RemoteLogClientSingleton>();
                Debug.Assert(_singleton);
            }
            return _singleton;
        }
    }

    // Public Fields
    public string _ipAddress = "127.0.0.1";
    public string _appName = "Unity";
    public bool _retry = true;
    public bool _launchOnly = false;
    public bool _includeStackTrace = false;

    // Private fields
    static RemoteLogClientSingleton _singleton = null;
    PluginHandle _cHandle = IntPtr.Zero;
    StringBuilder _stringBuilder = new StringBuilder();

    void Awake() {
        if (_singleton != null) {
            Debug.LogError("Attempted to instatiate second RemoteLogClientSingleton. Only one is allowed.");
            Application.Quit();
        }
        _singleton = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
        _cHandle = PluginManagerAPI.GetPlugin(PluginManagerSingleton.c_handle, "RemoteLogClient", PluginManagerAPI.ANY_PLUGIN_VERSION);

        // Launch and connect
        RemoteLogClientAPI.LaunchAndConnectToLogViewerApp(_cHandle, _ipAddress, _appName, _retry, _launchOnly);

        // Start capturing log messages
        Application.logMessageReceivedThreaded += OnLogMessageReceived;
    }

    void OnLogMessageReceived(string message, string stackTrace, LogType type) {
        if (PluginManagerSingleton.destroyed) {
            return; // RemoteLogClient.dll has been unloaded already; e.g. if we are shutting down
        }
        if (type == LogType.Log) {
            _stringBuilder.Clear();
            _stringBuilder.AppendFormat("(Managed) {0}", message);
            RemoteLogClientAPI.Log(_cHandle, _stringBuilder.ToString());
        }
        else {
            _stringBuilder.Clear();
            _stringBuilder.AppendFormat("{0}: (Managed) {1}", type.ToString(), message);
            RemoteLogClientAPI.Log(_cHandle, _stringBuilder.ToString());
        }
        if (_includeStackTrace) {
            RemoteLogClientAPI.Log(_cHandle, stackTrace);
        }
    }
}
