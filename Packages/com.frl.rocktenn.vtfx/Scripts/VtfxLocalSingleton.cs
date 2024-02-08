using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using AOT;
using Osiris;
using UnityEngine;
using UnityEngine.Networking;
using VtfxRuntimeContextHandle = System.IntPtr;
using VtfxDeviceHandle = System.IntPtr;
using EffectHandle = System.IntPtr;
using NodeHandle = System.IntPtr;
using ResourceLoaderHandle = System.IntPtr;
using CStr = System.IntPtr;

public class VtfxLocalSingleton : MonoBehaviour
{
    // Properties
    static public VtfxRuntimeContextHandle c_handle { get { return singleton?._cHandle ?? IntPtr.Zero; } }
    static public VtfxLocalSingleton singleton {
        get {
            if (PluginManagerSingleton.destroyed)
            {
                // Open question: silent null or throw exception? Opinions welcome.
                return null;
            }

            if (!_singleton) {
                GameObject new_go = new GameObject("VtfxLocalSingleton");
                new_go.AddComponent<VtfxLocalSingleton>();
                Debug.Assert(_singleton);
                GameObject.DontDestroyOnLoad(new_go);
            }

            return _singleton;
        }
    }

    // Public Fields
    public bool _ensureEarsDeviceExists = true;
    public bool _spinConnect = true; // VtfxLocalPluginAPI calls fail if not connected. If spinConnect disabled USER must take appropriate caution.
    public VtfxLocalPluginAPI.LogLevel _logLevel = VtfxLocalPluginAPI.LogLevel.Info;
    public bool _useDebugOptions = false;

    // Private fields
    static VtfxLocalSingleton _singleton = null;
    VtfxRuntimeContextHandle _cHandle = IntPtr.Zero;
    ResourceLoaderHandle _resourceLoaderHandle;
    VtfxLocalPluginAPI.LoadResourceFn _loadResourceCb;
    VtfxLocalPluginAPI.LogLevel _lastSetLogLevel;
    private Dictionary<EffectHandle, List<OnHapticEffectFinishedDelegate>> _onEffectCompleteDelegates;
    private HashSet<VtfxDeviceHandle> _excludeDevicesPoseUpdate = new HashSet<VtfxDeviceHandle>();

    // ResourceLoad data
    class LoadRequest {
        public string resourceName;
        public IntPtr loaderUserData;
        public IntPtr requestUserData;
        public VtfxLocalPluginAPI.LoadResourceOnSuccessCb onSuccess;
        public VtfxLocalPluginAPI.LoadResourceOnFailCb onFail;
    };
    static ConcurrentQueue<LoadRequest> _requests = new ConcurrentQueue<LoadRequest>();

    // Unity Methods
    void Awake()
    {
        if (_singleton != null)
        {
            Debug.LogError("Attempted to instantiate second VtfxLocalSingleton. Only one is allowed.");
            Application.Quit();
        }

        // Ensure paths in assets helper on main thread
        UnityStreamingAssetsHelper.EnsurePaths();

        _singleton = this;

        _cHandle = VtfxLocalPluginAPI.CreateRuntimeContext(PluginManagerSingleton.c_handle);

        VtfxLocalPluginAPI.Initialize(_cHandle, _useDebugOptions);

        VtfxLocalPluginAPI.SetLogLevel(_cHandle, _logLevel);
        _lastSetLogLevel = _logLevel;

        VtfxLocalPluginAPI.TryLoadingAllVtfxOutputPlugins(_cHandle);

        // Ensure ears device exists
        if (_ensureEarsDeviceExists)
        {
            PluginManagerSingleton.singleton.GetPlugin("VtfxXAudio2OutputPlugin", 0);
            VtfxLocalPluginAPI.EnsureEarsDeviceExists(_cHandle);
        }

        // Allow VTFXPlugin native code to call a Unity-based resourceLoader
        _loadResourceCb = new VtfxLocalPluginAPI.LoadResourceFn(LoadResource);
        _resourceLoaderHandle = VtfxLocalPluginAPI.RegisterResourceLoader(
            c_handle,
            "UnityAssetLoader",
            _loadResourceCb,
            IntPtr.Zero,
            null);

        // Log internetReachability. Accessing this in script is required for Unity to inject the
        // Android permission ACCESS_NETWORK_STATE which is (maybe?) required for RemoteDebugger to work.
        Debug.Log($"VtfxLocalSingleton - NetworkState: [{Application.internetReachability}]");

        _onEffectCompleteDelegates = new Dictionary<EffectHandle, List<OnHapticEffectFinishedDelegate>>();

        // Register shutdown code with PluginManagerSingleton in case it decides to go before VtfxLocalSingleton.
        PluginManagerSingleton.singleton.onPreDestroy.AddListener(TryShutdownVtfxRuntime);
    }

    void OnApplicationQuit()
    {
        TryShutdownVtfxRuntime();
    }

    void TryShutdownVtfxRuntime()
    {
        if (_cHandle != IntPtr.Zero)
        {
            VtfxLocalPluginAPI.PreShutDown(_cHandle);
            VtfxLocalPluginAPI.ShutDown(_cHandle);
            VtfxLocalPluginAPI.DestroyRuntimeContext(_cHandle);
            _cHandle = IntPtr.Zero;
        }
    }

    void Start()
    {
#if DEVELOPMENT_BUILD
        VtfxLocalPluginAPI.SetRemoteControlBroadcastEnabled(c_handle, true);
#endif
        StartCoroutine(LoaderRoutine());
    }

    void Update()
    {
        var vtfxRCH = c_handle;

        List<EffectHandle> finishedEffects = new List<EffectHandle>();
        foreach (KeyValuePair<EffectHandle, List<OnHapticEffectFinishedDelegate>> kvp in _onEffectCompleteDelegates)
        {
            if (!VtfxLocalPluginAPI.Effect_IsValid(vtfxRCH, kvp.Key))
            {
                foreach (var finishedDelegate in kvp.Value)
                {
                    try
                    {
                        finishedDelegate();
                    }
                    catch (Exception)
                    {
                        // Failed to run registered delegate, likely due to registree being destroyed.
                        // This is not invalid behavior, unregistering from delegates list
                    }
                }

                finishedEffects.Add(kvp.Key);
            }
        }

        foreach (EffectHandle finishedEffectHandle in finishedEffects)
        {
            _onEffectCompleteDelegates.Remove(finishedEffectHandle);
        }
    }

    public void SetDeviceListenerPose(VtfxDeviceHandle deviceHandle, Transform transform)
    {
        if (deviceHandle == VtfxLocalPluginAPI.kInvalidHandle)
        {
            return;
        }

        var vtfxRCH = c_handle;

        var ovrPose = ToOVRPose(transform);
        var headPos = ovrPose.Item1;
        var headOrient = ovrPose.Item2;

        Vector3 fwd = headOrient * Vector3.forward;
        Vector3 up = headOrient * Vector3.up;
        VtfxLocalPluginAPI.Device_SetListenerPose(vtfxRCH, deviceHandle,
            headPos.x, headPos.y, headPos.z,
            -fwd.x, -fwd.y, -fwd.z,
            up.x, up.y, up.z);
    }

    public void RegisterExcludedPoseDevice(VtfxDeviceHandle deviceHandle)
    {
        if (deviceHandle != VtfxLocalPluginAPI.kInvalidHandle)
        {
            _excludeDevicesPoseUpdate.Add(deviceHandle);
        }
    }

    public void UnregisterExcludedPoseDevice(VtfxDeviceHandle deviceHandle)
    {
        if (deviceHandle != VtfxLocalPluginAPI.kInvalidHandle)
        {
            _excludeDevicesPoseUpdate.Remove(deviceHandle);
        }
    }

    void LateUpdate()
    {
        var vtfxRCH = c_handle;

        // Refresh log level if needed
        if (_lastSetLogLevel != _logLevel)
        {
            VtfxLocalPluginAPI.SetLogLevel(vtfxRCH, _logLevel);
            _lastSetLogLevel = _logLevel;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            // Camera.main is null, use Camera.current instead.
            // This was required in Unity 2020.3.27f1
            camera = Camera.current;
        }

        if (camera)
        {
            VtfxDeviceHandle[] handles = new VtfxDeviceHandle[64];
            VtfxLocalPluginAPI.GetDevices(vtfxRCH, handles, 64, out int numDevices);

            foreach (var deviceHandle in handles)
            {
                // Set the listener's pose in world-space
                if (!_excludeDevicesPoseUpdate.Contains(deviceHandle))
                {
                    SetDeviceListenerPose(deviceHandle, camera.transform);
                }
            }
        }
    }

    public void RegisterOnEffectFinishedCallback(EffectHandle effect, OnHapticEffectFinishedDelegate callback)
    {
        List<OnHapticEffectFinishedDelegate> foundDelegates;
        bool found = _onEffectCompleteDelegates.TryGetValue(effect, out foundDelegates);

        if(found)
        {
            foundDelegates.Add(callback);
        }
        else
        {
            List<OnHapticEffectFinishedDelegate> newList = new List<OnHapticEffectFinishedDelegate>();
            newList.Add(callback);
            _onEffectCompleteDelegates[effect] = newList;
        }
    }

    // Same code at OVRCommon.cs::ToOVRPose. But without the hard dependency.
    static Tuple<Vector3, Quaternion> ToOVRPose(Transform t)
    {
        return new Tuple<Vector3, Quaternion>(
            new Vector3(t.position.x, t.position.y, -t.position.z),
            new Quaternion(-t.rotation.x, -t.rotation.y, t.rotation.z, t.rotation.w)
        );
    }

    // This function gets called via callback from C++. It will NOT be called from the Unity main thread
    // Function MUST be static and tagged with MonoPInvokeCallback to work with il2cpp
    [MonoPInvokeCallback(typeof(VtfxLocalPluginAPI.LoadResourceFn))]
    static void LoadResource(
        string resourceName,
        IntPtr loaderUserData,
        IntPtr requestUserData,
        VtfxLocalPluginAPI.LoadResourceOnSuccessCb onSuccess,
        VtfxLocalPluginAPI.LoadResourceOnFailCb onFail)
    {
        // Create new request
        string assetPath = Osiris.UnityStreamingAssetsHelper.FindStreamingAsset(resourceName);
#if !UNITY_EDITOR && UNITY_ANDROID
        // Android must load from compressed "streamingAssets"
        string resolvedPath = "jar:file://" + assetPath;
#else
        string resolvedPath = "file://" + assetPath;
#endif

        LoadRequest request = new LoadRequest {
            resourceName = resolvedPath,
            loaderUserData = loaderUserData,
            requestUserData = requestUserData,
            onSuccess = onSuccess,
            onFail = onFail
        };

        // Add request to queue. Will be pulled from LoaderRoutine
        _requests.Enqueue(request);
    }

    IEnumerator LoaderRoutine()
    {
        while (true) {
            // Get next request
            LoadRequest request = null;
            bool success = _requests.TryDequeue(out request);

            // If no request sleep until next frame
            if (!success) {
                yield return null;
                continue;
            }

            UnityWebRequest uwr = new UnityWebRequest(request.resourceName);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();

            while (!uwr.isDone) {
                yield return null;
            }

            if (uwr.isNetworkError || uwr.isHttpError) {
                string errMsg =
                    $"VTFXLocalSingleton::LoaderRoutine failed to load [{request.resourceName}] due to error [{uwr.error}]";
                request.onFail(request.resourceName, errMsg, request.requestUserData);
            }
            else {
                var data = uwr.downloadHandler.data;
                request.onSuccess(request.resourceName, data, (UInt64)data.Length, request.requestUserData);
            }
        }
    }
}
