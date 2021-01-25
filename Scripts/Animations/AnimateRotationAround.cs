using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimateRotationAround : Animate<float>
{
    public Transform point;

    private Vector3 pointPosition;
    private float lastLerp=0f;

    protected override void Start()
    {
        base.Start();
        pointPosition = point.position;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void AnimateValues(float _from, float _to, float lerp)
    {
        float lerpIncrement=lerp-lastLerp;
        transform.RotateAround(pointPosition, Vector3.forward, lerpIncrement*(to-from));
        lastLerp = lerp;
        //transform.localEulerAngles = Vector3.Lerp(_from, _to, lerp);
    }

    public override void PlayForward()
    {
        base.PlayForward();
        lastLerp = 0f;
    }

    public override void PlayAt(float point)
    {
        base.PlayAt(point);
        lastLerp = 0f;
    }

    public override void PlayBackward()
    {
        base.PlayBackward();
        lastLerp = 0f;
    }
}
