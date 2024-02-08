using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PluginManagerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var pms = PluginManagerSingleton.singleton;

        // This will return IntPtr.Zero and log an error.
        // Because "Foo" is not a plugin that exists.
        string pluginName = "DebugConsolePlugin";
        var plugin = pms.GetPlugin(pluginName, PluginManagerAPI.ANY_PLUGIN_VERSION);
        if (plugin == System.IntPtr.Zero) {
            Debug.Log(string.Format("PluginManagerTest: Unable to load plugin [{0}]", pluginName));
        } else {
            Debug.Log(string.Format("PluginManagerTest: Loaded plugin [{0}]", pluginName));
        }
    }
}
