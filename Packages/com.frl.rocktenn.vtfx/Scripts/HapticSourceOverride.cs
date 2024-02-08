using System.ComponentModel;
using UnityEngine;

using EffectHandle = System.IntPtr;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class HapticSourceOverride : MonoBehaviour
{
    private ConfigureEffectBeforePlayDelegate configureEffectBeforePlayDelegate_;

    void OnEnable()
    {
        // create a delegate instance once, so it can be registered and unregistered correctly
        configureEffectBeforePlayDelegate_ = OnConfigureEffectBeforePlay;

        HapticSource[] hapticSources = gameObject.GetComponents<HapticSource>();
        foreach (var hapticSource in hapticSources)
        {
            hapticSource.RegisterConfigureEffectBeforePlayDelegate(configureEffectBeforePlayDelegate_);
        }
    }

    void OnDisable()
    {
        HapticSource[] hapticSources = gameObject.GetComponents<HapticSource>();
        foreach (var hapticSource in hapticSources)
        {
            hapticSource.UnregisterConfigureEffectBeforePlayDelegate(configureEffectBeforePlayDelegate_);
        }
    }

    protected abstract void OnConfigureEffectBeforePlay(EffectHandle effectHandle);
}
