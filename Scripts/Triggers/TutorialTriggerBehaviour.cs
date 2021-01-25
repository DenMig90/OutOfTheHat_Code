using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialTriggerBehaviour : MonoBehaviour {

    public string tagThatTriggers = "Player";

    public TutorialAction action;
    public float delay;
    public bool stopTime = false;
    public bool startImmediatly = false;
    public bool dontResetOnDeath = false;
    public bool disableObject = true;
    public bool disableCollider = true;
    public bool stopTutorialOnExit = false;
    public bool disableObjectOnExit = true;
    public bool disableColliderOnExit = true;
    public bool stopTutorialWithDelay = false;
    public float stopDelay;

    public UnityEvent onTutorialCompleted;

    private new Collider collider;
    private bool triggered;

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    // Use this for initialization
    void Start () {
        if(!dontResetOnDeath)
            GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        triggered = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ResetToStart()
    {
        collider.enabled = true;
        triggered = false;
        CancelInvoke("StopTutorial");
    }

    private void OnTriggerStay(Collider other)
    {
        if (triggered)
            return;
        if(other.gameObject.tag == tagThatTriggers)
        {
            //Debug.Log("triggero");
            GameController.instance.tutorialManager.StartTutorial(action, delay, stopTime, startImmediatly, onTutorialCompleted);
            if(stopTutorialWithDelay)
                Invoke("StopTutorial", stopDelay);
            if (disableObject)
                gameObject.SetActive(false);
            else if(disableCollider)
                collider.enabled = false;
            triggered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == tagThatTriggers && triggered)
        {
            //Debug.Log("triggero exit");
            if (stopTutorialOnExit)
            {
                GameController.instance.tutorialManager.StopTutorial(action);
            }
            if (disableObjectOnExit)
                gameObject.SetActive(false);
            else if (disableColliderOnExit)
                collider.enabled = false;
            triggered = false;
        }
    }

    private void StopTutorial()
    {
        GameController.instance.tutorialManager.StopTutorial(action);
    }
}
