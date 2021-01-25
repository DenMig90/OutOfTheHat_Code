using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundWaveBehaviour : PooledObject {

    public float maxRadius;
    public float time;

    private AnimateColor animColor;
    private AnimateScale animScale;

    private void Awake()
    {
        animColor = GetComponent<AnimateColor>();
        animScale = GetComponent<AnimateScale>();

        animColor.duration = time;
        animScale.duration = time;
        animScale.from = new Vector3(0, 0, 0);
        animScale.to = new Vector3(maxRadius * 2, maxRadius * 2, maxRadius * 2);
        animScale.onFinish.AddListener(ReturnToPool);
    }

    private void OnEnable()
    {
        animScale.PlayForward();
        animColor.PlayForward();
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
    }

    private void OnDisable()
    {
        GameController.instance.AddOnDeathDelegate(ResetToStart);
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
