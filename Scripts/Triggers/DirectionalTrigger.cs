using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DirectionalTrigger : MonoBehaviour {
    public string tagThatTriggers = "Player";
    public float enterDelay = 0;
    public float exitDelay = 0;
    public UnityEvent eventsToTriggerOnEnterRight;
    public UnityEvent eventsToTriggerOnEnterLeft;
    public UnityEvent eventsToTriggerOnExitRight;
    public UnityEvent eventsToTriggerOnExitLeft;

    private Coroutine enterRightRoutine;
    private Coroutine enterLeftRoutine;
    private Coroutine exitRightRoutine;
    private Coroutine exitLeftRoutine;
    private new Collider collider;

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    // Use this for initialization
    void Start () {
        enterRightRoutine = null;
        exitRightRoutine = null;
        enterLeftRoutine = null;
        exitLeftRoutine = null;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == tagThatTriggers)
        {
            if (other.transform.position.x > transform.position.x)
            {
                TriggerEnterRight();
            }
            else
            {
                TriggerEnterLeft();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == tagThatTriggers)
        {
            if (other.transform.position.x > transform.position.x)
            {
                TriggerExitRight();
            }
            else
            {
                TriggerExitLeft();
            }
        }
    }

    private void TriggerEnterRight()
    {
        //Debug.Log("enter right");
        if (enterDelay == 0)
            eventsToTriggerOnEnterRight.Invoke();
        else
        {
            if (enterRightRoutine != null)
                StopCoroutine(enterRightRoutine);
            enterRightRoutine = StartCoroutine(TriggetEventDelayed(true, true));
        }
    }

    private void TriggerEnterLeft()
    {
        //Debug.Log("enter left");
        if (enterDelay == 0)
            eventsToTriggerOnEnterLeft.Invoke();
        else
        {
            if (enterLeftRoutine != null)
                StopCoroutine(enterLeftRoutine);
            enterLeftRoutine = StartCoroutine(TriggetEventDelayed(true, false));
        }
    }

    private void TriggerExitRight()
    {
        //Debug.Log("exit right");
        if (exitDelay == 0)
            eventsToTriggerOnExitRight.Invoke();
        else
        {
            if (exitRightRoutine != null)
                StopCoroutine(exitRightRoutine);
            exitRightRoutine = StartCoroutine(TriggetEventDelayed(false, true));
        }
    }

    private void TriggerExitLeft()
    {
        //Debug.Log("exit left");
        if (exitDelay == 0)
            eventsToTriggerOnExitLeft.Invoke();
        else
        {
            if (exitLeftRoutine != null)
                StopCoroutine(exitLeftRoutine);
            exitLeftRoutine = StartCoroutine(TriggetEventDelayed(false, false));
        }
    }

    private IEnumerator TriggetEventDelayed(bool enter, bool right)
    {
        float time = 0;
        float _delay = enter ? enterDelay : exitDelay;
        while (time < _delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        if (enter)
        {
            if (right)
            {
                eventsToTriggerOnEnterRight.Invoke();
                enterRightRoutine = null;
            }
            else
            {
                eventsToTriggerOnEnterLeft.Invoke();
                enterLeftRoutine = null;
            }
        }
        else
        {
            if (right)
            {
                eventsToTriggerOnExitRight.Invoke();
                exitRightRoutine = null;
            }
            else
            {
                eventsToTriggerOnExitLeft.Invoke();
                exitLeftRoutine = null;
            }
        }
    }
}
