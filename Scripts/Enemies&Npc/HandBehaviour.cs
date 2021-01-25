using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using FMODUnity;
using FMOD.Studio;

public enum HandState
{
    Inactive,
    Active,
    Preparation,
    Attack,
    Return
}

public class HandBehaviour : MonoBehaviour {

    [SerializeField] private HandState actualState;
    [Tooltip("Only Y is considered")]
    public Transform activePos;
    public Collider activationTrigger;
    public PlayerKiller killer;
    public LayerMask collisionLayermask;
    public Animator animator;
    public float activationSpeed;
    public float deactivationSpeed;
    public float movementSpeed;
    public float attackSpeed;
    public float returnSpeed;
    public float lateralActivationDelay;
    public float preparationDelay;
    public float attackDelay;
    public float attackDelayAdditionalIfPlayerInRange;
    public float returnDelay;
    public int nrOfAttacksAfterDeactivation;
    public float attackSpeedAfterDeactivation;
    public float returnSpeedAfterDeactivation;
    public float returnDelayAfterDeactivation;
    //public float returnHeightAfterDeactivationMultiplier = 2f;
    //public float shakeDelay;
    public float intangibleDuration;
    public float playerDetectionRange;
    public bool noPlayerDetection = false;
    public int nrOfHitsBeforeDeactivating;
    public int increasedSpeedMinDistance;
    public float increasedSpeedMultiplier = 2f;
    public SpriteShapeController shadow;
    public int shadowTrackingPoints = 5;
    public LayerMask shadowProjectionMask;
    public float shadowWidth;
    public float shadowOffsetY;
    public float shadowMaxDistance=50f;
    [Range(0f,1f)]
    public float shadowSmoothness = 0.5f;
    [Range(0f, 1f)]
    public float shadowFadeRatio = 0.2f;
    [Range(0f, 1f)]
    public float minShadowScale = 0.25f;
    public GameObject[] objectsToActivate;
    public UnityEvent onHit;
    public UnityEvent onActivation;
    public UnityEvent onDeactivation;
    public bool houseHand = false;
    public ParticleSystem houseVfx = null;
    public Transform houseStartHeight = null;
    public float houseHandStopDelay = 0.5f;
    public UnityEvent onHouseHit;
    [Header("FMOD Events")]
    [EventRef] public string strikeSound;
    [EventRef] public string riserSound;
    [EventRef] public string houseCrashSound;
    public float riserMin = 0.9f;
    public float musicMaxDistance = 30f;
    public AnimationCurve musicRiseCurve;
    //[EventRef] public string multiStrikeSound;
    public bool debug;

    private Vector3 startPos;
    private Coroutine delayRoutine;
    //private Coroutine shakeRoutine;
    private new MeshRenderer renderer;
    private Rigidbody rb;
    private new Collider collider;
    private Color startColor;
    private bool canMove;
    private bool hasToInactive;
    private bool hasToActive;
    private bool canPrepare;
    private bool beforeFirstAttack=true;
    private bool noLateralAttack=true;
    private bool returned;
    private int attackAfterDeactivation;
    private int nrOfHits;
    private bool wasActive = false;
    private bool inActivation = false;
    private bool sounded;
    private float shadowPrevY = 0f;
    private float shadowScale = 0f;
    private Color startShadowColor;
    private float currentShadowAlpha = 0f;
    private bool houseVfxPlayed = false;
    private bool isAttackingAfterDeactivation = false;
    private float shadowY = 0f;
    private float distanceFromGroundParam = 0f;
    private float attackHeight;
    private float attackMovement;
    private EventInstance riser;
    private float ground;

    public HandState ActualState
    {
        set
        {
            HandState prevState = actualState;
            actualState = value;
            switch(actualState)
            {
                case HandState.Inactive:
                    if(debug) Debug.Log("Inactive");
                    //transform.position = startPos;
                    renderer.material.color = startColor;
                    if (delayRoutine != null)
                    {
                        StopCoroutine(delayRoutine);
                        delayRoutine = null;
                    }
                    hasToInactive = false;
                    StopCoroutine("PrepareDelayed");
                    killer.gameObject.SetActive(false);
                    beforeFirstAttack = true;
                    animator.SetTrigger("Idle");
                    if (wasActive)
                        onDeactivation.Invoke();
                    else
                    {
                        //shadow.gameObject.SetActive(false);
                        foreach (GameObject go in objectsToActivate)
                        {
                            go.SetActive(false);
                        }
                    }
                    break;
                case HandState.Active:
                    if(debug) Debug.Log("Active");
                    wasActive = true;
                    inActivation = true;
                    if (delayRoutine != null)
                    {
                        StopCoroutine(delayRoutine);
                        delayRoutine = null;
                    }
                    canPrepare = false;
                    StartCoroutine("PrepareDelayed");
                    //delayRoutine = StartCoroutine(ChangeStateDelayed(preparationDelay, HandState.Preparation));
                    killer.gameObject.SetActive(false);
                    nrOfHits = 0;
                    if (!isAttackingAfterDeactivation)
                        animator.SetTrigger("Idle");

                    //shadow.gameObject.SetActive(true);
                    foreach (GameObject go in objectsToActivate)
                    {
                        go.SetActive(true);
                    }
                    //onActivation.Invoke();
                    break;
                case HandState.Preparation:
                    if(debug) Debug.Log("Preparation");
                    if (delayRoutine != null)
                    {
                        StopCoroutine(delayRoutine);
                        delayRoutine = null;
                    }
                    delayRoutine = StartCoroutine(ChangeStateDelayed(attackDelay, attackDelay + attackDelayAdditionalIfPlayerInRange, HandState.Attack));
                    //renderer.material.color = Color.red;
                    killer.gameObject.SetActive(false);
                    animator.SetTrigger("Preparation");
                    break;
                case HandState.Attack:
                    if(debug) Debug.Log("Attack canMove");
                    canMove = true;
                    sounded = false;
                    killer.gameObject.SetActive(true);
                    beforeFirstAttack = false;
                    nrOfHits++;
                    if (isAttackingAfterDeactivation)
                    {
                        attackAfterDeactivation--;

                        //if(debug) Debug.Log("I'm attcking AttackAfter=" + attackAfterDeactivation);
                    }
                    if (nrOfHitsBeforeDeactivating > 0 && nrOfHits >= nrOfHitsBeforeDeactivating)
                    {
                        //if(debug) Debug.Log("Deactivating");
                        Deactivate();
                    }
                    RaycastHit hit;
                    if (Physics.BoxCast(collider.bounds.center, collider.bounds.extents, -transform.up, out hit, Quaternion.identity, float.MaxValue, collisionLayermask, QueryTriggerInteraction.Ignore))
                    {
                        attackHeight = hit.distance;
                        attackMovement = 0f;
                    }

                    riser = GameController.instance.audioManager.PlayGenericSound(riserSound, GameController.instance.player.gameObject, "GroundDistance", riserMin);
                    animator.SetTrigger("Attack");
                    break;
                case HandState.Return:
                    if(debug) Debug.Log("Return");
                    //renderer.material.color = startColor;
                    killer.gameObject.SetActive(false);
                    //if(!isAttackingAfterDeactivation)
                        animator.SetTrigger("Raise");
                    break;
            }
        }
        get { return actualState; }
    }

    public bool Returned { get { return returned; } }

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        startColor = renderer.material.color;
        startPos = transform.position;
        startShadowColor = shadow.spriteShapeRenderer.material.color;
        ActualState = HandState.Inactive;
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        GameController.instance.AddHand(this);
    }

    private void ResetToStart()
    {
        
        wasActive = false;
        transform.position = startPos;
        ActualState = HandState.Inactive;
        hasToActive = false;
        hasToInactive = false;
        houseVfxPlayed = false;
        if (houseVfx != null)
        {
            houseVfx.Stop();
            houseVfx.Clear();
        }
        if (riser.isValid())
            GameController.instance.audioManager.DestroyInstance(riser);
    }

    // Use this for initialization
    void Start ()
    {
        if (shadow != null)
        {
            shadow.spline.Clear();
            for (int i = 0; i < (shadowTrackingPoints * 2) - 2; i++)
            {
                shadow.spline.InsertPointAt(i, Vector3.zero + Vector3.right * i);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {

        bool atTheEdge = false;
        switch (actualState)
        {
            case HandState.Inactive:
                if (Mathf.Abs(transform.position.y - startPos.y) > 0)
                {
                    shadowScale = (startPos.y - transform.position.y) / (startPos.y - activePos.position.y);
                    Vector3 translation = new Vector3(transform.position.x, startPos.y, transform.position.z);
                    transform.position = Vector3.MoveTowards(transform.position, translation, deactivationSpeed * Time.deltaTime);
                }
                else
                {
                    shadowScale = 0f;
                    //shadow.gameObject.SetActive(false);
                    foreach (GameObject go in objectsToActivate)
                    {
                        go.SetActive(false);
                    }
                }
                if (hasToActive && GameController.instance.AllHandsReturned())
                {
                    ActualState = HandState.Active;
                    attackAfterDeactivation = nrOfAttacksAfterDeactivation;
                    isAttackingAfterDeactivation = false;
                    StartCoroutine(lateralActivationTimer());
                    shadowY = GameController.instance.player.transform.position.y;
                }
                break;
            case HandState.Active:
                if (hasToInactive)
                {
                    //if(debug) Debug.Log("AttackAfter="+attackAfterDeactivation);
                    if (attackAfterDeactivation > 0)
                    {
                        if (delayRoutine != null)
                        {
                            StopCoroutine(delayRoutine);
                            delayRoutine = null;
                        }
                        StopCoroutine("PrepareDelayed");
                        ActualState = HandState.Attack;
                        //if(debug) Debug.Log("Attack");
                        isAttackingAfterDeactivation = true;
                    }
                    else
                    {
                        ActualState = HandState.Inactive;
                        isAttackingAfterDeactivation = false;
                    }
                }
                if (Mathf.Abs(transform.position.y - activePos.position.y) > 0)
                {
                    shadowScale = (startPos.y - transform.position.y) / (startPos.y - activePos.position.y);
                    Vector3 translation = new Vector3(transform.position.x, activePos.position.y, transform.position.z);
                    transform.position = Vector3.MoveTowards(transform.position, translation, activationSpeed * Time.deltaTime);
                    break;
                }
                else
                {
                    shadowScale = 1f;
                    if(inActivation)
                    {
                        onActivation.Invoke();
                        inActivation = false;
                    }
                }
                if (movementSpeed != 0)
                {
                    float speed = movementSpeed;
                    if (increasedSpeedMinDistance != 0 && Mathf.Abs(transform.position.x - GameController.instance.player.Rigidbody.position.x) > increasedSpeedMinDistance)
                        speed = movementSpeed*increasedSpeedMultiplier;

                    Vector3 translation = new Vector3(GameController.instance.player.Rigidbody.position.x, transform.position.y, transform.position.z);
                    Vector3 movement = (translation - transform.position).normalized * speed * Time.deltaTime;
                    if (movement.magnitude > Vector3.Distance(translation, transform.position)) movement = movement.normalized * Vector3.Distance(translation, transform.position);
                    if(Mathf.Abs(activationTrigger.transform.position.x-(transform.position.x+movement.x))+collider.bounds.extents.x>activationTrigger.bounds.extents.x)
                    {
                        movement = movement.normalized * Mathf.Abs(activationTrigger.bounds.extents.x - (Mathf.Abs(activationTrigger.transform.position.x - transform.position.x) + collider.bounds.extents.x));
                        //if(debug) Debug.Log(activationTrigger.bounds.extents.x +"-"+ Vector3.Distance(activationTrigger.transform.position, transform.position) +"+"+ collider.bounds.extents.x);
                        atTheEdge = true;
                    }
                    
                    transform.position += movement;
                    //transform.position = Vector3.MoveTowards(transform.position, translation, movementSpeed * Time.deltaTime);
                }
                if(canPrepare)
                {
                    RaycastHit hit;
                    Vector3 dir = GameController.instance.Down;
                    float distance = Vector3.Distance(transform.position, GameController.instance.player.transform.position);
                    bool seen;
                    if (!noPlayerDetection)
                        seen = Physics.SphereCast(transform.position, playerDetectionRange, dir, out hit, distance, 1 << LayerMask.NameToLayer("Player"), QueryTriggerInteraction.Ignore);
                    else
                        seen = true;

                    if(atTheEdge && (!beforeFirstAttack || !noLateralAttack))
                    {
                        if(Mathf.Abs(GameController.instance.player.Rigidbody.position.x - activationTrigger.transform.position.x) - (activationTrigger.bounds.extents.x - collider.bounds.extents.x) > 0 &&
                           Mathf.Abs(GameController.instance.player.Rigidbody.position.x - activationTrigger.transform.position.x) - (activationTrigger.bounds.extents.x) < 0)
                        {
                            seen = true;
                        }
                    }

                    //Debug.DrawRay(transform.position, dir * distance, Color.red);
                    //if(debug) Debug.Log(seen);
                    if (seen)
                    {
                        ActualState = HandState.Preparation;
                    }
                }
                //SetMusicParam(false);
                break;
            case HandState.Preparation:
                shadowScale = 1f;
                //SetMusicParam(false);
                break;
            case HandState.Attack:
                shadowScale = 1f;
                if (canMove)
                {
                    if(debug) Debug.Log("Attack"+ collider.bounds.extents);
                    float speed = attackSpeed;
                    if (isAttackingAfterDeactivation && attackSpeedAfterDeactivation != 0)
                        speed = attackSpeedAfterDeactivation;
                    transform.Translate(GameController.instance.Down * speed * Time.deltaTime);
                    attackMovement += speed * Time.deltaTime;
                    if(riser.isValid())
                    {
                        riser.setParameterByName("GroundDistance", ((Mathf.Clamp01(attackMovement / attackHeight)*(1-riserMin))+riserMin));
                    }

                    Vector3 centerWithMovement = collider.bounds.center;
                    centerWithMovement.y += (speed * Time.deltaTime)/2;
                    Vector3 boundsAndMovement = collider.bounds.extents;
                    boundsAndMovement.y += speed * Time.deltaTime;
                    Collider[] colls = Physics.OverlapBox(centerWithMovement, boundsAndMovement, transform.rotation, collisionLayermask, QueryTriggerInteraction.Ignore);
                    int soilColls = 0;
                    int bramblesColls = 0;
                    int waterColls = 0;
                    foreach (Collider col in colls)
                    {
                        if(debug)Debug.Log("col name " + col.name);
                        ManageCollision(col.gameObject);
                        switch (col.tag)
                        {
                            case "Soil":
                                soilColls++;
                                break;
                            case "Brambles":
                                bramblesColls++;
                                break;
                            case "Water":
                                waterColls++;
                                break;
                        }
                    }
                    ground = 0;
                    if (waterColls >= bramblesColls && waterColls >= soilColls)
                        ground = 2;
                    else if (bramblesColls > soilColls)
                        ground = 1;
                }
                if(houseHand && !houseVfxPlayed)
                {
                    if(transform.position.y<houseStartHeight.position.y)
                    {
                        //if(debug) Debug.Log("PlayVFX");

                        houseVfx.gameObject.SetActive(true);
                        houseVfx.Play();
                        onHouseHit.Invoke();
                        houseVfxPlayed = true;
                        PlayHouseCrashSound();
                    }
                }
                //SetMusicParam(false);
                break;
            case HandState.Return:
                shadowScale = 1f;
                float returnY = activePos.position.y;
                if (isAttackingAfterDeactivation || hasToInactive) returnY = Mathf.Lerp(activePos.position.y, startPos.y, 0.5f);
                if(debug) Debug.Log("ReturnY=" + returnY + "posY=" + transform.position.y);
                if (Mathf.Abs(transform.position.y - returnY) > 0)
                {
                    float speed = returnSpeed;
                    if ((isAttackingAfterDeactivation || hasToInactive) && returnSpeedAfterDeactivation!=0)
                        speed = returnSpeedAfterDeactivation;
                    Vector3 translation = new Vector3(transform.position.x, returnY, transform.position.z);
                    transform.position = Vector3.MoveTowards(transform.position, translation, speed * Time.deltaTime);
                }
                else
                {

                    //if(debug) Debug.Log("hastoinactive="+hasToInactive+"attackafter="+attackAfterDeactivation);
                    if (hasToInactive && !(attackAfterDeactivation > 0))
                    {
                        ActualState = HandState.Inactive;
                        //isAttackingAfterDeactivation = false;
                        //if(debug) Debug.Log("Return Inactive");
                    }
                    else
                    {
                        if (hasToInactive)
                        {
                            isAttackingAfterDeactivation = true;
                        }

                        ActualState = HandState.Active;
                    }
                }
                //houseVfxPlayed = false;
                //SetMusicParam(false);
                break;
        }
    }

    private void LateUpdate()
    {
        returned = Mathf.Abs(transform.position.y - startPos.y) == 0;

        ManageShadow();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ActualState == HandState.Attack)
        {
            ManageCollision(collision.gameObject);
        }
    }

    private void ManageCollision(GameObject collision)
    {
        if(debug) Debug.Log("Manage Collision tag=" + collision.tag + " Name="+collision.name);
        if (collision.tag != "Teleport" && collision.tag != "Player")
        {
            if (houseVfx != null)
            {
                Invoke("StopHouseVfx", houseHandStopDelay);
            }
            if(debug) Debug.Log(canMove = false);
            if(riser.isValid())
                GameController.instance.audioManager.DestroyInstanceFaded(riser);
            canMove = false;
            onHit.Invoke();
            if (collision.tag != "Player")
            {
                if (delayRoutine != null)
                {
                    StopCoroutine(delayRoutine);
                    delayRoutine = null;
                }
                animator.SetTrigger("HitGround");
                if(!sounded)
                {
                    sounded = true;
                    PlayStrikeSound(ground);
                }
                delayRoutine = StartCoroutine(ChangeStateDelayed((isAttackingAfterDeactivation && returnSpeedAfterDeactivation > 0) ? returnDelayAfterDeactivation : returnDelay, HandState.Return));
            }
            if(collision.GetComponent<BouncingAnimation>())
            {
                //if(debug) Debug.Log("Performing bounce");
                collision.GetComponent<BouncingAnimation>().PerformBounce(0f, true);
            }
        }
    }

    private void StopHouseVfx()
    {
        houseVfx.Stop();
        //houseVfx.gameObject.SetActive(false);
    }

    public void Activate()
    {
        hasToActive = true;
        hasToInactive = false;
    }

    public void Deactivate()
    {
        hasToActive = false;
        hasToInactive = true;
    }

    //public void BeginShake()
    //{
    //    if(shakeRoutine != null)
    //    {
    //        StopCoroutine(shakeRoutine);
    //    }
    //    shakeRoutine = StartCoroutine(Shake());
    //}

    //public void StopShake()
    //{
    //    if (shakeRoutine != null)
    //    {
    //        StopCoroutine(shakeRoutine);
    //    }
    //}

    public void StartShake()
    {
        StartCoroutine(Intangible());
    }

    private IEnumerator ChangeStateDelayed(float delay, HandState newState)
    {
        float time = 0;
        while(time <= delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        delayRoutine = null;
        ActualState = newState;
    }

    private IEnumerator ChangeStateDelayed(float delay, float delayIfPlayerInRange, HandState newState)
    {
        float time = 0;

        float myDelay = delay;
        if(GameController.instance.player.transform.position.x + GameController.instance.player.GetComponent<CapsuleCollider>().radius > collider.bounds.center.x - collider.bounds.extents.x &&
           GameController.instance.player.transform.position.x - GameController.instance.player.GetComponent<CapsuleCollider>().radius < collider.bounds.center.x + collider.bounds.extents.x)
        {
            myDelay = delayIfPlayerInRange;
        }

        while (time <= myDelay)
        {
            yield return new WaitForEndOfFrame();

            if (GameController.instance.player.transform.position.x + GameController.instance.player.GetComponent<CapsuleCollider>().radius > collider.bounds.center.x - collider.bounds.extents.x &&
               GameController.instance.player.transform.position.x - GameController.instance.player.GetComponent<CapsuleCollider>().radius < collider.bounds.center.x + collider.bounds.extents.x)
            {
                myDelay = delayIfPlayerInRange;
            }
            else
            {
                myDelay = delay;
            }
            time += Time.deltaTime;
        }
        delayRoutine = null;
        ActualState = newState;
    }

    private IEnumerator PrepareDelayed()
    {
        float time = 0;
        while (time <= preparationDelay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        canPrepare = true;
    }

    //private IEnumerator Shake()
    //{
    //    float time = 0;
    //    while (time <= shakeDelay)
    //    {
    //        yield return new WaitForEndOfFrame();
    //        time += Time.deltaTime;
    //    }
    //    StartCoroutine(Intangible());
    //    shakeRoutine = null;
    //}

    private IEnumerator Intangible()
    {
        SetIntangible(true);
        float time = 0;
        while (time <= intangibleDuration)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        SetIntangible(false);
    }

    public void SetIntangible(bool value)
    {
        collider.enabled = !value;
        if(value)
        {
            renderer.material.color = Color.green;
        }
        else
        {
            renderer.material.color = startColor;
        }
    }

    IEnumerator lateralActivationTimer()
    {
        noLateralAttack = true;
        yield return new WaitForSeconds(lateralActivationDelay);
        noLateralAttack = false;
    }

    void ManageShadow()
    {
        if (shadow == null)
            return;

        if (shadowScale < minShadowScale)
        {
            if (currentShadowAlpha != 0f)
            {
                shadow.spriteShapeRenderer.material.color = new Color(shadow.spriteShapeRenderer.material.color.r,
                                                                      shadow.spriteShapeRenderer.material.color.g,
                                                                      shadow.spriteShapeRenderer.material.color.b,
                                                                      0f);
                currentShadowAlpha = 0f;
            }
            return;
        }

        if(shadowScale<shadowFadeRatio)
        {
            float myAlpha = startShadowColor.a * (shadowScale / shadowFadeRatio);
            if (currentShadowAlpha != myAlpha)
            {
                shadow.spriteShapeRenderer.material.color = new Color(shadow.spriteShapeRenderer.material.color.r,
                                                                      shadow.spriteShapeRenderer.material.color.g,
                                                                      shadow.spriteShapeRenderer.material.color.b,
                                                                      myAlpha);
                currentShadowAlpha = myAlpha;
            }
        }
        else
        {
            if (currentShadowAlpha != startShadowColor.a)
            {
                shadow.spriteShapeRenderer.material.color = startShadowColor;
                currentShadowAlpha = startShadowColor.a;
            }
        }

        shadow.transform.position = new Vector3(transform.position.x, shadowY/*GameController.instance.player.transform.position.y*/, shadow.transform.position.z);

        Vector3 positionOffset = new Vector3(shadow.transform.position.x, shadow.transform.position.y, 0f);

        float lastKnownY = 0f;
        int notSetAtTheBeginning = 0;
        for(int i=0; i<shadowTrackingPoints; i++)
        {
            float xPos = collider.bounds.min.x + i * (collider.bounds.size.x / (shadowTrackingPoints-1));
            xPos = Mathf.Lerp(collider.bounds.center.x, xPos, shadowScale > 0.25f ? shadowScale: 0.25f);
            RaycastHit hit;
            if(Physics.Raycast(new Vector3(xPos,transform.position.y,0f),-transform.up,out hit,shadowMaxDistance,shadowProjectionMask))
            {
                //if(debug) Debug.Log(i + " - " + hit.point);
                SetShadowPoint(i, hit.point - positionOffset);

                lastKnownY = (hit.point - positionOffset).y;
                if (notSetAtTheBeginning>0)
                {
                    for(int j=0; j<notSetAtTheBeginning;j++)
                    {
                        float xPos2 = collider.bounds.min.x + j * (collider.bounds.size.x / (shadowTrackingPoints-1));
                        SetShadowPoint(j, new Vector3(xPos2 - positionOffset.x, (hit.point - positionOffset).y, 0f));
                    }
                    notSetAtTheBeginning = 0;
                }
            }
            else
            {
                if(i==0 || notSetAtTheBeginning>0)
                {
                    notSetAtTheBeginning++;
                }
                else
                {
                    SetShadowPoint(i, new Vector3(xPos - positionOffset.x, lastKnownY, 0f));
                }
            }
        }

        for (int i = 0; i < (shadowTrackingPoints * 2) - 2; i++)
        {
            int prevI = i - 1;
            if (prevI < 0) prevI = (shadowTrackingPoints * 2) - 3;
            int nextI = i + 1;
            if (nextI >= (shadowTrackingPoints * 2) - 2) nextI = 0;

            Vector3 direction1 = shadow.spline.GetPosition(i) - shadow.spline.GetPosition(prevI);
            Vector3 direction2 = shadow.spline.GetPosition(nextI) - shadow.spline.GetPosition(i);
            shadow.spline.SetRightTangent(i, Vector3.Lerp(direction1, direction2, 0.5f) * 0.5f);

            direction1 = shadow.spline.GetPosition(prevI) - shadow.spline.GetPosition(i);
            direction2 = shadow.spline.GetPosition(i) - shadow.spline.GetPosition(nextI);
            shadow.spline.SetLeftTangent(i, Vector3.Lerp(direction1, direction2, 0.5f) * 0.5f);

            shadow.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }
        

       /* if(!shadow.spriteShapeRenderer.isVisible)
        {

            // Set the bounds of the mesh to be a 1x1x1 cube (actually doesn't matter what the size is)
            shadow.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0f);
            for (int i = 0; i < (shadowTrackingPoints * 2) - 2; i++)
            {
                shadow.spline.SetPosition(i, Vector3.zero + Vector3.right * i);
            }
        }*/
        /*
        // boundsTarget is the center of the camera's frustum, in world coordinates:
        Vector3 camPosition = Camera.main.transform.position;
        Vector3 normCamForward = Vector3.Normalize(Camera.main.transform.forward);
        float boundsDistance = (Camera.main.farClipPlane - Camera.main.nearClipPlane) / 2 + Camera.main.nearClipPlane;
        Vector3 boundsTarget = camPosition + (normCamForward * boundsDistance);

        // The game object's transform will be applied to the mesh's bounds for frustum culling checking.
        // We need to "undo" this transform by making the boundsTarget relative to the game object's transform:
        Vector3 realtiveBoundsTarget = shadow.transform.InverseTransformPoint(boundsTarget);

        // Set the bounds of the mesh to be a 1x1x1 cube (actually doesn't matter what the size is)
        Mesh mesh = GetComponent().mesh;
        mesh.bounds = new Bounds(realtiveBoundsTarget, Vector3.one);

        shadow.spriteShapeRenderer.bounds =new Bounds(realtiveBoundsTarget, Vector3.one);
        */
    }
    
    void SetShadowPoint(int index, Vector3 position)
    {
        position.y += shadowOffsetY;
        if (index!=0)
        {
            position.y = Mathf.Lerp(position.y, shadowPrevY, shadowSmoothness);
        }
        shadowPrevY = position.y;
        if (index == 0 || index == shadowTrackingPoints - 1)
        {
            shadow.spline.SetPosition(index, position);
        }
        else
        {
            float indexShifed = index - (shadowTrackingPoints / 2);
            float myH = (shadowWidth / 2) * Mathf.Sqrt(1 - ((indexShifed * indexShifed) / Mathf.Pow((shadowTrackingPoints - 1)/2, 2)));
            shadow.spline.SetPosition(index, position + transform.up * myH);

            shadow.spline.SetPosition(((shadowTrackingPoints * 2) - 2) - index, position - transform.up * myH);
        }
    }

    public void PlayStrikeSound()
    {
        PlayStrikeSound(0);
    }

    public void PlayStrikeSound(float ground)
    {
        GameController.instance.audioManager.PlayGenericSound(strikeSound, gameObject, "HandStrike", ground);
    }

    public void PlayHouseCrashSound()
    {
        GameController.instance.audioManager.PlayGenericSound(houseCrashSound, gameObject);
    }

    /*
    public void SetMusicParam(bool setOne)
    {
        SoundParameters[] sParams = new SoundParameters[1];
        SoundParameters sParam = new SoundParameters("HandDistance", setOne? 1f : 1f - musicRiseCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(transform.position.x - GameController.instance.player.transform.position.x) / musicMaxDistance)));
        sParams[0] = sParam;
        GameController.instance.audioManager.ChangeMusicParameters(sParams);
    }
    */
}
