using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LeafBehaviour : MonoBehaviour {

    public float fallingSpeed = 1;
    public float horizontalMovementScale = 1;
    public float randomnessScale = 1;
    public float rotationScale = 1;

    public float distanceToFollow;
    public float distanceToRotate = 2;

    public Transform target;

    public UnityEvent onFallen;
    public UnityEvent onNotFallen;

    public bool FollowTarget
    {
        set
        {
            followTarget = value;
        }
    }

    public bool RotateToTarget
    {
        set
        {
            rotateToTarget = value;
            if(rotateToTarget)
            {
                followPosition = transform.position;
                followRotation = transform.rotation;
            }
        }
    }

    public bool Fallen
    {
        set
        {
            fallen = value;
            if(fallen)
            {
                onFallen.Invoke();
            }
            else
            {
                onNotFallen.Invoke();
            }
        }
    }

    private float _timer = 0f;
    private bool followTarget;
    private bool rotateToTarget;
    private bool fallen;
    private Vector3 startPos;
    private Vector3 followPosition;
    private Quaternion followRotation;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
    }

    void OnEnable()
    {
        ResetToStart();   
    }

    public void ResetToStart()
    {
        followTarget = false;
        rotateToTarget = false;
        Fallen = false;
        transform.position = startPos;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        if (!followTarget)
        {
            float x = horizontalMovementScale * Mathf.PerlinNoise(Time.time * randomnessScale, 0.0F);

            pos.x = startPos.x + x;
            pos.y -= fallingSpeed * Time.deltaTime;

        }
        else if(!fallen)
        {
            pos = Vector3.MoveTowards(pos, target.position, fallingSpeed * Time.deltaTime);
        }
        if(!rotateToTarget)
        {
            float rotationX = Mathf.Lerp(0, 360, rotationScale * Mathf.PerlinNoise(Time.time * randomnessScale, 0.0F));
            float rotationY = Mathf.Lerp(0, 360, rotationScale * Mathf.PerlinNoise(Time.time * randomnessScale, 0.0F));
            float rotationZ = Mathf.Lerp(0, 360, rotationScale * Mathf.PerlinNoise(Time.time * randomnessScale, 0.0F));
            //Debug.Log(new Vector3(rotationX, rotationY, rotationZ));
            transform.eulerAngles = new Vector3(rotationX, rotationY, rotationZ);
        }
        else if(!fallen)
        {
            float actualDistance = Vector3.Distance(pos, target.position);
            float startDistance = Vector3.Distance(followPosition, target.position);
            //Debug.Log(actualDistance / startDistance);
            transform.rotation = Quaternion.Lerp(followRotation, target.rotation, 1f - (actualDistance / startDistance));
        }
        if (!fallen)
        {
            transform.position = pos;
            if (target != null && Mathf.Abs(transform.position.y - target.position.y) <= distanceToFollow && !followTarget)
            {
                FollowTarget = true;
            }
            if(target != null && Mathf.Abs(transform.position.y - target.position.y) <= distanceToRotate && !rotateToTarget)
            {
                RotateToTarget = true;
            }
            if (followTarget && transform.position == target.position)
                Fallen = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

    }
}
