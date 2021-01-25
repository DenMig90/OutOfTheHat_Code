using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using FMODUnity;
using FMOD.Studio;
//public enum Direction
//{
//    None,
//    Up,
//    Right,
//    Left,
//    Down
//}

public class PlayerKiller : MonoBehaviour {

    public float upSpeed;
    public float rightSpeed;
    public float leftSpeed;
    public float downSpeed;
    public bool playOnStart;
    public float delay;
    public float duration = 0;
    //public Direction dirThatAffectsAnimations;
    public Animator[] animators;
    //public AnimatePosition positionAnim;
    //public float growAnimationDuration = 0.8f;
    public UnityEvent onGrow;
    public UnityEvent onReset;
    [EventRef] public string risingSound;
    [EventRef] public string stopSound;

    private Vector3 startSize;
    private Vector3 startPosition;
    private Coroutine routine;

    private void Start()
    {
        startSize = transform.localScale;
        startPosition = transform.localPosition;
        //GameController.instance.AddPlayerKiller(this);
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        //float effectiveSpeed = 1;
        //switch (dirThatAffectsAnimations)
        //{
        //    case Direction.Up:
        //        effectiveSpeed = upSpeed;
        //        break;
        //    case Direction.Right:
        //        effectiveSpeed = rightSpeed;
        //        break;
        //    case Direction.Left:
        //        effectiveSpeed = leftSpeed;
        //        break;
        //    case Direction.Down:
        //        effectiveSpeed = downSpeed;
        //        break;
        //}
        //if (anim != null)
        //{
        //    if (dirThatAffectsAnimations == Direction.None)
        //    {
        //        anim.speed = growAnimationDuration/duration;
        //    }
        //    else
        //        anim.speed = effectiveSpeed;
        //}
        //if (positionAnim != null)
        //{
        //    positionAnim.duration = duration;
        //    positionAnim.to = positionAnim.from + Vector3.up * effectiveSpeed * duration;
        //}
        routine = null;
        if(playOnStart)
            StartExpanding();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.tag != "Player")
            return;

        if (GameController.instance.player.IsCaptured)
            return;

        GameController.instance.player.StartKill();
        GameController.instance.player.PlayExplosionSound();
    }


    void OnTriggerEnter(Collider col)
    {
        if (col.tag != "Player")
            return;

        if (GameController.instance.player.IsCaptured)
            return;

        GameController.instance.player.StartKill();
        GameController.instance.player.PlayExplosionSound();
    }

    public void StartExpanding()
    {
        if (routine == null)
        {
            routine = StartCoroutine(Expansion());
        }
    }

    public void StopExpanding()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        foreach (Animator anim in animators)
        {
            if (anim != null)
                anim.SetBool("IsActive", false);
        }
        StopRisingSound();
    }

    public void ResetToStart()
    {
        onReset.Invoke();
        StopExpanding();
        transform.localScale = startSize;
        transform.localPosition = startPosition;
        foreach (Animator anim in animators)
        {
            if (anim != null)
                anim.Rebind();
        }
        //if (positionAnim != null)
        //    positionAnim.AnimationReset();
    }

    public void PlayRisingSound()
    {
        GameController.instance.audioManager.PlayGenericSound(risingSound, gameObject);
    }

    public void StopRisingSound()
    {
        GameController.instance.audioManager.PlayGenericSound(stopSound, gameObject);
    }

    private IEnumerator Expansion()
    {
        float time = 0;
        while (time < delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        foreach (Animator anim in animators)
        {
            if (anim != null)
                anim.SetBool("IsActive", true);
        }
        //if (positionAnim != null)
        //    positionAnim.PlayForward();
        PlayRisingSound();
        onGrow.Invoke();
        time = 0;
        while (gameObject.activeSelf)
        {
            if(duration != 0)
            {
                if (time > duration)
                    break;
            }
            transform.localScale += new Vector3((rightSpeed + leftSpeed) * Time.deltaTime, (upSpeed + downSpeed) * Time.deltaTime, 0);
            transform.position += new Vector3((rightSpeed / 2 - leftSpeed / 2) * Time.deltaTime, (upSpeed / 2 - downSpeed / 2) * Time.deltaTime, 0);
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        foreach (Animator anim in animators)
        {
            if (anim != null)
                anim.SetBool("IsActive", false);
        }
        StopRisingSound();
    }
}
