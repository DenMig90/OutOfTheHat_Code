using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderAtZOrigin : MonoBehaviour {

    private List<BoxCollider> colliders;

    private void Awake()
    {
        colliders = new List<BoxCollider>();
        //foreach(BoxCollider col in GetComponents<BoxCollider>())
        //{
        //    colliders.Add(col);
        //}
        foreach (BoxCollider col in GetComponentsInChildren<BoxCollider>())
        {
            colliders.Add(col);
        }
    }

    // Use this for initialization
    void Start () {
		foreach(BoxCollider col in colliders)
        {
            col.center = new Vector3(col.center.x, col.center.y, col.center.z -transform.position.z);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
