using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerInteraction : MonoBehaviour {
    public string message;
    public UnityEvent onInteraction;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddInteraction()
    {
        GameController.instance.player.AddInteraction(InteractionEvent);
        GameController.instance.inputManager.SetInteraction(true);
    }

    public void RemoveInteraction()
    {
        GameController.instance.player.AddInteraction(RemoveInteraction);
        GameController.instance.inputManager.SetInteraction(false);
    }

    public void InteractionEvent()
    {
        onInteraction.Invoke();
    }
}
