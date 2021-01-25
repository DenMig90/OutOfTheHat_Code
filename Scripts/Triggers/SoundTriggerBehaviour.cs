using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class SoundTriggerBehaviour : MonoBehaviour {

    [EventRef] public string soundToTrigger;
    public float delay;
    public float zPosition;
    public SoundParameters[] parameters;
    public MusicParameterBehaviour[] parameterChangers;
    private GameObject projection;

    private EventInstance soundInstance;

    // Use this for initialization
    void Start()
    {
        if (parameterChangers.Length != 0)
        {
            foreach (MusicParameterBehaviour changer in parameterChangers)
            {
                changer.SetInstance(soundInstance);
            }
        }
        projection = new GameObject("Projection");
        projection.transform.parent = transform;
        projection.transform.localPosition = Vector3.zero;
        //GameController.instance.AddSoundTrigger(this);
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetToStart()
    {
        GameController.instance.audioManager.DestroyInstance(soundInstance);
    }

    public void Trigger()
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

        if (GameController.instance == null)
            yield break;

        projection.transform.position = new Vector3(projection.transform.position.x, projection.transform.position.y, zPosition);
        if (parameters.Length == 0)
            soundInstance = GameController.instance.audioManager.PlayGenericSound(soundToTrigger, projection);
        else
            soundInstance = GameController.instance.audioManager.PlayGenericSound(soundToTrigger, projection, parameters);
        if (parameterChangers.Length != 0)
        {
            foreach (MusicParameterBehaviour changer in parameterChangers)
            {
                changer.SetInstance(soundInstance);
            }
        }
    }
}
