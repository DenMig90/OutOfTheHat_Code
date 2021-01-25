using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(TransportBehaviour))]
public class ActualPlatformBehaviour : MonoBehaviour {

    public MovingPlatformController target
    {
        set
        {
            _target = value;
            ManageDirection();
        }
        get
        {
            return _target;
        }
    }
    private MovingPlatformController _target;
    public float additionalSpeed=1f;
    public float maxAcceleration;
    public bool die = false;
    public float dieTime = 10f;
    [Range(0f,1f)]
    public float smoothness = 0.5f;

    public GameObject spriteContainer;
    public Animator bodyAnimator;
    public Animator frontlegsAnimator;
    public Animator backlegsAnimator;

    public float walkingSpeedTreshold=0.5f;
    public float walkingCarrotMaxAngle = 45f;
    public float hoveringSpeedTreshold = 0.5f;
    public float hoveringCarrotDistanceTreshold = 0.2f;
    public float changeAnimationSpeedMultiplier = 3f;
    public float stillCarrotChangeDirectionMinDistance = 0.5f;

    public float legsLenght = 37f;

    public float minBarritoTime = 3f;
    public float maxBarritoTime = 7f;

    public float cameraOffsetMultiplier = 0.5f;
    public float cameraOffsetMax = 10f;
    public float savingEndDistance = 3f;

    [Header("Walk Bobbing")]
    public float bobbingTime = 1f;
    public float bobbingAmount = 0.2f;
    public AnimationCurve bobbingCurve;

    [Header("Hover Bobbing")]
    public float hoverBobbingTime = 1f;
    public float hoverBobbingAmount = 0.2f;
    public AnimationCurve hoverBobbingCurve;

    [Header("Idle Bobbing")]
    public float idleBobbingTime = 1f;
    public float idleBobbingAmount = 0.2f;
    public AnimationCurve idleBobbingCurve;

    public float bobbingAttackReleaseTime = 1f;
    public GameObject platformSoundEmitterTransform;
    [Header("FMOD")]
    [EventRef] public string stepSound;
    [EventRef] public string barritoSound;
    [EventRef] public string hoverSound;
    [EventRef] public string turnLeftSound;
    [EventRef] public string turnRightSound;
    [EventRef] public string eyeDropSound;

    public bool isActive = true;

    public Material deadElephantMaterial;
    public float deadColorChangeTime;

    public float activationDistance = 100f;

    public bool debug = false;

    private float maxSpeed;
    private Vector3 currentMovement = Vector3.zero;
    private Collider col;
    private float startFixedDeltaTime;
    //private float bobbingStartTime;
    private float bobbingTimeCounter=0f;
    private bool isWalking = true;
    private bool isHovering = true;
    private bool isWalkingBobbing = true;
    private bool isHoveringBobbing = true;
    private bool right = true;
    private bool isChangingDirection = false;
    private float changingDirectionTime;
    private Vector3 lastCarrotPosition;
    private float lastBobbingValue;
    private float endPrevBobbingValue;
    private float legsStartY;
    private float lastBarritoTime;
    private float barritoTime;
    [HideInInspector]
    public Vector3 startPosition;
    private MovingPlatformController lastTarget = null;
    private SpriteRenderer bodySprite;
    private SpriteRenderer frontLegsSprite;
    private SpriteRenderer backLegsSprite;
    private SortingGroup bodySortingGroup;
    private bool isShaking = false;
    private bool isSavingPlayer = false;
    private Material startMaterial;

    // Use this for initialization
    void Awake () {
        
        col = GetComponent<Collider>();
        startFixedDeltaTime = Time.fixedDeltaTime;
        startPosition = transform.position;
    }

    private void Start()
    {
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        lastCarrotPosition = target.transform.position;
        legsStartY = frontlegsAnimator.transform.localPosition.y;
        isWalking = false;
        isHovering = false;
        isChangingDirection = false;
        isShaking = false;
        bodySprite = bodyAnimator.GetComponent<SpriteRenderer>();
        frontLegsSprite = frontlegsAnimator.GetComponent<SpriteRenderer>();
        backLegsSprite = backlegsAnimator.GetComponent<SpriteRenderer>();
        bodySortingGroup = bodyAnimator.GetComponent<SortingGroup>();
        startMaterial = bodySprite.sharedMaterial;
    }

    private void ResetToStart()
    {
        if(target && !die)
        {
            //transform.position = target.transform.position;
            transform.position = startPosition;
            ManageDirection();
            //die = false;
        }
        else
        {
            Remove();
        }

        currentMovement = Vector3.zero;
        isWalking = false;
        isHovering = false;
        isChangingDirection = false;
        isShaking = false;
    }

    // Update is called once per frame
    void FixedUpdate () {

        if (Vector3.Distance(transform.position, GameController.instance.player.transform.position) > activationDistance)
            return;

        if (platformSoundEmitterTransform != null)
        {
            Vector3 closestPoint = col.bounds.ClosestPoint(GameController.instance.player.transform.position);
            platformSoundEmitterTransform.transform.position = new Vector3(closestPoint.x, closestPoint.y, 0);
        }

        if (!isActive)
        {
            bodyAnimator.SetBool("isWalking", false);
            frontlegsAnimator.SetBool("isWalking", false);
            backlegsAnimator.SetBool("isWalking", false);

            bodyAnimator.SetBool("isHovering", false);
            frontlegsAnimator.SetBool("isHovering", false);
            backlegsAnimator.SetBool("isHovering", false);

            return;
        }

        if (!target && !die)
            return;

        maxSpeed = target.speed + additionalSpeed;
        if(die)
        {
            col.enabled = false;
            bodySprite.sortingOrder = -1;
            frontLegsSprite.sortingOrder = -1;
            backLegsSprite.sortingOrder = -1;
            if(bodySortingGroup)
                bodySortingGroup.sortingOrder = -1;
            spriteContainer.transform.localPosition = new Vector3(0, 0, 1000);
        }
        else
        {

            bodySprite.sortingOrder = 0;
            frontLegsSprite.sortingOrder = 0;
            backLegsSprite.sortingOrder = 0;
            if(bodySortingGroup)
                bodySortingGroup.sortingOrder = 0;
            spriteContainer.transform.localPosition = new Vector3(0, 0, 0);
        }

        if(isSavingPlayer && target)
        {
            float distanceYFromTarget = target.transform.position.y - transform.position.y;
            if(distanceYFromTarget > savingEndDistance)
                GameController.instance.mainCamera.offsetAdditive = new Vector2(0f, Mathf.Clamp((distanceYFromTarget - savingEndDistance) * cameraOffsetMultiplier, 0f, cameraOffsetMax));
            else
            {
                GameController.instance.mainCamera.offsetAdditive = Vector2.zero;
                isSavingPlayer = false;
            }
        }

        Vector3 newMovement = new Vector3();
        if (!die)
            newMovement = (target.transform.position - (transform.position-transform.up *lastBobbingValue))*(1-smoothness);
        else
            newMovement = Vector3.down * maxSpeed;

        //Debug.Log("speed=" + newMovement.magnitude);
        if(newMovement.magnitude> maxSpeed * startFixedDeltaTime)
        {
            newMovement = newMovement.normalized * maxSpeed * startFixedDeltaTime;
        }

        //Debug.Log("max speed speed=" + newMovement.magnitude);

        if ((newMovement-currentMovement).magnitude> maxAcceleration * startFixedDeltaTime * startFixedDeltaTime)
        {
            newMovement = currentMovement + ((newMovement - currentMovement).normalized * maxAcceleration * startFixedDeltaTime * startFixedDeltaTime); // Non è un errore, è che sto lavorando con le accelerazioni
        }


        //Debug.Log("max accel speed=" + newMovement.magnitude);

        currentMovement = newMovement;

        transform.position += currentMovement * (Time.fixedDeltaTime/startFixedDeltaTime);
        float bobbingLerpValue;
        if (bobbingTimeCounter < bobbingAttackReleaseTime)
            bobbingLerpValue = bobbingTimeCounter / bobbingAttackReleaseTime;
        //else if (Time.fixedTime - bobbingStartTime < 2f * bobbingAttackReleaseTime)
        //    bobbingMultiplier = (((Time.fixedTime - bobbingAttackReleaseTime) - bobbingStartTime) / bobbingAttackReleaseTime);
        else
            bobbingLerpValue = 1f;

        /*if (!isChangingDirection)
        {*/
        float myBobbingValue = 0f;

        if (isWalkingBobbing)
        {
            myBobbingValue = bobbingAmount * bobbingCurve.Evaluate(Mathf.Repeat(bobbingTimeCounter / bobbingTime, 1f));
        }
        if (isHoveringBobbing)
        {
            myBobbingValue = hoverBobbingAmount * hoverBobbingCurve.Evaluate(Mathf.Repeat(bobbingTimeCounter / hoverBobbingTime, 1f));
        }
        if (!isWalkingBobbing && !isHoveringBobbing)
        {
            myBobbingValue = idleBobbingAmount * idleBobbingCurve.Evaluate(Mathf.Repeat(bobbingTimeCounter / idleBobbingTime, 1f));
        }

        myBobbingValue = Mathf.Lerp(endPrevBobbingValue, myBobbingValue, bobbingLerpValue);
        transform.position += transform.up * (myBobbingValue - lastBobbingValue);

        lastBobbingValue = myBobbingValue;
        /*}
       else
        {
            float myBobbingValue = Mathf.Lerp(endPrevBobbingValue, 0, bobbingLerpValue);

            transform.position += transform.up * (myBobbingValue - lastBobbingValue);

            lastBobbingValue = myBobbingValue;
        }*/

        if(debug)
        {
            //Debug.Log(Mathf.Abs(currentMovement.normalized.y) + ">" + Mathf.Abs(Mathf.Sin(walkingCarrotMaxAngle * Mathf.Deg2Rad)));
        }

        if ((currentMovement.magnitude / startFixedDeltaTime <= walkingSpeedTreshold && lastCarrotPosition == target.transform.position) ||
            (currentMovement.magnitude / startFixedDeltaTime > walkingSpeedTreshold && Mathf.Abs(currentMovement.normalized.y) > Mathf.Abs(Mathf.Sin(walkingCarrotMaxAngle * Mathf.Deg2Rad))) ||
            (lastCarrotPosition.x == target.transform.position.x && lastCarrotPosition.y != target.transform.position.y))
        {
            if (isWalking)
            {
                if (debug) Debug.Log(1);
                if (!isChangingDirection)
                {
                    bodyAnimator.speed = changeAnimationSpeedMultiplier;
                    frontlegsAnimator.speed = changeAnimationSpeedMultiplier;
                    backlegsAnimator.speed = changeAnimationSpeedMultiplier;
                }
                bodyAnimator.SetBool("isWalking", false);
                frontlegsAnimator.SetBool("isWalking", false);
                backlegsAnimator.SetBool("isWalking", false);
                isWalking = false;
            }


            // TODO Riattivare quando ci sarà l'animazione del sollevamento
            /*RaycastHit hit;
            if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + legsStartY, 0f), Vector3.down, out hit, legsLenght, GameController.instance.player.groundLayer, QueryTriggerInteraction.Ignore))
            {
                if (isHovering)
                {
                    bodyAnimator.speed = changeAnimationSpeedMultiplier;
                    frontlegsAnimator.speed = changeAnimationSpeedMultiplier;
                    backlegsAnimator.speed = changeAnimationSpeedMultiplier;
                    bodyAnimator.SetBool("isHovering", false);
                    frontlegsAnimator.SetBool("isHovering", false);
                    backlegsAnimator.SetBool("isHovering", false);
                    isHovering = false;
                }

                frontlegsAnimator.transform.localPosition = new Vector3(frontlegsAnimator.transform.localPosition.x, legsStartY + (legsLenght - hit.distance), frontlegsAnimator.transform.localPosition.z);
                backlegsAnimator.transform.localPosition = new Vector3(backlegsAnimator.transform.localPosition.x, legsStartY + (legsLenght - hit.distance), backlegsAnimator.transform.localPosition.z);
            }
            else
            {
                frontlegsAnimator.transform.localPosition = new Vector3(frontlegsAnimator.transform.localPosition.x, legsStartY, frontlegsAnimator.transform.localPosition.z);
                backlegsAnimator.transform.localPosition = new Vector3(backlegsAnimator.transform.localPosition.x, legsStartY, backlegsAnimator.transform.localPosition.z);
                */

            if (debug) Debug.Log(currentMovement.y / startFixedDeltaTime);
            if (!isHovering && currentMovement.y / startFixedDeltaTime > hoveringSpeedTreshold && !die)
            {
                if (debug) Debug.Log(2);
                if (!isChangingDirection && !isShaking)
                {
                    bodyAnimator.speed = changeAnimationSpeedMultiplier;
                    frontlegsAnimator.speed = changeAnimationSpeedMultiplier;
                    backlegsAnimator.speed = changeAnimationSpeedMultiplier;
                }
                bodyAnimator.SetBool("isHovering", true);
                frontlegsAnimator.SetBool("isHovering", true);
                backlegsAnimator.SetBool("isHovering", true);
                isHovering = true;
            }
            else if ((isHovering && currentMovement.y / startFixedDeltaTime <= hoveringSpeedTreshold) || (die && isHovering))
            {

                if (debug) Debug.Log(3);
                if (!isChangingDirection && !isShaking)
                {
                    bodyAnimator.speed = changeAnimationSpeedMultiplier;
                    frontlegsAnimator.speed = changeAnimationSpeedMultiplier;
                    backlegsAnimator.speed = changeAnimationSpeedMultiplier;
                }
                bodyAnimator.SetBool("isHovering", false);
                frontlegsAnimator.SetBool("isHovering", false);
                backlegsAnimator.SetBool("isHovering", false);
                isHovering = false;
            }
            // }
        }
        else
        {

            if (lastCarrotPosition != target.transform.position || Mathf.Abs(target.transform.position.x - transform.position.x) > stillCarrotChangeDirectionMinDistance)
            {
                if (!isWalking)
                {
                    if (debug) Debug.Log(4);
                    if (!isChangingDirection && !isShaking)
                    {
                        bodyAnimator.speed = changeAnimationSpeedMultiplier;
                        frontlegsAnimator.speed = changeAnimationSpeedMultiplier;
                        backlegsAnimator.speed = changeAnimationSpeedMultiplier;
                    }
                    bodyAnimator.SetBool("isWalking", true);
                    frontlegsAnimator.SetBool("isWalking", true);
                    backlegsAnimator.SetBool("isWalking", true);

                    bodyAnimator.SetBool("isHovering", false);
                    frontlegsAnimator.SetBool("isHovering", false);
                    backlegsAnimator.SetBool("isHovering", false);

                    isHovering = false;

                    isWalking = true;
                }
            }
        }

        if (!isChangingDirection)
        {
            ManageDirection();
        }
        else
        {
            if(die && (isWalking || isHovering))
            {
                if (debug) Debug.Log(5);
                bodyAnimator.speed = changeAnimationSpeedMultiplier;
                frontlegsAnimator.speed = changeAnimationSpeedMultiplier;
                backlegsAnimator.speed = changeAnimationSpeedMultiplier;
                bodyAnimator.SetBool("isHovering", false);
                frontlegsAnimator.SetBool("isHovering", false);
                backlegsAnimator.SetBool("isHovering", false);
                isHovering = false;
                bodyAnimator.SetBool("isWalking", false);
                frontlegsAnimator.SetBool("isWalking", false);
                backlegsAnimator.SetBool("isWalking", false);
                isWalking = false;
            }
        }

        
        if(isWalking)
        {
            if(Time.fixedTime>lastBarritoTime+barritoTime)
            {
                bodyAnimator.SetBool("Barrito", true);
                lastBarritoTime = Time.fixedTime;
                barritoTime = Random.Range(minBarritoTime, maxBarritoTime);
            }
        }

        if(target!=null)
            lastCarrotPosition = target.transform.position;

        bobbingTimeCounter += Time.fixedDeltaTime;// * bodyAnimator.speed;

        if(lastTarget!=target)
        {
            ManageDirection();
        }

        lastTarget = target;
        
    }

    private void ManageDirection()
    {
        if (target == null)
            return;

        if (lastCarrotPosition != target.transform.position || Mathf.Abs(target.transform.position.x - transform.position.x) > stillCarrotChangeDirectionMinDistance)
        {
            if (right && target.transform.position.x < transform.position.x)
            {
                isChangingDirection = true;
                //isWalking = false;
                bodyAnimator.SetTrigger("ChangeDir");
                if (isWalking)
                {
                    frontlegsAnimator.SetTrigger("ChangeDir");
                    backlegsAnimator.SetTrigger("ChangeDir");
                }
                changingDirectionTime = Time.fixedTime;
                right = false;

                /*bodyAnimator.SetBool("Right", right);
                frontlegsAnimator.SetBool("Right", right);
                backlegsAnimator.SetBool("Right", right);*/
            }
            if (!right && target.transform.position.x > transform.position.x)
            {
                isChangingDirection = true;
                //isWalking = false;
                bodyAnimator.SetTrigger("ChangeDir");
                if (isWalking)
                {
                    frontlegsAnimator.SetTrigger("ChangeDir");
                    backlegsAnimator.SetTrigger("ChangeDir");
                }
                changingDirectionTime = Time.fixedTime;
                right = true;

                /*bodyAnimator.SetBool("Right", right);
                frontlegsAnimator.SetBool("Right", right);
                backlegsAnimator.SetBool("Right", right);*/
            }
        }
    }

    public void OnBarrito()
    {
        bodyAnimator.SetBool("Barrito", false);
        PlayBarritoSound();
    }

    public void OnHover()
    {
        PlayHoverSound();
    }

    public void OnStep()
    {
        PlayStepSound();
    }

    public void OnChangeDirStart()
    {
        if (right)
        {
            PlayTurnRightSound();
        }
        else
        {
            PlayTurnLeftSound();
        }
    }

    public void OnChangeDirEnd()
    {

        //Debug.Break();

        

        isChangingDirection = false;
        //isWalking = true;
        //bobbingStartTime = Time.fixedTime;

        //yield return new WaitForEndOfFrame();
        /* yield return new WaitForEndOfFrame();
         yield return new WaitForEndOfFrame();
         yield return new WaitForEndOfFrame();
         yield return new WaitForEndOfFrame();*/

        if (right)
        {
            spriteContainer.transform.localScale = new Vector3(Mathf.Abs(spriteContainer.transform.localScale.x), spriteContainer.transform.localScale.y, spriteContainer.transform.localScale.z);
        }
        else
        {
            spriteContainer.transform.localScale = new Vector3(-Mathf.Abs(spriteContainer.transform.localScale.x), spriteContainer.transform.localScale.y, spriteContainer.transform.localScale.z);
        }
            

    }

    public void OnBobbingReset()
    {
        if (isWalking != isWalkingBobbing || isHovering != isHoveringBobbing || isShaking)
        {
            //if (debug) Debug.Break();
            bobbingTimeCounter = 0f;
            frontlegsAnimator.SetTrigger("AlignAfterShake");
            backlegsAnimator.SetTrigger("AlignAfterShake");
        }
        isShaking = false;
        //bobbingStartTime = Time.fixedTime;
        bodyAnimator.speed = 1;
        frontlegsAnimator.speed = 1;
        backlegsAnimator.speed = 1;

        endPrevBobbingValue = lastBobbingValue;
        isWalkingBobbing = isWalking;
        isHoveringBobbing = isHovering;

        PlayEyedropSound();
    }

    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log("Entro");
        if (!target || die)
            return;

        if (((col.tag == "Player" && target.activableByPlayer) || (col.tag == "Teleport" && target.activableByHat)))
        {
            target.Activate();
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (!target || die)
            return;

        if (((col.tag == "Player" && target.activableByPlayer) || (col.tag == "Teleport" && target.activableByHat)) && target.hasToReturn && target.IsActive && !target.startActive)
        {
            target.Return();
        }
    }

    public void Die()
    {
        die = true;
        Invoke("Remove", dieTime);
        /*bodySprite.material = deadElephantMaterial;
        frontLegsSprite.material = deadElephantMaterial;
        backLegsSprite.material = deadElephantMaterial;*/
        StartCoroutine(DeathChangeColor());
    }

    private IEnumerator DeathChangeColor()
    {
        float startTime = Time.time;

        while(Time.time - startTime < deadColorChangeTime)
        {
            bodySprite.material.Lerp(startMaterial, deadElephantMaterial, (Time.time - startTime) / deadColorChangeTime);
            frontLegsSprite.material.Lerp(startMaterial, deadElephantMaterial, (Time.time - startTime) / deadColorChangeTime);
            backLegsSprite.material.Lerp(startMaterial, deadElephantMaterial, (Time.time - startTime) / deadColorChangeTime);
            yield return new WaitForEndOfFrame();
        }
    }

    public void Remove()
    {
        //Destroy(gameObject);
        gameObject.SetActive(false);
        CancelInvoke();
    }

    public void OnEnable()
    {
        die = false;
        col.enabled = true;
        bobbingTimeCounter = 0f;
        //bobbingStartTime = Time.fixedTime;
        isWalking = false;
        isHovering = false;
        isChangingDirection = false;
        isShaking = false;
        transform.position = startPosition;
        currentMovement = Vector3.zero;
        if (startMaterial)
        {
            bodySprite.material = startMaterial;
            frontLegsSprite.material = startMaterial;
            backLegsSprite.material = startMaterial;
        }

        if (target)
        {
            target.EnableAllActualPlatforms(true);
        }

        ManageDirection();
    }

    public void OnDisable()
    {
        if (this.enabled==false && gameObject.activeSelf == true && target)
        {
            target.EnableAllActualPlatforms(false);
        }
        /*
        bodyAnimator.ResetTrigger("ChangeDir");
        bodyAnimator.SetBool("isWalking", false);
        bodyAnimator.SetBool("isHovering", false);
        bodyAnimator.SetBool("Barrito", false);
        bodyAnimator.ResetTrigger("Shake");

        frontlegsAnimator.ResetTrigger("ChangeDir");
        frontlegsAnimator.SetBool("isWalking", false);
        frontlegsAnimator.SetBool("isHovering", false);
        frontlegsAnimator.ResetTrigger("AlignAfterShake");

        backlegsAnimator.ResetTrigger("ChangeDir");
        backlegsAnimator.SetBool("isWalking", false);
        backlegsAnimator.SetBool("isHovering", false);
        backlegsAnimator.ResetTrigger("AlignAfterShake");
        */
        bodyAnimator.Rebind();
        frontlegsAnimator.Rebind();
        backlegsAnimator.Rebind();

    }

    public void Shake()
    {
        bodyAnimator.SetTrigger("Shake");
        isShaking = true;
    }

    public void SetIsActive(bool value)
    {
        isActive = value;
        if(target)
        {
            target.SetIsActiveAllActualPlatforms(value);
        }
    }

    public void SetIsActiveNotRecursive(bool value)
    {
        isActive = value;
    }

    public void PlayStepSound()
    {
        GameController.instance.audioManager.PlayGenericSound(stepSound, platformSoundEmitterTransform);
    }

    public void PlayBarritoSound()
    {
        GameController.instance.audioManager.PlayGenericSound(barritoSound, platformSoundEmitterTransform);
    }

    public void PlayHoverSound()
    {
        GameController.instance.audioManager.PlayGenericSound(hoverSound, platformSoundEmitterTransform);
    }

    public void PlayEyedropSound()
    {
        GameController.instance.audioManager.PlayGenericSound(eyeDropSound, platformSoundEmitterTransform);
    }

    public void PlayTurnLeftSound()
    {
        GameController.instance.audioManager.PlayGenericSound(turnLeftSound, platformSoundEmitterTransform);
    }

    public void PlayTurnRightSound()
    {
        GameController.instance.audioManager.PlayGenericSound(turnRightSound, platformSoundEmitterTransform);
    }

    public void OnPlayerSaving()
    {
        isSavingPlayer = true;
    }

    public void CancelPlayerSaving()
    {
        isSavingPlayer = false;
        GameController.instance.mainCamera.offsetAdditive = Vector2.zero;
    }
}
