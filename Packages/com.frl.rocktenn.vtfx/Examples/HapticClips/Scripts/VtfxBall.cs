using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class VtfxBall : MonoBehaviour
{
    public UnityEvent onCollisionEnter;

    private void OnCollisionEnter(Collision collision)
    {
        if (onCollisionEnter != null)
            onCollisionEnter.Invoke();
    }
}
