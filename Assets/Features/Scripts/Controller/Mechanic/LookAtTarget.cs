
using System;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform target;
    private void Awake()
    {
        target = Camera.main.transform;
    }

    public void Update()
    {
        if (target )
        {
            transform.LookAt(transform.position + target.transform.rotation * Vector3.forward,
                target.transform.rotation * Vector3.up);
        }
    }
}
