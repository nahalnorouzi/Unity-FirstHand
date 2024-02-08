using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using PluginManagerHandle = System.IntPtr;


// Singleton access to PluginManager
// May be lazily evaluated at run-time
// May be configured at editor-time.
//     - Requires setting script execution order for PluginManagerSingleton to occur first
public class PluginManagerSingleton : MonoBehaviour
{
    // Static C-Pointer
    static public PluginManagerHandle c_handle { get { return singleton._cHandle; } }

    // Static C# Object
    static public PluginManagerSingleton singleton
    {
        get
        {
            if (destroyed)
            {
                throw new System.Exception("Attempted to re-create PluginManagerSingleton after it was destroyed");
            }

            if (_singleton == null)
            {
                GameObject new_go = new GameObject("PluginManagerSingleton");
                new_go.AddComponent<PluginManagerSingleton>();
                Debug.Assert(_singleton != null); // Awake should set
            }

            return _singleton;
        }
    }

    // Properties
    static public bool destroyed { get; private set; }

    // Invoked just before the PluginManager is destroyed (and subsequntly before all other plugins are destroyed too).
    public UnityEvent onPreDestroy { get { return _onPreDestroy; } }

    // Fields
    static PluginManagerSingleton _singleton = null;
    PluginManagerHandle _cHandle = IntPtr.Zero;
    UnityEvent _onPreDestroy = new UnityEvent();
    PluginManagerAPI.LogCallbackFn _logCb;

    // Methods
    void Awake()
    {
        if (_singleton != null)
        {
            Debug.LogError("Attempted to instatiate second PluginManagerSingleton. Only one is allowed.");
            Application.Quit();
        }

        _singleton = this;
        GameObject.DontDestroyOnLoad(this.gameObject);

        // Create PluginManager
        _cHandle = PluginManagerAPI.Initialize();

        // Register logger
        // NOTE: This callback setup does not appear to work in all cases on 2019.4.0f1.
        // It works in the basic case and the PluginManager example scene. However when
        // other Unity game objects make calls into Native C++ plugins and those plugins
        // trigger a log Unity explodes in the guts of mono. Why? Not sure.
        // This behavior was confirmed to be fixed in Unity 2020.3.16f1. The exact version
        // where it was fixed is currently unknown. - Forrest Smith 11/17/2021
        // But does not work on Mac with 2020.3.16 - Brenton Rayner
#if UNITY_2020_3_OR_NEWER && !(UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
        _logCb = new PluginManagerAPI.LogCallbackFn(RocktennLog);
        PluginManagerAPI.SetLogCallback(_cHandle, _logCb);
#endif

        // Add NativePluginLoaderSingleton paths to PluginManager because there may be Plugins not exposed to unity
        var paths = NativePluginLoaderSingleton.singleton.LoadedPluginDirectories;
        if (paths != null)
        {
            foreach (var path in paths)
            {
                Debug.Log(string.Format("PluginManagerSingleton adding plugin search path: [{0}]", path));
                PluginManagerAPI.AddPluginPath(_cHandle, path);
            }
        }

#if !(UNITY_ANDROID && !UNITY_EDITOR) && !(UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
        // Load RemoteLogClient so we can see plugin log messages in RemoteLogViewer
        if (RemoteLogClientSingleton.singleton == null) {
          Debug.LogWarning("Failed to get RemoteLogClient singleton");
        }
        // Load InputPlugin so we can call InputPlugin::HostAPI::StartFrame, allowing ImGui debug panels to handle input
        if (InputPluginSingleton.singleton == null) {
          Debug.LogWarning("Failed to get InputPlugin singleton");
        }
#endif
        // Load HostAppPlugin so plugins can get a main thread tick
        if (HostAppPluginSingleton.singleton == null) {
          Debug.LogWarning("Failed to get HostAppPlugin singleton");
        }
    }

    void Update() {
        // Tick InputPlugin and HostApp::MainThreadTick at some reasonable rate (traditionally, the render frame rate).
        // These plugins should be ticked together, and InputPlugin should be ticked first.
        // This allows native plugins that rely on InputPlugin and HostAppPlugin to function.
#if !(UNITY_ANDROID && !UNITY_EDITOR) && !(UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
        if (InputPluginSingleton.singleton != null) {
            if (InputPluginAPI.HostAPI_StartFrame != null) {
                InputPluginAPI.HostAPI_StartFrame(InputPluginSingleton.c_handle);
            }
        }
#endif
        if (HostAppPluginSingleton.singleton != null) {
            if (HostAppPluginAPI.OnMainThreadTick != null) {
                HostAppPluginAPI.OnMainThreadTick(HostAppPluginSingleton.c_handle);
            }
        }
    }

    void OnApplicationQuit() {
        destroyed = true;
        if (_cHandle != IntPtr.Zero) {
            Debug.Log("Destroying plugin manager");
            if (_onPreDestroy != null)
                _onPreDestroy.Invoke();
            PluginManagerAPI.Destroy(_cHandle);
            _cHandle = IntPtr.Zero;
            _singleton = null;
        }
    }

    public IntPtr GetPlugin(string pluginName, int requestedVersionNumber)
    {
        if (_cHandle == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        return PluginManagerAPI.GetPlugin(_cHandle, pluginName, requestedVersionNumber);
    }

    public IntPtr GetPluginWithFlags(string pluginName, int requestedVersionNumber, PluginManagerAPI.Flags flags)
    {
        if (_cHandle == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        return PluginManagerAPI.GetPluginWithFlags(_cHandle, pluginName, requestedVersionNumber, (int)flags);
    }

    // This function gets called via callback from C++. It will NOT be called from the Unity main thread
    // Function MUST be static and tagged with MonoPInvokeCallback to work with il2cpp
    [AOT.MonoPInvokeCallback(typeof(PluginManagerAPI.LogCallbackFn))]
    static void RocktennLog(int level, string msg, string file, int line) {
        if (level == 0) {
            Debug.Log(msg);
        } else {
            Debug.LogWarning(msg);
        }
    }
}
