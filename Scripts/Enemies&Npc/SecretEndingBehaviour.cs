using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SecretEndingBehaviour : MonoBehaviour {

    public UnityEvent onNormalEnding;
    public UnityEvent onSecretEnding;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TriggerEnding()
    {
        if (GameController.instance.alternativeEndingUnlocked)
            onSecretEnding.Invoke();
        else
            onNormalEnding.Invoke();
    }
}
