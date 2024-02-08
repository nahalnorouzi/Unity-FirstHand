
using PluginHandle = System.IntPtr;

[Osiris.PluginAttr("InputPlugin", editorPath: "../Packages/com.frl.rocktenn.pluginmanager/Plugins")]
public class InputPluginAPI {
    [Osiris.PluginFunctionAttr]
    public static Input_HostAPI_StartFrame HostAPI_StartFrame = null;
    public delegate bool Input_HostAPI_StartFrame(PluginHandle vtfx);

}
