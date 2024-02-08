using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ------------------------------------------------------------------------
// Singleton class to help with loading and unloading of native plugins
// See NativePluginLoader.cs for details.
// ------------------------------------------------------------------------
[System.Serializable]
public class NativePluginLoaderSingleton : MonoBehaviour, ISerializationCallbackReceiver
{
    // Public fields
    public bool _enableReloadAfterRecompileWhilePlaying = false;

    // Static fields
    static NativePluginLoaderSingleton _singleton;

    // Private fields
    Osiris.NativePluginLoadResult _loadResult;
    string _dataPath;

    // Return a list of all directories that contain a loaded plugin
    public List<String> LoadedPluginDirectories
    {
        get
        {
            return _loadResult?.loadedPluginDirectories ?? null;
        }
    }

    // Static Properties
    public static NativePluginLoaderSingleton singleton
    {
        get
        {
            if (_singleton == null)
            {
                var go = new GameObject("PluginLoader");
                var pl = go.AddComponent<NativePluginLoaderSingleton>();
                Debug.Assert(_singleton == pl); // should be set by awake
            }

            return _singleton;
        }
    }

    // Methods
    public virtual void Awake()
    {
        if (_singleton != null)
        {
            Debug.LogError($"Created multiple NativePluginLoader objects. Destroying duplicate created on GameObject [{this.gameObject.name}]");
            Destroy(this);
            return;
        }

        _singleton = this;
        DontDestroyOnLoad(this.gameObject);
#if UNITY_EDITOR || WINDOWS_UWP
        // WSA uses LoadPackagedLibrary which loads the library by name only, this value does not matter
        _dataPath = Application.dataPath;
#elif UNITY_ANDROID
        // Note: This only supports ARM64. The relative path from the APK to the extracted libs is undocumented by android, and subject to change without notice. :-(
        string[] folderArray = Application.dataPath.Split('/');
        var folders = folderArray.ToList();
        folders.RemoveAt(folders.Count - 1);
        var newPath = string.Join("/", folders);
        _dataPath = newPath + "/lib/arm64";
#elif UNITY_STANDALONE_WIN
        if (UnityVersion.year >= 2020 || (UnityVersion.year == 2019 && UnityVersion.major >= 4)) {
            _dataPath = System.IO.Path.Combine(Application.dataPath, "Plugins", "x86_64");
        } else {
            _dataPath = System.IO.Path.Combine(Application.dataPath, "Plugins");
        }
#elif UNITY_STANDALONE_OSX
        _dataPath = System.IO.Path.Combine(Application.dataPath, "Plugins");
#else
#error Plugin path not defined for the target platform
#endif

        LoadAll();
    }

    protected void OnDestroy()
    {
        UnloadAll();
        _singleton = null;
    }

    // Free all loaded libraries
    void UnloadAll()
    {
        if (_loadResult != null)
        {
            Osiris.NativePluginLoader.UnloadAll(_loadResult);
            _loadResult = null;
        }
    }

    // Load all plugins with 'PluginAttr'
    // Load all functions with 'PluginFunctionAttr'
    void LoadAll()
    {
        IntPtr unity_interfaces = UnityPluginInterfacesHelper.GetUnityInterfacePtr();
        if (unity_interfaces == IntPtr.Zero)
        {
            Debug.LogWarning("NativePluginLoaderSingleton failed to get UnityInterface*. UnityPluginLoad will not be called.");
        }

        _loadResult = Osiris.NativePluginLoader.LoadAll(_dataPath, unity_interfaces);
    }

    // It is *strongly* recommended to set Editor->Preferences->Script Changes While Playing = Recompile After Finished Playing
    // Properly support reload of native assemblies requires extra work.
    // However the following code will re-fixup delegates.
    // More importantly, it prevents a dangling DLL which results in a mandatory Editor reboot
    // NOTE: enabling this code has been reported to cause issues when switching scenes.
    bool _reloadAfterDeserialize = false;
    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        int numLoaded = _loadResult?.loadedPlugins.Count ?? 0;
        if (numLoaded > 0 && _enableReloadAfterRecompileWhilePlaying)
        {
            UnloadAll();
            _reloadAfterDeserialize = true;
        }
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        if (_reloadAfterDeserialize)
        {
            LoadAll();
            _reloadAfterDeserialize = false;
        }
    }

    // Helper to resolve Unity version to control paths
    static class UnityVersion {
        static int[] _versionNums = null;

        static int Ensure(int idx) {
            if (_versionNums == null) {
                // Version should be something like: "2020.4.0f1" = year.major.minor.patch
                var versionString = UnityEngine.Application.unityVersion;
                _versionNums = versionString.Split(new char[]{'.', 'f', 'p'}).Select(Int32.Parse).ToArray();
            }

            return _versionNums[idx];
        }

        public static int year { get { return Ensure(0); } }
        public static int major { get { return Ensure(1); } }
        public static int minor { get { return Ensure(2); } }
        public static int patch { get { return Ensure(3); } }
    }
}
