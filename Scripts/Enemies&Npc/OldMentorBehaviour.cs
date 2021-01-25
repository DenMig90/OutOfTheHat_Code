using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OldMentorBehaviour : MonoBehaviour {

    public float minIdleDuration;
    public float maxIdleDuration;
    public float minAltDuration;
    public float maxAltDuration;
    public float movementSpeed;
    public float glideVelocity;
    public float ascensionMultiplier = 1;
    public Vector3 glideDirection = new Vector3(0,-1,0);
    public bool glideInput;
    public bool isWaiting;
    public bool simulated;
    public float distanceToReaction;

    public GameObject rig;
    public GameObject mesh;
    public Axis axisToMirror = Axis.Z;
    public Animator puffAnim;
    public float animPauseDuration = 1;
    //public const string puffAnimName = "Puff";

    public UnityEvent onGlideStart;
    public UnityEvent onGlideEnd;
    public UnityEvent onTransform;
    public UnityEvent onTransformationEnd;
    public UnityEvent onAngryEnd;
    public UnityEvent onCannotPass;

    [SerializeField]
    private bool right;
    private bool isAscending;
    private bool lastAscending;
    private bool isGliding;
    private bool lastGliding;
    private bool isGrounded;
    private bool startWaiting;
    private bool startRight;
    private Vector3 startPos;
    private Animator anim;
    private Rigidbody myRB;
    private new Collider collider;
    private float lastTimeGrounded;
    private Vector3 lastGroundedPosition;

    private Coroutine idleRoutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        collider = GetComponent<Collider>();
        myRB = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void Start () {
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        startWaiting = isWaiting;
        startRight = right;
        ResetToStart();
        //anim.SetBool("IsWaiting", isWaiting);
    }

    public void ResetToStart()
    {
        isGliding = false;
        lastGliding = false;
        isAscending = false;
        lastAscending = false;
        isGrounded = false;
        //right = false;
        right = startRight;
        isWaiting = startWaiting;
        anim.SetBool("IsInAir", !isGrounded);
        anim.SetBool("IsFalling", myRB.velocity.y <= 0.5);
        bool nowGliding = isAscending || isGliding;
        anim.SetBool("IsGliding", nowGliding);
        myRB.velocity = Vector3.zero;
        Show(true);
        myRB.isKinematic = false;
        anim.enabled = true;
        anim.Rebind();
    }
	
	// Update is called once per frame
	void Update () {
        ManageRigMirroring();
        if (simulated)
            return;
        if(isWaiting)
        {
            //isWaiting = false;
            anim.SetBool("ComeHere", Mathf.Abs(transform.position.x - GameController.instance.player.transform.position.x) <= distanceToReaction);
        }
	}

    private void FixedUpdate()
    {
        if (simulated)
            return;
        IsOnTheGround();
        anim.SetBool("IsInAir", !isGrounded);
        anim.SetBool("IsFalling", myRB.velocity.y <= 0.5);
        if (!isGrounded && myRB.velocity.y <= 0 && glideInput)
        {
            if (!isGliding)
            {
                myRB.useGravity = false;
                myRB.velocity = Vector3.zero;
                myRB.AddForce(glideDirection.normalized * ((glideVelocity != 0)? glideVelocity : GameController.instance.player.glideVelocity), ForceMode.VelocityChange);
                isGliding = true;
            }
        }
        else
        {
            isGliding = false;
            myRB.useGravity = true;
        }

        if(movementSpeed != 0)
        {
            Vector3 movedirection = (movementSpeed > 0) ? Vector3.right : Vector3.left;
            //Debug.Log(collider.bounds.center);
            //Debug.DrawRay(collider.bounds.center + (movedirection * collider.bounds.extents.x * 0.1f), Vector3.right * collider.bounds.extents.x * 0.9f, Color.red, Time.fixedDeltaTime);
            //Debug.DrawRay(collider.bounds.center + (movedirection * collider.bounds.extents.x * 0.1f), Vector3.left * collider.bounds.extents.x * 0.9f, Color.red, Time.fixedDeltaTime);
            //Debug.DrawRay(collider.bounds.center + (movedirection * collider.bounds.extents.x * 0.1f), Vector3.up * collider.bounds.extents.y * 0.9f, Color.red, Time.fixedDeltaTime);
            //Debug.DrawRay(collider.bounds.center + (movedirection * collider.bounds.extents.x * 0.1f), Vector3.down * collider.bounds.extents.y * 0.9f, Color.red, Time.fixedDeltaTime);
            bool collideInDirection = Physics.CheckBox(collider.bounds.center + (movedirection * collider.bounds.extents.x * 0.1f), collider.bounds.extents * 0.9f, transform.rotation, GameController.instance.player.wallLayer, QueryTriggerInteraction.Ignore);
            //Debug.Log(collideInDirection);
            if (!collideInDirection)
                myRB.position += Vector3.right * movementSpeed * Time.fixedDeltaTime;
        }
    }

    private void LateUpdate()
    {
        if (simulated)
            return;
        bool nowGliding = isAscending || isGliding;
        anim.SetBool("IsGliding", nowGliding);

        //if (lastGliding != isGliding)
        //{
        //    if (isGliding)
        //    {
        //        if (!isAscending)
        //            PlayStartGlideSound();

        //    }
        //    else
        //        PlayStopGlideSound();
        //}
        lastGliding = isGliding;

        //if (lastAscending != isAscending)
        //{
        //    if (isAscending)
        //        PlayStartAscendingSound();
        //    else
        //        PlayStopAscendingSound();
        //}
        lastAscending = isAscending;

        if (myRB.velocity.y <= 0 || !glideInput)
            isAscending = false;
    }

    public void OnGlideStart()
    {
        onGlideStart.Invoke();
    }

    public void OnGlideEnd()
    {
        onGlideEnd.Invoke();
    }

    public void CannotPass()
    {
        onCannotPass.Invoke();
    }

    public void PlayPuffAnimation()
    {
        puffAnim.SetTrigger("Puff");
    }

    public void OnTransform()
    {
        onTransform.Invoke();
    }

    public void PauseAnimator()
    {
        StartCoroutine(PauseAnimatorRoutine(animPauseDuration));
    }

    public void ChangeGlideInput(bool value)
    {
        glideInput = value;
    }

    public void SimulatedGlide(bool value)
    {
        anim.SetBool("IsGliding", value);
    }

    public void ChangeGlideDirection(Vector3 dir)
    {
        glideDirection = dir;
    }

    public void ChangeMovementSpeed(float amount)
    {
        movementSpeed = amount;
        right = movementSpeed >= 0;
    }

    public void CheckSecrets()
    {
        if (GameController.instance.AllSecretsUnlocked)
            TriggerTransformation();
        else
            TriggerAngryReaction();
    }

    public void Ascend(Vector3 forceVector)
    {
        if (glideInput)
        {
            myRB.AddForce(forceVector * ascensionMultiplier, ForceMode.Acceleration);
            isAscending = true;
        }
    }

    public void TriggerTransformation()
    {
        //isWaiting = false;
        //anim.SetBool("IsWaiting", isWaiting);
        anim.SetTrigger("Transformation");
    }

    public void TriggerAngryReaction()
    {
        //isWaiting = false;
        //anim.SetBool("IsWaiting", isWaiting);
        anim.SetTrigger("Angry");
    }

    public void TriggerDefence()
    {
        anim.SetTrigger("Defence");
    }

    public void Deactivate()
    {
        Show(false);
        myRB.isKinematic = true;
        anim.enabled = false;
    }

    public void Show(bool value)
    {
        mesh.gameObject.SetActive(value);
    }

    public void TransformationEnd()
    {
        Show(false);
        PlayPuffAnimation();
        onTransformationEnd.Invoke();
    }

    public void AngryEnd()
    {
        onAngryEnd.Invoke();
    }

    public void StartLoop(Animator anim)
    {
        StopLoop();
        idleRoutine = StartCoroutine(IdleLoop(anim));
    }

    public void StopLoop()
    {
        if (idleRoutine != null)
        {
            StopCoroutine(idleRoutine);
            idleRoutine = null;
        }
    }

    private IEnumerator IdleLoop(Animator anim)
    {
        bool alt = false;
        anim.SetBool("IdleAlt", alt);
        float random;
        while (true)
        {
            random = (alt) ? Random.Range(minAltDuration, maxAltDuration) : Random.Range(minIdleDuration, maxIdleDuration);
            yield return new WaitForSeconds(random);
            alt = !alt;
            anim.SetBool("IdleAlt", alt);
        }
    }

    public void ManageRigMirroring()
    {
        switch (axisToMirror)
        {
            case Axis.X:
                if ((right && rig.transform.localScale.x > 0) || (!right && rig.transform.localScale.x < 0))
                    rig.transform.localScale = new Vector3(rig.transform.localScale.x * -1, rig.transform.localScale.y, rig.transform.localScale.z);
                break;
            case Axis.Y:
                if ((right && rig.transform.localScale.y > 0) || (!right && rig.transform.localScale.y < 0))
                    rig.transform.localScale = new Vector3(rig.transform.localScale.x, rig.transform.localScale.y * -1, rig.transform.localScale.z);
                break;
            case Axis.Z:
                if ((right && rig.transform.localScale.z > 0) || (!right && rig.transform.localScale.z < 0))
                    rig.transform.localScale = new Vector3(rig.transform.localScale.x, rig.transform.localScale.y, rig.transform.localScale.z * -1);
                break;
        }
    }

    private void IsOnTheGround()
    {
        Vector3 bounds = collider.bounds.extents;
        bounds.x -= 0.01f;
        Collider[] hits = Physics.OverlapBox(collider.bounds.center + Vector3.down * GameController.instance.player.maxDistanceFromGround, new Vector3(bounds.x * 0.9f, bounds.y, bounds.z), transform.rotation, GameController.instance.player.groundLayer, QueryTriggerInteraction.Ignore);
        isGrounded = hits.Length > 0;
        if (isGrounded)
        {
            lastTimeGrounded = Time.timeSinceLevelLoad;
            lastGroundedPosition = transform.position;
        }
    }

    private IEnumerator PauseAnimatorRoutine(float delay)
    {
        anim.speed = 0;
        float time = 0;
        //Debug.Log(delay);
        while (time < delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        anim.speed = 1;
    }
}
