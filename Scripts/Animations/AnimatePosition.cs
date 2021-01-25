using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatePosition :Animate<Vector3>
{
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void AnimateValues(Vector3 _from, Vector3 _to, float lerp)
    {
        transform.localPosition = Vector3.Lerp((Vector3)_from, (Vector3)_to, lerp);
    }
}
