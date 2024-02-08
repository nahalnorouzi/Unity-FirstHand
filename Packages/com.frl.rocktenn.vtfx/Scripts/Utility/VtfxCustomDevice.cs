using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using VtfxHandle = System.IntPtr;
using VtfxRuntimeContextHandle = System.IntPtr;

[System.Serializable]
public class OnNewSample : UnityEvent<float>
{
}

[System.Serializable]
public class OnNewSamples : UnityEvent<float[]>
{
}

// VtfxCustomDevice ceates a custom VTFX "device" that can be used to obtain floating 
// point samples from VTFX effects. Possible use cases include driving material properties,
// transforms, prefab parameters, etc. Use your imagination!
//
// Specify the name of the device and the number of channels required. Effects sent to
// the device will be sampled in `FixedUpdate` at the rate of `Time.fixedDeltaTime`.
// You can get the latest value for each channel with GetOutput, or hook up callbacks
// by adding UnityEvent listeners to `onNewSamples`. Listener callbacks should take the
// form of `void foo(float t)`. It is typical to add listeners to UnityEvents from the
// Unity Inspector (see Cubes examples), but you can add them programatically as well. 

public class VtfxCustomDevice : MonoBehaviour
{
    /// <summary>
    /// Describes how multiple samples should be combined into a single value.
    /// </summary>
    public enum OversamplingMode
    {
        First,   // return the first sample of the channel's samples
        AbsMax,  // return the absolute maximum of the channel's samples
        Average, // return the mean of the channel's samples
        RMS      // return the RMS of the channel's samples
    }

    [Tooltip("The name of the device as it will appear to VTFX. Use this string in your VTFX effect to filter outputs as necessary.")]
    public string deviceName = "MyProxyDevice";

    [Tooltip("The number of channels the device will support.")]
    public int numChannels = 1;
    [Tooltip("The number of samples per channel that will be read each FixedUpdate. Increase this if you need VTFX to generate a smoother curve.")]
    public int numSamples = 1;
    [Tooltip("How should multiple samples be combined when passed to OnNewSample callbacks?")]
    public OversamplingMode oversamplingMode = OversamplingMode.First;

    [Tooltip("If true, callbacks will not be invoked when there are not active effects playing")]
    public bool silenceIfNoEffects = true;

    [Tooltip("Array of Unity Events that will be invoked when new samples are available. The samples for the channel will be combined according to oversampling mode. The nth array element corresponds to the nth device channel.")]
    public OnNewSample[] onNewSamples = new OnNewSample[1];
    [Tooltip("Array of Unity Events that will be invoked when new samples are available. All samples for the channel will be provided. The nth array element corresponds to the nth device channel.")]
    public OnNewSamples[] onNewSamplesArray = new OnNewSamples[1];

    public VtfxHandle deviceHandle { get; private set; }

    private VtfxRuntimeContextHandle _vtfx = System.IntPtr.Zero;
    private byte[] _rawBuffer;
    private float[] _buffer;

    void Start()
    {
        int samplesPerSecond = (int)(1.0 / Time.fixedDeltaTime) * numSamples;
        _rawBuffer = new byte[sizeof(float) * numChannels * numSamples];
        _buffer = new float[numChannels * numSamples];
        System.Array.Resize(ref onNewSamples, numChannels);
        System.Array.Resize(ref onNewSamplesArray, numChannels);
        _vtfx = VtfxLocalSingleton.c_handle;

        // make sure there aren't two devices with this name
        deviceHandle = VtfxLocalPluginAPI.FindDeviceByName(_vtfx, deviceName);
        if (deviceHandle == VtfxHandle.Zero)
        {
            VtfxLocalPluginAPI.VtfxDeviceInfo deviceInfo;
            deviceInfo._numChannels = numChannels;
            deviceInfo._signedSample = true;
            deviceInfo._floatSample = true;
            deviceInfo._bufferType = (VtfxLocalPluginAPI.BufferType)0;
            deviceInfo._bytesPerSample = 4;
            deviceInfo._samplesPerChannelPerSec = samplesPerSecond;
            deviceInfo._samplesPerChannelPerBuffer = numSamples;
            deviceInfo._handedness = (VtfxLocalPluginAPI.DeviceHandedness)(-1);
            deviceInfo._parentDeviceHandle = (System.IntPtr)0;
            deviceInfo._parentDeviceChannelOffset = 0;
            deviceHandle = VtfxLocalPluginAPI.CreateDevice(_vtfx, deviceInfo, deviceName, "", "", "");
        }
        else
        {
            Debug.LogError("[VTFX] Attempted to create a VtfxCustomDevice which already exists!");
        }
    }

    public float GetOutputSingle(int channel, OversamplingMode mode)
    {
        Debug.Assert(channel < numChannels);
        switch (mode)
        {
            case OversamplingMode.First:
                return _buffer[channel];
            case OversamplingMode.AbsMax:
                float max = 0;
                for (int i = 0; i < numSamples; ++i)
                    max = System.Math.Max(max, System.Math.Abs(_buffer[i * numChannels + channel]));
                return max;
            case OversamplingMode.Average:
                float avg = 0;
                float den = 1.0f / numSamples;
                for (int i = 0; i < numSamples; ++i)
                    avg += _buffer[i * numChannels + channel] * den;
                return avg;
            case OversamplingMode.RMS:
                float sum_sq = 0;
                for (int i = 0; i < numSamples; ++i)
                    sum_sq += _buffer[i * numChannels + channel] * _buffer[i * numChannels + channel];
                return (float)System.Math.Sqrt(sum_sq / numSamples);
        }
        return _buffer[channel];
    }

    public float[] GetOutputArray(int channel)
    {
        Debug.Assert(channel < numChannels);
        float[] temp = new float[numSamples];
        for (int i = 0; i < numSamples; ++i)
            temp[i] = _buffer[i * numChannels + channel];
        return temp;
    }

    private void OnValidate()
    {
        System.Array.Resize(ref onNewSamples, numChannels);
        System.Array.Resize(ref onNewSamplesArray, numChannels);
    }

    private void OnDestroy()
    {
        if (VtfxLocalSingleton.c_handle != System.IntPtr.Zero && deviceHandle != System.IntPtr.Zero)
        {
            VtfxLocalPluginAPI.Device_Release(VtfxLocalSingleton.c_handle, deviceHandle);
        }
    }

    void FixedUpdate()
    {
        if (deviceHandle != System.IntPtr.Zero)
        {
            var result = VtfxLocalPluginAPI.Device_FillNextOutputBuffer(_vtfx, deviceHandle, _rawBuffer, _rawBuffer.Length);
            System.Buffer.BlockCopy(_rawBuffer, 0, _buffer, 0, _rawBuffer.Length);
            if ((silenceIfNoEffects && result._numActiveEffects > 0) || !silenceIfNoEffects)
            {
                for (int i = 0; i < numChannels; ++i)
                {
                    if (onNewSamples[i] != null)
                        onNewSamples[i].Invoke(GetOutputSingle(i, oversamplingMode));
                    if (onNewSamplesArray[i] != null)
                        onNewSamplesArray[i].Invoke(GetOutputArray(i));
                }
            }
        }
    }
}
