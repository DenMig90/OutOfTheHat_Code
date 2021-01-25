using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayedEvent : MonoBehaviour {

    public float delay;
    public UnityEvent onDelay;

    public void CallEvent()
    {
        Invoke("Call", delay);
    }

    private void Call()
    {
        onDelay.Invoke();
    }

}
