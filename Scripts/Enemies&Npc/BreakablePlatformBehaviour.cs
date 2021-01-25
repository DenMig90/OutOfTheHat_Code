using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public enum PlatformType
{
    Liana
}

public class BreakablePlatformBehaviour : MonoBehaviour {

    public float destructionDelay;
    public bool hasToReset;
    public Animator anim;
    public PlatformType type;
    [EventRef] public string startBreakingSound;
    [EventRef] public string breakSound;
    public float breakSoundDelay = 0.31f;

    private Coroutine routine;
    private new BoxCollider collider;
    private bool up;
    private bool hasSounded;

    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
    }

    // Use this for initialization
    void Start () {
        if (hasToReset)
        {
            GameController.instance.AddOnDeathDelegate(ResetToStart);
            GameController.instance.AddOnNewGameDelegate(ResetToStart);
        }
        routine = null;
        up = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider col)
    {
        //bool contactedUp = ((col.contacts[0].point.y > transform.position.y && transform.up.y > 0) || (col.contacts[0].point.y < transform.position.y && transform.up.y < 0)) && Mathf.Abs(col.contacts[0].point.x - transform.position.x) < (collider.size.x*transform.localScale.x) / 2;
        if ((col.gameObject.tag == "Player" || col.gameObject.tag == "Bunny" )/* && contactedUp*/ && collider.enabled)
        {
            up = col.gameObject.transform.position.y >= transform.position.y;
            //Debug.Log("inizio autodistruzione");
            if (routine == null)
                routine = StartCoroutine(Destruction());
        }

        //if (col.gameObject.tag == "Teleport" && GameController.instance.player.teleportHat.IsHatarang && collider.enabled)
        //{
        //    up = col.gameObject.transform.position.y >= transform.position.y;
        //    PlayBreakSound();
        //    if (anim != null)
        //        anim.SetTrigger((up ? "Down" : "Up") + "Destr");
        //    collider.enabled = false;
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Teleport" && GameController.instance.player.teleportHat.IsHatarang && collider.enabled)
        {
            up = collision.gameObject.transform.position.y >= transform.position.y;
            PlayBreakSound();
            if (anim != null)
                anim.SetTrigger((up ? "Down" : "Up") + "Destr");
            collider.enabled = false;
        }
    }

    public void ResetToStart()
    {
        if(routine != null )
            StopCoroutine(routine);
        routine = null;
        collider.enabled = true;
        if (anim != null)
            anim.Rebind();
    }

    private IEnumerator Destruction()
    {
        float time = 0;
        PlayStartBreakingSound();
        hasSounded = false;
        if (anim != null)
            anim.SetTrigger("StartDestr");
        while (time < destructionDelay)
        {
            if (time >= destructionDelay - breakSoundDelay && !hasSounded)
            {
                PlayBreakSound();
                hasSounded = true;
            }
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        if(anim!=null)
            anim.SetTrigger((up ? "Down" : "Up") + "Destr");
        collider.enabled = false;
        routine = null;
    }

    public void PlayBreakSound()
    {
        GameController.instance.audioManager.PlayGenericSound(breakSound, gameObject, "PlatformType", (float)type);
    }

    public void PlayStartBreakingSound()
    {
        GameController.instance.audioManager.PlayGenericSound(startBreakingSound, gameObject, "PlatformType", (float)type);
    }
}
