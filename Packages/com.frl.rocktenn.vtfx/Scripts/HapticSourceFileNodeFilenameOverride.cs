using EffectHandle = System.IntPtr;
using NodeHandle = System.IntPtr;

public class HapticSourceFileNodeFilenameOverride : HapticSourceOverride
{
    public string nodeName;
    public string filename;

    protected override void OnConfigureEffectBeforePlay(EffectHandle effectHandle)
    {
        var vtfx = VtfxLocalSingleton.c_handle;

        NodeHandle node = VtfxLocalPluginAPI.Effect_FindNodeByName(vtfx, effectHandle, nodeName);
        if (node != VtfxLocalPluginAPI.kInvalidHandle)
        {
            VtfxLocalPluginAPI.Node_RawData_Load(vtfx, node, filename);
        }
    }
}
