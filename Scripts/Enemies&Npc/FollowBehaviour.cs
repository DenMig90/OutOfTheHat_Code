using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBehaviour : MonoBehaviour {
    public Transform target;
    public bool followX;
    public bool followY;
    public bool followZ;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (target != null)
        {
            float targetX = followX ? target.position.x : transform.position.x;
            float targetY = followY ? target.position.y : transform.position.y;
            float targetZ = followZ ? target.position.z : transform.position.z;
            transform.position = new Vector3(targetX, targetY, targetZ);
        }
        else
        {
            Debug.LogWarning("Target to follow on (" + gameObject.name + ") is empty");
        }
	}
}
