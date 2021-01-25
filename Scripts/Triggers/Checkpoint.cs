using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

public class Checkpoint : MonoBehaviour {
    public bool isFake;
    public Animator anim;
    [Tooltip("Direction is green arrow")]
    public Transform playerRespawn;
    [EventRef] public string checkpointSound;

    private bool activated;
    private bool canShake;

    private void Awake()
    {
        canShake = true;
        anim.SetBool("IsCustom", false);
    }

    private void Start()
    {
        activated = false;
        //GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
    }

    public void ResetToStart()
    {
        activated = false;
        anim.Rebind();
    }

    void OnTriggerEnter(Collider col)
    {
        //Debug.Log(col.tag);
        if (col.tag != "Player")
            return;

        GameController.instance.SetActualCheckpoint(this, playerRespawn.position, playerRespawn.up, !isFake);

        if (!activated)
        {
            activated = true;
            canShake = false;
            anim.ResetTrigger("ShakeRX");
            anim.ResetTrigger("ShakeSX");
            anim.SetTrigger("Open");
            PlayCheckpointSound();
        }
    }

    public void PlayCheckpointSound()
    {
        GameController.instance.audioManager.PlayGenericSound(checkpointSound, gameObject);
    }

    public void Shake()
    {
        if(activated && canShake)
        {
            anim.ResetTrigger("ShakeRX");
            anim.ResetTrigger("ShakeSX");
            canShake = false;
            if(GameController.instance.player.transform.position.x < transform.position.x)
                anim.SetTrigger("ShakeRX");
            else
                anim.SetTrigger("ShakeSX");
        }
    }

    public void OnAnimationEnd()
    {
        canShake = true;
    }
}
