using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssistedDestinationBehaviour : MonoBehaviour {

    public Transform destination;
    public Transform pointLeft;
    public Transform pointRight;
    public MovingPlatformController movingPlatform;

    private Vector3 lastDestinationPos;
    private Vector3 platformDestination;

    private void Awake()
    {
        if(destination != null && movingPlatform == null)
        {
            movingPlatform = destination.parent.GetComponent<MovingPlatformController>();
        }
    }

    // Use this for initialization
    void Start () {
        lastDestinationPos = destination.position;
	}

    private void Update()
    {
        if (movingPlatform != null)
        {
            platformDestination = movingPlatform.ActualPointPosition;
        }
    }

    // Update is called once per frame
    void LateUpdate () {
		if(pointRight != null && pointLeft != null)
        {
            bool right = lastDestinationPos.x < destination.position.x;
            bool left = lastDestinationPos.x > destination.position.x;
            if (movingPlatform == null)
            {
                if (right)
                {
                    destination.position = pointRight.position;
                }
                else if (left)
                {
                    destination.position = pointLeft.position;
                }
            }
            else
            {
                float platDestDistance = Vector3.Distance(movingPlatform.transform.position, platformDestination);
                float pointABDistance = Vector3.Distance(pointLeft.position, pointRight.position);
                if (right)
                {
                    destination.position = Vector3.Lerp(pointLeft.position, pointRight.position, Mathf.Clamp01(platDestDistance / pointABDistance));
                }
                else if(left)
                {
                    destination.position = Vector3.Lerp(pointRight.position, pointLeft.position, Mathf.Clamp01(platDestDistance / pointABDistance));
                }
            }
            lastDestinationPos = destination.position;
        }
	}

    public void Assign()
    {
        GameController.instance.player.AssignAssistedDestination(destination);
    }
}
