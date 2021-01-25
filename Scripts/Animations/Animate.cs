using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum AnimationType
{
    Once,
    Loop,
    PingPong
}

public class Animate<T> : MonoBehaviour {
    public string id;
    public T from;
    public T to;
    public float startDelay;
    public float duration;
    public bool playOnStart;
    public bool ignoreTimeScale;
    public AnimationType type;
    public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public UnityEvent onStart;
    public UnityEvent onStartForward;
    public UnityEvent onStartBackward;
    public UnityEvent onFinish;
    public UnityEvent onFinishForward;
    public UnityEvent onFinishBackward;
    public UnityEvent onPause;
    public UnityEvent onResume;
    public UnityEvent onReset;
    public UnityEvent onSetEnd;
    public UnityEvent onEnable;
    public UnityEvent onDisable;
    [Header("PingPong/Loop Only")]
    public UnityEvent onLoopForward;
    public UnityEvent onLoopBackward;

    public float ActualLerp { get { return lerp; } }

    protected AnimationCurve reversedCurve;
    protected bool forward;
    protected bool paused = false;
    protected float lerp;

    protected virtual void Awake()
    {
        reversedCurve = new AnimationCurve();
        foreach (Keyframe key in curve.keys)
        {
            reversedCurve.AddKey(new Keyframe(1 - key.time, key.value));
        }
    }

    protected virtual void OnDisable()
    {
        Stop();
        onDisable.Invoke();
    }

    protected virtual void OnEnable()
    {
        onEnable.Invoke();
        if (playOnStart)
            PlayForward();
    }

    protected virtual void Start()
    {
        if (playOnStart)
            PlayForward();
    }

    public void AnimationReset()
    {
        Stop();
        AnimateValues(from, to, curve.Evaluate(0));
        onReset.Invoke();
    }

    public void AnimationSetEnd()
    {
        Stop();
        AnimateValues(from, to, curve.Evaluate(1));
        onSetEnd.Invoke();
    }

    public virtual void PlayForward()
    {
        Stop();
        if (gameObject.activeSelf)
        {
            StartCoroutine(Animation(from, to, duration, 0, curve));
            onStartForward.Invoke();
            forward = true;
        }
    }

    public virtual void PlayAt(float _point)
    {
        Stop();
        if (gameObject.activeSelf)
        {
            StartCoroutine(Animation(from, to, duration, _point, curve));
        }
    }

    public virtual void PlayBackward()
    {
        Stop();
        if (gameObject.activeSelf)
        {
            StartCoroutine(Animation(from, to, duration, 0, reversedCurve));
            onStartBackward.Invoke();
            forward = false;
        }
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    public void Pause()
    {
        paused = true;
        onPause.Invoke();
    }

    public void Resume()
    {
        paused = false;
        onResume.Invoke();
    }

    protected IEnumerator Animation(T _from, T _to, float _duration, float _startLerp, AnimationCurve _actualCurve)
    {
        lerp = _startLerp;
        AnimateValues(_from, _to, _actualCurve.Evaluate(lerp));
        yield return new WaitForSeconds(startDelay);
        switch (type)
        {
            case AnimationType.Once:
                onStart.Invoke();
                while (lerp < 1)
                {
                    lerp += DeltaTime() / _duration;
                    AnimateValues(_from, _to, _actualCurve.Evaluate(lerp));
                    yield return new WaitForEndOfFrame();
                    yield return new WaitWhile(() => paused);
                }
                onFinish.Invoke();
                if (forward)
                    onFinishForward.Invoke();
                else
                    onFinishBackward.Invoke();
                break;
            case AnimationType.Loop:
                while (this.enabled)
                {
                    lerp = 0;
                    onStart.Invoke();
                    onLoopForward.Invoke();
                    while (lerp < 1)
                    {
                        lerp += DeltaTime() / _duration;
                        AnimateValues(_from, _to, _actualCurve.Evaluate(lerp));
                        yield return new WaitForEndOfFrame();
                        yield return new WaitWhile(() => paused);
                    }
                    onFinish.Invoke();
                    if (forward)
                        onFinishForward.Invoke();
                    else
                        onFinishBackward.Invoke();
                }
                break;
            case AnimationType.PingPong:
                while (this.enabled)
                {
                    onStart.Invoke();
                    onLoopForward.Invoke();
                    while (lerp < 1)
                    {
                        lerp += DeltaTime() / _duration;
                        AnimateValues(_from, _to, _actualCurve.Evaluate(lerp));
                        yield return new WaitForEndOfFrame();
                        yield return new WaitWhile(() => paused);
                    }
                    onFinish.Invoke();
                    if (forward)
                        onFinishForward.Invoke();
                    else
                        onFinishBackward.Invoke();
                    onStart.Invoke();
                    onLoopBackward.Invoke();
                    while (lerp > 0)
                    {
                        lerp -= DeltaTime() / _duration;
                        AnimateValues(_from, _to, _actualCurve.Evaluate(lerp));
                        yield return new WaitForEndOfFrame();
                        yield return new WaitWhile(() => paused);
                    }
                    onFinish.Invoke();
                    if (forward)
                        onFinishForward.Invoke();
                    else
                        onFinishBackward.Invoke();
                }
                break;
        }
    }

    protected float DeltaTime()
    {
        float returnable = (ignoreTimeScale) ? Time.unscaledDeltaTime : Time.deltaTime;
        return returnable;
    }

    protected virtual void AnimateValues(T _from, T _to, float _lerp)
    {
        // to override
    }
}
