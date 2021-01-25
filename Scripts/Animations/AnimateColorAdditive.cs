using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateColorAdditive : AnimateColor {

    public float modifier;

    protected override void Awake()
    {
        base.Awake();
        from = color;
        to = new Color(color.r + (modifier/255f), color.g + (modifier/255f), color.b + (modifier/255f), color.a);
    }
}
