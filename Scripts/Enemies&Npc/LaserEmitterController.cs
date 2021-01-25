using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class LaserEmitterController : MonoBehaviour
{
    public TrapBehaviour trap;
    public float onTime;
    public float offTime;
    public float disabledTime = 2f;
    public float distanceToOpenMouth;
    public float distanceParallelToOpenMultiplier = 2f;
    public float laserSpeed = 10f;
    public float maxLaserLimitStepLenght = 10f;
    public float maxLaserLenght = 100f;
    public Animator anim;
    public Animator eyelidAnim;
    public SpriteRenderer coverRenderer;
    public ParticleSystem hitParticle;
    public ParticleSystem mouthParticle;
    public LayerMask mask;
    public LayerMask viewMask;
    public float maxDistanceFromPlayerToSound = 50;
    public EyeBrambleScriptableObject brambleData;
    [EventRef] public string beamSound;
    [EventRef] public string openMouthSound;

    public LineRenderer view;

    //public FieldOfViewBehaviour fieldOfView;

    public UnityEvent onPlayerEnterMouthRange;
    public UnityEvent onPlayerExitMouthRange;

    //public bool debug = false;

    [SerializeField]
    private bool isActive = true;
    private EventInstance soundInstance;
    private bool soundAllowed = true;
    private bool nearPlayer;
    private new Collider collider;
    private Coroutine timerRoutine;
    private Coroutine disableRoutine;
    private bool isPlayerInRange = false;
    private float distBPoint;
    [SerializeField]
    private bool IsOpen = false;
    private bool prevIsOpen = false;
    private bool playerInMouthRange = false;
    private GameObject beamEmitter;

    private bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            isActive = value;
            if (isActive)
            {
                if (view != null)
                {
                    view.SetPosition(1, view.GetPosition(0));
                    view.SetPosition(2, view.GetPosition(0));
                    view.SetPosition(3, view.GetPosition(0));
                }
                PlayBeamSound();
                CheckPlayerDistance();
            }
            else
                mouthParticle.gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        collider = GetComponent<Collider>();
        beamEmitter = new GameObject("BeamEmitter");
        beamEmitter.transform.parent = transform;
        beamEmitter.transform.localPosition = Vector3.zero;
    }

    // Use this for initialization
    void OnEnable()
    {
        //anim.SetBool("EyeOpened", true);
        //if (fieldOfView != null)
        //    fieldOfView.Init();
        //IsActive = true;
        //anim.SetBool("EyeOpened", true);
        IsOpen = true;
    }

    private void Start()
    {
        disableRoutine = null;
        //anim.SetBool("EyeOpened", true);
        ResetToStart();
        //CreateBeamSound();
        //IsActive = true;
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);

        if(brambleData != null)
        {
            if(eyelidAnim != null)
            {
                eyelidAnim.runtimeAnimatorController = brambleData.eyeLidAnimController;
            }
            if(coverRenderer!= null)
            {
                coverRenderer.sprite = brambleData.coverSprite;
            }

            if(trap != null)
            {
                if (trap.clawAnim != null)
                {
                    trap.clawAnim.runtimeAnimatorController = brambleData.handAnimController;
                }
                if (trap.armRenderer != null)
                {
                    trap.armRenderer.sprite = brambleData.armSprite;
                }
            }
        }

        distBPoint = Vector3.Distance(transform.position, trap.endPos.position);
        //CheckPlayerDistance();
    }

    public void ResetToStart()
    {
        if (disableRoutine != null)
        {
            StopCoroutine(disableRoutine);
            disableRoutine = null;
        }
        if (timerRoutine == null)
        {
            timerRoutine = StartCoroutine(Timer());
            //anim.SetBool("EyeOpened", true);
            IsOpen = true;
        }
        //fieldOfView.DrawFieldOfView();
        CheckPlayerDistance();
    }

    // Update is called once per frame
    void Update()
    {
        if (view != null)
        {
            view.enabled = IsActive && (trap == null || trap.canTrigger);
            //if(!view.enabled)
            //{
            //    hitParticle.gameObject.SetActive(false);
            //}
        }

        bool playerNowInRange = CheckPlayerInRange(false) || (trap != null && !trap.canTrigger);
        //if(debug)
        //    Debug.Log(playerNowInRange);
        if (playerNowInRange != isPlayerInRange || IsOpen != prevIsOpen)
        {
            if (playerNowInRange)
            {
                if (eyelidAnim != null)
                {
                    eyelidAnim.SetBool("EyeOpened", IsOpen);
                }
            }
            else
            {
                if (eyelidAnim != null)
                {
                    eyelidAnim.SetBool("EyeOpened", false);
                }
            }
        }
        isPlayerInRange = playerNowInRange;
        prevIsOpen = IsOpen;
        if (trap != null)
            trap.canUpdate = isPlayerInRange;

        bool nowPlayerInMouthRange = CheckPlayerInRange(true) && IsActive || (trap != null && !trap.canTrigger); // da fare;
        if (nowPlayerInMouthRange)
        {
            if (!playerInMouthRange)
            {
                anim.SetBool("MouthOpened", true);
                PlayMouthOpenSound();
                mouthParticle.gameObject.SetActive(IsActive);
                onPlayerEnterMouthRange.Invoke();
            }
        }
        else
        {
            if (playerInMouthRange)
            {
                anim.SetBool("MouthOpened", false);
                mouthParticle.gameObject.SetActive(false);
                onPlayerExitMouthRange.Invoke();
            }
        }
        playerInMouthRange = nowPlayerInMouthRange;

        //collider.enabled = trap.PlayerCaught;

        if (IsActive)
        {

            RaycastHit hit;
            //if (line != null)
            //    line.SetPosition(0, transform.position);

            if (Physics.Raycast(transform.position, transform.right, out hit, float.MaxValue, mask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.tag == "Player" && Application.isPlaying && !GameController.instance.player.IsHidden)
                {
                    //if(line.material.color != Color.red)
                    //    line.material.color = Color.red;
                    if (trap != null && trap.canTrigger)
                        trap.Trigger();
                }
                //if (line != null)
                //    line.SetPosition(1, hit.point);
            }

            CheckPlayerDistance();
            //else
            //{
            //    //if (line != null)
            //    //    line.SetPosition(1, transform.position + transform.right * 1000);
            //    //if (line.material.color != Color.green)
            //    //    line.material.color = Color.green;
            //}

        }
    }

    private bool CheckPlayerInRange(bool mouth)
    {
        //float distPlayerX = Mathf.Abs(transform.position.x - GameController.instance.player.transform.position.x);
        //float distPlayerY = Mathf.Abs(transform.position.y - GameController.instance.player.transform.position.y);
        float distPlayerParallel = Mathf.Abs(transform.InverseTransformPoint(GameController.instance.player.transform.position).y);
        float distPlayerPerpendicular = Mathf.Abs(transform.InverseTransformPoint(GameController.instance.player.transform.position).x);

        float distCompParallel = distBPoint * distanceParallelToOpenMultiplier;
        float distCompPerpendicular = distBPoint;
        if(mouth)
        {
            distCompParallel = distanceToOpenMouth;
        }
        if (distPlayerParallel < distCompParallel && distPlayerPerpendicular < distCompPerpendicular)
            return true;
        else
            return false;
    }

    private void LateUpdate()
    {
        if (IsActive && view != null)
        {
            if (isPlayerInRange)
            {
                RaycastHit hit;
                Vector3 destination;
                Vector3 startPoint;
                Vector3 finalPoint;

                if (Physics.Raycast(transform.position, transform.right, out hit, float.MaxValue, viewMask, QueryTriggerInteraction.Ignore))
                {
                    destination = view.transform.InverseTransformPoint(hit.point);
                }
                else
                {
                    destination = view.transform.InverseTransformPoint(transform.position + transform.right * maxLaserLenght);
                }

                if (Vector3.Distance(view.GetPosition(0), destination) > Vector3.Distance(view.GetPosition(0), view.GetPosition(3)))
                {
                        view.SetPosition(3, Vector3.MoveTowards(view.GetPosition(3), destination, laserSpeed * Time.deltaTime));
                }
                else
                {
                    view.SetPosition(3, destination);
                }
                startPoint = view.GetPosition(0);
                finalPoint = view.GetPosition(3);

                Vector3 secondPoint = Vector3.Lerp(startPoint, finalPoint, 0.33f);
                if (Vector3.Distance(startPoint, secondPoint) > maxLaserLimitStepLenght)
                {
                    secondPoint = startPoint + (finalPoint-startPoint).normalized * maxLaserLimitStepLenght;
                }
                view.SetPosition(1, secondPoint);
                Vector3 thirdPoint = Vector3.Lerp(startPoint, finalPoint, 0.66f);
                if (Vector3.Distance(finalPoint, thirdPoint) > maxLaserLimitStepLenght)
                {
                    thirdPoint = finalPoint + (startPoint - finalPoint).normalized * maxLaserLimitStepLenght;
                }
                view.SetPosition(2, thirdPoint);

                if (hitParticle != null)
                {
                    //if (!hitParticle.gameObject.activeSelf)
                    //    hitParticle.gameObject.SetActive(true);
                    if(((view.GetPosition(3) == destination) && trap.canTrigger) && /*hitParticle.isStopped*/ !hitParticle.gameObject.activeSelf)
                    {
                        //hitParticle.Play(true);
                        hitParticle.gameObject.SetActive(true);
                    }
                    else if((view.GetPosition(3) != destination || !trap.canTrigger) && /*hitParticle.isPlaying*/ hitParticle.gameObject.activeSelf)
                    {
                        //hitParticle.Stop(true);
                        //Debug.Log("stoppo");
                        hitParticle.gameObject.SetActive(false);
                    }
                    if (hitParticle.gameObject.activeSelf)
                        hitParticle.transform.position = view.transform.TransformPoint(finalPoint);
                }

                if (soundAllowed)
                {
                    Vector3 start = view.transform.TransformPoint(startPoint);
                    Vector3 end = view.transform.TransformPoint(finalPoint);
                    beamEmitter.transform.position = Vector3.Project((GameController.instance.player.transform.position - start), (end - start)) + start;
                }
            }
        }
        else
        {
            if (hitParticle != null && hitParticle.gameObject.activeSelf)
            {
                hitParticle.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && trap.PlayerCaught)
        {
            //GameController.instance.player.Show(false);
        }
        if (other.gameObject.tag == "Teleport" && GameController.instance.player.teleportHat.IsHatarang)
        {
            Debug.Log("entrato");
            if (disableRoutine == null)
            {
                disableRoutine = StartCoroutine(DisableTemporary());
            }
        }
    }

    private void CheckPlayerDistance()
    {
        if (Vector3.Distance(GameController.instance.player.transform.position, transform.position) < maxDistanceFromPlayerToSound)
        {
            if (!soundAllowed)
            {
                ResumeBeamSound();
            }
            nearPlayer = true;
        }
        else
        {
            if (soundAllowed)
            {
                PauseBeamSound();
            }
            nearPlayer = false;
        }
    }

    //public void CreateBeamSound()
    //{
    //if (beamSound == "")
    //    Debug.LogError(gameObject.name);
    //soundInstance = GameController.instance.audioManager.PlayGenericSound(beamSound, gameObject);
    //soundAllowed = true;
    //}

    public void PlayBeamSound()
    {
        GameController.instance.audioManager.DestroyInstance(soundInstance);
        soundInstance = GameController.instance.audioManager.PlayGenericSound(beamSound, beamEmitter);
        //soundInstance.start();
        soundAllowed = true;
        //Debug.Log("play");
    }

    public void PlayMouthOpenSound()
    {
        GameController.instance.audioManager.PlayGenericSound(openMouthSound, gameObject);
    }

    public void ResumeBeamSound()
    {
        soundInstance.setPaused(false);
        soundAllowed = true;
        //Debug.Log("resume");
    }

    public void PauseBeamSound()
    {
        soundInstance.setPaused(true);
        soundAllowed = true;
        //Debug.Log("pause");
    }

    public void SetIsActive(bool value)
    {
        IsActive = value;
    }

    private IEnumerator Timer()
    {
        while (gameObject.activeSelf && onTime != 0 && offTime != 0)
        {
            if (onTime != 0)
            {
                //IsActive = true;
                //anim.SetBool("EyeOpened", true);
                IsOpen = true;
                //anim.SetBool("EyeOpened", true);
                yield return new WaitForSeconds(onTime);
            }
            yield return new WaitUntil(() => trap.canTrigger);
            if (offTime != 0)
            {
                //anim.SetBool("EyeOpened", false);
                IsOpen = false;
                SetIsActive(false);
                //IsActive = false;
                //anim.SetBool("EyeOpened", false);
                yield return new WaitForSeconds(offTime);
            }
        }
    }

    private IEnumerator DisableTemporary()
    {
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
        }
        timerRoutine = null;
        //anim.SetBool("EyeOpened", false);
        IsOpen = false;
        float time = 0;
        while (time < disabledTime)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        timerRoutine = StartCoroutine(Timer());
        //anim.SetBool("EyeOpened", true);
        IsOpen = true;
        disableRoutine = null;
    }
}
