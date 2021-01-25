using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class EaterTrapBehaviour : MonoBehaviour {
    public float closingDelay;
    public float openingDelay;
    public float closingTime;
    public float openingTime;
    public GameObject mouthLeft;
    public GameObject mouthRight;
    public PlayerKiller killingArea;
    public Animator animator;
    [EventRef] public string carnivorousSound;

    //private bool isEating;
    private Coroutine eatRoutine;
    private Quaternion leftStartRot;
    private Quaternion leftEndRot;
    private Quaternion rightStartRot;
    private Quaternion rightEndRot;
    private float closeAnimDuration = 0;
    private float openAnimDuration = 0;
    private EventInstance soundInstance;
    private bool isAlive;

    // Use this for initialization
    void Start () {
        //isEating = false;
        eatRoutine = null;
        killingArea.gameObject.SetActive(false);
        //GameController.instance.AddEaterTrap(this);
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        leftStartRot = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        rightStartRot = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        leftEndRot = Quaternion.Euler(new Vector3(0f, 0f, -90f));
        rightEndRot = Quaternion.Euler(new Vector3(0f, 0f, 90f));

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            //Debug.Log(clip.name);
            switch (clip.name)
            {
                case "RedPlantOpen":
                    openAnimDuration = clip.length;
                    break;
                case "RedPlantSnap":
                    closeAnimDuration = clip.length;
                    break;
            }
        }
        animator.SetFloat("OpenSpeed",openingTime/openAnimDuration);
        animator.SetFloat("CloseSpeed",closingTime/closeAnimDuration);
        animator.SetFloat("Alerted", 0);
        isAlive = true;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player" && isAlive)
        {
            if (eatRoutine == null)
            {
                eatRoutine = StartCoroutine(Eat(true));
            }
        }
        if (other.gameObject.tag == "Bomblebee" && isAlive)
        {
            if (eatRoutine == null)
            {
                eatRoutine = StartCoroutine(Eat(false));
            }
            //Kill();
        }
    }

    public void Kill()
    {
        isAlive = false;
    }

    public void ResetToStart()
    {
        if(eatRoutine != null)
        {
            StopCoroutine(eatRoutine);
            eatRoutine = null;
        }
        killingArea.gameObject.SetActive(false);
        mouthLeft.transform.localRotation = leftStartRot;
        mouthRight.transform.localRotation = rightStartRot;
        animator.Rebind();
        animator.SetFloat("OpenSpeed", openingTime / openAnimDuration);
        animator.SetFloat("CloseSpeed", closingTime / closeAnimDuration);
        animator.SetFloat("Alerted", 0);
        GameController.instance.audioManager.DestroyInstance(soundInstance);
        isAlive = true;
    }

    private IEnumerator Eat(bool delayed)
    {
        float time = 0;
        animator.SetFloat("Alerted", 1);
        if (delayed)
        {
            PlayCarnivorousSound();
            while (time <= closingDelay)
            {
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
            }
        }
        time = 0;
        animator.SetTrigger("Close");
        while (time <= closingTime)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
            mouthLeft.transform.localRotation = Quaternion.Lerp(leftStartRot, leftEndRot, time/closingTime);
            mouthRight.transform.localRotation = Quaternion.Lerp(rightStartRot, rightEndRot, time / closingTime);
        }
        killingArea.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();
        killingArea.gameObject.SetActive(false);
        if (delayed)
        {
            time = Time.deltaTime;
            while (time <= openingDelay)
            {
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
            }
        }
        time = 0;
        animator.SetTrigger("Open");
        animator.SetFloat("Alerted", 0);
        while (time <= openingTime)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
            mouthLeft.transform.localRotation = Quaternion.Lerp(leftEndRot, leftStartRot, time / openingTime);
            mouthRight.transform.localRotation = Quaternion.Lerp(rightEndRot, rightStartRot, time / openingTime);
        }
        eatRoutine = null;
    }

    public void PlayCarnivorousSound()
    {
        soundInstance = GameController.instance.audioManager.PlayGenericSound(carnivorousSound, gameObject);
    }
}
