using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableBehaviour : MonoBehaviour {

    public float speed;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void Start () {
        startPosition = transform.position;
        startRotation = transform.rotation;
        //GameController.instance.AddMovable(this);
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
    }

    public void ResetToStart()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.velocity = Vector3.zero;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Move(Vector3 dir)
    {
        transform.Translate(dir * speed *Time.fixedDeltaTime);
    }
}
