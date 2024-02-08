using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using HostAppPluginHandle = System.IntPtr;

[Osiris.PluginAttr("HostAppPlugin", editorPath: "../Packages/com.frl.rocktenn.pluginmanager/Plugins")]
public class HostAppPluginAPI {

    [Osiris.PluginFunctionAttr]
    public static HostApp_HostAPI_OnMainThreadTick OnMainThreadTick = null;
    public delegate void HostApp_HostAPI_OnMainThreadTick(HostAppPluginHandle hostAppPluginHandle);
}
