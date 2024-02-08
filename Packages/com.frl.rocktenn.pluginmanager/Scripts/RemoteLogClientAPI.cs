using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using RemoteLogClientHandle = System.IntPtr;

[Osiris.PluginAttr("RemoteLogClient", editorPath: "../Packages/com.frl.rocktenn.pluginmanager/Plugins")]
public class RemoteLogClientAPI {

    [Osiris.PluginFunctionAttr]
    public static RLC_LaunchAndConnectToLogViewerApp LaunchAndConnectToLogViewerApp = null;
    public delegate bool RLC_LaunchAndConnectToLogViewerApp(RemoteLogClientHandle rlcHandle,
      [MarshalAs(UnmanagedType.LPStr)] string address, [MarshalAs(UnmanagedType.LPStr)] string appName, bool retry, bool launchOnly);

    [Osiris.PluginFunctionAttr]
    public static RLC_Disconnect Disconnect = null;
    public delegate void RLC_Disconnect(RemoteLogClientHandle rlcHandle);

    [Osiris.PluginFunctionAttr]
    public static RLC_IsConnected IsConnected = null;
    public delegate bool RLC_IsConnected(RemoteLogClientHandle rlcHandle);

    [Osiris.PluginFunctionAttr]
    public static RLC_EnableStreamRedirect EnableStreamRedirect = null;
    public delegate void RLC_EnableStreamRedirect(RemoteLogClientHandle rlcHandle, bool enable);

    [Osiris.PluginFunctionAttr]
    public static RLC_Log Log = null;
    public delegate void RLC_Log(RemoteLogClientHandle rlcHandle, [MarshalAs(UnmanagedType.LPStr)] string message);
}
