using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedEyeBehaviour : MonoBehaviour {

    public LaserEmitterController laser;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnEyeOpen()
    {
        laser.SetIsActive(true);
    }

    public void OnEyeClose()
    {
        laser.SetIsActive(false);
    }
}
