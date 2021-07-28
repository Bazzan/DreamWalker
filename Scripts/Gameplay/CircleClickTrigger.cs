using System;
using UnityEngine;

public class CircleClickTrigger : MonoBehaviour
{
    public Action<Collider> triggered;
    
    private void OnTriggerEnter(Collider other)
    {
        triggered?.Invoke(other);
    }
}
