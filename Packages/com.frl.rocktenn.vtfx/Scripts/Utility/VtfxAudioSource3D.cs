using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VtfxHandle = System.IntPtr;
using VtfxRuntimeContextHandle = System.IntPtr;

// VtfxAudioSource3D provides the means to render VTFX audio effects inside of Unity's audio
// engine. The advantage of doing this versus rendering the VTFX effect directly to your
// audio output device is that the effect will be mixed and 3D spatialized by Unity.
//
// Simply add this script to your GameObject and specify the name of the device as you want
// it to appear to VTFX. The script attaches an AudioSource component to the GameObject, if it
// does not already exist, from which your audio effects will emit. You can pre-attach the
// AudioSource component if you need to customize any 3D spatialization parameters.

public class VtfxAudioSource3D : MonoBehaviour {
    // Public fields
    [Tooltip("The name of the device as it will appear to VTFX. Use this string in your VTFX effect to filter outputs as necessary.")]
    public string deviceName = "My Unity Device";

    public AudioSource _audioSource;


    // Private fields
    private VtfxRuntimeContextHandle _vtfx = System.IntPtr.Zero;
    private VtfxHandle _device = System.IntPtr.Zero;
    private byte[] _vtfxBuffer;
    private readonly object _vtfxLock = new object();
    private bool _processAudio = true;


    // Constants
    private const int kBytesPerSample = sizeof(float);


    // Unity Methods
    void Start() {
        // Get informaton about Unity's audio output device.
        int numChannels = 0;
        switch (AudioSettings.speakerMode){
            case AudioSpeakerMode.Mono:
                numChannels = 1; break;
            case AudioSpeakerMode.Stereo:
                numChannels = 2; break;
            case AudioSpeakerMode.Quad:
                numChannels = 4; break;
            case AudioSpeakerMode.Surround:
                numChannels = 5; break;
            case AudioSpeakerMode.Mode5point1:
                numChannels = 6; break;
            case AudioSpeakerMode.Mode7point1:
                numChannels = 8; break;
            case AudioSpeakerMode.Prologic:
                numChannels = 2; break;
        }
        int bufferSize;
        int numBuffers; // unused
        AudioSettings.GetDSPBufferSize(out bufferSize, out numBuffers); // bufferSize is set by unity to 'best match' the project, numBuffers is the number of buffers in a ring buffer
        int onAudioFilterReadSamples = bufferSize * numChannels; // The bufferSize is per channel

        // Make VTFX device and intermediate buffer.
        _vtfx = VtfxLocalSingleton.c_handle;
        int vtfxSamplesPerBuffer = onAudioFilterReadSamples / numChannels;

        VtfxLocalPluginAPI.VtfxDeviceInfo deviceInfo;
        deviceInfo._numChannels = 1; // VTFX produces mono-channel signal, unity spatializes
        deviceInfo._signedSample = true;
        deviceInfo._floatSample = true;
        deviceInfo._bufferType = (VtfxLocalPluginAPI.BufferType)0;
        deviceInfo._bytesPerSample = kBytesPerSample;
        deviceInfo._samplesPerChannelPerSec = AudioSettings.outputSampleRate; // default is 48 kHz
        deviceInfo._samplesPerChannelPerBuffer = vtfxSamplesPerBuffer;
        deviceInfo._handedness = (VtfxLocalPluginAPI.DeviceHandedness)(-1);
        deviceInfo._parentDeviceHandle = (System.IntPtr)0;
        deviceInfo._parentDeviceChannelOffset = 0;

        _device = VtfxLocalPluginAPI.CreateDevice(_vtfx, deviceInfo, deviceName, "", "", "");
        _vtfxBuffer = new byte[kBytesPerSample * vtfxSamplesPerBuffer];

        // Ensure that we stop processing audio BEFORE VTFX plugin is unloaded, otherwise Unity will crash.
        PluginManagerSingleton.singleton.onPreDestroy.AddListener(StopProcessingAudio);

        // We need to have a dummy audio clip playing for 3D audio to work.
        // We set this clip to output a constant value of 1. Unity
        // will apply 3D spatialization to the clip before the buffer
        // gets passed to OnAudioFilterRead. [Keep reading below]
        var audioClip = AudioClip.Create("VtfxAudioSourceClip", 1, 1, AudioSettings.outputSampleRate, false);
        audioClip.SetData(new float[] { 1 }, 0);
        _audioSource.loop = true;
        _audioSource.clip = audioClip;
        _audioSource.Play();
    }

    void OnAudioFilterRead(float[] data, int numChannels) {
        if (_vtfx != System.IntPtr.Zero && _processAudio) {
            lock (_vtfxLock) {
                VtfxLocalPluginAPI.Device_FillNextOutputBuffer(_vtfx, _device, _vtfxBuffer, _vtfxBuffer.Length);
            }

            // Thus, the "data" buffer contains just the weights of spatial calculations since
            // our dummy audio clip is just outputting 1. So instead of data[i] = x, we
            // will multiply buffers so that VTFX audio is spatialized.

            // VTFX is single channel, data is interleaved. Apply VTFX sample to all channels
            int numVtfxSamples = _vtfxBuffer.Length / kBytesPerSample;
            for (int i = 0; i < numVtfxSamples; ++i) {
                float vtfxSample = System.BitConverter.ToSingle(_vtfxBuffer, kBytesPerSample * i);
                float finalSample = vtfxSample;
                for (int j = 0; j < numChannels; ++j) {
                    data[i * numChannels + j] *= vtfxSample;
                }
            }
        }
    }

    void StopProcessingAudio() {
        // Wait for OnAudioFilterRead to unlock call into VTFX plugin.
        lock (_vtfxLock) {
            _processAudio = false;
        }
    }

}

// https://stackoverflow.com/questions/38843408/realtime-3d-audio-streaming-and-playback
// https://githubmemory.com/repo/rorywalsh/CsoundUnity/issues/22
