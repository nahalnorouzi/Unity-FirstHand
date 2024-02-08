using System;
using System.Runtime.InteropServices;

public static class UnityPluginInterfacesHelper 
{
    [DllImport("UnityPluginInterfaceHelper")]
    public static extern IntPtr GetUnityInterfacePtr();
}
