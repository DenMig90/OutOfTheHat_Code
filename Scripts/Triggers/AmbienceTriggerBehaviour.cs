using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AmbienceTriggerBehaviour : MonoBehaviour {

    [EventRef] public string ambienceToTrigger;
    public SoundParameters[] parameters;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartMusicToTrigger()
    {
        if (parameters.Length == 0)
            GameController.instance.audioManager.ChangeAmbience(ambienceToTrigger);
        else
            GameController.instance.audioManager.ChangeAmbience(ambienceToTrigger, parameters);
    }
}
