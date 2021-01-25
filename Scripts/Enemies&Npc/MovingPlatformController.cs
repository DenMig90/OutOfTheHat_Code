using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;

public enum MovementType
{
    Linear,
    Loop,
    PingPong
}

public class ToMove
{
    public Rigidbody rb;
    public Collider collider;
    public bool justAdded = true;
}

[RequireComponent(typeof(TransportBehaviour))]
public class MovingPlatformController : MonoBehaviour {

    public Transform[] points;
    //public Transform pointB;
    public float speed=5F;
    public float activationDelay;
    public float returnDelay;
    public MovementType movementType;
    public bool isActive = true;
    public bool hasToReturn = false;
    public bool activableByPlayer;
    public bool activableByHat;
    public bool savePlayer;
    public bool resetCarrotOnSave = false;
    public ActualPlatformBehaviour actualPlatform;
    public int maxPlatforms = 3;
    public float maxDistanceFromPlayer = 50;
    public bool bounce = false;
    public float bounceTime = 1f;
    public AnimationCurve bounceCurve;
    public float bounceAmount = 1f;
    public float bounceMaxSpeed = 5f;
    public LayerMask bounceCollisionLayer;
    public float bounceVelocityMax = 10f;
    public float bounceMinDelay = 1f;
    public Animator spiderAnim;
    public int altAnimationProb = 25;
    public float savingPlatformDistance = 2f;
    [EventRef] public string movingSound;
    [EventRef] public string additionalMovingSound;
    public GameObject platformSoundEmitterTransform;

    public Vector3 ActualPointPosition
    {
        get { return points[actualPoint].position; }
    }

    //private bool toPointB=false;
    private Collider col;
    private int actualPoint;
    private int increment;
    private Vector3 movement;
    private Vector3 startPos;
    [HideInInspector]
    public bool startActive;
    [SerializeField]
    private bool soundAllowed = true;
    //[SerializeField]
    private Coroutine activationRoutine;
    private Coroutine returnRoutine;
    private EventInstance soundInstance;
    private EventInstance additionalSoundInstance;
    private ActualPlatformBehaviour[] actualPlatforms;
    private Coroutine bouncingCoroutine=null;
    private float lastBounceTime;
    private Vector3 bouncingPosition = Vector3.zero;
    private Vector3 posNoBounce;
    private Vector3 lastBouncePosition = Vector3.zero;
    //private float lastBouncePosition = 0f;

    public bool IsActive
    {
        set
        {
            isActive = value;
            if (spiderAnim != null && spiderAnim.gameObject.activeSelf)
                spiderAnim.SetBool("IsMoving", isActive);
            if(isActive)
            {
                //CheckPlayerDistance();
                PlayMovingSound();
            }
            else
            {
                StopMovingSound();
            }
        }
        get
        {
            return isActive;
        }
    }

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    // Use this for initialization
    void Start () {
        startPos = transform.position;
        posNoBounce = transform.position;
        //mainActive = isActive;
        startActive = isActive;
        actualPoint = 0;
        increment = 1;
        //GameController.instance.AddMovingPlatform(this);
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        activationRoutine = null;
        returnRoutine = null;
        //CreateMovingSound();
        //PlayMovingSound();
        //CheckPlayerDistance();
        //soundAllowed = true;
        IsActive = isActive;
        if (startActive && hasToReturn)
            hasToReturn = false;


        if (actualPlatform)
        {
            actualPlatform.target = this;
            if(maxPlatforms>1)
            {
                actualPlatforms = new ActualPlatformBehaviour[maxPlatforms];
                actualPlatforms[0] = actualPlatform;
                for(int i =1; i<maxPlatforms; i++)
                {
                    ActualPlatformBehaviour p = GameObject.Instantiate(actualPlatform, transform.parent);
                    p.gameObject.SetActive(false);
                    actualPlatforms[i] = p;
                }
            }
        }

        if(spiderAnim != null)
        {
            StartCoroutine(AltAnimationRoutine());
        }
    }

    private void Update()
    {
        if (IsActive)
            CheckPlayerDistance();

        if (platformSoundEmitterTransform != null)
        {
            Vector3 closestPoint = col.bounds.ClosestPoint(GameController.instance.player.transform.position);
            platformSoundEmitterTransform.transform.position = new Vector3(closestPoint.x, closestPoint.y, 0);
        }
        //PLAYBACK_STATE state;
        //soundInstance.getPlaybackState(out state);
        //Debug.Log(state.ToString());
        //RuntimeManager.AttachInstanceToGameObject(soundInstance, GetComponent<Transform>(), GetComponent<Rigidbody>());
#if UNITY_EDITOR
        if (startActive && hasToReturn)
            hasToReturn = false;
#endif
    }

    // Update is called once per frame
    void FixedUpdate () {

        if (IsActive)
        {
            Vector3 direction;
            direction = (points[actualPoint].position - posNoBounce);

            if (direction.magnitude < speed * Time.fixedDeltaTime)
            {
                if (movementType == MovementType.Linear && actualPoint == points.Length - 1)
                {
                    StopMovingSound();
                    return;
                }
                if (movementType == MovementType.PingPong)
                {
                    if (actualPoint >= points.Length - 1)
                    {

                        increment = -1;
                    }
                    else if (actualPoint <= 0)
                    {
                        increment = 1;
                    }
                }
                actualPoint += increment;
                if (movementType == MovementType.Loop)
                {
                    if (actualPoint < 0)
                        actualPoint = points.Length - 1;
                    else if (actualPoint > points.Length - 1)
                        actualPoint = 0;
                }
            }
            direction = direction.normalized;
            movement = direction * speed * Time.fixedDeltaTime;
            posNoBounce += movement;
            Vector3 myBouncingPosition = bouncingPosition;
            if(Vector3.Distance(myBouncingPosition,lastBouncePosition)>bounceMaxSpeed * Time.fixedDeltaTime)
            {
                myBouncingPosition = lastBouncePosition + (myBouncingPosition - lastBouncePosition).normalized * bounceMaxSpeed * Time.fixedDeltaTime;
            }
            transform.position = posNoBounce + myBouncingPosition;

            lastBouncePosition = myBouncingPosition;
        }
	}

    public void ResetToStart()
    {
        transform.position = startPos;
        posNoBounce = transform.position;
        IsActive = startActive;
        if (activationRoutine != null)
        {
            StopCoroutine(activationRoutine);
            activationRoutine = null;
        }
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }
        actualPoint = 0;
        increment = 1;
    }
    
    private void OnTriggerEnter(Collider col)
    {
        if (((col.tag == "Player" && activableByPlayer) || (col.tag == "Teleport" && activableByHat)))
        {
            Activate();
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (((col.tag == "Player" && activableByPlayer) || (col.tag == "Teleport" && activableByHat)) && hasToReturn && IsActive && !startActive)
        {
            Return();
        }
    }

   /* private void OnCollisionEnter(Collision collision)
    {
        if(bounce)
        {
            if(collision.collider.tag == "Player")
            {
                float vel = -(Vector3.Project(collision.relativeVelocity, transform.up).y);
                Debug.Log(vel);
                if (vel > 0)
                {
                    if (bouncingCoroutine != null)
                    {
                        StopCoroutine(bouncingCoroutine);
                    }
                    vel = Mathf.Clamp01(vel / bounceVelocityMax);
                    bouncingCoroutine = StartCoroutine(Bounce(vel));
                }
            }
        }
    } */

    public void TryBouncing()
    {
        if(bounce)
        {
            float vel = -GameController.instance.player.GetComponent<Rigidbody>().velocity.y;
            if (vel > 0)
            {
                if (bouncingCoroutine == null)
                {
                    vel = Mathf.Clamp01(vel / bounceVelocityMax);
                    bouncingCoroutine = StartCoroutine(Bounce(vel));
                }
            }
        }
    }

    private IEnumerator Bounce(float amount)
    {
        bouncingPosition = Vector3.zero;
        if (Time.time > lastBounceTime + bounceMinDelay)
        {
            lastBounceTime = Time.time;

            float startTime = Time.timeSinceLevelLoad;
            float evaluateOffset = 0f;
            while ((Time.timeSinceLevelLoad - startTime) + evaluateOffset < bounceTime)
            {
                float curveEvaluation = bounceCurve.Evaluate(((Time.timeSinceLevelLoad - startTime) / bounceTime) + evaluateOffset);

                if (!Physics.BoxCast(col.bounds.center, col.bounds.extents, -transform.up, Quaternion.identity, bounceAmount * curveEvaluation, bounceCollisionLayer))
                {
                    //transform.position -= transform.up * bounceAmount * (curveEvaluation - lastBouncePosition);
                    //float magnitude = Mathf.Clamp(,-bounceMaxSpeed * Time.deltaTime, bounceMaxSpeed*Time.deltaTime);

                    bouncingPosition = -transform.up * bounceAmount * curveEvaluation;

                    Debug.Log(bouncingPosition);
                }

                yield return null;
            }

            //transform.position += transform.up * lastBouncePosition;
            bouncingPosition = Vector3.zero;

            bouncingCoroutine = null;
        }
    }

    private IEnumerator AltAnimationRoutine()
    {
        float time = 5;
        while (gameObject.activeSelf)
        {
            TryToTriggerAlternateAnimation();
            yield return new WaitForSeconds(time);
        }
    }

    public void TryToTriggerAlternateAnimation()
    {
        if (Random.Range(0, 101) <= altAnimationProb)
        {
            spiderAnim.SetTrigger("AltMoving");
        }
    }

    public void Activate()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
            IsActive = true;
        }
        if (!IsActive && activationRoutine == null)
            activationRoutine = StartCoroutine(Activation());
    }

    public void Return()
    {
        if (returnRoutine == null)
            returnRoutine = StartCoroutine(ReturnRoutine());
    }

    private void CheckPlayerDistance()
    {
        if (Vector3.Distance(GameController.instance.player.transform.position, transform.position) < maxDistanceFromPlayer)
        {
            if (!soundAllowed)
            {
                PlayMovingSound();
                soundAllowed = true;
            }
        }
        else
        {
            if (soundAllowed)
            {
                soundAllowed = false;
                StopMovingSound();
            }
        }
    }

    //public void CreateMovingSound()
    //{
    //    //soundInstance = GameController.instance.audioManager.PlayLoopSound(movingSound, platformCameraPlaneProjection);
    //    GameController.instance.audioManager.DestroyInstance(soundInstance);
    //    soundInstance = GameController.instance.audioManager.PlayLoopSound(movingSound, gameObject, "Platform", 1);
    //}

    public void PlayMovingSound()
    {
        //Debug.Log("PlayMovingSound");

        //GameController.instance.audioManager.ChangeInstanceParameter(soundInstance, "Platform", 1);
        //soundInstance.start();
        //GameController.instance.audioManager.ChangeInstanceParameter(soundInstance, "Platform", 1);
        GameController.instance.audioManager.DestroyInstance(soundInstance);
        soundInstance = GameController.instance.audioManager.PlayLoopSound(movingSound, platformSoundEmitterTransform, "Platform", 1);
        if (spiderAnim != null)
        {
            GameController.instance.audioManager.DestroyInstance(additionalSoundInstance);
            additionalSoundInstance = GameController.instance.audioManager.PlayLoopSound(additionalMovingSound, spiderAnim.gameObject, "Platform", 1);
        }
        //soundInstance.start();
        soundAllowed = true;
        //Debug.Log("suona");
    }

    public void StopMovingSound()
    {
        //Debug.Log("StopMovingSound");
        //GameController.instance.audioManager.ChangeInstanceParameter(soundInstance, "Platform", 0);
        soundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        if (spiderAnim != null)
        {
            additionalSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
            soundAllowed = false;
        //Debug.Log("stop");
    }

    private IEnumerator Activation()
    {
        float time = 0;
        while(time < activationDelay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        IsActive = true;
        activationRoutine = null;
    }

    private IEnumerator ReturnRoutine()
    {
        float time = 0;
        while (time < returnDelay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        isActive = false;

        Vector3 direction;

        do
        {
            direction = (points[0].position - posNoBounce);
            Vector3 dirnorm = direction.normalized;
            movement = dirnorm * speed * Time.fixedDeltaTime;
            posNoBounce += movement;
            transform.position = posNoBounce + bouncingPosition;

            yield return new WaitForFixedUpdate();
        } while (direction.magnitude >= speed * Time.fixedDeltaTime);

        StopMovingSound();
        returnRoutine = null;
        actualPoint = 0;
    }

    public bool SavePlayer()
    {
        if (!savePlayer)
            return false;

        if(!actualPlatform)
        {
            Debug.LogWarning("Settare la actual platform!");
        }

        

        for (int i=0; i<maxPlatforms; i++)
        {
            if(!actualPlatforms[i].gameObject.activeSelf)
            {

                ActualPlatformBehaviour newPlatform = actualPlatforms[i];
                // L'ordine di queste righe è importante
                newPlatform.startPosition = actualPlatform.startPosition;
                newPlatform.gameObject.SetActive(true);
                newPlatform.transform.position = GameController.instance.player.transform.position + (GameController.instance.player.GetComponent<Rigidbody>().velocity * Time.deltaTime) - GameController.instance.Up * savingPlatformDistance;
                newPlatform.SetIsActive(true);
                    //GameObject.Instantiate(actualPlatform, GameController.instance.player.transform.position + (GameController.instance.player.GetComponent<Rigidbody>().velocity * Time.deltaTime), Quaternion.identity);
                actualPlatform.Die();
                actualPlatform = newPlatform;
                newPlatform.target = this;
                newPlatform.OnPlayerSaving();
                if (resetCarrotOnSave)
                {
                    transform.position = startPos;
                    posNoBounce = transform.position;
                    actualPoint = 0;
                    increment = 1;
                }
                return true;
            }
        }
        return false;
    }

    public void EnableAllActualPlatforms(bool value)
    {
        foreach(ActualPlatformBehaviour aPB in actualPlatforms)
        {
            if(aPB)
                aPB.enabled = value;
        }
    }



    public void SetIsActiveAllActualPlatforms(bool value)
    {
        foreach (ActualPlatformBehaviour aPB in actualPlatforms)
        {
            if (aPB)
                aPB.SetIsActiveNotRecursive(value);
        }
    }
}
