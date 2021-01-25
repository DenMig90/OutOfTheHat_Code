using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraTargetBehaviour : MonoBehaviour {

    public AnimationCurve goToTargetCurve;
    public AnimationCurve backToPlayerCurve;
    public float goToTargetTime;
    public float backToPlayerTime;
    public bool ignoreXOffset = true;
    public bool ignoreYOffset = true;
    public UnityEvent OnTargetReached;
    public UnityEvent OnPlayerReached;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        /*
		if(Input.GetKeyDown(KeyCode.O))
        {
            GoToTarget();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            BackToPlayer();
        }
        */
    }

    public void GoToTarget()
    {
        if (goToTargetTime != 0)
            GameController.instance.mainCamera.TargetChangeSpeed(goToTargetTime);
        if (goToTargetCurve != null)
            GameController.instance.mainCamera.TargetChangeCurve(goToTargetCurve);
        if (OnTargetReached != null)
            GameController.instance.mainCamera.SetOnTargetReached(OnTargetReached);
        if (ignoreXOffset)
            GameController.instance.mainCamera.ChangeOffsetXImmediate(0f);
        if (ignoreYOffset)
            GameController.instance.mainCamera.ChangeOffsetYImmediate(0f);
        GameController.instance.mainCamera.StopFollowing(transform.position);
    }

    public void BackToPlayer()
    {
        if (backToPlayerTime != 0)
            GameController.instance.mainCamera.TargetChangeSpeed(backToPlayerTime);
        if (backToPlayerCurve != null)
            GameController.instance.mainCamera.TargetChangeCurve(backToPlayerCurve);
        if (OnPlayerReached != null)
            GameController.instance.mainCamera.SetOnTargetReached(OnPlayerReached);
        GameController.instance.mainCamera.Follow();
    }
}
