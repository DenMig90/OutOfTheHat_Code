using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimateScaleAlt : Animate<float>
{
    public Transform transformToKeep;

    private Vector3 startPos;

    protected override void Start()
    {
        base.Start();
        startPos = transformToKeep.localPosition;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void AnimateValues(float _from, float _to, float lerp)
    {
        float one = Mathf.Lerp(_from, _to, lerp);
        float two = _to/one;
        //Debug.Log(one + " " + two);
        transform.localScale = new Vector3((transform.localScale.x > 0 ?  1 : -1 ) * one, (transform.localScale.y > 0 ? 1 : -1) * one, (transform.localScale.z > 0 ? 1 : -1) * one);
        transformToKeep.localScale = new Vector3(two, two, two);
        transformToKeep.localPosition = startPos * two;
    }
}

