using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehaviour : PooledObject {

    [HideInInspector]
    public AnimateScale animScale;

    private void Awake()
    {
        animScale = GetComponent<AnimateScale>();
    }

    private void OnEnable()
    {
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
    }

    private void OnDisable()
    {
        GameController.instance.RemoveOnDeathDelegate(ResetToStart);
        GameController.instance.RemoveOnNewGameDelegate(ResetToStart);
    }

    public void ResetToStart()
    {
        ReturnToPool();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
