using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using PluginManagerHandle = System.IntPtr;

[Osiris.PluginAttr("PluginManager", editorPath: "../Packages/com.frl.rocktenn.pluginmanager/Plugins")]
public class PluginManagerAPI {
    public const int ANY_PLUGIN_VERSION = 0;

    [Osiris.PluginFunctionAttr]
    public static PluginManager_Initialize Initialize = null;
    public delegate PluginManagerHandle PluginManager_Initialize();

    [Osiris.PluginFunctionAttr]
    public static PluginManager_Destroy Destroy = null;
    public delegate void PluginManager_Destroy(PluginManagerHandle managerHandle);

    [Osiris.PluginFunctionAttr]
    public static PluginManager_GetPlugin GetPlugin = null;
    public delegate IntPtr PluginManager_GetPlugin(PluginManagerHandle managerHandle, [MarshalAs(UnmanagedType.LPStr)]string pluginName, int requestedVersionNumber);

    public enum Flags
    {
        /** This plugin is required; on failure to load it will log an error and abort the process. */
        FatalErrorIfNotLoaded = 0b00,
        /** This plugin is optional and will return a nullptr on failure to load. */
        Optional = 0b01,
        /** Check if the plugin is currently loaded; nullptr is returned on failure to find plugin. */
        OptionalDoNotLoad = Optional | 0b10,
    };

    [Osiris.PluginFunctionAttr]
    public static PluginManager_GetPluginWithFlags GetPluginWithFlags = null;
    public delegate IntPtr PluginManager_GetPluginWithFlags(PluginManagerHandle managerHandle, [MarshalAs(UnmanagedType.LPStr)] string pluginName, int requestedVersionNumber, int flags);

    [Osiris.PluginFunctionAttr]
    public static PluginManager_AddPluginPath AddPluginPath = null;
    public delegate void PluginManager_AddPluginPath(PluginManagerHandle managerHandle, [MarshalAs(UnmanagedType.LPStr)]string path);

    public delegate void LogCallbackFn(int level, [MarshalAs(UnmanagedType.LPStr)] string msg, [MarshalAs(UnmanagedType.LPStr)] string file, int line);

    [Osiris.PluginFunctionAttr]
    public static PluginManager_SetLogCallback SetLogCallback = null;
    public delegate void PluginManager_SetLogCallback(PluginManagerHandle managerHandle, LogCallbackFn logFn);
}

