using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NunBehaviour : MonoBehaviour {
    public Transform pointA;
    public Transform pointB;
    public float walkSpeed = 5;
    public float alertedSpeed = 10;
    public bool canSee;
    [Header("Parameters for Sighted version")]
    public Transform eyes;
    public FieldOfViewBehaviour view;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    public float range;
    public float angle;

    private bool isRight;
    private bool isAlerted;
    private bool playerInSight;
    private Vector3 actualDestination;
    private float distanceTollerance = 0.1f;
    private Rigidbody rb;
    private Vector3 startScale;
    private Vector3 startPos;
    private new Collider collider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startScale = transform.localScale;
        startPos = transform.position;
        collider = GetComponent<Collider>();
    }

    // Use this for initialization
    void Start () {
        isAlerted = false;
        isRight = true;
        ChangeDestination(pointA.position);
        if(view != null)
        {
            view.enabled = canSee;
        }
        if(canSee)
        {
            view.viewAngle = angle;
            view.viewRadius = range;
            view.DrawFieldOfView();
        }
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
	}

    public void ResetToStart()
    {
        transform.position = startPos;
        isAlerted = false;
        //isRight = true;
        ChangeDestination(pointA.position);
    }

    private void OnEnable()
    {
        if(view != null && canSee)
        {
            view.Init();
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        float actualSpeed = (isAlerted) ? alertedSpeed : walkSpeed;
        float movement = actualSpeed * Time.fixedDeltaTime;
        Vector3 movementV3 = transform.right * ((isRight) ? 1 : -1) * movement;
        Vector3 destination = new Vector3(actualDestination.x, transform.position.y, transform.position.z);
        if (distanceTollerance < movement)
            distanceTollerance = movement;
        bool obstacleAhead = Physics.CheckBox(collider.bounds.center + (movementV3), collider.bounds.extents * 0.9f, transform.rotation, obstacleLayer, QueryTriggerInteraction.Ignore);
        if(Vector3.Distance(transform.position, destination) <= distanceTollerance || (obstacleAhead && !(canSee && playerInSight)))
        {
            Patrol();
        }
        rb.MovePosition(rb.position + movementV3);
	}

    private void LateUpdate()
    {
        if (view != null && view.enabled)
        {
            view.transform.position = eyes.transform.position;
            view.transform.rotation = eyes.transform.rotation;
            view.DrawFieldOfView();
        }

        if(canSee)
        {
            float distance = Vector3.Distance(GameController.instance.player.transform.position, transform.position);
            Vector3 playerDir = GameController.instance.player.transform.position - transform.position;
            playerInSight = (Vector3.Angle(playerDir, eyes.right) < angle / 2f) && Physics.Raycast(transform.position, playerDir, range, playerLayer, QueryTriggerInteraction.Ignore) && !Physics.Raycast(transform.position, playerDir, distance, ~playerLayer, QueryTriggerInteraction.Ignore);
            if (playerInSight)
                Alert(GameController.instance.player.transform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            GameController.instance.player.StartKill();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "SoundWave" && !canSee)
        {
            Alert(other.gameObject.transform.position);
        }
    }

    public void Alert(Vector3 newDest)
    {
        isAlerted = true;
        ChangeDestination(newDest);
        //CancelInvoke("Patrol");
        //Invoke("Patrol", 1);
    }

    public void Patrol()
    {
        if (isAlerted)
            isAlerted = false;
        ChangeDestination();
    }

    private void ChangeDestination()
    {
        Vector3 dest;
        if (actualDestination == pointA.position)
        {
            dest = pointB.position;
        }
        else
        {
            dest = pointA.position;
        }
        ChangeDestination(dest);
    }

    private void ChangeDestination(Vector3 _dest)
    {
        actualDestination = _dest;
        ManageDirection();
    }

    private void ManageDirection()
    {
        if(isRight && actualDestination.x < transform.position.x)
        {
            isRight = false;
            transform.localScale = new Vector3(-startScale.x, startScale.y, startScale.z);
            eyes.transform.Rotate(transform.forward, 180);
        }
        else if(!isRight && actualDestination.x > transform.position.x)
        {
            isRight = true;
            transform.localScale = new Vector3(startScale.x, startScale.y, startScale.z);
            eyes.transform.Rotate(transform.forward, 180);
        }
    }
}
