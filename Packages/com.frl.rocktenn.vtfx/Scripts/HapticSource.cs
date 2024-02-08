using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NodeHandle = System.IntPtr;
using EffectHandle = System.IntPtr;
using TunableHandle = System.IntPtr;
using Handedness = VtfxLocalPluginAPI.DeviceHandedness;

public delegate void OnHapticEffectFinishedDelegate();
public delegate void ConfigureEffectBeforePlayDelegate(EffectHandle effectHandle);

[System.Serializable]
public class EffectTunableEntry
{
    public string tunableName;
    public float tunableValue;
}

public class HapticSource : MonoBehaviour
{
    // Public fields
    public HapticClip hapticClip;
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;
    public Handedness handedness = Handedness.NotApplicable;
    public List<EffectTunableEntry> effectTunableMap;
    public bool PlayOnStart = false;
    public bool SpatializeAudio = false;
    public float SpatializeFalloffStart = 0;
    public float SpatializeFalloffSize = 0;

    public List<string> spatializeAudioNodeNames = new List<string>() { "SpatializeAudio" };


    // Private fields
    private List<ConfigureEffectBeforePlayDelegate> effectConfigureBeforePlayDelegates = new List<ConfigureEffectBeforePlayDelegate>();
    private HapticClip sourceHapticClip = null;
    private EffectHandle baseEffectHandle;
    private List<EffectHandle> playingEffects = new List<EffectHandle>();

    // Methods
    void Start()
    {
        if (PlayOnStart)
        {
            Play();
        }
    }

    public void Play()
    {
        if (hapticClip)
        {
            Stop();
            PlayInternal(hapticClip, volume, this.handedness, this.effectConfigureBeforePlayDelegates);
        }
    }

    public void SetPlaying(bool play)
    {
        if (play)
        {
            if (!IsPlaying())
            {
                Play();
            }
        }
        else
        {
            if (IsPlaying())
            {
                Stop();
            }
        }
    }

    public void Play(Handedness handedness)
    {
        if (hapticClip)
        {
            Stop();
            PlayInternal(hapticClip, volume, handedness, this.effectConfigureBeforePlayDelegates);
        }
    }

    public void Play(Handedness handedness, List<ConfigureEffectBeforePlayDelegate> effectConfigureBeforePlayDelegates)
    {
        if (hapticClip)
        {
            Stop();
            PlayInternal(hapticClip, volume, handedness, effectConfigureBeforePlayDelegates);
        }
    }

    public void Play(List<ConfigureEffectBeforePlayDelegate> effectConfigureBeforePlayDelegates)
    {
        if (hapticClip)
        {
            Stop();
            PlayInternal(hapticClip, volume, this.handedness, effectConfigureBeforePlayDelegates);
        }
    }

    public void Play(Handedness handedness, ConfigureEffectBeforePlayDelegate configureEffectBeforePlay)
    {
        if (hapticClip)
        {
            Stop();
            PlayInternal(hapticClip, volume, handedness, new List<ConfigureEffectBeforePlayDelegate> {configureEffectBeforePlay});
        }
    }

    public void Play(ConfigureEffectBeforePlayDelegate configureEffectBeforePlay)
    {
        if (hapticClip)
        {
            Stop();
            PlayInternal(hapticClip, volume, this.handedness, new List<ConfigureEffectBeforePlayDelegate> {configureEffectBeforePlay});
        }
    }

    public void PlayOneShot(HapticClip clip)
    {
        PlayInternal(clip, volume, this.handedness, this.effectConfigureBeforePlayDelegates);
        baseEffectHandle = EffectHandle.Zero;
    }

    public void PlayOneShot(HapticClip clip, Handedness handedness, float volumeScale = 1.0f)
    {
        PlayInternal(clip, volume * volumeScale, handedness, this.effectConfigureBeforePlayDelegates);
        baseEffectHandle = EffectHandle.Zero;
    }

    public void PlayOneShot(HapticClip clip, ConfigureEffectBeforePlayDelegate configureEffectBeforePlay)
    {
        PlayInternal(clip, volume, this.handedness, new List<ConfigureEffectBeforePlayDelegate> {configureEffectBeforePlay});
        baseEffectHandle = EffectHandle.Zero;
    }

    public void PlayOneShot(HapticClip clip, List<ConfigureEffectBeforePlayDelegate> effectConfigureBeforePlayDelegates)
    {
        PlayInternal(clip, volume, this.handedness, effectConfigureBeforePlayDelegates);
        baseEffectHandle = EffectHandle.Zero;
    }

    public void Stop()
    {
        var vtfx = VtfxLocalSingleton.c_handle;
        foreach (var effect in playingEffects)
        {
            if (VtfxLocalPluginAPI.Effect_IsValid(vtfx, effect))
            {
                VtfxLocalPluginAPI.ReleaseEffect(vtfx, effect);
            }
        }
    }

    // Pause currently does not work in VTFX
    //public void Pause()
    //{
    //    var vtfx = VtfxLocalSingleton.c_handle;
    //    foreach (var effect in playingEffects)
    //    {
    //        if (VtfxLocalPluginAPI.Effect_IsValid(vtfx, effect))
    //        {
    //            VtfxLocalPluginAPI.Effect_SetPlaying(vtfx, effect, false);
    //        }
    //    }
    //}

    public void UnPause()
    {
        var vtfx = VtfxLocalSingleton.c_handle;
        foreach (var effect in playingEffects)
        {
            if (VtfxLocalPluginAPI.Effect_IsValid(vtfx, effect))
            {
                VtfxLocalPluginAPI.Effect_SetPlaying(vtfx, effect, true);
            }
        }
    }

    public void SetTunable(string name, float value, bool persist = false)
    {
        var vtfx = VtfxLocalSingleton.c_handle;
        foreach (var effect in playingEffects)
        {
            if (VtfxLocalPluginAPI.Effect_IsValid(vtfx, effect))
            {
                var tunable = VtfxLocalPluginAPI.Effect_FindTunableByName(vtfx, effect, name);
                VtfxLocalPluginAPI.SetTunableValue(vtfx, tunable, value);
                if (persist)
                {
                    bool tunableFound = false;

                    for (int i = 0; i < effectTunableMap.Count; ++i)
                    {
                        if (effectTunableMap[i].tunableName == name)
                        {
                            effectTunableMap[i].tunableValue = value;
                            tunableFound = true;
                            break;
                        }
                    }

                    if (!tunableFound)
                    {
                        effectTunableMap.Add(new EffectTunableEntry() { tunableName = name, tunableValue = value });
                    }
                }
            }
        }
    }

    public void SetNextPlayTime(float playTimeSeconds)
    {
        var vtfx = VtfxLocalSingleton.c_handle;
        foreach (var effect in playingEffects)
        {
            VtfxLocalPluginAPI.Effect_SetNextPlayTime(vtfx, effect, playTimeSeconds);
        }
    }

    public void SetNextPlayTimeNormalized(string wavNodeName, float playTimeNormalized)
    {
        playTimeNormalized = Mathf.Clamp01(playTimeNormalized);

        var vtfx = VtfxLocalSingleton.c_handle;
        foreach (var effect in playingEffects)
        {
            if (!VtfxLocalPluginAPI.Effect_IsValid(vtfx, effect))
            {
                continue;
            }

            NodeHandle node = VtfxLocalPluginAPI.Effect_FindNodeByName(vtfx, effect, wavNodeName);
            if (node == VtfxLocalPluginAPI.kInvalidHandle)
            {
                continue;
            }

            float nodeDurationSeconds = VtfxLocalPluginAPI.Node_RawData_GetDurationSeconds(vtfx, node);
            VtfxLocalPluginAPI.Effect_SetNextPlayTime(vtfx, effect, playTimeNormalized * nodeDurationSeconds);
        }
    }

    public void RegisterConfigureEffectBeforePlayDelegate(ConfigureEffectBeforePlayDelegate configureDelegate)
    {
        effectConfigureBeforePlayDelegates.Add(configureDelegate);
    }

    public void UnregisterConfigureEffectBeforePlayDelegate(ConfigureEffectBeforePlayDelegate configureDelegate)
    {
        effectConfigureBeforePlayDelegates.Remove(configureDelegate);
    }

    public List<EffectHandle> GetPlayingEffects()
    {
        return playingEffects;
    }

    public bool IsPlaying()
    {
        return playingEffects != null && playingEffects.Count > 0;
    }

    internal void PlayInternal(HapticClip clip, float volume, Handedness handedness, List<ConfigureEffectBeforePlayDelegate> effectConfigureBeforePlayDelegates)
    {
        var vtfx = VtfxLocalSingleton.c_handle;

        // If the source clip has changed since last time, reset baseEffectHandle so we refetch/recreate the effect
        if (sourceHapticClip != clip)
        {
            baseEffectHandle = System.IntPtr.Zero;
            sourceHapticClip = clip;
        }

        // Lazily fetch effect from Vtfx
        if (baseEffectHandle == System.IntPtr.Zero)
        {
            baseEffectHandle = VtfxLocalPluginAPI.FindEffectByName(vtfx, clip.name);
        }

        // Lazily create effect from Json
        if (baseEffectHandle == System.IntPtr.Zero)
        {
            baseEffectHandle = VtfxLocalPluginAPI.CreateEffectJson(vtfx, clip.name, clip.json);
        }

        // Clone and play effect
        EffectHandle newEffect = VtfxLocalPluginAPI.Effect_Clone(vtfx, baseEffectHandle, "");
        float masterVolume = VtfxLocalPluginAPI.Effect_GetMasterVolume(vtfx, newEffect);
        VtfxLocalPluginAPI.Effect_SetMasterVolume(vtfx, newEffect, masterVolume * volume);
        VtfxLocalPluginAPI.Effect_SetDestroyOnComplete(vtfx, newEffect, true);
        if (handedness != Handedness.NotApplicable)
        {
            VtfxLocalPluginAPI.Effect_SetHandedness(vtfx, newEffect, handedness);
        }
        InitializeTunableValues(newEffect);

        if (effectConfigureBeforePlayDelegates != null)
        {
            foreach (var configureEffectBeforePlay in effectConfigureBeforePlayDelegates)
            {
                configureEffectBeforePlay(newEffect);
            }
        }

        // Register callback to remove playing effect from list when completed
        OnHapticEffectFinishedDelegate removeEffectFromPlaying = () => {
            playingEffects.Remove(newEffect);
        };
        VtfxLocalSingleton.singleton.RegisterOnEffectFinishedCallback(newEffect, removeEffectFromPlaying);

        VtfxLocalPluginAPI.Effect_SetPlaying(vtfx, newEffect, true);

        playingEffects.Add(newEffect);
    }

    public void SetVolumeOfPlayingEffects(float volume)
    {
        var vtfx = VtfxLocalSingleton.c_handle;
        foreach (var effect in playingEffects)
        {
            VtfxLocalPluginAPI.Effect_SetMasterVolume(vtfx, effect, volume);
        }
    }

    internal void InitializeTunableValues(EffectHandle effectHandle)
    {
        var vtfx = VtfxLocalSingleton.c_handle;
        foreach (EffectTunableEntry tunableEntry in effectTunableMap)
        {
            TunableHandle tunableHandle =
                VtfxLocalPluginAPI.Effect_FindTunableByName(vtfx, effectHandle, tunableEntry.tunableName);
            VtfxLocalPluginAPI.SetTunableValue(vtfx, tunableHandle, tunableEntry.tunableValue);
        }
    }

    void Update()
    {
        if (SpatializeAudio)
        {
            Vector3 pos = transform.position;
            var vtfx = VtfxLocalSingleton.c_handle;
            foreach (var effect in playingEffects)
            {
                foreach (var nodeName in spatializeAudioNodeNames)
                {
                    NodeHandle spatializeNode = VtfxLocalPluginAPI.Effect_FindNodeByName(vtfx, effect, nodeName);
                    if (spatializeNode != System.IntPtr.Zero)
                    {
                        VtfxLocalPluginAPI.Node_SetDParamValue(vtfx, spatializeNode, VtfxLocalPluginAPI.DPARAM_SPATIALIZE_3D_POSITION_X, pos.x);
                        VtfxLocalPluginAPI.Node_SetDParamValue(vtfx, spatializeNode, VtfxLocalPluginAPI.DPARAM_SPATIALIZE_3D_POSITION_Y, pos.y);
                        VtfxLocalPluginAPI.Node_SetDParamValue(vtfx, spatializeNode, VtfxLocalPluginAPI.DPARAM_SPATIALIZE_3D_POSITION_Z, -pos.z);
                    }
                }
            }
        }
    }
}
