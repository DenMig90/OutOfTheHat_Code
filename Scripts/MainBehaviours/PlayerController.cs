using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;

public enum Axis
{
    X,
    Y,
    Z
}

public enum GroundType
{
    Grass,
    Rock,
    Soil,
    Wood,
    Mush,
    Mud,
    Snow,
    None
}

public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    public float runSpeed = 10f;
    public float walkSpeed = 5f;
    //public float maxDistanceFromWallJoWallJump = 0.2f;
    //public float wallMagneteForce=2f;
    //public float wallGravityContrast=0.7f;
    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public LayerMask noisyGroundLayer;
    public float jumpForce = 2f;
    public float respawnJumpForce;
    public float respawnDelay = 1f;
    public float abyssLimit = -100F;
    public float clingMaxDistance = 0.1f;
    public float climbAdvancement = 0.2f;
    public bool automaticClimb = true;
    public SoundWaveBehaviour soundWavePrefab;
    public float maxSlope;
    public float maxJumpSlope;
    public float accelerationTime = 0f;
    public float accelerationTimeTurn = 0f;
    public AnimationCurve accelerationCurve;
    public float decelerationTime = 0f;
    public float decelerationTimeTurn = 0f;
    public AnimationCurve decelerationCurve;
    [Range(0f, 1f)]
    public float airControl = 1f;
    public float airControlReturnTime = 1f;
    public AnimationCurve airControlReturnCurve;
    public float jumpCancelTime = 0.1f;
    public AnimationCurve jumpCancelCurve;
    public float jumpCancelFadeTime = 0.1f;
    public AnimationCurve jumpCancelFadeCurve;
    //[Header("Input")]
    //public float jumpFromHatForce = 2f;
    [Header("Hat & Teleport")]
    public int maxTeleportBeforeLand = 3;
    public float teleportBeforeLandResetTime = 2f;
    //public int maxJumpBeforeLand = 1;
    public GameObject hat;
    public HatBehaviour teleportHat;
    public Transform teleportShootPoint;
    public LayerMask trajectoryLayer;
    public float teleportVelocity;
    public float teleportDelay = 0.5f;
    public float delayBetweenHatShoots = 0.5f;
    public float distanceToReturn = 10f;
    public float tolleranceForInAirTeleport = 0.1f;
    public bool shootCooldownAfterLaunch = true;
    public bool shootCooldownAfterTeleport = false;
    public float hatShakingPeriod = 0.5f;
    public float hatShakingAmount = 0.2f;
    public AnimationCurve hatShakingCurve;
    //public bool hasToStopTimeAtFirstTeleportTutorial = true;
    //public float firstTeleportTutorialDelay = 0.4f;
    [Header("Drag")]
    public LayerMask dragLayer;
    public float dragMaxDistance = 0.2f;
    [Header("Glide")]
    public float glideVelocity = 3f;
    //public float waitBeforeCanJumpFromHat = 1f;
    public float glideDelay = 0.5f;
    public float glideMinDistanceToObjects = 1f;
    [Header("BullHat Time")]
    public float bullHatTimeAttackDuration;
    public float bullHatTimeReleaseDuration;
    public AnimationCurve bullHatTimeAttackCurve;
    public AnimationCurve bullHatTimeReleaseCurve;
    public float jumpWhileAimingMinTimeScale = 0.5f;
    [Header("Chatcher")]
    public AnimationCurve timerCurve;
    public SpriteRenderer hatLightRenderer;
    public ExplosionBehaviour explosionPrefab;
    [Header("Bohat")]
    public float bohatJumpForce = 20f;
    public float waterMaxDistance = 10f;
    public LayerMask waterLayer;
    [Header("Hatarang")]
    public float hatarangSpeed = 5f;
    public float hatarangReturnDelay = 5f;
    public Transform hatarangShootPoint;
    //public bool controller = true;
    [Header("Noob Rage Avoider")]
    public float notGroundedJumpTollerance = 0.2f;
    public float maxDistanceFromGround = 0.1F;
    [Range(0, 1)]
    public float playerHatCameraRange = 0.2f;
    //public bool assistedMode = false;
    public Transform assistedDestination;
    public float assistedTollerance = 0.5f;
    public float freezeAfterLaunchDuration = 0;
    public AnimationCurve freezeVelocityCurve;
    //public int checkGroundedPrecision = 5;
    [Header("Unlockable")]
    public bool shootingEnabled = false;
    public bool clingEnabled = true;
    public bool glideUnlocked;
    public bool bohatUnlocked;
    public bool chatcherUnlocked;
    public bool hidehatUnlocked;
    public bool hatarangUnlocked;
    [Header("References & General")]
    public GameObject tempBohat;
    public Transform cameraTarget;
    public Transform plantingPosition;
    public Transform clingingPosition;
    public GameObject rig;
    public Axis axisToMirror = Axis.Z;
    public Renderer mesh;
    public Animator deathVFXAnim;
    //public Animator magicVFXAnim;
    public ParticleSystem landingVFXParticle;
    public ParticleSystem slidingVFXParticle;
    public SpriteRenderer hitPointLight;
    public Color canHitColor = new Color(1, 1, 1, 1);
    public Color cannotHitColor = new Color(1, 0, 0, 1);
    [Range(0, 100)]
    public int alternativeIdleProb = 20;
    public float stoppedTimeToEnableAltIdle = 5;
    public float maxFallingVelocity = 10;
    public bool hasToWakeUp;
    public bool changeIdle;
    public bool startIsWalking = true;
    public float walkToRunDelay = 1;
    private bool wakeUpAtStart;
    public Gradient normalAimColor;
    public Gradient assistedAimColor;
    public Gradient hatarangAimColor;
    public const string wakeUpAnimName = "WakeUp3";
    public ParticleSystem vfxShadowSmoke;
    public int hatAnimScaleGlideIndex;
    public int hatAnimPosGlideIndex;
    public int hatAnimScaleHideIndex;
    public int hatAnimPosHideIndex;
    public int hatAnimRotHideIndex;
    public int hatAnimScaleBohatIndex;
    public int hatAnimPosBohatIndex;
    public int hatAnimRotBohatIndex;
    public UnityEvent onWakeUp;
    public UnityEvent onTeleport;
    public UnityEvent onMovement;
    public UnityEvent onJump;
    public UnityEvent onAim;
    public UnityEvent onHatThrow;
    public UnityEvent onGlideStart;
    public UnityEvent onGlideEnd;
    public UnityEvent onHideStart;
    public UnityEvent onHideEnd;
    public UnityEvent onBohatStart;
    public UnityEvent onBohatEnd;
    public UnityEvent onCancel;
    public UnityEvent onDeath;
    public UnityEvent onDelayedDeath;
    public UnityEvent onInteraction;
    [Header("FMOD Events")]
    [EventRef] public string wakeUpSound;
    [EventRef] public string wakeUpMusic;
    [EventRef] public string stepRightSound;
    [EventRef] public string stepLeftSound;
    [EventRef] public string idleSound;
    [EventRef] public string idleAltSound;
    [EventRef] public string idleJellSound;
    [EventRef] public string jumpSound;
    [EventRef] public string landingSound;
    [EventRef] public string aimSound;
    [EventRef] public string hatThrowSound;
    [EventRef] public string backflipSound;
    [EventRef] public string teleportSound;
    [EventRef] public string deathSound;
    [EventRef] public string explosionSound;
    [EventRef] public string respawnSound;
    [EventRef] public string startGlideSound;
    [EventRef] public string stopGlideSound;
    [EventRef] public string startAscensionSound;
    [EventRef] public string stopAscensionSound;
    [EventRef] public string hatFlapSound;
    [EventRef] public string scarySound;
    [EventRef] public string takeHat;
    [EventRef] public string slidingSound;
    [EventRef] public string stopSlidingSound;

    public bool IsDead
    {
        get
        {
            return isDead;
        }
    }

    public Collider Collider
    {
        get
        {
            return collider;
        }
    }

    public Rigidbody Rigidbody { get { return myRB; } }

    public bool IsInWind
    {
        get
        {
            return isInWind;
        }
        set
        {
            isInWind = value;
        }
    }

    public bool IsHidden
    {
        set
        {
            isHidden = value;
            //isMovementBlocked = isHidden;
            //isThrowBlocked = isHidden;
            anim.speed = isHidden ? 0 : 1;
            if (isHidden)
            {
                OnHideStart();
            }
            else
            {
                OnHideEnd();
            }
        }
        get { return isHidden; }
    }

    public bool IsBoat
    {
        get { return isBoat; }
        set
        {
            isBoat = value;
            //isMovementBlocked = value;
            //isThrowBlocked = value;
            if (isBoat)
            {
                OnBohatStart();
            }
            else
            {
                OnBohatEnd();
            }
        }
    }

    public bool IsGrounded
    {
        get { return isGrounded; }
    }

    public bool IsBlocked
    {
        get { return isMovementBlocked; }
    }

    public bool IsCaptured { get { return isCaptured; } }

    public Vector3 LastGroundedPosition
    {
        get { return lastGroundedPosition; }
    }

    public bool AlternateIdleDisabled
    {
        get { return alternateIdleDisabled; }
        set { alternateIdleDisabled = value; }
    }

    public bool HasStarted
    {
        get { return started; }
    }

    public bool IsWalking
    {
        set
        {
            isWalking = value;
            //anim.SetFloat("WalkMod", isWalking ? 1 : 0);
            //speed = isWalking ? walkSpeed : runSpeed;
            isThrowBlocked = isWalking;
            isTeleportBlocked = isWalking;
        }
    }

    private bool IsClinging
    {
        set
        {
            isClinging = value;
            gravity = !value;
            myRB.isKinematic = value;
            isMovementBlocked = value;
            isThrowBlocked = value;
            isTeleportBlocked = value;
            isRetrieveBlocked = value;
            isAimBlocked = value;
        }
        get { return isClinging; }
    }

    [HideInInspector]
    public GameObject lastGroundObject = null;

    //[HideInInspector]
    //public bool canShoot = false;
    [Header("Debug")]
    private float speed;
    private float _walkToRunDelay;
    private float aboveWallMinHeight;
    private Bounds myBounds;
    private new Collider collider;
    private Renderer hatRenderer;
    private Rigidbody myRB;
    //private Rigidbody teleportRB;
    private Vector3 teleportVelocityV3;
    private Vector3 previewVelocityV3;
    //private Vector3 previewVelocityNotNormalized;
    private LineRenderer line;
    private Vector3 lastGroundedPosition;
    private Vector3 velocityBeforeFreeze;
    private Animator anim;
    private int actualTeleportBeforeLand;
    //private int actualJumpBeforeLand;
    [SerializeField]
    private bool right = true;
    private bool prevRight;
    private bool isGliding;
    private bool lastGliding;
    private bool isDragging;
    private bool isGrounded;
    private bool isClinging;
    private bool isOnWater;
    private bool gravity;
    private bool canTeleport;
    private bool canClimb;
    private bool isInBullHatTime;
    private bool alreadyJumped;
    private bool isAiming;
    private bool aimCancelled;
    private bool launchButtonReleased;
    private bool shootingBlocked;
    //[SerializeField]
    private bool canShoot;
    [SerializeField]
    private bool isMovementBlocked;
    [SerializeField]
    private bool isThrowBlocked;
    private bool isAimBlocked;
    private bool isTeleportBlocked;
    private bool isRetrieveBlocked;
    private bool isJumpBlocked;
    private bool isAimingHatarang;
    [SerializeField]
    private bool teleportTutorialEnabled;
    //private bool teleportTutorialStopTime;
    //private bool isJumpBlocked;
    //private bool isTeleportBlocked;
    private bool isCaptured;
    //[SerializeField]
    private bool isAscending;
    //[SerializeField]
    private bool lastAscending;
    private bool isInWind;
    private bool isDead;
    private bool isHidden;
    private bool isBoat;
    private bool hatCanEnable;
    private bool isWalking;
    private bool godMode;
    private bool alternateIdleDisabled;
    //private bool canJumpFromHat;
    private float prevYVelocity;
    private float wakeUpAnimDuration = 1;
    private float lastFallingSpeed;
    private float draggingSpeed;
    private float lastThrowTime=0;
    private MovableBehaviour movable;
    //private Vector3 velocityBeforeHatTime;
    private Coroutine waitJumpFromHatRoutine;
    private Coroutine waitBeforeGlideRoutine;
    private Coroutine waitBeforeTeleportRoutine;
    private Coroutine bullHatTimeRoutine;
    private Coroutine shootsDelayRoutine;
    private Coroutine killDelayedRoutine;
    private Coroutine explosionRoutine;
    private Coroutine freezeAfterLaunchRoutine;
    private Coroutine startSlidingRoutine;
    private float lastTimeGrounded;
    private Vector3 rigStartScale;
    //private bool previousGlidingAnim;
    private float startFixedDeltaTime;
    private bool canReachAssistedDestination;
    private AnimateScale hatAnimScaleGlide;
    private AnimatePosition hatAnimPosGlide;
    private AnimateScale hatAnimScaleHide;
    private AnimatePosition hatAnimPosHide;
    private AnimateRotation hatAnimRotHide;
    private AnimateScale hatAnimScaleBohat;
    private AnimatePosition hatAnimPosBohat;
    private AnimateRotation hatAnimRotBohat;

    /* Start data */
    private Vector3 startPosition;
    private Vector3 startScale;
    //public Vector3 respawnJumpDirection;
    //public Vector3 savedLastGroundedPosition;
    private bool startGlideUnlocked;
    private bool startHatUnlocked;
    //private bool alternateIdleTriggered;
    private bool avoidSliding;
    private Vector3 groundNormal;
    private Vector3 groundCollisionPoint;
    [SerializeField]
    private bool started;

    private bool movingLeft = false;
    private bool movingRight = false;
    private bool isAccelerating = false;
    private bool isDecelerating = false;
    private float accelerationStartTime = 0f;
    private float decelerationStartTime = 0f;
    private float accelerationStartSpeed = 0f;
    private float decelerationStartSpeed = 0f;
    private bool accelerationDecelerationBlend = false;
    private float currentSpeed = 0f;
    private bool cancelInAirMomentum = false;
    private Vector3 lastPosition;
    private float lastMovedTime;

    private float startingJumpSpeed = 0f;
    private bool startingJumpDirectionRight = false;
    private float lastJumpTime = 0f;
    private bool isCancelingJump = false;
    private float cancelingJumpStartTime = 0f;
    private float cancelingJumpTarget = 0f;

    private bool stopShakingHat = false;
    private Coroutine hatShakingCorountine = null;
    private bool shadowThrow = false;
    private bool forcedLaunch = false;
    private TransportBehaviour lastTrasport = null;
    private float hatX;
    private EventInstance jellIdleSoundInstance;
    private EventInstance idleSoundInstance;

    private bool vfxShadowSmokeIsPlaying = false;
    private bool vfxSlidingIsPlaying = false;

    private bool teleportFrame = false;
    //private float decelerationActualTime;
    private bool wasGroundedLastFrame = true;

    // Use this for initialization
    void Awake()
    {

        myRB = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        myBounds = collider.bounds;
        anim = GetComponent<Animator>();
        hatRenderer = hat.GetComponentInChildren<MeshRenderer>();

        line = GetComponent<LineRenderer>();

        AnimateScale[] hatScaleAnims = hat.GetComponents<AnimateScale>();

        if (hatAnimScaleGlideIndex >= hatScaleAnims.Length)
        {
            Debug.LogError("Index of Glide Scale Animation is out of range");
        }
        else
        {
            hatAnimScaleGlide = hatScaleAnims[hatAnimScaleGlideIndex];
        }
        if (hatAnimScaleHideIndex >= hatScaleAnims.Length)
        {
            Debug.LogError("Index of Hide Scale Animation is out of range");
        }
        else
        {
            hatAnimScaleHide = hatScaleAnims[hatAnimScaleHideIndex];
        }
        if (hatAnimScaleBohatIndex >= hatScaleAnims.Length)
        {
            Debug.LogError("Index of Bohat Scale Animation is out of range");
        }
        else
        {
            hatAnimScaleBohat = hatScaleAnims[hatAnimScaleBohatIndex];
        }

        AnimatePosition[] hatPosAnims = hat.GetComponents<AnimatePosition>();

        if (hatAnimPosGlideIndex >= hatPosAnims.Length)
        {
            Debug.LogError("Index of Glide Position Animation is out of range");
        }
        else
        {
            hatAnimPosGlide = hatPosAnims[hatAnimPosGlideIndex];
        }
        if (hatAnimPosHideIndex >= hatPosAnims.Length)
        {
            Debug.LogError("Index of Hide Position Animation is out of range");
        }
        else
        {
            hatAnimPosHide = hatPosAnims[hatAnimPosHideIndex];
        }
        if (hatAnimPosBohatIndex >= hatPosAnims.Length)
        {
            Debug.LogError("Index of Bohat Position Animation is out of range");
        }
        else
        {
            hatAnimPosBohat = hatPosAnims[hatAnimPosBohatIndex];
        }

        AnimateRotation[] hatRotAnims = hat.GetComponents<AnimateRotation>();

        if (hatAnimRotHideIndex >= hatRotAnims.Length)
        {
            Debug.LogError("Index of Hide Rotation Animation is out of range");
        }
        else
        {
            hatAnimRotHide = hatRotAnims[hatAnimRotHideIndex];
        }
        if (hatAnimRotBohatIndex >= hatRotAnims.Length)
        {
            Debug.LogError("Index of Bohat Rotation Animation is out of range");
        }
        else
        {
            hatAnimRotBohat = hatRotAnims[hatAnimRotBohatIndex];
        }

        GameController.instance.player = this;
        startPosition = transform.position;
        startScale = transform.localScale;
        startGlideUnlocked = glideUnlocked;
        startHatUnlocked = shootingEnabled;
        startFixedDeltaTime = Time.fixedDeltaTime;
        wakeUpAtStart = true;
        changeIdle = false;
        started = false;
        IsWalking = startIsWalking;
        teleportTutorialEnabled = false;
        _walkToRunDelay = walkToRunDelay;
        anim.SetFloat("WalkMod", isWalking ? 1 : 0);
        speed = isWalking ? walkSpeed : runSpeed;
        //GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        GameController.instance.AddOnUpsideDown(CheckUpsideDown);
        GameController.instance.AddOnDifficultyChanged(ManageDifficulty);

        hatX = hat.transform.localPosition.x;

        //lastSavedPosition = transform.position;
        actualTeleportBeforeLand = maxTeleportBeforeLand;
        prevYVelocity = 0;
        gravity = true;
        alreadyJumped = false;
        isAiming = false;
        isAimingHatarang = false;
        anim.SetBool("IsAiming", isAiming);
        //magicVFXAnim.SetBool("IsAiming", isAiming);
        anim.SetBool("IsPlanting", false);
        aimCancelled = false;
        canShoot = true;
        isGrounded = false;
        isGliding = false;
        lastGliding = false;
        isAscending = false;
        lastAscending = false;
        godMode = false;
        IsInWind = false;
        isBoat = false;
        //buttonReleasedAfterTeleport = true;
        waitJumpFromHatRoutine = null;
        waitBeforeGlideRoutine = null;
        waitBeforeTeleportRoutine = null;
        bullHatTimeRoutine = null;
        shootsDelayRoutine = null;
        freezeAfterLaunchRoutine = null;
        lastTimeGrounded = 0;
        lastGroundedPosition = transform.position;
        //respawnJumpDirection = transform.up;
        //alternateIdleTriggered = false;
        rigStartScale = rig.transform.localScale;
        right = !GameController.instance.startUpsideDown;
        prevRight = right;
        ResetMovementStuff();
        //Debug.Log("Start");
        ManageRigMirroring();
        isDead = false;
        isCaptured = false;
        //previousGlidingAnim = false;
        tempBohat.SetActive(false);
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            //Debug.Log(clip.name);
            switch (clip.name)
            {
                case wakeUpAnimName:
                    wakeUpAnimDuration = clip.length;
                    break;
            }
        }
        //if (checkGroundedPrecision < 1)
        //    checkGroundedPrecision = 1;
        isMovementBlocked = false;
        isThrowBlocked = false;
        isAimBlocked = false;
        isTeleportBlocked = false;
        isRetrieveBlocked = false;
        isJumpBlocked = false;
        hatLightRenderer.enabled = false;
        hatCanEnable = false;
        movable = null;
        if (wakeUpAtStart)
            wakeUpAtStart = hasToWakeUp;
        //isJumpBlocked = false;
        //isTeleportBlocked = false;
        if (wakeUpAtStart)
        {
            anim.SetTrigger("WakeUpLoop");
        }
        //Debug.Log("cambio started nella start");
        //started = false;
    }

    private void Start()
    {
        StartCoroutine(ScarySound());
        ManageDifficulty();
        aboveWallMinHeight = Mathf.Tan(maxSlope * Mathf.Deg2Rad) * clingMaxDistance;
        float maxFrameYMovFalling = Mathf.Abs(Physics.gravity.y * Time.fixedDeltaTime);
        float maxFrameYMovJumping = jumpForce * Time.fixedDeltaTime;
        if (aboveWallMinHeight < maxFrameYMovFalling)
            aboveWallMinHeight = maxFrameYMovFalling;
        if (aboveWallMinHeight < maxFrameYMovJumping)
            aboveWallMinHeight = maxFrameYMovJumping;
    }

    public void ResetToStart()
    {
        speed = runSpeed;
        transform.position = startPosition;
        glideUnlocked = startGlideUnlocked;
        shootingEnabled = startHatUnlocked;
        lastGroundedPosition = startPosition;
        anim.Rebind();
        myRB.velocity = Vector3.zero;
        Show(true);
        //mesh.enabled = true;
        collider.enabled = true;
        isMovementBlocked = false;
        isThrowBlocked = false;
        isAimBlocked = false;
        isTeleportBlocked = false;
        isRetrieveBlocked = false;
        isJumpBlocked = false;
        //isJumpBlocked = false;
        //isTeleportBlocked = false;
        right = !GameController.instance.startUpsideDown;
        prevRight = right;
        ResetMovementStuff();
        //Debug.Log("ResetToStart");
        ManageRigMirroring();
        isCaptured = false;
        isDead = false;
        //Debug.Log(shootingEnabled);
        //if (shootingEnabled)
        teleportHat.gameObject.SetActive(false);
        tempBohat.SetActive(false);
        isInBullHatTime = false;
        canShoot = true;
        BlockShooting(false);
        killDelayedRoutine = null;
        isAiming = false;
        isAimingHatarang = false;
        godMode = false;
        isGliding = false;
        lastGliding = false;
        isAscending = false;
        lastAscending = false;
        isDragging = false;
        isHidden = false;
        isBoat = false;
        IsWalking = startIsWalking;
        anim.SetFloat("WalkMod",isWalking ? 1 : 0);
        speed = isWalking ? walkSpeed : runSpeed;
        changeIdle = false;
        hatCanEnable = false;
        teleportTutorialEnabled = false;
        alternateIdleDisabled = false;
        //teleportTutorialStopTime = hasToStopTimeAtFirstTeleportTutorial;
        gravity = true;
        //alternateIdleTriggered = false;
        anim.SetBool("IsAiming", isAiming);
        //magicVFXAnim.SetBool("IsAiming", isAiming);
        anim.SetBool("IsPlanting", false);
        StopAllCoroutines();
        waitJumpFromHatRoutine = null;
        waitBeforeGlideRoutine = null;
        waitBeforeTeleportRoutine = null;
        bullHatTimeRoutine = null;
        shootsDelayRoutine = null;
        freezeAfterLaunchRoutine = null;
        hatLightRenderer.enabled = false;
        movable = null;
        StopAllCoroutines();
        PlayStopGlideSound();
        hatAnimScaleGlide.AnimationReset();
        hatAnimPosGlide.AnimationReset();
        if (explosionRoutine != null)
        {
            StopCoroutine(explosionRoutine);
            explosionRoutine = null;
            hatLightRenderer.enabled = false;
            teleportHat.ResetLightColor();
        }
        teleportHat.HasBomblebee = false;
        //Debug.Log("cambio started nel reset");
        started = false;
        //if (wakeUpAtStart)
        //{
        //    anim.SetTrigger("WakeUpLoop");
        //}
    }

    // Update is called once per frame
    void Update()
    {
        teleportFrame = false;
        anim.SetFloat("InverseTimescale", (Time.timeScale != 0) ? 1 / Time.timeScale : 1);
        anim.SetFloat("WalkMod", Mathf.MoveTowards(anim.GetFloat("WalkMod"), isWalking ? 1 : 0, (1f/_walkToRunDelay)*Time.deltaTime));
        speed = Mathf.MoveTowards(speed, isWalking ? walkSpeed : runSpeed, ((runSpeed-walkSpeed)/ _walkToRunDelay) *Time.deltaTime);
        //Debug.Log(anim.GetFloat("WalkMod"));
        if ((lastGroundedPosition.y - transform.position.y) > abyssLimit)
            Kill();
        //Debug.Log(transform.position.y - lastGroundedPosition.y);
        //if (teleportHat.transform.position.y < abyssLimit)
        //teleportHat.gameObject.SetActive(false);

        if (teleportHat.gameObject.activeSelf && Vector3.Distance(teleportHat.transform.position, transform.position) > distanceToReturn)
        {
            ReturnHat();
        }

        //if(Input.GetJoystickNames()[0]=="")
        //{

        //if((Input.GetJoystickNames()[0] == "" && Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)).x < transform.position.x) ||
        //    (Input.GetJoystickNames()[0] != "" && Input.GetAxis("RightStickH")<0))
        //bool right = (controller && (Input.GetAxis("HorizontalRight") >= triggerDeadZone || !(Input.GetAxis("HorizontalRight") <= -triggerDeadZone))) || (!controller && Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)).x > transform.position.x);
        if ((isGrounded || IsInWind || isBoat || Time.time>lastThrowTime+teleportBeforeLandResetTime) && actualTeleportBeforeLand != maxTeleportBeforeLand)
            actualTeleportBeforeLand = maxTeleportBeforeLand;
        //if (IsOnTheGround() && actualJumpBeforeLand != maxJumpBeforeLand)
        //    actualJumpBeforeLand = maxJumpBeforeLand;

        if(shootingEnabled && !teleportHat.gameObject.activeSelf && hidehatUnlocked)
        {
            if(isHidden && GameController.instance.inputManager.HideInputReleased)
            {
                IsHidden = false;
            }
            else if(!isHidden && GameController.instance.inputManager.HideInputPressed)
            {
                IsHidden = true;
            }
            //IsHidden = !isHidden;
        }

        //if (!teleportHat.gameObject.activeSelf && shootingEnabled)
        //{
        if(!isCaptured)
        {
            //Debug.Log(Physics.Raycast(transform.position, Vector3.down, waterMaxDistance, waterLayer, QueryTriggerInteraction.Ignore));
            //Debug.DrawRay(collider.bounds.center + Vector3.down * collider.bounds.extents.y, Vector3.down * waterMaxDistance, Color.blue, Time.deltaTime);
            if (bohatUnlocked && !teleportHat.gameObject.activeSelf && !isBoat && GameController.instance.inputManager.BohatInput &&
                /*Physics.Raycast(collider.bounds.center + Vector3.down * collider.bounds.extents.y, Vector3.down, waterMaxDistance, waterLayer, QueryTriggerInteraction.Ignore)*/
                isOnWater)
            //Debug.DrawRay(collider.bounds.center - transform.up*collider.bounds.extents.y, Vector3.down * (maxDistanceFromGround), isGrounded ? Color.green : Color.red, Time.fixedDeltaTime);)
            {
                IsBoat = true;
            }
            if (bohatUnlocked && isBoat && (!GameController.instance.inputManager.BohatInput || !isOnWater))
            {
                IsBoat = false;
                myRB.velocity = Vector3.zero;
                myRB.AddForce(transform.up * bohatJumpForce, ForceMode.VelocityChange);
            }
        }
        if (!isAimBlocked && !isCaptured && !isBoat && !isHidden)
        {

            //}
            //else
            //{
            //    teleportVelocityV3 = new Vector3(Input.GetAxis("RightStickH"), -Input.GetAxis("RightStickV"),0);
            //}
            //previewVelocityV3.z = 0;
            //previewVelocityV3 = Vector3.Normalize(previewVelocityV3);
            if (!isDragging)
            {

                if(movingRight)
                {
                    right = GameController.instance.UpsideDown? false : true;
                }
                else if(movingLeft)
                {
                    right = GameController.instance.UpsideDown ? true : false;
                }
                else
                {
                    if (GameController.instance.inputManager.LastAimInputStored.x > 0)
                        right = true;
                    else if(GameController.instance.inputManager.LastAimInputStored.x < 0)
                        right = false;
                }
                /*
                if (((GameController.instance.inputManager.LastAimInputStored.x > 0 && !movingLeft) || (movingRight && GameController.instance.inputManager.MovementInput)) && !right)
                {
                        right = true;
                }
                else if (((GameController.instance.inputManager.LastAimInputStored.x < 0 && !movingRight) || (movingLeft && GameController.instance.inputManager.MovementInput)) && right)
                {
                        right = false;
                }
                */
                if (prevRight != right)
                {
                    if (!GameController.instance.inputManager.DirectionBlocked)
                    //    ManageRigMirroring();
                    //else
                    {
                        anim.ResetTrigger("Turn");
                        anim.SetTrigger("Turn");
                    }
                }
                prevRight = right;
            }

            canReachAssistedDestination = false;
            if ((GameController.instance.assistedMode || shadowThrow) && assistedDestination != null)
            {
                //previewVelocityV3
                float v = teleportVelocity;
                float g = Physics.gravity.magnitude;
                float x = assistedDestination.position.x - teleportShootPoint.position.x;
                float y = assistedDestination.position.y - teleportShootPoint.position.y;
                float underSqrt = Mathf.Pow(v, 4f) - g * (g * Mathf.Pow(x, 2) + 2f * y * Mathf.Pow(v, 2f));
                float den = 0;
                float numPlus = 0;
                float numMinus = 0;
                float alpha1 = 45 * Mathf.Deg2Rad;
                float alpha2 = 45 * Mathf.Deg2Rad;
                float alpha = 45 * Mathf.Deg2Rad;
                bool canCalculate = right ? x >= 0 : x <= 0;
                if (!right)
                    alpha = 135 * Mathf.Deg2Rad;
                if (underSqrt > 0 && canCalculate)
                {
                    //Debug.Log("si può fare");
                    if (!right)
                        x = -x;
                    den = g * x;
                    numPlus = v * v + Mathf.Sqrt(underSqrt);
                    numMinus = v * v - Mathf.Sqrt(underSqrt);
                    alpha1 = Mathf.Atan(numMinus / den);
                    alpha2 = Mathf.Atan(numPlus / den);
                    bool alpha1ok = !(alpha1 < 0 || alpha1 > (Mathf.PI / 2f));
                    bool alpha2ok = !(alpha2 < 0 || alpha2 > (Mathf.PI / 2f));
                    if (!right)
                    {
                        if (alpha1ok)
                            alpha1 = (Mathf.PI / 2f) + ((Mathf.PI / 2f) - alpha1);
                        if (alpha2ok)
                            alpha2 = (Mathf.PI / 2f) + ((Mathf.PI / 2f) - alpha2);
                    }
                    Vector3 simulationVelocity = Vector3.zero;
                    if (alpha1ok)
                    {
                        simulationVelocity = new Vector3(Mathf.Cos(alpha1), Mathf.Sin(alpha1), 0);
                        simulationVelocity *= teleportVelocity;
                        bool hitted = DrawTraject(teleportShootPoint.position, simulationVelocity, true, assistedDestination.position);
                        //Debug.Log(hittedpoint);
                        if (!hitted)
                        {
                            alpha1ok = false;
                        }
                    }
                    if (alpha2ok)
                    {
                        simulationVelocity = new Vector3(Mathf.Cos(alpha2), Mathf.Sin(alpha2), 0);
                        simulationVelocity *= teleportVelocity;
                        bool hitted = DrawTraject(teleportShootPoint.position, simulationVelocity, true, assistedDestination.position);
                        if (!hitted)
                        {
                            alpha2ok = false;
                        }
                    }

                    if (alpha1ok && alpha2ok)
                    {
                        if (right)
                        {
                            if (alpha1 < alpha2)
                            {
                                alpha = alpha1;
                            }
                            else
                            {
                                alpha = alpha2;
                            }
                        }
                        else
                        {
                            if (alpha1 > alpha2)
                            {
                                alpha = alpha1;
                            }
                            else
                            {
                                alpha = alpha2;
                            }
                        }
                    }
                    else if (alpha1ok)
                    {
                        alpha = alpha1;
                    }
                    else if (alpha2ok)
                    {
                        alpha = alpha2;
                    }

                    if (alpha1ok || alpha2ok)
                        canReachAssistedDestination = true;
                    //alpha = alpha1;
                }
                if (GameController.instance.inputManager.AimInput && !shadowThrow)
                {
                    //Debug.Log(GameController.instance.inputManager.AimAngleSine);
                    if (GameController.instance.inputManager.AimAngleSine > 0)
                    {
                        alpha = Mathf.Lerp(alpha, Mathf.PI / 2f, GameController.instance.inputManager.AimAngleSine);
                    }
                    else
                    {
                        alpha = Mathf.Lerp(alpha, right ? 0 : Mathf.PI, -GameController.instance.inputManager.AimAngleSine);
                    }
                }
                previewVelocityV3 = new Vector3(Mathf.Cos(alpha), Mathf.Sin(alpha), 0);
                previewVelocityV3 *= teleportVelocity;
                teleportVelocityV3 = previewVelocityV3;
            }
            else
            {
                previewVelocityV3 = GameController.instance.inputManager.LastAimInputStored;
                previewVelocityV3 *= teleportVelocity;
                teleportVelocityV3 = GameController.instance.inputManager.FirstAimInputStored;
                teleportVelocityV3 *= teleportVelocity;
            }
            //previewVelocityV3 *= (GameController.instance.Right.x > 0) ? 1 : -1;
            //teleportVelocityV3 *= (GameController.instance.Right.x > 0) ? 1 : -1;
            //}
            //canShoot = teleportVelocityV3.magnitude != 0;
        }

        if(GameController.instance.inputManager.InteractionInputPressed)
        {
            onInteraction.Invoke();
        }

        if (!isBoat && !isHidden)
        {
            TeleportManagement();
            HatarangManagement();
        }


        //if (Input.GetKeyDown(KeyCode.K))
        //    StartKill();
    }

    private void LateUpdate()
    {
        DrawTraject(teleportShootPoint.position, previewVelocityV3, false, null);
        //Vector3 traslated = GameController.instance.inputManager.LastAimInputStored;
        //traslated.y -= Mathf.Sin(Mathf.PI/4);
        DrawHatarangTraject(hatarangShootPoint.position, GameController.instance.inputManager.LastAimInputStoredTranslated.normalized);
        if (teleportHat.gameObject.activeSelf)
        {
            cameraTarget.position = Vector3.Lerp(transform.position, teleportHat.transform.position, playerHatCameraRange);
        }
        else
        {
            cameraTarget.position = transform.position;
        }
        bool nowGliding = isAscending || isGliding;
        anim.SetBool("IsGliding", nowGliding);
        //if(nowGliding)
        //    anim.ResetTrigger("Turn");
        //if (previousGlidingAnim != nowGliding)
        //{
        //    if (nowGliding)
        //        OnGlideStart();
        //    else
        //        OnGlideEnd();
        //}

        if (lastGliding != isGliding)
        {
            if (isGliding)
            {
                if (!isAscending)
                    PlayStartGlideSound();

            }
            else
                PlayStopGlideSound();
        }
        lastGliding = isGliding;

        if (lastAscending != isAscending)
        {
            if (isAscending)
                PlayStartAscendingSound();
            else
                PlayStopAscendingSound();
        }
        lastAscending = isAscending;

        //previousGlidingAnim = nowGliding;
        if (myRB.velocity.y <= 0 || !GameController.instance.inputManager.GlideInput)
            isAscending = false;

        if(lastPosition != transform.position)
        {
            lastMovedTime = Time.timeSinceLevelLoad;
        }
        lastPosition = transform.position;

        if(anim.GetFloat("Speed") != 0)
            StopIdleSound();
    }

    private void FixedUpdate()
    {
        //if (Physics.Raycast(transform.position, Vector3.right, myBounds.extents.x * (1 + maxDistanceFromWallJoWallJump), wallLayer, QueryTriggerInteraction.Ignore))
        //{
        //    //Debug.Log("muro a destra");
        //    if (myRB.velocity.y < 0)
        //        myRB.velocity = (Vector3.right * wallMagneteForce) - (Vector3.down * wallGravityContrast);
        //}
        //else if (Physics.Raycast(transform.position, -Vector3.right, myBounds.extents.x * (1 + maxDistanceFromWallJoWallJump), wallLayer, QueryTriggerInteraction.Ignore))
        //{
        //    //Debug.Log("muro a sinistra");
        //    if (myRB.velocity.y < 0)
        //        myRB.velocity = -(Vector3.right * wallMagneteForce) - (Vector3.down * wallGravityContrast);
        //}
        //Debug.Log(myRB.velocity);
        if (isDead || isCaptured)
            return;
        IsOnTheGround();
        avoidSliding = CalculateSlope(myRB.position,true);
        InOnWater();
        if (!isGrounded && myRB.velocity.y < 0)
        {
            lastFallingSpeed = myRB.velocity.y;
        }
        anim.SetBool("IsInAir", !isGrounded);
        anim.SetBool("IsFalling", (!GameController.instance.UpsideDown && myRB.velocity.y <= 0.5) || (GameController.instance.UpsideDown && myRB.velocity.y >= 0.5));
        float prevIdleMod = anim.GetFloat("IdleMod");
        float actualIdleMod;
        if (lastGroundObject != null && lastGroundObject.tag == "Jellyphants" && isGrounded)
            actualIdleMod = 0.5f;
        else
            actualIdleMod = changeIdle ? 1 : 0;
        anim.SetFloat("IdleMod", actualIdleMod);

        if (prevIdleMod != actualIdleMod)
        {
            if (actualIdleMod == 0.5f)
                PlayIdleJellSound();
            else
                StopIdleJellSound();
        }

        if (!isMovementBlocked && !isHidden && !isBoat)
        {
            MovementManagement();
            DragManagement();

            if (!teleportHat.gameObject.activeSelf && shootingEnabled && !isGrounded && myRB.velocity.y <= 0 && GameController.instance.inputManager.GlideInput && glideUnlocked)
            {
                if (!isGliding)
                {
                    //Vector3 gravityContrastForce = Vector3.down * (-Physics.gravity.y) * glideGravityModifier;
                    //Debug.Log("sto planando " + gravityContrastForce);
                    if (waitBeforeGlideRoutine != null)
                        StopCoroutine(waitBeforeGlideRoutine);
                    waitBeforeGlideRoutine = StartCoroutine(GlideDelayed());
                    //Debug.Log("inizio a planare");
                    isGliding = true;
                }
                //Debug.Log(gravityContrastForce.y);
            }
            else
            {
                //Debug.Log("Sto cadendo");
                if (waitBeforeGlideRoutine != null)
                    StopCoroutine(waitBeforeGlideRoutine);
                isGliding = false;
                gravity = true;
            }
        }
        else
        {
            // Gestisco il caso in cui il movimento è bloccato
            ResetMovementStuff();
        }
        if (gravity)
        {
            //if (!isGrounded)
            //{
            Vector3 gravityContrastForce = Vector3.down * (-Physics.gravity.y);
            if (avoidSliding && isGrounded)
                gravityContrastForce = Vector3.Project(gravityContrastForce, groundNormal);
            
            myRB.AddForce(gravityContrastForce, ForceMode.Acceleration);
            //}
            //else
            //{
            //    myRB.velocity = Vector3.zero;
            //}
        }
        prevYVelocity = myRB.velocity.y;
        //Debug.Log(myRB.velocity.y);

        if(isGrounded && !wasGroundedLastFrame)
        {
            StopBullhatTime();
        }

        if (!isGrounded && wasGroundedLastFrame)
        {
            if(isAiming || isAimingHatarang)
                StartBullhatTime(true);
        }

        canClimb = CheckCanCling() && clingEnabled;

        if (!automaticClimb)
        {
            /* CLIMB VERSION IN WHICH THE PLAYER REMAINS CLINGING UNTIL THE PRESSION OF A BUTTON TO CLIMB OR DROP */

            if (!isGrounded && !isClinging && canClimb)
            {
                IsClinging = true;
            }

            if (isClinging)
            {
                if (GameController.instance.inputManager.VerticalMovementInput)
                {
                    if (GameController.instance.inputManager.VerticalMovementInputAmount > 0)
                    {
                        ClimbFromCling();
                    }
                    else
                    {
                        DropFromCling();
                    }
                }
            }

            /* END */
        }
        else
        {
            /* CLIMB VERSION IN WHICH THE PLAYER CLIMB AUTOMATICALLY IF HE CAN CLING IN THE DIRECTION HE IS MOVING */

            if (canClimb && GameController.instance.inputManager.MovementInput)
            {
                ClimbFromCling();
            }

            /* END */
        }

        wasGroundedLastFrame = isGrounded;
    }

    private void OnTriggerStay(Collider collision)
    {

    }

    public void ManageRigMirroring()
    {
        //Debug.Log(right);
        switch (axisToMirror)
        {
            case Axis.X:
                if ((right && rig.transform.localScale.x < 0) || (!right && rig.transform.localScale.x > 0))
                    rig.transform.localScale = new Vector3(rig.transform.localScale.x * -1, rig.transform.localScale.y, rig.transform.localScale.z);
                break;
            case Axis.Y:
                if ((right && rig.transform.localScale.y < 0) || (!right && rig.transform.localScale.y > 0))
                    rig.transform.localScale = new Vector3(rig.transform.localScale.x, rig.transform.localScale.y * -1, rig.transform.localScale.z);
                break;
            case Axis.Z:
                if ((right && rig.transform.localScale.z < 0) || (!right && rig.transform.localScale.z > 0))
                    rig.transform.localScale = new Vector3(rig.transform.localScale.x, rig.transform.localScale.y, rig.transform.localScale.z * -1);
                break;
        }
    }

    public void CheckUpsideDown()
    {
        transform.localScale = new Vector3(startScale.x, startScale.y * (GameController.instance.UpsideDown ? -1 : 1), startScale.z);
        EndFlip();
        //anim.SetTrigger("Flip");
    }

    public void AddInteraction(UnityAction action)
    {
        onInteraction.AddListener(action);
    }

    public void RemoveInteraction(UnityAction action)
    {
        onInteraction.RemoveListener(action);
    }

    public void Ascend(Vector3 forceVector)
    {
        if (!teleportHat.gameObject.activeSelf && shootingEnabled /* && !IsOnTheGround() */ && GameController.instance.inputManager.GlideInput && glideUnlocked)
        {
            //Debug.Log("ascendo");
            //myRB.velocity = Vector3.zero;
            myRB.AddForce(forceVector, ForceMode.Acceleration);
            isAscending = true;
            //isGliding = false;
        }
    }

    public void GodMode(bool value)
    {
        godMode = value;
    }

    public void WakeUp()
    {
        onWakeUp.Invoke();
        if (hasToWakeUp)
        {
            anim.SetTrigger("WakeUp");
            GameController.instance.inputManager.DirectionBlock(true);
            isMovementBlocked = true;
            isThrowBlocked = true;
            isAimBlocked = true;
            StartCoroutine(DelayedMovementRelease(wakeUpAnimDuration, true));
        }
        else
        {
            isMovementBlocked = false;
            isThrowBlocked = false;
            isAimBlocked = false;
        }
    }

    public void LoadSaved()
    {
        wakeUpAtStart = false;
        ResetToStart();
        //teleportTutorialStopTime = false;
        PlayerData save = GameController.instance.SaveData.playerData;
        shootingEnabled = save.hatUnlocked;
        //shootingEnabled = true; // temporary
        if (shootingEnabled)
            teleportHat.gameObject.SetActive(false);
        teleportHat.ManageDeath(true);
        glideUnlocked = save.glideUnlocked;
        transform.position = save.lastSavedPosition;
        lastGroundedPosition = save.savedLastGroundedPosition;
        teleportTutorialEnabled = save.teleportTutorialEnabled;
        changeIdle = save.changeIdle;
        //assistedMode = save.assistedMode;
        myRB.AddForce(save.respawnJumpDirection * respawnJumpForce, ForceMode.VelocityChange);
        onTeleport.Invoke();
        //lastGroundedPosition = transform.position;
        PlayRespawnSound();
        StartCoroutine(ScarySound());
        //Debug.Log("cambio started nel load");
        started = true;
        IsWalking = false;
        anim.SetFloat("WalkMod", isWalking ? 1 : 0);
        speed = isWalking ? walkSpeed : runSpeed;
        anim.SetBool("IsPlanting", false);
    }

    public void Kill()
    {
        //Debug.Log("kill");
        if (godMode)
            return;

        //Debug.Log("start kill");
        /*if (lastGroundObject)
        {
            ActualPlatformBehaviour lastPlatform = lastGroundObject.GetComponent<ActualPlatformBehaviour>();
            if (lastPlatform)
            {
                if (lastPlatform.target.savePlayer)
                {
                    if (lastPlatform.target.SavePlayer())
                    {
                        myRB.velocity = Vector3.zero;
                        return;
                    }
                }
            }
        }*/

        GameController.instance.OnDeath();
    }

    public void ManageDifficulty()
    {
        bullHatTimeReleaseDuration = GameController.instance.difficultyManager.difficultyData.bullHatTimeReleaseDuration;
        freezeAfterLaunchDuration = GameController.instance.difficultyManager.difficultyData.freezeAfterLaunchDuration;
    }

    public void AssignTeleportHat(HatBehaviour newHat)
    {
        teleportHat = newHat;
        //teleportRB = teleportHat.gameObject.GetComponent<Rigidbody>();

    }

    public void Capture()
    {
        isCaptured = true;
        isAiming = false;
        Show(false);
        StopBullhatTime();
        //magicVFXAnim.SetBool("IsAiming", isAiming);
        anim.SetFloat("Speed", 0);
        myRB.velocity = Vector3.zero;
        if(lastTrasport)
        {
            lastTrasport.RemovePlayer();
        }
        hatAnimPosGlide.AnimationReset();
        hatAnimScaleGlide.AnimationReset();
    }

    public void BlockMovement(bool blockThrow)
    {
        isMovementBlocked = true;
        if (blockThrow)
            isThrowBlocked = true;
        anim.SetFloat("Speed", 0);
    }

    public void BlockThrow(bool newBlock)
    {
        isThrowBlocked = newBlock;
    }

    public void BlockRetrieve(bool newBlock)
    {
        isRetrieveBlocked = newBlock;
    }

    public void BlockTeleport(bool newBlock)
    {
        isTeleportBlocked = newBlock;
    }

    public void ReleaseMovement()
    {
        isMovementBlocked = false;
        isThrowBlocked = false;
        isAimBlocked = false;
        //Debug.Log("mi muovo");
    }

    public void StartSliding(bool right)
    {
        isThrowBlocked = true;
        isRetrieveBlocked = true;
        isTeleportBlocked = true;
        isJumpBlocked = true;
        if(startSlidingRoutine != null)
        {
            StopCoroutine(startSlidingRoutine);
        }
        startSlidingRoutine = StartCoroutine(SlidingWaitForGrounded(right));
    }

    public void StartSliding()
    {
        isThrowBlocked = true;
        isRetrieveBlocked = true;
        isTeleportBlocked = true;
        isJumpBlocked = true;
        if (startSlidingRoutine != null)
        {
            StopCoroutine(startSlidingRoutine);
        }
        startSlidingRoutine = StartCoroutine(SlidingWaitForGrounded(GameController.instance.inputManager.MovementInputAmount > 0));
    }

    public void EndSliding()
    {
        GameController.instance.inputManager.StopScriptedMovement();
        anim.SetBool("IsSliding", false);
        isThrowBlocked = false;
        isRetrieveBlocked = false;
        isTeleportBlocked = false;
        isJumpBlocked = false;
        if (vfxSlidingIsPlaying)
        {
            slidingVFXParticle.Stop();
            vfxSlidingIsPlaying = false;
        }
        PlayStopSlidingSound();
        if (startSlidingRoutine != null)
        {
            StopCoroutine(startSlidingRoutine);
            startSlidingRoutine = null;
        }
    }

    public void EndSliding(float delay)
    {
        GameController.instance.inputManager.ScriptedMovement(0);
        anim.SetBool("IsSliding", false);
        StartCoroutine(EndSlidingDelayed(delay));
        if (vfxSlidingIsPlaying)
        {
            slidingVFXParticle.Stop();
            vfxSlidingIsPlaying = false;
        }
        PlayStopSlidingSound();
        if (startSlidingRoutine != null)
        {
            StopCoroutine(startSlidingRoutine);
            startSlidingRoutine = null;
        }
    }

    public void StartPlanting()
    {
        anim.SetBool("IsPlanting", true);
    }

    public void OnPlantingStarted()
    {
        BlockMovement(true);
        GameController.instance.inputManager.DirectionBlock(true);
    }

    public void EndPlanting()
    {
        anim.SetBool("IsPlanting", false);
        // Based on animation duration
        //Invoke("ReleaseMovement", 0.5f);
    }

    public void OnPlantingEnded()
    {
        GameController.instance.inputManager.DirectionBlock(false);
        ReleaseMovement();
    }

    public void StartFall()
    {
        BlockMovement(true);
        GameController.instance.inputManager.DirectionBlock(true);
        anim.SetBool("IsFallen", true);
    }

    public void EndFall()
    {
        anim.SetBool("IsFallen", false);
        // Based on animation duration
        Invoke("OnFallEnded", 1f);
    }

    public void OnFallEnded()
    {
        GameController.instance.inputManager.DirectionBlock(false);
        ReleaseMovement();
    }

    public void StartWalk()
    {
        StartWalk(walkToRunDelay);
    }

    public void StartWalk(float blend)
    {
        IsWalking = true;
        _walkToRunDelay = blend;
    }

    public void StopWalk()
    {
        StopWalk(walkToRunDelay);
    }

    public void StopWalk(float blend)
    {
        IsWalking = false;
        _walkToRunDelay = blend;
    }

    public void Bend()
    {
        anim.SetTrigger("Bend");
    }

    public void AssignAssistedDestination(Transform destination)
    {
        assistedDestination = destination;
    }

    //public void BlockJump(bool value)
    //{
    //    isJumpBlocked = value;
    //}

    //public void BlockTeleport(bool value)
    //{
    //    isTeleportBlocked = value;
    //}

    public void StartKill()
    {
        if (godMode)
            return;
        onDeath.Invoke();
        deathVFXAnim.SetTrigger("Death");
        myRB.velocity = Vector3.zero;
        if (!teleportHat.gameObject.activeSelf)
        {
            teleportHat.transform.position = hat.transform.position;
            teleportHat.ManageDeath(false);
        }
        hat.gameObject.SetActive(false);
        teleportHat.gameObject.SetActive(true);
        Show(false);
        DelayedKill(respawnDelay);
    }

    //public void FallKill(float delay)
    //{
    //    if (godMode)
    //        return;
    //    PlayFallSound();
    //    DelayedKill(delay);
    //}

    public void DelayedKill(float delay)
    {
        if (godMode)
            return;

        /*if (lastGroundObject)
        {
            ActualPlatformBehaviour lastPlatform = lastGroundObject.GetComponent<ActualPlatformBehaviour>();
            if (lastPlatform)
            {
                if (lastPlatform.target.savePlayer)
                {
                    if (lastPlatform.target.SavePlayer())
                    {
                        myRB.velocity = Vector3.zero;
                        return;
                    }
                }
            }
        }*/
        onDelayedDeath.Invoke();
        PlayDeathSound();
        //Debug.Log("delayed kill");
        BlockShooting(true);
        //mesh.enabled = false;
        collider.enabled = false;
        isDead = true;
        isAiming = false;
        //magicVFXAnim.SetBool("IsAiming", isAiming);
        ChangeTimeScale(1);
        GameController.instance.mainCamera.Block(true);
        if (killDelayedRoutine == null)
            killDelayedRoutine = StartCoroutine(DelayedKillCoroutine(delay));
    }

    public void Save(Vector3 position, Vector3 direction)
    {
        PlayerData save = GameController.instance.SaveData.playerData;
        position.z = transform.position.z;
        save.lastSavedPosition = position;
        save.respawnJumpDirection = direction;
        save.savedLastGroundedPosition = lastGroundedPosition;
        save.glideUnlocked = glideUnlocked;
        save.hatUnlocked = shootingEnabled;
        save.teleportTutorialEnabled = teleportTutorialEnabled;
        save.changeIdle = changeIdle;
        //save.assistedMode = assistedMode;
        //myRB.velocity = Vector3.zero;
    }

    public void EnableShooting()
    {
        PlayTakeHatSound();
        onCancel.Invoke();
        shootingEnabled = true;
    }

    public void BlockShooting(bool value)
    {
        shootingBlocked = value;
    }

    public void UnlockGlide()
    {
        glideUnlocked = true;
    }

    public void AddVelocity(Vector3 _velocity)
    {
        myRB.velocity = _velocity;
    }

    //public void ClampPosition(float bottom, float top, float left, float right)
    //{
    //    //if(!GameController.instance.inputManager.InputBlocked)
    //    transform.position = new Vector3(Mathf.Clamp(transform.position.x, left, right), Mathf.Clamp(transform.position.y, bottom, top), transform.position.z);
    //    teleportHat.ClampPosition(bottom, top, left, right);
    //}

    public void Show(bool show)
    {
        //Debug.Log(mesh.name + show);
        mesh.gameObject.SetActive(show);
        hatRenderer.enabled = show;
    }

    void MovementManagement()
    {
        bool leftInput=false;
        bool rightInput=false;
        float currentAccelerationTime = accelerationTime;
        float currentDecelerationTime = decelerationTime;
        //float speed = 0f;
        //anim.SetFloat("Speed", Input.GetAxis("Horizontal") * (right? 1 :-1));

        if (alreadyJumped)
        {
            if (isGrounded)
                alreadyJumped = false;
        }
        if (GameController.instance.inputManager.MovementInput)
        {
            if (GameController.instance.inputManager.MovementInputAmount > 0)
                rightInput = true;
            else
                leftInput = true;

            // Se sono fermo
            if(!movingLeft && !movingRight)
            {
                //Debug.Log("------------>StartAccelerating");
                isAccelerating = true;
                accelerationStartTime = Time.fixedTime;
                accelerationStartSpeed = 0f;
                if(rightInput)
                    movingRight = true;
                if (leftInput)
                    movingLeft = true;
            }
            else
            {
                // Se stavo già decellerando
                if(isDecelerating && !isAccelerating && ((rightInput && movingRight)||(leftInput&&movingLeft)))
                {
                    //Debug.Log("-------------->StartAcceleratingWhileDecelerating");
                    isAccelerating = true;
                    accelerationStartTime = Time.fixedTime;
                    accelerationStartSpeed = 0f;
                    accelerationDecelerationBlend = true;
                }

                // Se mi sto girando
                if((movingRight && leftInput) || (movingLeft && rightInput))
                {
                    //Debug.Log("--------------------------------------->Mi giro");
                    currentAccelerationTime = accelerationTimeTurn;
                    currentDecelerationTime = decelerationTimeTurn;
                    
                    if (!isDecelerating)
                    {
                        //Debug.Log("-------------->StartDecelerating");
                        isAccelerating = false;
                        isDecelerating = true;
                        decelerationStartTime = Time.fixedTime;
                        decelerationStartSpeed = currentSpeed;
                        //decelerationActualTime = decelerationTime; // * (currentSpeed / speed);
                    }
                    
                }

            }
        }
        else
        {
            // Se ho smesso di dare l'input di muoversi
            if(movingLeft||movingRight)
            {
                if(!isDecelerating)
                {
                    //Debug.Log("-------------->StartDecelerating");
                    isAccelerating = false;
                    isDecelerating = true;
                    decelerationStartTime = Time.fixedTime;
                    decelerationStartSpeed = currentSpeed;
                    //decelerationActualTime = decelerationTime; // * (currentSpeed / speed);
                }
            }
            isAccelerating = false;

        }

        float accelSpeed = 0f;
        float decelSpeed = 0f;
        // Gestisco Accelerazioni e decelerazioni
        if (isAccelerating)
        {
            if (Time.fixedTime - accelerationStartTime > currentAccelerationTime)
            {
                //Debug.Log("----------->StopAccelerating");
                isAccelerating = false;
                accelerationDecelerationBlend = false;
            }
            else
            {
                accelSpeed = Mathf.LerpUnclamped(accelerationStartSpeed, speed, accelerationCurve.Evaluate((Time.fixedTime - accelerationStartTime) / currentAccelerationTime));
                if (!accelerationDecelerationBlend)
                    currentSpeed = accelSpeed;
            }
        }
        if(isDecelerating)
        {
            if (Time.fixedTime - decelerationStartTime > currentDecelerationTime)
            {
                //Debug.Log("------------->StopDecelerating");
                isDecelerating = false;
                accelerationDecelerationBlend = false;
                if(!accelerationDecelerationBlend)
                {
                    movingLeft = false;
                    movingRight = false;
                    currentSpeed = 0f;
                }
            }
            else
            {
                decelSpeed = Mathf.LerpUnclamped(decelerationStartSpeed, 0f, decelerationCurve.Evaluate((Time.fixedTime - decelerationStartTime) / currentDecelerationTime));
                if (!accelerationDecelerationBlend)
                    currentSpeed = decelSpeed;
            }
        }

        if(accelerationDecelerationBlend)
        {
            currentSpeed = Mathf.Lerp(decelSpeed, accelSpeed, (Time.fixedTime - accelerationStartTime) / ((decelerationStartTime + currentDecelerationTime) - decelerationStartTime));
        }


        if ((movingLeft || movingRight) && !isAccelerating && !isDecelerating)
            currentSpeed = speed;

        //Debug.Log("CurrentSpeed=" + currentSpeed);


        if (isGrounded)
        {
            cancelInAirMomentum = false;
            startingJumpSpeed = currentSpeed;
            if(movingRight)
            {
                startingJumpDirectionRight = true;
            }
            else if (movingLeft)
            {
                startingJumpDirectionRight = false;
            }
        }
        else
        {
            float airTime = Time.timeSinceLevelLoad - lastTimeGrounded;
            float myAirControl = Mathf.Lerp(airControl, 1f, airControlReturnCurve.Evaluate(Mathf.Clamp01(airTime / airControlReturnTime)));
            if (cancelInAirMomentum)
                myAirControl = 1f;
            bool directionChanged = false;
            if ((startingJumpDirectionRight && movingLeft) || (!startingJumpDirectionRight && movingRight))
                directionChanged = true;
            currentSpeed = (myAirControl * currentSpeed) + ((1 - myAirControl) * startingJumpSpeed * (directionChanged?-1f:1f));
        }

        if (movingLeft || movingRight)
        { 
            onMovement.Invoke();
            float actualSpeed = (isDragging) ? draggingSpeed : currentSpeed;
            Vector3 movedirection = GameController.instance.Right * actualSpeed * (movingRight ? 1f : -1f) * Time.fixedDeltaTime; //(GameController.instance.inputManager.MovementInputAmount > 0) ? GameController.instance.Right : GameController.instance.Left;
            //Debug.Log(collider.bounds.center);
            //Debug.DrawRay(collider.bounds.center + (movedirection * collider.bounds.extents.x * 0.1f), Vector3.right * collider.bounds.extents.x * 0.9f, Color.red, Time.fixedDeltaTime);
            //Debug.DrawRay(collider.bounds.center + (movedirection * collider.bounds.extents.x * 0.1f), Vector3.left * collider.bounds.extents.x * 0.9f, Color.red, Time.fixedDeltaTime);
            //Debug.DrawRay(collider.bounds.center + (movedirection * collider.bounds.extents.x * 0.1f), Vector3.up * collider.bounds.extents.y * 0.9f, Color.red, Time.fixedDeltaTime);
            //Debug.DrawRay(collider.bounds.center + (movedirection * collider.bounds.extents.x * 0.1f), Vector3.down * collider.bounds.extents.y * 0.9f, Color.red, Time.fixedDeltaTime);
            //bool collideInDirection = Physics.CheckBox(collider.bounds.center + (movedirection * collider.bounds.extents.x * 0.1f), collider.bounds.extents * 0.9f, transform.rotation, wallLayer, QueryTriggerInteraction.Ignore);
            CapsuleCollider cap = (CapsuleCollider)collider;
            bool collideInDirection = false;

            //Controllo di fronte a me
            if (Physics.CheckCapsule(transform.position + cap.center - (transform.up * ((cap.height / 2) - cap.radius)) + (movedirection),
                                     transform.position + cap.center + (transform.up * ((cap.height / 2) - cap.radius)) + (movedirection),
                                     cap.radius, wallLayer, QueryTriggerInteraction.Ignore))
            {
                // Controllo se sbatto la testa
                if (Physics.CheckSphere(transform.position + cap.center + (GameController.instance.Up * ((cap.height / 2) - cap.radius)) + (movedirection),
                                        cap.radius, wallLayer, QueryTriggerInteraction.Ignore))
                {
                    collideInDirection = true;
                }
                else
                {
                    // controllo usando la max slope
                    RaycastHit hit;
                    if (Physics.SphereCast(transform.position + cap.center + (GameController.instance.Up * ((cap.height / 2) - cap.radius)) + (movedirection), cap.radius,
                                            -GameController.instance.Up, out hit, (cap.height - (2 * cap.radius)), wallLayer, QueryTriggerInteraction.Ignore))
                    {
                        if (Mathf.Abs(Vector3.SignedAngle(hit.normal, GameController.instance.Up, Vector3.forward)) >= maxSlope)
                        {
                            collideInDirection = true;
                        }
                    }
                }
                /*RaycastHit hit;
                if( Physics.Raycast(new Vector3( transform.position.x, (groundCollisionPoint + (GameController.instance.Up * Mathf.Sin(maxSlope) * movedirection.magnitude)).y, 0), 
                                    movedirection.normalized, out hit, 
                                    movedirection.magnitude + cap.radius, 
                                    wallLayer,QueryTriggerInteraction.Ignore) )
                {

                    if(hit.distance<=movedirection.magnitude+(cap.radius * Mathf.Sin(Mathf.Abs(Vector3.SignedAngle(hit.normal, GameController.instance.Up, Vector3.forward)))))
                    //if ((Mathf.Abs(Vector3.SignedAngle(hit.normal, GameController.instance.Up, Vector3.forward)))>maxSlope)
                        collideInDirection = true;
                }*/
            /*
collideInDirection = Physics.CheckCapsule(transform.position + cap.center - (transform.up * ((cap.height / 2) - cap.radius)) + (movedirection) + (GameController.instance.Up * Mathf.Sin(maxSlope) * movedirection.magnitude),
                                  transform.position + cap.center + (transform.up * ((cap.height / 2) - cap.radius)) + (movedirection) + (GameController.instance.Up * Mathf.Sin(maxSlope) * movedirection.magnitude),
                                  cap.radius, wallLayer, QueryTriggerInteraction.Ignore);
*/
        }
        else if (isGrounded)
            {
                RaycastHit hit;
                if (Physics.SphereCast(transform.position + cap.center - (GameController.instance.Up * ((cap.height / 2) - cap.radius)) + (movedirection), cap.radius,
                                       -GameController.instance.Up, out hit, (movedirection.magnitude*Mathf.Tan(maxSlope*Mathf.Deg2Rad))+maxDistanceFromGround, wallLayer, QueryTriggerInteraction.Ignore))
                {
                    //Debug.Log("Mi sposto");
                    //float movementAmount = movedirection.magnitude;
                    movedirection += -GameController.instance.Up * (hit.distance);
                    //movedirection = movedirection.normalized * movementAmount;
                }
            }

            // Se sono in glide lascio un po' di distanza con gli oggetti per non rischiare di fare attrito
            if (!isGrounded && !teleportFrame && isGliding)
            {
                RaycastHit hit;
                if (Physics.CapsuleCast(transform.position + cap.center - (transform.up * ((cap.height / 2) - cap.radius)),
                                        transform.position + cap.center + (transform.up * ((cap.height / 2) - cap.radius)),
                                        cap.radius, GameController.instance.Right * (movingRight ? 1f : -1f), out hit, glideMinDistanceToObjects, wallLayer, QueryTriggerInteraction.Ignore))
                {
                    collideInDirection = true;
                    myRB.position = new Vector3(hit.point.x + (movingRight ? -1f : 1f)*glideMinDistanceToObjects, myRB.position.y, myRB.position.z);
                }
            }

            //Debug.Log(collideInDirection);
            Vector3 blockedCameraPosition = transform.position + cap.center + movedirection + GameController.instance.Right * cap.radius * (movedirection.x > 0 ? 1 : -1);

            if (!collideInDirection && GameController.instance.mainCamera.CheckAllowedPosition(blockedCameraPosition))
            {
                if(isGrounded)
                {
                    myRB.velocity = Vector3.zero;
                }
                myRB.position += movedirection;
                anim.ResetTrigger("AlternateIdle");
                anim.SetFloat("Speed", currentSpeed/speed);//(right ? 1 : -1));
            }
            else
            {
                anim.SetFloat("Speed", 0f);
            }
        }
        else
        {
            anim.SetFloat("Speed", 0f);
        }
        //Debug.DrawRay(transform.position - Vector3.forward, Vector3.right * ((Input.GetAxis("Horizontal") < 0) ? -myBounds.extents.x * 1.1f : myBounds.extents.x * 1.1f), Color.red, 1);
        //anim.SetBool("Walk", Input.GetAxis("Horizontal") != 0);
        if (GameController.instance.inputManager.JumpInput && !isJumpBlocked /*&& !isAiming && !isAimingHatarang*/ && !isWalking)
        {
            //Debug.Log("1");

            //Debug.Log("2");
            Vector3 direction = GameController.instance.Up;
            //Vector3 inputDir = Vector3.right * Input.GetAxis("Horizontal");
            //RaycastHit hit;
            //bool right = Physics.Raycast(transform.position, Vector3.right, out hit, myBounds.extents.x * (1 + maxDistanceFromWallJoWallJump), wallLayer, QueryTriggerInteraction.Ignore);
            //bool left = Physics.Raycast(transform.position, -Vector3.right, out hit, myBounds.extents.x * (1 + maxDistanceFromWallJoWallJump), wallLayer, QueryTriggerInteraction.Ignore);
            //bool wallJump = right || left;
            ////Physics.Raycast(transform.position, , out hit,100f,1<< gameObject.layer, QueryTriggerInteraction.Ignore);
            ////Debug.Log(wallJump);
            //if (wallJump && !IsOnTheGround())
            //{
            //    //Vector3 collisionNormal = hit.normal;
            //    //collisionNormal.Normalize();
            //    //Vector3 collisionDir = Vector3.right * collisionNormal.x;
            //    //if (collision.gameObject.tag == "Wall")
            //    Debug.Log(Input.GetAxis("Horizontal"));
            //    if (right && Input.GetAxis("Horizontal") > 0f)
            //    {
            //        direction += Vector3.right;
            //    }
            //    else if (left && Input.GetAxis("Horizontal") < 0f)
            //    {
            //        direction += Vector3.left;
            //    }
            //    else if (Input.GetAxis("Horizontal") == 0f)
            //    {
            //        myRB.MovePosition(transform.position + (((right) ? Vector3.right : (left) ? Vector3.left : Vector3.zero) * (myBounds.extents.x * (1.01f + maxDistanceFromWallJoWallJump))));
            //        return;
            //    }
            //    else
            //        return;
            //    //else
            //    //    direction += ((collisionDir.x * inputDir.x >= 0) ? inputDir : Vector3.zero);
            //}
            //Debug.Log("provo a saltare");
            if ((isGrounded || ((Time.timeSinceLevelLoad - lastTimeGrounded) < notGroundedJumpTollerance)) && !alreadyJumped /*|| wallJump*/)
            {
                anim.SetTrigger("Jump");
                lastJumpTime = Time.fixedTime;
                //Debug.Log("Salto " + IsOnTheGround());
                direction.Normalize();
                myRB.velocity = Vector3.zero;
                myRB.position += GameController.instance.Up * maxDistanceFromGround;
                myRB.AddForce(direction.normalized * jumpForce, ForceMode.VelocityChange);
                alreadyJumped = true;
                onJump.Invoke();

                /*if(isAiming || isAimingHatarang)
                {
                    Debug.Log("-------------------------------<><>-----------------------------");
                    StartBullhatTime(true);
                }*/
            }

        }

        if(GameController.instance.inputManager.JumpInputReleased && !isGrounded && Time.timeSinceLevelLoad-lastJumpTime<jumpCancelTime && (GameController.instance.UpsideDown? myRB.velocity.y < 0:myRB.velocity.y>0))
        {
            isCancelingJump = true;
            cancelingJumpStartTime = Time.timeSinceLevelLoad;
            cancelingJumpTarget = myRB.velocity.y * jumpCancelCurve.Evaluate((Time.timeSinceLevelLoad - lastJumpTime) / jumpCancelTime);

            anim.SetBool("IsFalling", true);
        }

        if(isCancelingJump)
        {
            if (isGrounded || Time.timeSinceLevelLoad - cancelingJumpStartTime > jumpCancelFadeTime)
                isCancelingJump = false;
            else
            {
                myRB.velocity = new Vector3(myRB.velocity.x, Mathf.Lerp(myRB.velocity.y, cancelingJumpTarget, jumpCancelFadeCurve.Evaluate((Time.timeSinceLevelLoad - cancelingJumpStartTime) / jumpCancelFadeTime)), myRB.velocity.z);
            }

        }
    }

    private void ResetMovementStuff()
    {
        movingLeft = false;
        movingRight = false;
        isAccelerating = false;
        isDecelerating = false;
        accelerationStartTime = 0f;
        decelerationStartTime = 0f;
        accelerationStartSpeed = 0f;
        decelerationStartSpeed = 0f;
        accelerationDecelerationBlend = false;
        currentSpeed = 0f;
    }

    private bool CheckCanCling()
    {
        Vector3 aboveWallPosition = clingingPosition.position + GameController.instance.Up * aboveWallMinHeight;
        bool nearWall = Physics.Raycast(clingingPosition.position, clingingPosition.right, clingMaxDistance, wallLayer, QueryTriggerInteraction.Ignore);
        bool gripOutsideCollider = !Physics.Raycast(transform.position, clingingPosition.position - transform.position, Vector3.Distance(transform.position, clingingPosition.position), wallLayer, QueryTriggerInteraction.Ignore) && !Physics.Raycast(transform.position, aboveWallPosition - transform.position, Vector3.Distance(transform.position, aboveWallPosition), wallLayer, QueryTriggerInteraction.Ignore);
        bool emptyAboveWall = !Physics.Raycast(aboveWallPosition, clingingPosition.right, clingMaxDistance, wallLayer, QueryTriggerInteraction.Ignore);
        return nearWall && emptyAboveWall && gripOutsideCollider;
    }

    private void DropFromCling()
    {
        transform.position += GameController.instance.Down * aboveWallMinHeight;
        IsClinging = false;
    }

    private void ClimbFromCling()
    {
        CapsuleCollider cap = (CapsuleCollider)collider;
        Vector3 aboveWallPosition = clingingPosition.position + GameController.instance.Up * aboveWallMinHeight;
        transform.position = aboveWallPosition + GameController.instance.Up * cap.height/2 + clingingPosition.transform.right*climbAdvancement;
        if(!automaticClimb)
            IsClinging = false;
    }

    private void StartBullhatTime(bool sustain)
    {
        if (bullHatTimeRoutine != null)
            StopCoroutine(bullHatTimeRoutine);
        if (!isGrounded || sustain)
        {
            bullHatTimeRoutine = StartCoroutine(BullHatTime(sustain));
            isInBullHatTime = true;
        }
    }

    private void StopBullhatTime()
    {
        if (bullHatTimeRoutine != null)
        {
            StopCoroutine(bullHatTimeRoutine);
            ChangeTimeScale(1);
            bullHatTimeRoutine = null;
        }
    }

    private void StartFreezeAfterLaunch()
    {
        //Debug.Log("freeze");
        if (freezeAfterLaunchRoutine != null)
            StopCoroutine(freezeAfterLaunchRoutine);
        if (!isGrounded && freezeAfterLaunchDuration > 0)
        {
            freezeAfterLaunchRoutine = StartCoroutine(FreezeAfterLaunch());
        }
    }

    private void StopFreezeAfterLaunch()
    {
        if (freezeAfterLaunchRoutine != null)
        {
            StopCoroutine(freezeAfterLaunchRoutine);
            freezeAfterLaunchRoutine = null;
            gravity = true;
            myRB.velocity = velocityBeforeFreeze;
            isMovementBlocked = false;
            isAimBlocked = false;
        }
    }
        
    void TeleportManagement()
    {
        if (GameController.instance.inputManager.ThrowInputReleased)
            launchButtonReleased = true;

        if (!shootingEnabled || shootingBlocked || isAimingHatarang)
            return;

        if (GameController.instance.inputManager.TeleportInputPressed && actualTeleportBeforeLand >= 1 && teleportHat.gameObject.activeSelf /* && !teleportHat.IsBoat */ && canTeleport && /*!isMovementBlocked &&*/ !isCaptured && !isTeleportBlocked && !teleportHat.shadowMode)
        {
            launchButtonReleased = false;
            aimCancelled = false;
            Vector3 direction = Vector3.zero;
            RaycastHit hitUp;
            RaycastHit hitDown;
            //BoxCollider boxcol = (BoxCollider)collider;
            bool canUp = !Physics.Raycast(teleportHat.transform.position, Vector3.up, out hitUp, myBounds.extents.y * 2, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            bool canDown = !Physics.Raycast(teleportHat.transform.position, Vector3.down, out hitDown, myBounds.extents.y * 2, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            bool canMiddle = Vector3.Distance(hitUp.point, hitDown.point) >= myBounds.extents.y * 2;
            if (canUp)
                direction = Vector3.up;
            else if (canDown)
                direction = Vector3.down;
            //Debug.Log(canUp + " " + canDown);
            //Debug.Log(myBounds.extents.y * 2);
            if ((canUp || canDown || canMiddle))
            {
                StopFreezeAfterLaunch();
                myRB.velocity = Vector3.zero;
                if (canUp || canDown)
                    myRB.MovePosition(teleportHat.transform.position + direction * myBounds.extents.y -direction*teleportHat.GetComponent<Collider>().bounds.extents.y);
                else
                    myRB.MovePosition(Vector3.Lerp(hitUp.point, hitDown.point, 0.5f));

                teleportFrame = true;
                PlayTeleportSound();
                if (teleportHat.IsGrounded(myBounds.extents.y * 2 + tolleranceForInAirTeleport))
                    anim.SetTrigger("TeleportInPlace");
                else
                    anim.SetTrigger("Teleport");
                teleportHat.gameObject.SetActive(false);
                hat.gameObject.SetActive(true);
                onTeleport.Invoke();
                alreadyJumped = true;
                cancelInAirMomentum = true;
                actualTeleportBeforeLand--;
                lastThrowTime = Time.time;
                if (shootCooldownAfterTeleport)
                {
                    canShoot = false;
                    if (shootsDelayRoutine != null)
                        StopCoroutine(shootsDelayRoutine);
                    shootsDelayRoutine = StartCoroutine(RenableHatShoot());
                }
                //buttonReleasedAfterTeleport = false;
            }
            return;
        }
        //else if(GameController.instance.inputManager.TeleportInputReleased && !isAiming)
        //{
        //    launchButtonReleased = true;
        //}

        if ((GameController.instance.inputManager.ThrowInputPressed || (GameController.instance.inputManager.ThrowInput && !isAiming && launchButtonReleased)) && actualTeleportBeforeLand >= 1 && !isThrowBlocked && !isCaptured)
        {
            aimCancelled = false;
            //if(teleportHat.IsBoat)
            //{
            //    teleportHat.gameObject.SetActive(false);
            //}
            if (!teleportHat.gameObject.activeSelf)
            {
                if (canShoot)
                {
                    isAiming = true;
                    anim.SetBool("IsAiming", isAiming);
                    //magicVFXAnim.SetBool("IsAiming", isAiming);
                    onAim.Invoke();
                    GameController.instance.inputManager.ClearAimInputStored();
                    launchButtonReleased = false;
                    StartBullhatTime(false);
                }
            }
        }
        else if ((GameController.instance.inputManager.ThrowInputReleased && isAiming)||forcedLaunch)
        {
            //Debug.Log("devo lanciare");
            if (!teleportHat.gameObject.activeSelf && actualTeleportBeforeLand >= 1 && !aimCancelled && !isThrowBlocked && !isCaptured)
            {
                //Debug.Log("devo lanciare 2");
                if (canShoot)
                {
                    //Debug.Log("devo lanciare 3");
                    //Debug.DrawRay(transform.position, teleportShootPoint.position - transform.position, Color.red, 10);
                    //Debug.Log(teleportHat.Collider.bounds.extents);
                    bool canHat = !Physics.BoxCast(transform.position, teleportHat.Collider.size / 2f, teleportShootPoint.position - transform.position, Quaternion.identity,Vector3.Distance(teleportShootPoint.position, transform.position), groundLayer, QueryTriggerInteraction.Ignore)
                        /*&& Physics.OverlapBox(teleportShootPoint.position, teleportHat.Collider.bounds.extents, Quaternion.identity, groundLayer, QueryTriggerInteraction.Ignore).Length == 0*/;
                    //Debug.Log(canHat);
                    if (canHat && hatCanEnable)
                    {
                        //Debug.Log("devo lanciare 4");
                        if (vfxShadowSmokeIsPlaying)
                        {
                            //vfxShadowSmoke.Stop();
                            vfxShadowSmoke.gameObject.SetActive(false);
                            vfxShadowSmokeIsPlaying = false;
                        }
                        teleportHat.gameObject.SetActive(true);
                        teleportHat.ResetToStart();
                        teleportHat.transform.position = teleportShootPoint.position;
                        teleportHat.AssignVelocity(teleportVelocityV3);
                        anim.SetTrigger("Throw");
                        onHatThrow.Invoke();
                        if(isHidden)
                        IsHidden = false;
                        canTeleport = false;
                        ShakeHat(false);
                        if (waitBeforeTeleportRoutine != null)
                            StopCoroutine(waitBeforeTeleportRoutine);
                        waitBeforeTeleportRoutine = StartCoroutine(TeleportDelayed());
                        if (shootCooldownAfterLaunch)
                        {
                            canShoot = false;
                            if (shootsDelayRoutine != null)
                                StopCoroutine(shootsDelayRoutine);
                            shootsDelayRoutine = StartCoroutine(RenableHatShoot());
                        }

                        StartFreezeAfterLaunch();
                    }
                    GameController.instance.inputManager.ResetAimInputStored();
                    StopBullhatTime();
                    myRB.isKinematic = false;
                    //myRB.velocity = velocityBeforeHatTime;
                    
                    isInBullHatTime = false;

                    
                    //if (waitJumpFromHatRoutine != null)
                    //    StopCoroutine(waitJumpFromHatRoutine);
                    //waitJumpFromHatRoutine = StartCoroutine(WaitJumpFromHat());
                }
            }
            isAiming = false;
            anim.SetBool("IsAiming", isAiming);
            //magicVFXAnim.SetBool("IsAiming", isAiming);
            //buttonReleasedAfterTeleport = true;
            aimCancelled = false;
            forcedLaunch = false;
            //launchButtonReleased = true;
        }
        //if ((Input.GetButtonDown("Fire1") || rightTriggerDown) && !teleportHat.gameObject.activeSelf && actualTeleportBeforeLand >= 1)
        //{
        //    //velocityBeforeHatTime = myRB.velocity;
        //    if (bullHatTimeRoutine != null)
        //        StopCoroutine(bullHatTimeRoutine);
        //    if (!IsOnTheGround())
        //    {
        //        bullHatTimeRoutine = StartCoroutine(BullHatTime());
        //        isInBullHatTime = true;
        //    }
        //}
        //if ((Input.GetButtonUp("Fire1") || rightTriggerUp) && actualJumpBeforeLand >= 1)
        //{
        //    if (teleportHat.gameObject.active && canJumpFromHat)
        //    {
        //        myRB.velocity = Vector3.zero;
        //        myRB.MovePosition(teleportHat.transform.position + Vector3.up * myBounds.extents.y);
        //        teleportHat.gameObject.SetActive(false);
        //        myRB.AddForce(Vector3.up * jumpFromHatForce, ForceMode.VelocityChange);
        //        actualJumpBeforeLand--;
        //    }
        //}
        if (GameController.instance.inputManager.CancelInput && !isRetrieveBlocked)
        {
            CancelLaunch();
        }
    }

    public void CancelLaunch()
    {
        if (teleportHat.shadowMode)
            return;
        //Debug.Log("cancelled");
        isAiming = false;
        anim.SetBool("IsAiming", isAiming);
        //magicVFXAnim.SetBool("IsAiming", isAiming);
        if (teleportHat.gameObject.activeSelf)
        {
            teleportHat.gameObject.SetActive(false);
            StopFreezeAfterLaunch();
        }
        else
        {
            aimCancelled = true;
            StopBullhatTime();
        }
        if (waitBeforeTeleportRoutine != null)
            StopCoroutine(waitBeforeTeleportRoutine);
        waitBeforeTeleportRoutine = null;
        if (freezeAfterLaunchRoutine != null)
            StopCoroutine(freezeAfterLaunchRoutine);
        freezeAfterLaunchRoutine = null;
        canTeleport = false;
        onCancel.Invoke();
        //aimCancelled = true;
    }

    private void HatarangManagement()
    {
        if(!hatarangUnlocked || isAiming)
        {
            if (isAimingHatarang)
                isAimingHatarang = false;
            return;
        }
        if(GameController.instance.inputManager.HatarangInputPressed && !teleportHat.gameObject.activeSelf && !isAimingHatarang)
        {
            isAimingHatarang = true;
            GameController.instance.inputManager.ClearAimInputStored();
            StartBullhatTime(false);
        }
        if (GameController.instance.inputManager.HatarangInputReleased && isAimingHatarang)
        {
            isAimingHatarang = false;
            teleportHat.gameObject.SetActive(true);
            teleportHat.ResetToStart();
            teleportHat.transform.position = hatarangShootPoint.position;
            //Debug.Log(GameController.instance.inputManager.FirstAimInputStoredTranslated.normalized);
            teleportHat.LaunchHatarang(GameController.instance.inputManager.FirstAimInputStoredTranslated.normalized);
            IsHidden = false;
            GameController.instance.inputManager.ResetAimInputStored();
            StopBullhatTime();
        }
        if (GameController.instance.inputManager.CancelInput)
        {
            isAimingHatarang = false;
            if (teleportHat.gameObject.activeSelf)
            {
                teleportHat.gameObject.SetActive(false);
            }
            StopBullhatTime();
        }
    }

    private void DragManagement()
    {
        if (GameController.instance.inputManager.DragInput)
        {
            if (isDragging)
            {
                if(GameController.instance.inputManager.MovementInput)
                    movable.Move(transform.right * (GameController.instance.inputManager.MovementInputAmount > 0 ? 1 : -1));
            }
            else
            {
                Vector3 facedirection = right ? GameController.instance.Right : GameController.instance.Left;
                Collider[] draggable = Physics.OverlapBox(collider.bounds.center + (facedirection * collider.bounds.extents.x), new Vector3(dragMaxDistance / 2f, collider.bounds.extents.y * 0.9f, collider.bounds.extents.z * 0.9f), transform.rotation, dragLayer, QueryTriggerInteraction.Ignore);
                if (draggable.Length != 0)
                {
                    MovableBehaviour _movable = draggable[0].gameObject.GetComponent<MovableBehaviour>();
                    if (_movable != null)
                    {
                        isDragging = true;
                        movable = _movable;
                        draggingSpeed = movable.speed;
                    }
                }
            }
        }
        else
        {
            if(isDragging)
            {
                isDragging = false;
                movable = null;
            }
        }
    }

    private void IsOnTheGround()
    {
        //bool rightGrounded = Physics.Raycast(collider.bounds.center - transform.up * myBounds.extents.y + transform.up * 0.1F + transform.right * myBounds.extents.x, -transform.up, maxDistanceFromGroundToJump + 0.1F, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        //bool leftGrounded = Physics.Raycast(collider.bounds.center - transform.up * myBounds.extents.y + transform.up * 0.1F - transform.right * myBounds.extents.x, -transform.up, maxDistanceFromGroundToJump + 0.1F, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        //float den = checkGroundedPrecision - 1;
        //if (den < 1)
        //    den = 1;
        //float step = myBounds.extents.x *2f / den;
        //Debug.Log(step + " " + myBounds.extents.x);
        //for (int i = 0; i < checkGroundedPrecision; i++)
        //{
        //    bool thisGrounded = Physics.Raycast(collider.bounds.center - transform.up * myBounds.extents.y + transform.up * 0.1F + (transform.right * step *(i+0.5f-checkGroundedPrecision/2f)), -transform.up, maxDistanceFromGround + 0.1F, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        //    Debug.DrawRay(collider.bounds.center - transform.up * myBounds.extents.y + transform.up * 0.1F + (transform.right * step*(i+0.5f-checkGroundedPrecision/2f)), -transform.up * (maxDistanceFromGround + 0.1F), thisGrounded ? Color.green : Color.red, Time.fixedDeltaTime);
        //    isGrounded |= thisGrounded;
        //}
        //RaycastHit hit;
        //Debug.Log(collider.bounds.center);
        isGrounded = false;
        Vector3 bounds = collider.bounds.extents;
        bounds.x -= 0.01f;
        CapsuleCollider cap = (CapsuleCollider)collider;
        //Collider[] hits = Physics.OverlapBox(collider.bounds.center + GameController.instance.Down * maxDistanceFromGround, new Vector3(bounds.x * 0.9f, bounds.y, bounds.z), transform.rotation, groundLayer, QueryTriggerInteraction.Ignore);
        RaycastHit[] hits;
        /*hits = Physics.CapsuleCastAll(transform.position + cap.center - (transform.up * ((cap.height / 2) - cap.radius)) + (GameController.instance.Up * 0.5f),
                                    transform.position + cap.center + (transform.up * ((cap.height / 2) - cap.radius)) + (GameController.instance.Up * 0.5f),
                                    cap.radius, GameController.instance.Down, maxDistanceFromGround + 0.5f, groundLayer, QueryTriggerInteraction.Ignore);*/

        hits = Physics.SphereCastAll(transform.position + cap.center - (GameController.instance.Up * ((cap.height / 2) - cap.radius)) + (GameController.instance.Up * 0.5f),
                                    cap.radius, GameController.instance.Down, maxDistanceFromGround + 0.5f, groundLayer, QueryTriggerInteraction.Ignore);

        TransportBehaviour myTransport = null;
        if (hits.Length > 0)
        {
            float slope = 361f;
            foreach (RaycastHit hit in hits)
            {
                if (hit.distance == 0)
                    continue;

                float mySlope = Mathf.Abs(Vector3.SignedAngle(hit.normal, GameController.instance.Up, Vector3.forward));

                //Debug.Log("myslope=" + mySlope);
                //Debug.DrawRay(transform.position, hit.normal, Color.red);
                if (mySlope < slope)
                {

                    groundNormal = hit.normal;
                    groundCollisionPoint = hit.point;
                    //Debug.Log("ok");
                    if (mySlope < maxJumpSlope)
                    {
                        isGrounded = true;
                        if (hit.collider.gameObject.GetComponent<TransportBehaviour>() != null)
                        {
                            myTransport = hit.collider.gameObject.GetComponent<TransportBehaviour>();
                        }
                        else
                        {
                            myTransport = null;
                        }
                        lastGroundObject = hit.collider.gameObject;
                        GameController.instance.audioManager.SetGlobalParameter("TerrainType", (float)CheckGroundType(hit.collider));
                    }
                    slope = mySlope;
                }
            }

        }



        if (myTransport && myTransport != lastTrasport)
            myTransport.AddPlayer();

        if (lastTrasport && lastTrasport != myTransport)
            lastTrasport.RemovePlayer();

        lastTrasport = myTransport;
        //Debug.DrawRay(collider.bounds.center - transform.up*collider.bounds.extents.y, Vector3.down * (maxDistanceFromGround), isGrounded ? Color.green : Color.red, Time.fixedDeltaTime);
        //isGrounded = rightGrounded || leftGrounded;
        if (isGrounded)
        {
            lastTimeGrounded = Time.timeSinceLevelLoad;
            lastGroundedPosition = transform.position;
        }

        lastTrasport = myTransport;
        //Debug.DrawRay(collider.bounds.center - transform.up*collider.bounds.extents.y, Vector3.down * (maxDistanceFromGround), isGrounded ? Color.green : Color.red, Time.fixedDeltaTime);
        //isGrounded = rightGrounded || leftGrounded;
        if (isGrounded)
        {
            lastTimeGrounded = Time.timeSinceLevelLoad;
            lastGroundedPosition = transform.position;
        }
        //Debug.DrawRay(this.transform.position - transform.up * myBounds.extents.y + transform.up * 0.1F + transform.right * myBounds.extents.x, -transform.up * (maxDistanceFromGroundToJump + 0.1F), rightGrounded ? Color.green : Color.red, Time.fixedDeltaTime);
        //Debug.DrawRay(this.transform.position - transform.up * myBounds.extents.y + transform.up * 0.1F - transform.right * myBounds.extents.x, -transform.up * (maxDistanceFromGroundToJump + 0.1F), leftGrounded ? Color.green : Color.red, Time.fixedDeltaTime);
    }

    private bool CalculateSlope(Vector3 position, bool saveNormal)
    {
        if (!isGrounded)
            return false;

        RaycastHit hit;

        /*if (Physics.Raycast(position, -GameController.instance.Up, out hit))
        {
            if(saveNormal)
                groundNormal = hit.normal;

            float slope = Vector3.SignedAngle(hit.normal, GameController.instance.Up, Vector3.forward);*/

        float slope = Vector3.SignedAngle(groundNormal, GameController.instance.Up, Vector3.forward);
        if (Mathf.Abs(slope) >= maxSlope)
        {
            return false;
        }
        else
        {
            CapsuleCollider cap = (CapsuleCollider)collider;
            if ((movingRight || movingLeft) ||
                Physics.Raycast(transform.position + cap.center - (GameController.instance.Up * ((cap.height / 2) - cap.radius)) + (GameController.instance.Up * 0.5f),
                                GameController.instance.Down, maxDistanceFromGround + 0.5f + cap.radius, groundLayer, QueryTriggerInteraction.Ignore))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //}
    }

    private void InOnWater()
    {
        isOnWater = Physics.OverlapBox(collider.bounds.center + GameController.instance.Down * waterMaxDistance, new Vector3(collider.bounds.extents.x * 0.9f, collider.bounds.extents.y, collider.bounds.extents.z), transform.rotation, waterLayer, QueryTriggerInteraction.Ignore).Length != 0;
    }

    private GroundType CheckGroundType(Collider col)
    {
        GroundType returnable = GroundType.None;
        
        switch (col.transform.tag)
        {
            case "Grass":
                returnable = GroundType.Grass;
                break;
            case "Soil":
                returnable = GroundType.Soil;
                break;
            case "Rock":
                returnable = GroundType.Rock;
                break;
            case "Wood":
            case "Jellyphants":
                returnable = GroundType.Wood;
                break;
            case "Mush":
                returnable = GroundType.Mush;
                break;
            case "Mud":
                returnable = GroundType.Mud;
                break;
            case "Snow":
                returnable = GroundType.Snow;
                break;
        }

        return returnable;
    }

    private void CheckNoisyGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(collider.bounds.center - transform.up * myBounds.extents.y + transform.up * 0.1F, -transform.up, out hit, maxDistanceFromGround + 0.1F, noisyGroundLayer, QueryTriggerInteraction.Ignore))
        {
            SoundWaveBehaviour spawn = soundWavePrefab.GetPooledInstance<SoundWaveBehaviour>();
            spawn.transform.position = hit.point;
        }
    }

    bool DrawTraject(Vector3 startPos, Vector3 startVelocity, bool simulation, Vector3? simulationTarget)
    {
        if (!simulation)
        {
            if(hitPointLight!= null)
            hitPointLight.gameObject.SetActive(isAiming && canShoot);

            if (teleportHat.gameObject.activeSelf || !shootingEnabled)
            {
                line.enabled = false;

                hat.SetActive(false);

                return false;
            }

            if (!isAiming /*&& !isAimingHatarang*/)
            {
                if (isAimingHatarang)
                    return false;
                line.enabled = false;
                hat.SetActive(true);
                return false;
            }

            hat.SetActive(true);
        }
        if (canShoot)
        {
            //Debug.Log(line.widthMultiplier);
            if (Physics.BoxCast(transform.position, teleportHat.Collider.size / 2f, teleportShootPoint.position - transform.position, Quaternion.identity, Vector3.Distance(teleportShootPoint.position, transform.position), groundLayer, QueryTriggerInteraction.Ignore))
            {
                line.enabled = false;
                hatCanEnable = false;
                hitPointLight.color = cannotHitColor;
                hitPointLight.transform.position = teleportShootPoint.position;
                return false;
            }


            Vector3 pos = startPos;
            Vector3 vel = startVelocity;
            Vector3 lastPos = startPos;
            //Debug.Log(pos);
            int lineLenght = 200;
            int i = 0;
            float flyingTime = 0;
            if (!simulation)
            {
                line.colorGradient = /*isAiming?  (*/(GameController.instance.assistedMode && canReachAssistedDestination) ? assistedAimColor : normalAimColor/*) : hatarangAimColor*/;
                line.widthMultiplier = GameController.instance.CameraSizeDelta;
                line.positionCount = lineLenght;
                line.SetPosition(i, new Vector3(pos.x, pos.y, pos.z));
            }

            i++;

            while (i < lineLenght)
            {
                vel = vel + /*(isAiming?*/ Physics.gravity * startFixedDeltaTime /*: Vector3.zero)*/;
                pos = lastPos + vel * startFixedDeltaTime;
                if (simulation && Vector3.Distance(pos, (Vector3)simulationTarget) < assistedTollerance)
                    return true;
                if (Physics.BoxCast(lastPos, teleportHat.Collider.size / 2f, (pos - lastPos).normalized, Quaternion.identity, (pos - lastPos).magnitude, trajectoryLayer, QueryTriggerInteraction.Ignore))
                {
                    //Debug.Log("colpito");
                    break;
                }
                flyingTime += startFixedDeltaTime;
                lastPos = pos;
                //Debug.Log(pos);
                if (!simulation)
                    line.SetPosition(i, new Vector3(pos.x, pos.y, pos.z));
                //Debug.DrawLine(lastPos, pos, Color.red, Time.deltaTime);
                //Debug.Log(pos);
                i++;
            }
            hatCanEnable = flyingTime >= teleportHat.collectableDelay;
            if (!simulation)
            {
                line.positionCount = i;
                line.enabled = hatCanEnable;
            }
            if (hitPointLight != null)
            {
                hitPointLight.color = hatCanEnable ? canHitColor : cannotHitColor;
                hitPointLight.transform.position = lastPos;
            }
        }
        else if (!simulation)
        {
            line.enabled = false;
        }
        return false;
    }

    private bool DrawHatarangTraject(Vector3 startPos, Vector3 dir)
    {
        if (/*!isAiming &&*/ !isAimingHatarang)
        {
            if (isAiming)
                return false;
            line.enabled = false;
            //hat.SetActive(true);
            return false;
        }

        //hat.SetActive(true);

        if (canShoot)
        {
            line.colorGradient = hatarangAimColor;
            line.widthMultiplier = GameController.instance.CameraSizeDelta;
            Vector3 dest;
            RaycastHit hit;
            float distance = hatarangSpeed * hatarangReturnDelay;
            dest = startPos + dir * distance;
            if (Physics.BoxCast(startPos, teleportHat.Collider.size / 2f, dir, out hit, Quaternion.identity, distance, trajectoryLayer, QueryTriggerInteraction.Ignore))
            {
                dest = hit.point;
            }
            int lineLenght = (int) (Vector3.Distance(startPos, dest)/ (hatarangSpeed * Time.fixedDeltaTime));
            int i = 0;
            line.positionCount = lineLenght;
            Vector3 pos = startPos;
            line.SetPosition(i, pos);
            while (i < lineLenght)
            {
                pos += dir * hatarangSpeed * Time.fixedDeltaTime;
                
                line.SetPosition(i, pos);
                i++;
            }

            line.enabled = true;
            //Debug.Log(startPos + " " + dest);
        }
        else
        {
            line.enabled = false;
        }
        return false;
    }

    public void TryToTriggerAlternateIdle()
    {
        if (Random.Range(0, 101) <= alternativeIdleProb)
        {
            //alternateIdleTriggered = true;
            bool onJellyphant = lastGroundObject != null && lastGroundObject.tag == "Jellyphants";
            if (!alternateIdleDisabled && !onJellyphant && Time.timeSinceLevelLoad >= (lastMovedTime + stoppedTimeToEnableAltIdle))
                anim.SetTrigger("AlternateIdle");
            GameController.instance.audioManager.PlayGenericSound(scarySound, GameController.instance.player.gameObject);
        }
    }

    public void StartExplosion(float delay)
    {
        if (explosionRoutine != null)
            StopCoroutine(explosionRoutine);
        explosionRoutine = StartCoroutine(ExplosionRoutine(delay));
    }

    public void Explosion()
    {
        Vector3 center = teleportHat.gameObject.activeSelf ? teleportHat.transform.position : hat.transform.position;
        Collider[] toDamage = Physics.OverlapSphere(center, teleportHat.explosionRange, teleportHat.explosionDamageableLayer);
        foreach (Collider col in toDamage)
        {
            //Vector3 colDir = col.gameObject.transform.position - transform.position;
            //if (!Physics.Raycast(transform.position, colDir, colDir.magnitude, ~teleportHat.explosionDamageableLayer, QueryTriggerInteraction.Ignore))
            //{
                if (col.gameObject.tag == "Player")
                    GameController.instance.player.StartKill();
                if (col.gameObject.tag == "Destroyable")
                {
                    DestroyableBehaviour obj = col.gameObject.GetComponent<DestroyableBehaviour>();
                    obj.Damage(1);
                    //Debug.Log(obj.gameObject.name);
                }
            if (col.gameObject.tag == "Eater")
            {
                EaterTrapBehaviour obj = col.gameObject.GetComponent<EaterTrapBehaviour>();
                obj.Kill();
            }
            //}
        }
        ExplosionBehaviour spawn = explosionPrefab.GetPooledInstance<ExplosionBehaviour>();
        spawn.transform.position = center;
        spawn.animScale.to = new Vector3(teleportHat.explosionRange * 2f, teleportHat.explosionRange * 2f, teleportHat.explosionRange * 2f);
        if(explosionRoutine != null)
        {
            StopCoroutine(explosionRoutine);
        }
        hatLightRenderer.enabled = false;
        teleportHat.ResetLightColor();
        teleportHat.HasBomblebee = false;
        teleportHat.gameObject.SetActive(false);
        explosionRoutine = null;
        //Debug.Log("esplodo");
    }

    public void OnGlideStart()
    {
        hatAnimPosGlide.PlayForward();
        hatAnimScaleGlide.PlayForward();
        onGlideStart.Invoke();
    }

    public void OnGlideEnd()
    {
        hatAnimPosGlide.PlayBackward();
        hatAnimScaleGlide.PlayBackward();
        onGlideEnd.Invoke();
        if (isGrounded)
            PlayLandingSound();
    }

    public void OnHideStart()
    {
        hatAnimPosHide.PlayForward();
        hatAnimScaleHide.PlayForward();
        hatAnimRotHide.PlayForward();
        onHideStart.Invoke();
    }

    public void OnHideEnd()
    {
        hatAnimPosHide.PlayBackward();
        hatAnimScaleHide.PlayBackward();
        hatAnimRotHide.PlayBackward();
        onHideEnd.Invoke();
    }

    public void OnBohatStart()
    {
        hatAnimPosBohat.PlayForward();
        hatAnimScaleBohat.PlayForward();
        hatAnimRotBohat.PlayForward();
        hatRenderer.enabled = false;
        tempBohat.SetActive(true);
        onBohatStart.Invoke();
    }

    public void OnBohatEnd()
    {
        hatAnimPosBohat.PlayBackward();
        hatAnimScaleBohat.PlayBackward();
        hatAnimRotBohat.PlayBackward();
        hatRenderer.enabled = true;
        tempBohat.SetActive(false);
        onBohatEnd.Invoke();
    }

    public void EndFlip()
    {
        anim.ResetTrigger("Turn");
        //Debug.Log("EndFlip");
        ManageRigMirroring();
    }

    public void OnBendStart()
    {
        BlockMovement(false);
        GameController.instance.inputManager.DirectionBlock(true);
    }

    public void OnBendEnd()
    {
        ReleaseMovement();
        GameController.instance.inputManager.DirectionBlock(false);
    }

    public void ResetHatAnimations()
    {
        hatAnimPosGlide.AnimationReset();
        hatAnimScaleGlide.AnimationReset();
        hatAnimPosHide.AnimationReset();
        hatAnimScaleHide.AnimationReset();
        hatAnimRotHide.AnimationReset();
        hatAnimScaleBohat.AnimationReset();
        hatAnimPosBohat.AnimationReset();
        hatAnimRotBohat.AnimationReset(); ;
    }

    public void EnableTeleportTutorial(bool enable)
    {
        teleportTutorialEnabled = enable;
    }

    public void StartTeleportTutorial(float delay)
    {
        //Debug.Log(teleportTutorialEnabled);
        if (teleportTutorialEnabled && !shadowThrow)
        {
            GameController.instance.tutorialManager.StartTutorial(TutorialAction.Teleport,delay, false, false, new UnityEvent());
            //teleportTutorialStopTime = false;
        }
    }

    public void PlayStepSound(int right)
    {
        //FMODUnity.RuntimeManager.StudioSystem.setParameterByName("TerrainType", (float)CheckGroundType());
        GameController.instance.audioManager.PlayGenericSound((right == 0) ? stepRightSound : stepLeftSound, gameObject); //, "TerrainType", (float)CheckGroundType());
        CheckNoisyGround();
        //Debug.Log(right + " " + CheckGroundType().ToString());
    }

    public void PlayIdleSound()
    {
        idleSoundInstance = GameController.instance.audioManager.PlayGenericSound(idleSound, gameObject);
    }

    public void StopIdleSound()
    {
        GameController.instance.audioManager.DestroyInstance(idleSoundInstance);
    }

    public void PlayIdleAltSound()
    {
        GameController.instance.audioManager.PlayGenericSound(idleAltSound, gameObject);
    }

    public void PlayIdleJellSound()
    {
        Debug.Log("inizio jell idle loop");
        GameController.instance.audioManager.DestroyInstance(jellIdleSoundInstance);
        jellIdleSoundInstance = GameController.instance.audioManager.PlayGenericSound(idleJellSound, gameObject);
    }

    public void StopIdleJellSound()
    {
        Debug.Log("stoppo jell idle loop");
        GameController.instance.audioManager.DestroyInstanceFaded(jellIdleSoundInstance);
    }

    public void PlayWakeUpSound()
    {
        GameController.instance.audioManager.PlayGenericSound(wakeUpSound, gameObject);
        GameController.instance.audioManager.ChangeMusic(wakeUpMusic, false);
    }

    public void PlayAimSound()
    {
        GameController.instance.audioManager.PlayGenericSound(aimSound, gameObject);
    }

    public void PlayHatThrowSound()
    {
        GameController.instance.audioManager.PlayGenericSound(hatThrowSound, gameObject);
    }

    public void PlayBackflipSound()
    {
        GameController.instance.audioManager.PlayGenericSound(backflipSound, gameObject);
    }

    public void PlayTeleportSound()
    {
        GameController.instance.audioManager.PlayGenericSound(teleportSound, gameObject);
    }

    public void PlayExplosionSound()
    {
        GameController.instance.audioManager.PlayGenericSound(explosionSound, gameObject);
    }

    public void PlayDeathSound()
    {
        GameController.instance.audioManager.PlayGenericSound(deathSound, gameObject);
    }

    public void PlayRespawnSound()
    {
        GameController.instance.audioManager.PlayGenericSound(respawnSound, gameObject);
    }

    public void PlayStartGlideSound()
    {
        GameController.instance.audioManager.PlayGenericSound(startGlideSound, gameObject);
    }

    public void PlayStopGlideSound()
    {
        //Debug.Log("StopGlide");
        GameController.instance.audioManager.PlayGenericSound(stopGlideSound, gameObject);
    }

    public void PlayStartAscendingSound()
    {
        GameController.instance.audioManager.PlayGenericSound(startAscensionSound, gameObject);
    }

    public void PlayStopAscendingSound()
    {
        //Debug.Log("StopAscension");
        GameController.instance.audioManager.PlayGenericSound(stopAscensionSound, gameObject);
    }

    public void PlayHatFlapSound()
    {
        GameController.instance.audioManager.PlayGenericSound(hatFlapSound, hat);
    }

    public void PlayJumpSound()
    {
        //FMODUnity.RuntimeManager.StudioSystem.setParameterByName("TerrainType", (float)CheckGroundType());
        GameController.instance.audioManager.PlayGenericSound(jumpSound, gameObject); //, "TerrainType", (float)CheckGroundType());
    }

    public void PlayTakeHatSound()
    {
        GameController.instance.audioManager.PlayGenericSound(takeHat, gameObject);
    }

    public void PlayLandingSound()
    {
        //Debug.Log(lastFallingSpeed);
        /*SoundParameters[] parameters = new SoundParameters[2];
        parameters[0] = new SoundParameters("TerrainType", (float)CheckGroundType());
        parameters[1] = new SoundParameters("LandingSpeed", Mathf.Clamp01(Mathf.Abs(lastFallingSpeed) / maxFallingVelocity));*/
        //FMODUnity.RuntimeManager.StudioSystem.setParameterByName("TerrainType", (float)CheckGroundType());
        GameController.instance.audioManager.PlayGenericSound(landingSound, gameObject, "LandingSpeed", Mathf.Clamp01(Mathf.Abs(lastFallingSpeed) / maxFallingVelocity));
        CheckNoisyGround();
    }

    public void PlaySlidingSound()
    {
        GameController.instance.audioManager.PlayGenericSound(slidingSound, gameObject);
    }

    public void PlayStopSlidingSound()
    {
        GameController.instance.audioManager.PlayGenericSound(stopSlidingSound, gameObject);
    }

    public void PlayLandingVFX()
    {
        landingVFXParticle.Play();
    }

    public void GetScaredByWizard()
    {
        anim.SetTrigger("ScaredByWizard");
    }

    //private IEnumerator WaitJumpFromHat()
    //{
    //    float time = 0;
    //    canJumpFromHat = false;
    //    while (time < waitBeforeCanJumpFromHat)
    //    {
    //        yield return new WaitForEndOfFrame();
    //        time += Time.deltaTime;
    //    }
    //    canJumpFromHat = true;
    //    waitJumpFromHatRoutine = null;
    //}

    private IEnumerator GlideDelayed()
    {
        float time = 0;
        while (time < glideDelay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        gravity = false;
        //Debug.Log("plano");
        myRB.velocity = Vector3.zero;
        myRB.AddForce(GameController.instance.Down * glideVelocity, ForceMode.VelocityChange);
        waitBeforeGlideRoutine = null;
    }

    private IEnumerator BullHatTime(bool sustain)
    {
        float time = 0;
        ChangeTimeScale(0);
        while (time <= bullHatTimeAttackDuration)
        {
            yield return new WaitForEndOfFrame();
            if (!GameController.instance.tutorialManager.TimeStopped && GameController.instance.ActualState != GameState.Pause)
            {
                time += Time.unscaledDeltaTime;
                float timeScaleValue = bullHatTimeAttackCurve.Evaluate(time / bullHatTimeAttackDuration);
                if(sustain)
                {
                    timeScaleValue = (timeScaleValue * (1 - jumpWhileAimingMinTimeScale)) + jumpWhileAimingMinTimeScale;
                }
                ChangeTimeScale(timeScaleValue);
            }   
            if(isDead)
            {
                break;
            }
        }
        if (!sustain)
        {
            time = 0;
            while (time <= bullHatTimeReleaseDuration)
            {
                yield return new WaitForEndOfFrame();
                if (!GameController.instance.tutorialManager.TimeStopped && GameController.instance.ActualState != GameState.Pause)
                {
                    time += Time.unscaledDeltaTime;
                    ChangeTimeScale(bullHatTimeReleaseCurve.Evaluate(time / bullHatTimeReleaseDuration));
                }
                if (isDead)
                {
                    break;
                }
            }
            ChangeTimeScale(1);
            //myRB.isKinematic = false;
            //myRB.velocity = velocityBeforeHatTime;
            isInBullHatTime = false;
            bullHatTimeRoutine = null;
        }
    }

    private void ChangeTimeScale(float amount)
    {
        GameController.instance.ScaleTime(amount);
        Time.fixedDeltaTime = startFixedDeltaTime * ((amount == 0) ? 0.001f : amount);
    }

    private IEnumerator TeleportDelayed()
    {
        float time = 0;
        while (time <= teleportDelay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        canTeleport = true;
        //Debug.Log("plano");
        waitBeforeTeleportRoutine = null;
    }

    private IEnumerator RenableHatShoot()
    {
        float time = 0;
        while (time <= delayBetweenHatShoots)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        //while (!buttonReleasedAfterTeleport)
        //    yield return new WaitForEndOfFrame();
        canShoot = true;
        //Debug.Log("plano");
        shootsDelayRoutine = null;
    }

    private IEnumerator DelayedKillCoroutine(float delay)
    {
        float time = 0;
        while (time < delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        Kill();
    }

    private IEnumerator DelayedMovementRelease(float delay, bool isStart)
    {
        float time = 0;
        while (time < delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        ReleaseMovement();

        if (isStart)
        {
            GameController.instance.inputManager.DirectionBlock(false);
            StartCoroutine(ScarySound());
            //Debug.Log("cambio started nel release movement");
            started = true;
        }
    }

    private IEnumerator ScarySound()
    {
        float time = 5;
        while (gameObject.activeSelf)
        {
            TryToTriggerAlternateIdle();
            yield return new WaitForSeconds(time);
        }
    }

    private IEnumerator ExplosionRoutine(float delay)
    {
        float time = 0;
        hatLightRenderer.enabled = true;
        Color color;
        while (time <= delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
            color = new Color(1, 0, 0, timerCurve.Evaluate(time / delay));
            if(teleportHat.gameObject.activeSelf)
            {
                teleportHat.ChangeLightColor(color);
            }
            else
            {
                hatLightRenderer.color = color;
            }
        }
        explosionRoutine = null;
        Explosion();
    }

    private IEnumerator FreezeAfterLaunch()
    {
        gravity = false;
        velocityBeforeFreeze = myRB.velocity;
        //Debug.Log(myRB.velocity);
        float velocityX = 0f;
        //if (movingRight)
        //    velocityX = currentSpeed;
        //if (movingLeft)
        //    velocityX = -currentSpeed;
        Vector3 freezeVelocity = new Vector3(velocityX, 0f, 0f);
        myRB.velocity = freezeVelocity;
        isMovementBlocked = true;
        isAimBlocked = true;
        float time = 0;
        while (time <= freezeAfterLaunchDuration)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
            //myRB.velocity = Vector3.Lerp(freezeVelocity, velocityBeforeFreeze, freezeVelocityCurve.Evaluate(time / freezeAfterLaunchDuration));
            //Debug.Log("freeze vel: " + myRB.velocity);
        }
        gravity = true;
        //myRB.velocity = velocityBeforeFreeze;
        isMovementBlocked = false;
        isAimBlocked = false;
        freezeAfterLaunchRoutine = null;
    }

    private IEnumerator EndSlidingDelayed(float delay)
    {
        float time = 0;
        while (time <= delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        GameController.instance.inputManager.StopScriptedMovement();
        isThrowBlocked = false;
        isRetrieveBlocked = false;
        isTeleportBlocked = false;
        isJumpBlocked = false;
    }

    private IEnumerator SlidingWaitForGrounded(bool right)
    {
        while(!isGrounded)
        {
            yield return new WaitForEndOfFrame();
        }
        GameController.instance.inputManager.ScriptedMovement(right ? 1 : -1);
        anim.SetBool("IsSliding", true);
        if (!vfxSlidingIsPlaying)
        {
            slidingVFXParticle.Play();
            vfxSlidingIsPlaying = true;
        }
        PlaySlidingSound();
        startSlidingRoutine = null;
    }

    public void QuakeReactionAnimation()
    {
        anim.SetTrigger("Quake");
    }

    public void SetRight(bool value)
    {
        right = !value;
        prevRight = value;
        //ManageRigMirroring();
    }

    public void ShakeHat(bool start)
    {
        if(start)
        {
            if (!vfxShadowSmokeIsPlaying)
            {
                //vfxShadowSmoke.Play();
                vfxShadowSmoke.gameObject.SetActive(true);
                vfxShadowSmokeIsPlaying = true;
            }
            anim.SetTrigger("HatSurprise");
            if(hatShakingCorountine != null)
            {
                StopCoroutine(hatShakingCorountine);
                hatShakingCorountine = null;
            }

            stopShakingHat = false;
            hatShakingCorountine = StartCoroutine(HatShaking());
        }
        else
        {
            stopShakingHat = true;
        }
    }

    private IEnumerator HatShaking()
    {
        yield return new WaitUntil(() => hat.transform.localPosition.x == hatX);
        float shakingTime = 0f;
        while(!stopShakingHat)
        {
            hat.transform.localPosition = new Vector3(hatX + hatShakingAmount * hatShakingCurve.Evaluate(Mathf.Repeat(shakingTime / hatShakingPeriod,1f)), hat.transform.localPosition.y, hat.transform.localPosition.z);
            yield return new WaitForEndOfFrame();
            shakingTime += Time.deltaTime;
        }

        hat.transform.localPosition = new Vector3(hatX, hat.transform.localPosition.y, hat.transform.localPosition.z);
        hatShakingCorountine = null;
    }

    public void ShadowThrow(Transform target)
    {
        shadowThrow = true;
        teleportHat.shadowAttractionPoint = target;
        if (teleportHat.gameObject.activeSelf)
        {
            ReturnHat();
        }
    }

    public void ForcedLaunch()
    {
        if (teleportHat.gameObject.activeSelf == false)
        {
            //forcedLaunch = true;
            hat.SetActive(false);
            PlayHatThrowSound();
            teleportHat.transform.position = teleportShootPoint.position;
            teleportHat.AssignVelocity(teleportVelocityV3);
            teleportHat.gameObject.SetActive(true);
            ShakeHat(false);

            if (vfxShadowSmokeIsPlaying)
            {
                //vfxShadowSmoke.Stop();
                vfxShadowSmoke.gameObject.SetActive(false);
                vfxShadowSmokeIsPlaying = false;
            }
        }
    }

    public void ReturnHat()
    {
        teleportHat.gameObject.SetActive(false);
        onCancel.Invoke();
        if (waitBeforeTeleportRoutine != null)
            StopCoroutine(waitBeforeTeleportRoutine);
        waitBeforeTeleportRoutine = null;
        canTeleport = false;

    }

    public void ChangeIdle(bool value)
    {
        changeIdle = value;
    }
}
