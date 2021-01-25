using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResetEventListener : MonoBehaviour {

    public bool onDeath;
    public bool onNewGame = true;
    public UnityEvent onReset;

    //private bool startActive;
    private bool startCollider;
    private new Collider collider;
    private Vector3 startPos;
    private Vector3 startScale;
    private Quaternion startRotation;

    public void Awake()
    {
        collider = GetComponent<Collider>();
        if (collider)
            startCollider = collider.enabled;
        startPos = transform.position;
        startScale = transform.localScale;
        startRotation = transform.rotation;

    }

    // Use this for initialization
    void Start () {
        if(onDeath)
            GameController.instance.AddOnDeathDelegate(ResetToStart);
        if (onNewGame)
            GameController.instance.AddOnNewGameDelegate(ResetToStart);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ResetToStart()
    {
        if (collider)
            collider.enabled = startCollider;
        transform.position = startPos;
        transform.localScale = startScale;
        transform.rotation = startRotation;
        onReset.Invoke();
    }
}
