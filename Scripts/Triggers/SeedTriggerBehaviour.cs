using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FMOD.Studio;
using FMODUnity;

public class SeedTriggerBehaviour : MonoBehaviour {

    public int normalSeedNumber;
    public int easySeedNumber;
    public UnityEvent onTrigger;
    //public float maxDistanceToSound=10;
    [EventRef] public string seedIdle;
    [EventRef] public string seedCollect;
    //public Animator anim;

    //private bool enabled = true;

    private void Awake()
    {

    }

    void Start () {
        //GameController.instance.AddOnNewGameDelegate(ResetToStart);
        //GameController.instance.AddOnSaveStateDelegate(SaveState);
        //GameController.instance.AddOnLoadStateDelegate(LoadState);
    }

    private void OnEnable()
    {
        //if(GameController.instance != null)
        //LoadState();
    }

    //public void ResetToStart()
    //{
    //    enabled = true;
    //    anim.Rebind();
    //}

    //public void SaveState()
    //{
    //    bool state = enabled;
    //    if (GameController.instance.SaveData.seedsActiveStates.ContainsId(gameObject.GetInstanceID()))
    //        GameController.instance.SaveData.seedsActiveStates.Set(gameObject.GetInstanceID(), state);
    //    else
    //        GameController.instance.SaveData.seedsActiveStates.Add(gameObject.GetInstanceID(), gameObject.name, state);
    //}

    //public void LoadState()
    //{
    //    bool state = true;
    //    if (GameController.instance.SaveData.seedsActiveStates.ContainsId(gameObject.GetInstanceID()))
    //    {
    //        state = GameController.instance.SaveData.seedsActiveStates.Get(gameObject.GetInstanceID());
    //    }
    //    enabled = state;
    //    //Debug.Log(enabled);
    //    if (enabled)
    //    {
    //        ResetToStart();
    //    }
    //    else
    //    {
    //        anim.SetTrigger("Collect");
    //    }
    //}

    // Update is called once per frame
    void Update () {
		
	}

    public void AddInteraction()
    {
        GameController.instance.StartInteraction(Trigger);
    }

    public void RemoveInteraction()
    {
        GameController.instance.EndInteraction(Trigger);
    }

    public void Trigger()
    {
        //if (enabled)
        //{
        int seeds = 0;
        switch (GameController.instance.difficultyManager.ActualDifficulty)
        {
            case Difficulty.Easy:
                seeds = easySeedNumber;
                break;
            case Difficulty.Normal:
                seeds = normalSeedNumber;
                break;
        }
        GameController.instance.AddCheckpointSeeds(seeds);
        PlaySeedCollectSound();
        onTrigger.Invoke();
        RemoveInteraction();
        gameObject.SetActive(false);
        //    enabled = false;
        //    anim.SetTrigger("Collect");
        //}
    }

    public void PlaySeedIdleSound()
    {
        //if(Vector3.Distance(transform.position, GameController.instance.player.transform.position) < maxDistanceToSound)
            GameController.instance.audioManager.PlayGenericSound(seedIdle, gameObject);
    }

    public void PlaySeedCollectSound()
    {
        GameController.instance.audioManager.PlayGenericSound(seedCollect, gameObject);
    }
}
