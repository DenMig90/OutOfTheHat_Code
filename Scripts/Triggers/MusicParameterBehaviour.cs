using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MusicParameterBehaviour : MonoBehaviour {

    public SoundParameters[] parameters;

    private EventInstance? instance = null;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetInstance(EventInstance _instance)
    {
        instance = _instance;
    }

    public void ChangeParameters()
    {
        if (instance != null)
        {
            //Debug.Log("no null");
            GameController.instance.audioManager.ChangeInstanceParameters((EventInstance)instance, parameters);
        }
        else
        {
            //Debug.Log("null");
            GameController.instance.audioManager.ChangeMusicParameters(parameters);
        }
    }
}
