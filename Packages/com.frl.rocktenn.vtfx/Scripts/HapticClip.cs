using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This can be uncommented to add an entry to the project folder Right Click -> Create context menu:
public class HapticClip : ScriptableObject
{
    public string description;
    public float duration = 0;
    public bool looping = false;
    public float masterVolume = 1;
    [TextArea(1,1000)]
    public string json;
}
