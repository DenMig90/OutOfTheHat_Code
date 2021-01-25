using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatePositionSingleAxis :Animate<float>
{
    public Axis axis;
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void AnimateValues(float _from, float _to, float lerp)
    {
        switch(axis)
        {
            case Axis.X:
                transform.localPosition = new Vector3(Mathf.Lerp(_from, _to, lerp), transform.localPosition.y, transform.localPosition.z);
                break;
            case Axis.Y:
                transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(_from, _to, lerp), transform.localPosition.z);
                break;
            case Axis.Z:
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, Mathf.Lerp(_from, _to, lerp));
                break;
        }
    }
}
