using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicFlameBehaviour : MonoBehaviour {
    public Transform anchor;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = anchor.position;
	}
}
