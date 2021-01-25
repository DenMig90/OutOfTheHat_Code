using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum TutorialAction
{
    None,
    Movement,
    Jump,
    Aim,
    HatThrow,
    Teleport,
    Glide,
    TeleportNoRetrieve,
    CheckpointCreation,
    Interaction
}

[System.Serializable]
public class TutorialSet
{
    public string controllerIndication;
    public string controllerAction;
    public Sprite controllerKey;
    public string keyboardIndication;
    public string keyboardAction;
    public Sprite keyboardKey;
}

[System.Serializable]
public class Tutorial
{
    public TutorialAction action;
    public float delay;
    public bool stopTime;
    public bool immediate;
    public UnityEvent onCompleted;

    public Tutorial(TutorialAction _action, float _delay, bool _stopTime, bool _immediate, UnityEvent _onCompleted)
    {
        action = _action;
        delay = _delay;
        stopTime = _stopTime;
        immediate = _immediate;
        onCompleted = _onCompleted;
    }
}

public class TutorialManager : MonoBehaviour {
    public Text indication;
    public Text action;
    public Image key;
    public AnimateColor[] tutorialAnimations;

    private bool isController;
    [HideInInspector]
    public List<TutorialSet> sets;
    [SerializeField]
    private List<Tutorial> tutorialQueue;
    [SerializeField]
    private TutorialAction actualAction;
    private Coroutine routine;
    [SerializeField]
    private bool isShown;
    private bool timeStopped;

    public bool TimeStopped { get { return timeStopped; } }

    //private bool teleportShown;
	// Use this for initialization
	void Start () {
        //routine = null;
        actualAction = TutorialAction.None;
        isController = GameController.instance.inputManager.IsJoystick;
        GameController.instance.SetTutorialManager(this);
        isShown = false;
        tutorialQueue = new List<Tutorial>();
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        StartCoroutine(CustomUpdate());
        //teleportShown = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //public void StartTelportTutorial(float delay)
    //{
    //    //Debug.Log("tel");
    //    StartTutorial(TutorialAction.Teleport, delay, true);
    //}

    public void StartTutorial(TutorialAction action, float delay = 0, bool stopTime = false, bool immediate = false, UnityEvent onCompleted = null)
    {
        //if (actualAction != TutorialAction.None)
        //{
        //    StopActualTutorial();
        //    RemoveActionEvent();
        //}
        if(action != TutorialAction.None)
        {
            //actualAction = action;
            tutorialQueue.Add(new Tutorial(action, delay, stopTime, immediate, onCompleted));
            if (tutorialQueue.Count == 1)
            {
                actualAction = action;
                routine = StartCoroutine(ShowTutorial(delay));
            }
            AddActionEvent(action);
        }
    }

    //public void StartTeleportTutorial(float delay)
    //{
    //    //Debug.Log("1");
    //    //if (!teleportShown)
    //    //{
    //        //Debug.Log("2");
    //        if (actualAction != TutorialAction.None)
    //        {
    //            StopActualTutorial();
    //            RemoveActionEvent();
    //        }
            
    //        StartCoroutine(ShowTutorial(delay,TutorialAction.Teleport));
    //        //teleportShown = true;
    //    //}
    //}

    public void ResetToStart()
    {
        foreach (AnimateColor animation in tutorialAnimations)
        {
            animation.AnimationReset();
        }
        actualAction = TutorialAction.None;
        foreach(Tutorial tut in tutorialQueue)
        {
            RemoveActionEvent(tut.action);
        }
        tutorialQueue.Clear();
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        isShown = false;
        StopAllCoroutines();
        StartCoroutine(CustomUpdate());
        //Debug.Log(timeStopped);
        if (timeStopped)
        {
            GameController.instance.ScaleTime(1);
            //Debug.Log("rimetto a posto il tempo");
            timeStopped = false;
        }
    }

    public void StopTutorial(TutorialAction action)
    {
        if(actualAction == action)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
            if (isShown)
            {
                if(timeStopped)
                {
                    GameController.instance.ScaleTime(1);
                    timeStopped = false;
                }
                foreach (AnimateColor animation in tutorialAnimations)
                    animation.PlayBackward();
                isShown = false;
                if(tutorialQueue[0].onCompleted != null)
                    tutorialQueue[0].onCompleted.Invoke();
                if (tutorialQueue.Count > 0 && tutorialQueue[0].immediate)
                {
                    NextTutorial();
                }
                else
                //Debug.Log("stoppo " + action.ToString() + " e si vede");
                //actualAction = TutorialAction.None;
                StartCoroutine(GoToNextTutorial());
            }
            else
            {
                //Debug.Log("stoppo " + action.ToString() + " e non si vede");
                NextTutorial();
            }
        }
        Tutorial tut = null;
        foreach(Tutorial tutr in tutorialQueue)
        {
            if(tutr.action == action)
            {
                tut = tutr;
            }
        }
        if (tut != null)
        {
            if (action == actualAction && routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
            tutorialQueue.Remove(tut);
        }
        RemoveActionEvent(action);
        //actualAction = TutorialAction.None;
    }

    //public void StopTutorial()
    //{
    //    if (routine != null)
    //    {
    //        StopCoroutine(routine);
    //        routine = null;
    //    }
    //    if (isShown)
    //    {
    //        foreach (AnimateColor animation in tutorialAnimations)
    //            animation.PlayBackward();
    //        isShown = false;
    //        //actualAction = TutorialAction.None;
    //        Debug.Log("stoppo e si vede");
    //        StartCoroutine(GoToNextTutorial());
    //    }
    //    else
    //    {
    //        Debug.Log("stoppo e non si vede");
    //        NextTutorial();
    //    }
    //    Tutorial tut = null;
    //    foreach (Tutorial tutr in tutorialQueue)
    //    {
    //        if (tutr.action == actualAction)
    //        {
    //            tut = tutr;
    //        }
    //    }
    //    if (tut != null)
    //    {
    //        if (routine != null)
    //        {
    //            StopCoroutine(routine);
    //            routine = null;
    //        }
    //        tutorialQueue.Remove(tut);
    //    }
    //    RemoveActionEvent(actualAction);
    //    //actualAction = TutorialAction.None;
    //}

    private void NextTutorial()
    {
        if (tutorialQueue.Count > 0)
        {
            actualAction = tutorialQueue[0].action;
            routine = StartCoroutine(ShowTutorial(tutorialQueue[0].delay));
        }
        else
            actualAction = TutorialAction.None;
    }

    private void AddActionEvent(TutorialAction action)
    {
        switch(action)
        {
            case TutorialAction.Movement:
                GameController.instance.player.onMovement.AddListener(StopMovement);
                break;
            case TutorialAction.Jump:
                GameController.instance.player.onJump.AddListener(StopJump);
                break;
            case TutorialAction.Aim:
                GameController.instance.player.onAim.AddListener(StopAim);
                break;
            case TutorialAction.HatThrow:
                GameController.instance.player.onHatThrow.AddListener(StopHatThrow);
                break;
            case TutorialAction.Teleport:
                GameController.instance.player.onTeleport.AddListener(StopTeleport);
                GameController.instance.player.onCancel.AddListener(StopTeleport);
                break;
            case TutorialAction.TeleportNoRetrieve:
                GameController.instance.player.onTeleport.AddListener(StopTeleportNoRetrieve);
                break;
            case TutorialAction.Glide:
                GameController.instance.player.onGlideStart.AddListener(StopGlide);
                break;
            case TutorialAction.CheckpointCreation:
                GameController.instance.onCheckpointCreation.AddListener(StopCheckpointCreation);
                break;
        }
        actualAction = tutorialQueue[0].action;
    }

    private void RemoveActionEvent(TutorialAction action)
    {
        //Debug.Log("rimuovo " + action.ToString());
        switch (action)
        {
            case TutorialAction.Movement:
                GameController.instance.player.onMovement.RemoveListener(StopMovement);
                break;
            case TutorialAction.Jump:
                GameController.instance.player.onJump.RemoveListener(StopJump);
                break;
            case TutorialAction.Aim:
                GameController.instance.player.onAim.RemoveListener(StopAim);
                break;
            case TutorialAction.HatThrow:
                GameController.instance.player.onHatThrow.RemoveListener(StopHatThrow);
                break;
            case TutorialAction.Teleport:
                GameController.instance.player.onTeleport.RemoveListener(StopTeleport);
                GameController.instance.player.onCancel.RemoveListener(StopTeleport);
                break;
            case TutorialAction.TeleportNoRetrieve:
                GameController.instance.player.onTeleport.RemoveListener(StopTeleportNoRetrieve);
                break;
            case TutorialAction.Glide:
                GameController.instance.player.onGlideStart.RemoveListener(StopGlide);
                break;
            case TutorialAction.CheckpointCreation:
                GameController.instance.onCheckpointCreation.RemoveListener(StopCheckpointCreation);
                break;
        }
        //NextTutorial();
    }

    private void StopMovement()
    {
        StopTutorial(TutorialAction.Movement);
    }
    private void StopJump()
    {
        StopTutorial(TutorialAction.Jump);
    }
    private void StopAim()
    {
        StopTutorial(TutorialAction.Aim);
    }
    private void StopHatThrow()
    {
        StopTutorial(TutorialAction.HatThrow);
    }
    private void StopTeleport()
    {
        StopTutorial(TutorialAction.Teleport);
    }
    private void StopTeleportNoRetrieve()
    {
        StopTutorial(TutorialAction.TeleportNoRetrieve);
    }
    private void StopGlide()
    {
        StopTutorial(TutorialAction.Glide);
    }
    private void StopCheckpointCreation()
    {
        StopTutorial(TutorialAction.CheckpointCreation);
    }

    private IEnumerator ShowTutorial(float delay)
    {
        float time = 0;
        while (time < delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.unscaledDeltaTime;
        }
        if (GameController.instance.player.IsDead)
        {
            routine = null;
            yield break;
        }
        if (tutorialQueue.Count > 0 && tutorialQueue[0].stopTime)
        {
            GameController.instance.ScaleTime(0);
            //Debug.Log("stoppo il tempo"); 
            timeStopped = true;
        }
            foreach (AnimateColor animation in tutorialAnimations)
                animation.PlayForward();
            isShown = true;
        routine = null;
    }

    private IEnumerator GoToNextTutorial()
    {
        float time = 0;
        //yield return new WaitForEndOfFrame();
        if (!(tutorialQueue.Count > 0 && tutorialQueue[0].immediate))
        {
            while (time < tutorialAnimations[0].duration)
            {
                yield return new WaitForEndOfFrame();
                time += Time.unscaledDeltaTime;
                if (GameController.instance.player.IsDead)
                    yield break;
            }
        }
        NextTutorial();
    }

    //private IEnumerator ShowTutorial(float delay, TutorialAction action)
    //{
    //    float time = 0;
    //    while(time < delay)
    //    {
    //        yield return new WaitForEndOfFrame();
    //        time += Time.deltaTime;
    //    }
    //    actualAction = action;
    //    //Debug.Log("faccio");
    //    AddActionEvent();
    //    foreach (AnimateColor animation in tutorialAnimations)
    //        animation.PlayForward();
    //    isShown = true;
    //    routine = null;
    //}

    private IEnumerator CustomUpdate()
    {
        //Debug.Log("starto");
        while (true)
        {
            if (actualAction != TutorialAction.None)
            {
                //Debug.Log(actualAction);
                if (isController != GameController.instance.inputManager.IsJoystick)
                {
                    isController = GameController.instance.inputManager.IsJoystick;
                }
                if (isController)
                {
                    indication.text = sets[(int)actualAction - 1].controllerIndication.ToUpper();
                    action.text = sets[(int)actualAction - 1].controllerAction.ToUpper();
                    key.sprite = sets[(int)actualAction - 1].controllerKey;
                }
                else
                {
                    indication.text = sets[(int)actualAction - 1].keyboardIndication.ToUpper();
                    action.text = sets[(int)actualAction - 1].keyboardAction.ToUpper();
                    key.sprite = sets[(int)actualAction - 1].keyboardKey;
                }
            }
            yield return null;
        }
    }
}
