using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimateScale : Animate<Vector3>
{
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void AnimateValues(Vector3 _from, Vector3 _to, float lerp)
    {
        transform.localScale = Vector3.Lerp(_from, _to, lerp);
    }
}

