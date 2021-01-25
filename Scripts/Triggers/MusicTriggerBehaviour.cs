using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MusicTriggerBehaviour : MonoBehaviour {

    public float delay;
    public bool oneshot;
    [EventRef] public string musicToTrigger;
    public SoundParameters[] parameters;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartMusicToTrigger()
    {
        StartCoroutine(TriggerDelayed());
    }

    private IEnumerator TriggerDelayed()
    {
        float time = 0;
        while (time < delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        if (parameters.Length == 0)
            GameController.instance.audioManager.ChangeMusic(musicToTrigger, oneshot);
        else
            GameController.instance.audioManager.ChangeMusic(musicToTrigger, parameters, oneshot);
    }
}
