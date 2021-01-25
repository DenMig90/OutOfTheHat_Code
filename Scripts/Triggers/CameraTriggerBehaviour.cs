using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTriggerBehaviour : MonoBehaviour {

    public bool changeOffsetX;
    public bool changeOffsetY;
    public bool changeDistance;
    public bool changeOffsetSpeed;
    public bool changeDistanceSpeed;
    public bool changeOffsetCurve;
    public bool changeDistanceCurve;
    public bool blockVerticalDeadzone=true;
    public bool changeVerticalDeadzone;
    public bool maintainPlayerXPosition;
    public bool maintainPlayerYPosition;

    public float offsetX;
    public float offsetY;
    public float distance;
    public float offsetSpeed;
    public float distanceSpeed;
    public float verticalDeadZone;
    public AnimationCurve offsetCurve;
    public AnimationCurve distanceCurve;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Trigger()
    {
        if(changeOffsetX && changeOffsetY)
        {
            GameController.instance.mainCamera.ChangeOffset(new Vector2(offsetX, offsetY), changeOffsetCurve? offsetCurve : null);
        }
        else if(changeOffsetX)
        {
            GameController.instance.mainCamera.ChangeOffsetX(offsetX, changeOffsetCurve ? offsetCurve : null);
        }
        else if (changeOffsetY)
        {
            GameController.instance.mainCamera.ChangeOffsetY(offsetY, changeOffsetCurve ? offsetCurve : null);
        }

        if(changeDistance)
        {
            GameController.instance.mainCamera.DistanceCamera(distance, changeDistanceCurve ? distanceCurve : null);
        }

        if(changeOffsetSpeed)
        {
            GameController.instance.mainCamera.OffsetChangeSpeed(offsetSpeed);
        }

        if(changeDistanceSpeed)
        {
            GameController.instance.mainCamera.DistanceChangeSpeed(distanceSpeed);
        }

        if (changeVerticalDeadzone)
        {
            GameController.instance.mainCamera.ChangeVerticalDeadzone(verticalDeadZone);
        }

        if (blockVerticalDeadzone)
        {
            GameController.instance.mainCamera.BlockVerticalDeadzone();
        }

        if (maintainPlayerXPosition)
        {
            GameController.instance.mainCamera.MaintainPlayerXPosition();
        }

        if (maintainPlayerYPosition)
        {
            GameController.instance.mainCamera.MaintainPlayerYPosition();
        }

        //if(changeOffsetCurve)
        //{
        //    GameController.instance.mainCamera.ChangeOffsetCurve(offsetCurve);
        //}

        //if(changeDistanceCurve)
        //{
        //    GameController.instance.mainCamera.DistanceChangeCurve(distanceCurve);
        //}
    }
}
