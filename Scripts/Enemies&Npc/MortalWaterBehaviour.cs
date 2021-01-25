using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortalWaterBehaviour : MonoBehaviour {

    public PlayerKiller killer;
    public AnimatePosition anim;

	// Use this for initialization
	void Start () {
        anim.from = transform.localPosition;
        anim.to = transform.localPosition + Vector3.up * killer.duration * killer.upSpeed;
        anim.duration = killer.duration;
        anim.curve = new AnimationCurve(new Keyframe(0, 0,1,1), new Keyframe(1, 1,1,1));
        killer.onReset.AddListener(delegate { anim.AnimationReset(); });
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
