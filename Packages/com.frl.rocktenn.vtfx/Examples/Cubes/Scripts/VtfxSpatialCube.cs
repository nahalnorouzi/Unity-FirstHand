using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VtfxSpatialCube : MonoBehaviour
{
    public float radius = 10.0f;
    public float frequency = 0.5f;

    // Update is called once per frame
    void Update()
    {
        float x = radius * Mathf.Sin(2 * Mathf.PI * frequency * Time.time);
        float z = radius * Mathf.Cos(2 * Mathf.PI * frequency * Time.time);
        transform.position = new Vector3(x, 0, z);
    }
}
