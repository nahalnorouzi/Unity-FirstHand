using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VtfxCube : MonoBehaviour
{
    public Gradient gradient;

    public Vector3 moveGains = new Vector3(3,0,0);
    public Vector3 rotateGains = new Vector4(0, 45, 0);
    public float scaleGain = 0.5f;

    MeshRenderer _rend;
    Vector3 _initialPos;
    Vector3 _initialRot;

    // Start is called before the first frame update
    void Awake()
    {
        _rend = GetComponent<MeshRenderer>();
        _initialPos = transform.localPosition;
        _initialRot = transform.localEulerAngles;
    }

    public void BlendCube(float t)
    {
        _rend.material.color = gradient.Evaluate(t);
    }

    public void MoveCube(float t)
    {
        transform.localPosition = _initialPos + t * moveGains;
    }

    public void RotateCube(float t)
    {
        transform.localEulerAngles = _initialRot + t * rotateGains;
    }

    public void ScaleCube(float t)
    {
        t = 1 - t * scaleGain;
        transform.localScale = new Vector3(t, t, t);
    }

}
