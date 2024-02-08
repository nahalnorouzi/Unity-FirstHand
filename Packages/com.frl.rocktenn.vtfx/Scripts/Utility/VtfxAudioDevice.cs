using System.Collections.Generic;
using UnityEngine;

using VtfxHandle = System.IntPtr;

public class VtfxAudioDevice : MonoBehaviour
{
    private string outputName = "PortAudio"; // multiple devices don't work well with XAudio2, so use PortAudio

    [Tooltip("The prefix of the name of the audio device as it appears to the system.")]
    public string audioDeviceName = "";

    [Tooltip("The name of the device as it will appear to VTFX. Use this string in your VTFX effect to filter outputs as necessary.")]
    public string vtfxDeviceName = "MyAudioDevice";

    [Tooltip("The number of channels the device supports.")]
    public int numChannels = 2;

    [Tooltip("The sample rate for the device.")]
    public int samplesPerChannelPerSec = 48000;

    public List<string> deviceTags = new List<string>() { "Audio" };

    public VtfxHandle deviceHandle { get; private set; }

    void Start()
    {
        var vtfx = VtfxLocalSingleton.c_handle;
        var identifier = audioDeviceName == "" ? null : VtfxLocalPluginAPI.FindSystemAudioDeviceIdentifierByName(vtfx, outputName, audioDeviceName);
        if (identifier == null)
        {
          return;
        }

        // make sure there isn't an existing device with this name
        deviceHandle = VtfxLocalPluginAPI.FindDeviceByName(vtfx, vtfxDeviceName);
        if (deviceHandle != VtfxHandle.Zero)
        {
            Debug.LogError("[VTFX] Attempted to create a VtfxAudioDevice with a name which already exists!");
            return;
        }

        VtfxLocalPluginAPI.VtfxDeviceInfo deviceInfo;
        deviceInfo._numChannels = numChannels;
        deviceInfo._signedSample = true;
        deviceInfo._floatSample = true;
        deviceInfo._bufferType = VtfxLocalPluginAPI.BufferType.PCM;
        deviceInfo._bytesPerSample = 4;
        deviceInfo._samplesPerChannelPerSec = samplesPerChannelPerSec;
        deviceInfo._samplesPerChannelPerBuffer = 1024;
        deviceInfo._handedness = VtfxLocalPluginAPI.DeviceHandedness.NotApplicable;
        deviceInfo._parentDeviceHandle = (System.IntPtr)0;
        deviceInfo._parentDeviceChannelOffset = 0;

        deviceHandle = VtfxLocalPluginAPI.CreateDevice(vtfx, deviceInfo, vtfxDeviceName, "", identifier, "");
        if (deviceHandle == VtfxHandle.Zero)
        {
            Debug.LogError("[VTFX] Failed to create VtfxAudioDevice!");
            return;
        }

        VtfxLocalPluginAPI.CreateOutput(vtfx, outputName, deviceHandle);

        foreach (var tagName in deviceTags)
        {
            VtfxLocalPluginAPI.Device_AddTag(vtfx, deviceHandle, tagName);
        }
    }
}
