using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;

public class ClockTriggerBehaviour : MonoBehaviour {

    public float delayBetweenChimes = 1f;
    public UnityEvent onChime;

    private Coroutine routine;

    // Use this for initialization
    void Start () {
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
    }

    public void ResetToStart()
    {
        if(routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private void OnDestroy()
    {
        GameController.instance.RemoveOnNewGameDelegate(ResetToStart);
        GameController.instance.RemoveOnNewGameDelegate(ResetToStart);
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void StartChiming()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        routine = StartCoroutine(Chiming());
    }

    private IEnumerator Chiming()
    {
        int number = DateTime.Now.Hour;
        if (number > 12)
            number -= 12;
        int counter = 0;
        float time = 0;
        while (counter < number)
        {
            onChime.Invoke();
            counter++;
            time = 0;
            while(time < delayBetweenChimes)
            {
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
            }
        }
        routine = null;
    }
}
