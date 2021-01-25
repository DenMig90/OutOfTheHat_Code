using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCameraViewPoint : MonoBehaviour {
    public Transform viewPoint;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TriggerViewPoint()
    {
        if (viewPoint != null)
            GameController.instance.mainCamera.StopFollowing(new Vector2(viewPoint.position.x, viewPoint.position.y));
        else
            GameController.instance.mainCamera.StopFollowing();
        GameController.instance.mainCamera.BlockPlayer(true);
    }

    public void TriggerFollowPlayer()
    {
        GameController.instance.mainCamera.Follow();
        GameController.instance.mainCamera.BlockPlayer(false);
    }
}
