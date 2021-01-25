using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TagSelectorAttribute : PropertyAttribute
{
    public TagSelectorAttribute()
    {

    }
}

public class TriggerEvent : MonoBehaviour {

    [TagSelector]
    public string tagThatTriggers = "Player";
    public float delay = 0;
    public float exitDelay = 0;
    public UnityEvent eventsToTrigger;
    public UnityEvent eventsToTriggerOnExit;
    public bool compoundColliders;
    //public bool debug;

    private Coroutine routine;
    private Coroutine exitRoutine;
    private bool isInside;
    private bool prevInside;

    // Use this for initialization
    void Start () {
        routine = null;
        exitRoutine = null;
        isInside = false;
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
	}

    private void OnDestroy()
    {
        GameController.instance.RemoveOnNewGameDelegate(ResetToStart);
        GameController.instance.RemoveOnNewGameDelegate(ResetToStart);
    }

    public void ResetToStart()
    {
        StopAllCoroutines();
    }
	
	// Update is called once per frame
	void Update () {
        //if(debug)
        //    Debug.Log(isInside);
	}

    private void OnTriggerStay(Collider other)
    {
        if (compoundColliders)
        {
            if (other.gameObject.tag == tagThatTriggers)
            {
                isInside = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!compoundColliders)
        {
            if (other.gameObject.tag == tagThatTriggers)
            {
                TriggerEnter();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!compoundColliders)
        {
            if (other.gameObject.tag == tagThatTriggers)
            {
                TriggerExit();
            }
        }
    }

    private void LateUpdate()
    {
        if (compoundColliders)
        {
            if (prevInside != isInside)
            {
                if (isInside)
                    TriggerEnter();
                else
                    TriggerExit();
            }
            prevInside = isInside;
            isInside = false;
        }
    }

    private void TriggerEnter()
    {
        if (delay == 0)
            eventsToTrigger.Invoke();
        else
        {
            if (routine != null)
                StopCoroutine(routine);
            routine = StartCoroutine(TriggetEventDelayed(true));
        }
    }

    private void TriggerExit()
    {
        if (exitDelay == 0)
            eventsToTriggerOnExit.Invoke();
        else
        {
            if (exitRoutine != null)
                StopCoroutine(exitRoutine);
            exitRoutine = StartCoroutine(TriggetEventDelayed(false));
        }
    }

    private IEnumerator TriggetEventDelayed(bool enter)
    {
        float time = 0;
        float _delay = enter ? delay : exitDelay;
        while (time < _delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        if (enter)
            eventsToTrigger.Invoke();
        else
            eventsToTriggerOnExit.Invoke();
        if (enter)
            routine = null;
        else
            exitRoutine = null;
    }
}
