using EffectHandle = System.IntPtr;
using NodeHandle = System.IntPtr;

public class HapticSourceOutputNodeDeviceTagOverride : HapticSourceOverride
{
    public string nodeName;
    public string deviceTag;

    protected override void OnConfigureEffectBeforePlay(EffectHandle effectHandle)
    {
        var vtfx = VtfxLocalSingleton.c_handle;

        NodeHandle node = VtfxLocalPluginAPI.Effect_FindNodeByName(vtfx, effectHandle, nodeName);
        if (node != VtfxLocalPluginAPI.kInvalidHandle)
        {
            VtfxLocalPluginAPI.Node_Output_AddDeviceTag(vtfx, node, deviceTag);
        }
    }
}
