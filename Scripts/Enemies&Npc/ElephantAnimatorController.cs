using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElephantAnimatorController : MonoBehaviour {

    public ActualPlatformBehaviour elephant;

    public void OnChangeDirStart()
    {
        elephant.OnChangeDirStart();
    }

	public void OnChangeDirEnd()
    {
        //Debug.Log("OnChangeDirEnd");
        elephant.OnChangeDirEnd();
    }

    public void OnBobbingReset()
    {
        elephant.OnBobbingReset();
    }

    public void OnBarrito()
    {
        elephant.OnBarrito();
    }

    public void OnHover()
    {
        elephant.OnHover();
    }

    public void OnStep()
    {
        elephant.OnStep();
    }
}
