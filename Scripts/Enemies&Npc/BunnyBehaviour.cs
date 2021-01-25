using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

[ExecuteInEditMode]
public class BunnyBehaviour : MonoBehaviour {
    public float delayBeforeMoving;
    public float delayBeforeJumping;
    public float delayBeforeDisappear;
    public float delayBeforeOnCapture;
    public float jumpForce;
    public Transform jumpDirection;
    public Vector2 glideVelocity = new Vector2(1,-1);
    //public Transform startPos;
    public Transform destination;
    public Transform cameraPosition;
    public float movementSpeed;
    public bool hasToJumpAtEnd;
    public bool hasToGlide;
    public bool hasToDisappear = true;
    public bool hasToStopCamera;
    public bool hasToFreeCamera;
    public bool hastToReset;
    public int minIdleIndex;
    public int maxIdleIndex;
    public UnityEvent onCapture;
    public UnityEvent onReset;
    [EventRef] public string bunnySound;
    [EventRef] public string bunnyStepSound;
    [EventRef] public string bunnyFoundMusic;
    [EventRef] public string bunnyCapturedSound;
    [EventRef] public string bunnyGlideSound;

    public bool Enabled
    {
        set
        {
            enabled = value;
            renderer.enabled = enabled;
            collider.enabled = enabled;
            myrb.isKinematic = !enabled;
        }
        get { return enabled; }
    }

    private Rigidbody myrb;
    private Animator anim;
    private new Collider collider;
    private new Renderer renderer;
    private Vector3 startPos;
    private Quaternion startRot;
    private bool jumped;
    private bool isGliding;
    private bool enabled;
    private EventInstance soundinstance;

    public void Awake()
    {
        renderer = GetComponent<Renderer>();
        collider = GetComponent<Collider>();
        myrb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start () {
        startPos = transform.position;
        startRot = transform.rotation;
        jumped = false;
        isGliding = false;
        if (Application.isPlaying)
        {
            //GameController.instance.AddBunnyTR(this);

            GameController.instance.AddOnNewGameDelegate(ResetToBeginning);
            GameController.instance.AddOnNewGameDelegate(ResetToStart);
            if (hastToReset)
            {
                GameController.instance.AddOnDeathDelegate(ResetToStart);
                //GameController.instance.AddBunny(this);
            }
            GameController.instance.AddOnSaveStateDelegate(SaveState);
            GameController.instance.AddOnLoadStateDelegate(LoadState);
        }
	}
	
	// Update is called once per frame
	void Update () {
		if(!Application.isPlaying && hasToJumpAtEnd)
        {
            Vector3 pos = destination.position;
            Vector3 vel = jumpDirection.up * jumpForce;
            Vector3 lastPos = pos;
            int i = 1;
            i++;

            while (i < 1000)
            {
                vel = vel + Physics.gravity * Time.fixedDeltaTime;
                pos = pos + vel * Time.fixedDeltaTime;
                pos.z = 0;
                if (Physics.Raycast(lastPos, (pos - lastPos).normalized, (pos - lastPos).magnitude, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    break;
                }
                Debug.DrawLine(lastPos, pos, Color.yellow, Time.deltaTime);
                //line.SetPosition(i, new Vector3(pos.x, pos.y, 0));
                //line.SetPosition(i, new Vector3(pos.x, pos.y, 0));
                lastPos = pos;
                i++;
            }
        }
        //float volume;
        //float pitch;
        //int time;
        //soundinstance.getVolume(out volume);
        //soundinstance.getPitch(out pitch);
        //soundinstance.getTimelinePosition(out time);
        //Debug.Log(volume);
        //Debug.Log(pitch);
        //Debug.Log(time);
    }

    private void FixedUpdate()
    {
        if(jumped && hasToGlide && !isGliding && myrb.velocity.y < 0)
        {
            myrb.useGravity = false;
            myrb.velocity = Vector3.zero;
            myrb.velocity = new Vector3(glideVelocity.x, glideVelocity.y, 0);
            isGliding = true;
            anim.SetBool("IsGliding", true);
            PlayBunnyGlideSound();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.tag == "Teleport")
        {
            renderer.enabled = false;
            collider.enabled = false;
            myrb.isKinematic = true;
            onCapture.Invoke();
            PlayBunnyCapturedSound();
        }
    }

    public void StartMoving()
    {
        StartCoroutine(Move());
    }

    public void ResetToStart()
    {
        Enabled = true;
        jumped = false;
        isGliding = false;
        onReset.Invoke();
    }

    public void ResetToBeginning()
    {
        Enabled = true;
        isGliding = false;
        jumped = false;
        myrb.velocity = Vector3.zero;
        anim.Rebind();
        transform.position = startPos;
        transform.rotation = startRot;
        StopAllCoroutines();
    }

    public void SaveState()
    {
        bool state = Enabled;
        if (GameController.instance.SaveData.bunniesActiveStates.ContainsId(gameObject.GetInstanceID()))
            GameController.instance.SaveData.bunniesActiveStates.Set(gameObject.GetInstanceID(), state);
        else
            GameController.instance.SaveData.bunniesActiveStates.Add(gameObject.GetInstanceID(), gameObject.name, state);
    }

    public void LoadState()
    {
        bool state = Enabled;
        state = GameController.instance.SaveData.bunniesActiveStates.Get(gameObject.GetInstanceID());
        Enabled = state;
        if(enabled)
        {
            ResetToBeginning();
        }
    }

    //public void SetJumpVelocity(Vector3 velocity)
    //{

    //}

    public void Jump()
    {
        myrb.velocity = Vector3.zero;
        myrb.velocity = jumpDirection.up * jumpForce;
        jumped = true;
        //float num = Physics.gravity.magnitude * Mathf.Pow(x, 2);
        //float den = 2f * Mathf.Pow(Mathf.Cos(spawnAngleHeight * Mathf.Deg2Rad), 2) * (x * Mathf.Tan(spawnAngleHeight * Mathf.Deg2Rad) - y);
        //float velocityMagnitude = Mathf.Sqrt(num / den);
        //Vector3 velocity = new Vector3(velocityMagnitude * Mathf.Cos(spawnAngleHeight * Mathf.Deg2Rad), velocityMagnitude * Mathf.Sin(spawnAngleHeight * Mathf.Deg2Rad), 0);
    }

    private IEnumerator Move()
    {
        if (hasToStopCamera)
        {
            if (cameraPosition != null)
                GameController.instance.mainCamera.StopFollowing(new Vector2(cameraPosition.position.x, cameraPosition.position.y));
            else
                GameController.instance.mainCamera.StopFollowing();
            GameController.instance.mainCamera.BlockPlayer(true);
        }
        PlayBunnyFoundMusic();
        float time = 0;
        while (time < delayBeforeMoving)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        anim.SetBool("IsMoving", true);
        
        //time = 0;
        bool isRight = (transform.position.x < destination.position.x) ;
        //float movementDuration = Vector3.Distance(startPos.position, endPos.position) / movementSpeed;
        while ((isRight && transform.position.x < destination.position.x) || (!isRight && transform.position.x > destination.position.x))
        {
            yield return new WaitForFixedUpdate();
            //time += Time.deltaTime;
            myrb.position += ((isRight)? Vector3.right : Vector3.left) * movementSpeed * Time.fixedDeltaTime;
        }
        anim.SetBool("IsMoving", false);
        if (hasToJumpAtEnd)
        {
            time = 0;
            while (time < delayBeforeJumping)
            {
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
            }
            PlayBunnySound();
            Jump();
        }
        time = 0;
        if (hasToDisappear)
        {
            while (time < delayBeforeDisappear)
            {
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
            }
            GameController.instance.audioManager.DestroyInstanceFaded(soundinstance);
            Enabled = false;
        }
        if (hasToStopCamera && hasToFreeCamera)
        {
            GameController.instance.mainCamera.Follow();
            GameController.instance.mainCamera.BlockPlayer(false);
        }
    }

    public void SetIdle()
    {
        float random = Random.Range(minIdleIndex, maxIdleIndex + 1);
        anim.SetFloat("IdleIndex", random);
        //Debug.Log(random);
    }

    public void PlayBunnySound()
    {
        GameController.instance.audioManager.PlayGenericSound(bunnySound, gameObject);
    }

    public void PlayBunnyStepSound()
    {
        GameController.instance.audioManager.PlayGenericSound(bunnyStepSound, gameObject);
    }

    public void PlayBunnyFoundMusic()
    {
        if (bunnyFoundMusic != "")
        {
            GameController.instance.audioManager.ChangeMusic(bunnyFoundMusic, false);
        }
    }

    public void PlayBunnyCapturedSound()
    {
        GameController.instance.audioManager.PlayGenericSound(bunnyCapturedSound, gameObject);
    }

    public void PlayBunnyGlideSound()
    {
        soundinstance = GameController.instance.audioManager.PlayGenericSound(bunnyGlideSound, gameObject, new SoundParameters[] { new SoundParameters("Speed",1), new SoundParameters("Distance",0) });
    }
}
