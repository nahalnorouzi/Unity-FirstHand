using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Osiris
{

    /* ----------------------------------------------------------------------------------------------------------------
        Native API for loading/unloading NativePlugins.
        This file is "vanilla C#" and works within Unity or in a standalone C# project.

        Unity has an issue where the editor will never unload DLLs loaded via [DllImport]. This makes developing native
        plugins very difficult. Because updating the DLL requires rebooting the Unity editor.

        NativePluginLoader solves that problem. Instead of using [DllImport] you can call LoadLibrary, GetProcAddress,
        and UnloadLibrary. Attributes makes this process simple and painless. NativePluginLoaderSingleton.cs is a
        Unity monobehavior which automates the process.

    ----- OLD AND BUSTED
        public class FooAPI {
            [DllImport("MyFooDLL", EntryPoint = "Foo_DoThing")]
            public static extern bool DoThing();

            [DllImport("MyFooDLL", EntryPoint = "Foo_GetNumber")]
            public static extern float GetNumber();

            [DllImport("MyFooDLL", EntryPoint = "Foo_DoThingWithString")]
            public static extern void DoThingWithString([MarshalAs(UnmanagedType.LPStr)]string someString)
        }

    ----- NEW HOTNESS
        [Osiris.PluginAttr("MyFooDLL", path = "Path/ToDll")]
        public class FooAPI {
            [Osiris.PluginFunctionAttr]
            public static Foo_DoThing DoThing = null
            public delegate bool Foo_DoThing();

            [Osiris.PluginFunctionAttr]
            public static Foo_GetNumber GetNumber = null
            public delegate float Foo_GetNumber();

            [Osiris.PluginFunctionAttr]
            public static Foo_DoThingWithString DoThingWithString = null
            public delegate void Foo_DoThingWithString([MarshalAs(UnmanagedType.LPStr)]string someString);
        }

    ----- Syntax for calling API (exact same in both cases)
        public void RandomFunc() {
            bool result = FooAPI.DoThing();
            float num = FooAPI.GetNumber();
            FooAPI.DoThingWithString("Hello World");
        }


        delegates are dynamically loaded when calling NativePluginLoader.LoadAll(). They are unloaded after calling
        NativePluginLoader.UnloadAll(). A warning is logged if the function can't be found. (NativePluginLoaderSingleton
        is a script / Unity prefab to help automate calling LoadAll / UnloadAll.)

        The delegate syntax is slightly more verbose. But it enables normal intellisense and is fully interchangeable
        with a regular [DllImport] API.

    ---------------------------------------------------------------------------------------------------------------- */
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || OSIRIS_WIN
    public static class SystemLibrary
    {
        private const string Library = "kernel32";

        [DllImport(Library, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport(Library, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryEx")]
        static extern IntPtr LoadLibraryEx_internal(string dllToLoad, IntPtr zero, Int64 flags);
        public static IntPtr LoadLibraryEx(string dllToLoad)
        {
            const Int64 LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
            return LoadLibraryEx_internal(dllToLoad, IntPtr.Zero, LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
        }

        [DllImport(Library, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(Library)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        public static int GetLastError() {
            return Marshal.GetLastWin32Error();
        }
    }

#elif UNITY_ANDROID
    public static class SystemLibrary
    {
        private const string Library = "AndroidDl";

        [DllImport(Library, EntryPoint = "android_dlopen")]
        public static extern IntPtr LoadLibrary(string path);

        [DllImport(Library, EntryPoint = "android_dlclose")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr handle);

        [DllImport(Library, EntryPoint = "android_dlsym")]
        public static extern IntPtr GetProcAddress(IntPtr handle, string name);

        [DllImport(Library, EntryPoint = "android_dlerror")]
        static extern IntPtr _GetLastError();
        public static string GetLastError() {
            var ptr = _GetLastError();
            return Marshal.PtrToStringAuto(ptr);
        }
    }
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    public static class SystemLibrary
    {
        private const string Library = "libdl";

        [DllImport(Library, EntryPoint = "dlopen")]
        static extern IntPtr _LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string path, int mode);
        public static IntPtr LoadLibrary(string path) {
            int RTLD_NOW = 2;
            return _LoadLibrary(path, RTLD_NOW);
        }

        [DllImport(Library, EntryPoint = "dlclose")]
        static extern int _FreeLibrary(IntPtr handle);
        public static bool FreeLibrary(IntPtr handle) {
            // _FreeLibrary/dlclose returns 0 on success, non-zero on error
            int result = _FreeLibrary(handle);
            return result == 0;
        }

        [DllImport(Library, EntryPoint = "dlsym")]
        public static extern IntPtr GetProcAddress(IntPtr handle, string name);

        // TBD: Get from errno/dlerror, for now just assume things are OK
        [DllImport(Library, EntryPoint = "dlerror")]
        static extern IntPtr _GetLastError();
        public static string GetLastError() {
            var ptr = _GetLastError();
            return Marshal.PtrToStringAuto(ptr);
        }
    }
#elif WINDOWS_UWP
    // WINDOWS_UWP to use UWP-specific, non-Unity APIs. This will prevent them from trying to run in the Editor or on unsupported platforms.
    // This is equivalent to UNITY_WSA && !UNITY_EDITOR and should be used in favor of. WSA is short for Windows Store Apps.
    public static class SystemLibrary
    {
        private const string Library = "__Internal";

        [DllImport(Library, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "LoadPackagedLibrary")]
        static extern IntPtr LoadLibrary_internal(string dllToLoad, int reserved = 0);
        public static IntPtr LoadLibrary(string dllToLoad) {
            return LoadLibrary_internal(dllToLoad);
        }

        [DllImport(Library, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(Library)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        public static int GetLastError() {
            return Marshal.GetLastWin32Error();
        }
    }
#else
#error SystemLibrary has not been defined for the target platform.
#endif

    public class NativePluginLoadResult
    {
        public Dictionary<string, IntPtr> loadedPlugins = new Dictionary<string, IntPtr>();
        public List<System.Reflection.FieldInfo> loadedDelegates = new List<System.Reflection.FieldInfo>();
        public List<String> loadedPluginDirectories = new List<string>();
        public bool unityPluginLoad;
    }

    public static class NativePluginLoader
    {
        // Load all plugins with 'PluginAttr'
        // Load all functions with 'PluginFunctionAttr'
        static public NativePluginLoadResult LoadAll(string pluginPathRoot, IntPtr unityInterfaces)
        {
            var loadTimer = new System.Diagnostics.Stopwatch();
            loadTimer.Start();

            Log(string.Format("Loading native plugins.\n PluginPathRoot: [{0}]", pluginPathRoot));

            NativePluginLoadResult result = new NativePluginLoadResult();
            result.unityPluginLoad = unityInterfaces != IntPtr.Zero;

            HashSet<string> uniqueLoadedPluginDirectories = new HashSet<string>();

            // Loop over all assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                // Get types from the assembly
                Type[] types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (Exception e)
                {
                    LogError(string.Format("Failed to load types from assembly [{0}] with exception [{1}]. The assembly is missing dependencies.\n", assembly.FullName, e.Message));
                    types = new Type[0];
                }

                // Group incoming PluginAttrs by priority
                // Priority is sorted by lower values being higher priority
                SortedDictionary<uint, List<Type>> pluginsByPriority = new SortedDictionary<uint, List<Type>>();
                foreach (var type in types)
                {
                    var typeAttributes = type.GetCustomAttributes(typeof(PluginAttr), true);
                    if (typeAttributes.Length > 0)
                    {
                        System.Diagnostics.Debug.Assert(typeAttributes.Length == 1); // should not be possible

                        var typeAttribute = typeAttributes[0] as PluginAttr;

                        if (!pluginsByPriority.ContainsKey(typeAttribute.priority))
                        {
                            pluginsByPriority.Add(typeAttribute.priority, new List<Type>());
                        }

                        List<Type> pluginList = pluginsByPriority[typeAttribute.priority] as List<Type>;
                        pluginList.Add(type);
                    }
                }

                // Load Plugins in grouped order
                foreach(KeyValuePair<uint,List<Type>> kvp in pluginsByPriority)
                {
                    foreach(Type type in kvp.Value as List<Type>)
                    {
                        TryLoadPlugin(type, result, pluginPathRoot, unityInterfaces, uniqueLoadedPluginDirectories);
                    }
                }
            }

            result.loadedPluginDirectories = uniqueLoadedPluginDirectories.ToList<string>();

            // Print elapsed time to console
            Log(string.Format("NativePluginLoader.LoadAll completed in {0} milliseconds", loadTimer.Elapsed.Milliseconds));

            return result;
        }

        static private void TryLoadPlugin(Type type, NativePluginLoadResult result, string pluginPathRoot, IntPtr unityInterfaces, HashSet<string> uniqueLoadedPluginDirectories)
        {
            var typeAttributes = type.GetCustomAttributes(typeof(PluginAttr), true);
            var typeAttribute = typeAttributes[0] as PluginAttr;

            var pluginName = typeAttribute.pluginName;
            IntPtr pluginHandle = IntPtr.Zero;

            #if !UNITY_EDITOR && UNITY_ANDROID
            // Ensure plugin name begins with lib because Android demands it
            if (!pluginName.StartsWith("lib")) {
                pluginName = "lib" + pluginName;
            }
            #endif

            // Load plugin if it is not already loaded
            if (!result.loadedPlugins.TryGetValue(pluginName, out pluginHandle))
            {
                // Compute path to library
                var libraryName = pluginName + EXT;
                string libraryDirectory = pluginPathRoot;
                if (typeAttribute.relativePath != null) {
                    libraryDirectory = System.IO.Path.Combine(pluginPathRoot, typeAttribute.relativePath);
                }
                libraryDirectory = System.IO.Path.GetFullPath(libraryDirectory);

                Func<string, IntPtr> loadLibrary = loadDirectory => {
#if WINDOWS_UWP
                    // WSA apps use LoadPackagedLibrary which takes just the dll name
                    string finalPath = pluginName;
#else
                    string finalPath = System.IO.Path.Combine(loadDirectory, libraryName);
#endif
                    pluginHandle = SystemLibrary.LoadLibrary(finalPath);
                    if (pluginHandle == IntPtr.Zero) {
                        Log(string.Format("Failed to load ({0}) via [{1}]. Error: [{2}]", libraryName, finalPath, SystemLibrary.GetLastError()));
                        return IntPtr.Zero;
                    }

                    Log(string.Format("Loaded ({0}) via [{1}]", libraryName, finalPath));
                    uniqueLoadedPluginDirectories.Add(loadDirectory);

                    return pluginHandle;
                };

                pluginHandle = loadLibrary(libraryDirectory);
                if (pluginHandle == IntPtr.Zero) {
                    // Check platform subfolders
#if UNITY_EDITOR_WIN
                    pluginHandle = loadLibrary(System.IO.Path.Combine(libraryDirectory, "x64"));
#elif UNITY_EDITOR_OSX
                    pluginHandle = loadLibrary(System.IO.Path.Combine(libraryDirectory, "macOS"));
#endif
                }

                if (pluginHandle == IntPtr.Zero) {
                    LogError(string.Format("Failed to load plugin [{0}]. Path: [{1}]  Err: [{2}]", libraryName, libraryDirectory, SystemLibrary.GetLastError()));
                    return;
                }

                // Invoke UnityPluginLoad if UnityInterfaces pointer was provided and UnityPluginLoad exists in library
                if (unityInterfaces != IntPtr.Zero) {
                    var unityPluginLoadFnPtr = SystemLibrary.GetProcAddress(pluginHandle, "UnityPluginLoad");
                    if (unityPluginLoadFnPtr != IntPtr.Zero) {
                        var fnDelegate = (UnityPluginLoad)Marshal.GetDelegateForFunctionPointer(unityPluginLoadFnPtr, typeof(UnityPluginLoad));
                        fnDelegate(unityInterfaces);
                    }
                }

                // Add library to loaded result
                result.loadedPlugins.Add(pluginName, pluginHandle);
            }

            // Loop over fields in type
            var fields = type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                // Get custom attributes for field
                var fieldAttributes = field.GetCustomAttributes(typeof(PluginFunctionAttr), true);
                if (fieldAttributes.Length > 0)
                {
                    System.Diagnostics.Debug.Assert(fieldAttributes.Length == 1); // should not be possible

                    // Get PluginFunctionAttr attribute
                    var fieldAttribute = fieldAttributes[0] as PluginFunctionAttr;
                    var functionName = fieldAttribute.funcName ?? field.FieldType.Name;

                    // Get function pointer
                    var fnPtr = SystemLibrary.GetProcAddress(pluginHandle, functionName);
                    if (fnPtr == IntPtr.Zero)
                    {
                        LogError(string.Format("Failed to find function [{0}] in plugin [{1}]. Err: [{2}]\n", functionName, pluginName, SystemLibrary.GetLastError()));
                        continue;
                    }

                    // Get delegate pointer
                    var fnDelegate = Marshal.GetDelegateForFunctionPointer(fnPtr, field.FieldType);

                    // Set static field value
                    field.SetValue(null, fnDelegate);

                    // Store field so it can be cleared in Unload
                    result.loadedDelegates.Add(field);
                }
            }
        }

        static public void UnloadAll(NativePluginLoadResult result)
        {
            Log("Unloading native plugins");

            // Unload libraries
            foreach (var kvp in result.loadedPlugins)
            {
                var library = kvp.Value;

                // Call UnityPluginUnload if we previously called UnityPluginLoad
                if (result.unityPluginLoad)
                {
                    // Find "UnityPluginUnload" proc address
                    var unityPluginUnloadFnPtr = SystemLibrary.GetProcAddress(library, "UnityPluginUnload");
                    if (unityPluginUnloadFnPtr != IntPtr.Zero)
                    {
                        // Invoke UnityPluginUnload
                        var fnDelegate = (UnityPluginUnload)Marshal.GetDelegateForFunctionPointer(unityPluginUnloadFnPtr, typeof(UnityPluginUnload));
                        fnDelegate();
                    }
                }

                // Free Library
                Log(string.Format("Freeing system library [{0}] [{1}]", kvp.Key, kvp.Value));
                bool success = SystemLibrary.FreeLibrary(library);
                if (!success)
                {
                    throw new System.Exception(string.Format("Failed to unload plugin [{0}]. Err: [{1}]", kvp.Key, SystemLibrary.GetLastError()));
                }
            }

            // Clear delegates
            foreach (var field in result.loadedDelegates)
                field.SetValue(null, null);
        }

        static public void Log(string msg) {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || WINDOWS_UWP
            UnityEngine.Debug.Log(msg);
#elif UNITY_ANDROID
            // Prefix for logcat searching
            // System.Diagnostics because Unity callstack spew is unneeded
            System.Diagnostics.Debug.WriteLine("[NativePluginLoader] " + msg);
#else
            (msg);
#endif
        }

        static public void LogError(string msg) {
#if UNITY_EDITOR || UNITY_STANDALONE || WINDOWS_UWP
            UnityEngine.Debug.LogError(msg);
#else
            System.Diagnostics.Debug.WriteLine(msg);
#endif
        }

        // Delegates so function can be called on Plugins
        delegate void UnityPluginLoad(IntPtr unityInterfaces);
        delegate void UnityPluginUnload();

        // Constants
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || OSIRIS_WIN || WINDOWS_UWP
        const string EXT = ".dll";
#elif UNITY_ANDROID || UNITY_STANDALONE_LINUX || OSIRIS_LINUX
        const string EXT = ".so";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || OSIRIS_OSX
        const string EXT = ".bundle";
#else
#error Dynamic library extension not defined for the target platform.
#endif
    }


    // ------------------------------------------------------------------------
    // Attribute for Plugin APIs
    // ------------------------------------------------------------------------
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PluginAttr : System.Attribute
    {
        // Properties
        public string pluginName { get; set; }

        public string relativePath
        {
            get
            {
#if UNITY_EDITOR
                return editorPath;
#else
                return playerPath;
#endif
            }
        }

        // Lower values for 'priority' are loaded first.  Default value is '0' for highest priority.
        public uint priority { get; set; }

        // Private Fields
        string playerPath;
        string editorPath;

        // Methods
        public PluginAttr(string pluginName, string playerPath = null, string editorPath = null, uint priority = 0)
        {
            this.pluginName = pluginName;
            this.playerPath = playerPath;
            this.editorPath = editorPath;
            this.priority = priority;
        }
    }


    // ------------------------------------------------------------------------
    // Attribute for functions inside a Plugin API
    // ------------------------------------------------------------------------
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PluginFunctionAttr : System.Attribute
    {
        // Fields
        public string funcName { get; set; }

        // Methods
        public PluginFunctionAttr()
        {
            this.funcName = null;
        }
    }
} // namespace Osiris
