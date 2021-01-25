using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class HatBehaviour : MonoBehaviour {

    public Renderer mesh;
    public ParticleSystem vfx;
    //public ParticleSystem vfxShadowSmoke;
    //public ParticleSystem vfxShadowTrail;
    public new GameObject light;
    public float collectableDelay = 0;
    public Vector3 targetRotation;
    public float rotationSpeed;
    public AnimateRotation animRot;
    public AnimatePositionSingleAxis animPos;
    public Transform target;
    public LayerMask bohatWater;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    public float explosionDelay = 5;
    [HideInInspector]
    public LayerMask explosionDamageableLayer;
    [HideInInspector]
    public float explosionRange;
    [EventRef] public string collisionSound;
    [EventRef] public string hatFlipSound;
    public float delayBetweenSounds = 0.5f;
    public bool shadowMode = false;
    [LayerSelector]
    public int uncollectableLayer;
    public float uncollectableZ = -5;

    //private bool isBoat;
    //private bool isPlayerOn;
    //private int startLayer;
    private SpriteRenderer lightRenderer;
    private Color lightStartColor;
    private UnityEvent onFlippedLanding=null;
    private bool isUncollectable = false;

    public BoxCollider Collider
    {
        get
        {
            return collider;
        }
    }

    public Rigidbody Rigidbody
    {
        get { return myRB; }
    }

    //public bool IsBoat { get { return isBoat; } }
    public bool HasBomblebee { get { return hasBomblebee; } set { hasBomblebee = value; } }
    public bool IsHatarang { get { return isHatarang; } }

    public GameObject[] objectsToDeactivateAtDeath;
    public Material deathMaterial;

    public float shadowAttractionForce = 20f;
    public Transform shadowAttractionPoint = null;
    public float shadowPositionTolerance = 1f;
    //public float playerJumpForce;

    [SerializeField]
    private new BoxCollider collider;
    [SerializeField]
    private Rigidbody myRB;
    [SerializeField]
    private AnimateScale boatScaleAnimation;
    private Quaternion startRotation;
    private bool isCollectable;
    //private EventInstance soundInstance;
    private RigidbodyConstraints constraints;
    private Coroutine collectableRoutine;
    private Coroutine enableSoundRoutine;
    private Coroutine hatarangRoutine;
    //private Coroutine boatDisableRoutine;
    private bool canSound = true;
    private bool hasBomblebee;
    private bool isHatarang;
    private bool canExplodeOnTouch;
    private bool hatarangCollided;
    private Vector3 startScale;
    private float startBoatAnimFromY;
    private float startBoatAnimToY;
    private Material baseMat;
    private bool isJumping = false;
    private int startLayer;
    private float startZ;
    private bool lastIsCollectable = true;
    private bool vfxIsPlaying = false;
    //private bool vfxSSIsPlaying = false;
    //private bool vfxSTIsPlaying = false;

    //private Vector3 previousPos;
    //private Coroutine routine;
    //private bool rotated;

    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
        myRB = GetComponent<Rigidbody>();
        constraints = myRB.constraints;
        lightRenderer = light.GetComponent<SpriteRenderer>();
        lightStartColor = lightRenderer.color;
        GameController.instance.player.AssignTeleportHat(this);
        GameController.instance.AddOnUpsideDown(CheckUpsideDown);
        startScale = transform.localScale;
        startBoatAnimFromY = boatScaleAnimation.from.y;
        startBoatAnimToY = boatScaleAnimation.to.y;
        baseMat = mesh.material;
        startLayer = gameObject.layer;
        startZ = mesh.transform.localPosition.z;
        //Debug.Log(constraints);
    }

    // Use this for initialization
    void Start()
    {
        startRotation = transform.rotation;
        canSound = true;
        hasBomblebee = false;
        isHatarang = false;
        //gameObject.SetActive(false);
        //boatDisableRoutine = null;
        //startLayer = gameObject.layer;
        //routine = null;
        //rotated = false;
        animPos.onStart.AddListener(PlayHatFlipSound);
        animPos.onFinish.AddListener(OnFlippedLanding);
    }

    private void OnEnable()
    {
        isCollectable = false;
        //isBoat = false;
        //isPlayerOn = false;
        //gameObject.layer = startLayer;
        vfx.Play();
        vfxIsPlaying = true;
        boatScaleAnimation.AnimationReset();
        collider.enabled = false;
        collectableRoutine = StartCoroutine(SetCollectable());
        canExplodeOnTouch = true;
        SetUncollectable(false);
        //rotated = false;
    }

    private void OnDisable()
    {
        //Debug.Log("disablo");
        isHatarang = false;
        myRB.useGravity = true;
        myRB.isKinematic = false;
        myRB.velocity = Vector3.zero;
        myRB.angularVelocity = Vector3.zero;
        if (hatarangRoutine != null)
            StopCoroutine(hatarangRoutine);
        hatarangRoutine = null;
        if (collectableRoutine != null)
            StopCoroutine(collectableRoutine);
        if (enableSoundRoutine != null)
            StopCoroutine(enableSoundRoutine);
        //if (boatDisableRoutine != null)
        //    StopCoroutine(boatDisableRoutine);
    }

    // Update is called once per frame
    void Update()
    {
        if(light!=null)
            light.SetActive(!GameController.instance.player.IsDead);
        if (vfx != null)
        {
            if (vfxIsPlaying && GameController.instance.player.IsDead)
            {
                vfx.Stop();
                vfxIsPlaying = false;
            }
            //else if (vfx.isStopped && !GameController.instance.player.IsDead)
            //{
            //    vfx.Play();
            //}
        }

        if(shadowAttractionPoint != null)
        {
            if (vfxIsPlaying)
            {
                vfx.Stop();
                vfxIsPlaying = false;
            }

            shadowMode = true;
            if (Vector3.Distance(transform.position, shadowAttractionPoint.position) > shadowPositionTolerance)
            {
                /*if (!vfxSTIsPlaying)
                {
                    vfxShadowTrail.Play();
                    vfxSTIsPlaying = true;

                }*/
                transform.position += ((shadowAttractionPoint.position - transform.position).normalized * shadowAttractionForce * Time.deltaTime);
                //transform.position = new Vector3(transform.position.x, transform.position.y, 1f);
            }
            else
            {
                //transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                if(shadowAttractionPoint.GetComponent<HatTargetBehaviour>()!=null)
                {
                    onFlippedLanding = shadowAttractionPoint.GetComponent<HatTargetBehaviour>().onHatLanding;
                }
                //SetUncollectable(true);
                target = shadowAttractionPoint;
                Jump();

                /*if (vfxSTIsPlaying)
                {
                    vfxShadowTrail.Stop();
                    vfxSTIsPlaying = false;
                }*/
                shadowAttractionPoint = null;
            }
        }

        if(IsCollectable())
        {
            //Debug.Log("Collectable");
            mesh.transform.localPosition = new Vector3(mesh.transform.localPosition.x, mesh.transform.localPosition.y, startZ);
            gameObject.layer = startLayer;
            if (!lastIsCollectable)
            {
                //gameObject.SetActive(false);
                if (shadowMode)
                {
                    GameController.instance.player.ReturnHat();
                    shadowMode = false;
                }
               
                /*if (Physics.CheckBox(collider.bounds.center, collider.bounds.extents, transform.rotation, playerLayer))
                {
                    gameObject.SetActive(false);
                }*/
            }
            lastIsCollectable = true;
        }
        else
        {
            //Debug.Log("Not Collectable");
            mesh.transform.localPosition = new Vector3(mesh.transform.localPosition.x, mesh.transform.localPosition.y, uncollectableZ);
            gameObject.layer = uncollectableLayer;
            lastIsCollectable = false;
        }
        //Collider[] hits = Physics.OverlapBox(collider.bounds.center, collider.bounds.extents * 1.5f, transform.rotation, bohatWater, QueryTriggerInteraction.Ignore);
        //if(hits.Length != 0)
        //{
        //    if (GameController.instance.player.bohatUnlocked)
        //    {
        //        if (!isBoat)
        //        {
        //            isBoat = true;
        //            gameObject.layer = LayerMask.NameToLayer("BoHat");
        //            boatScaleAnimation.PlayForward();
        //            if (boatDisableRoutine != null)
        //            {
        //                StopCoroutine(boatDisableRoutine);
        //                boatDisableRoutine = null;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        GameController.instance.player.EnableShooting();
        //        gameObject.SetActive(false);
        //    }
        //}
        //else
        //{
        //    if (isBoat && GameController.instance.player.bohatUnlocked)
        //    {
        //        isBoat = false;
        //        gameObject.layer = startLayer;
        //        if (boatDisableRoutine != null)
        //            StopCoroutine(boatDisableRoutine);
        //        boatDisableRoutine = StartCoroutine(BoatDisable());
        //    }
        //}

        //hits = Physics.OverlapBox(collider.bounds.center + Vector3.up*0.2f, collider.bounds.extents * 0.9f, transform.rotation, playerLayer, QueryTriggerInteraction.Ignore);
        ////Debug.DrawRay(collider.bounds.center + Vector3.up * 0.6f, Vector3.up * (collider.bounds.extents.y * 0.9f), Color.red, Time.deltaTime);
        ////List<Collider> players = new List<Collider>();
        //isPlayerOn = false;
        //for (int i = 0; i < hits.Length; i++)
        //{
        //    if (hits[i].gameObject == GameController.instance.player.gameObject)
        //        isPlayerOn = true;
        //}

        //PLAYBACK_STATE control;
        //soundInstance.getPlaybackState(out control);
        //Debug.Log(control.ToString());
    }

    private void LateUpdate()
    {
        //Vector3 actualPos = transform.position;
        ////Debug.Log(isPlayerOn + " " + isBoat);
        //if (isBoat && isPlayerOn)
        //{
        //    Vector3 delta = actualPos - previousPos;
        //    GameController.instance.player.transform.position += delta;
        //}
        //previousPos = actualPos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Player")
        {
            //PLAYBACK_STATE control;
            //soundInstance.getPlaybackState(out control);
            //if(control == PLAYBACK_STATE.STOPPING)
            if (canSound)
            {
                GameController.instance.audioManager.PlayGenericSound(collisionSound, gameObject);
                canSound = false;
                enableSoundRoutine = StartCoroutine(EnableSound());
            }
            if (collision.gameObject.tag == "Bunny")
            {
                Jump();
            }
            if (collision.gameObject.tag == "Destroyable" && isHatarang)
            {
                DestroyableBehaviour obj = collision.gameObject.GetComponent<DestroyableBehaviour>();
                obj.Damage(1);
            }
            if (collision.gameObject.tag == "Bomblebee")
            {
                BomblebeeBehaviour bomblebee = collision.gameObject.GetComponent<BomblebeeBehaviour>();
                if (GameController.instance.player.chatcherUnlocked && !hasBomblebee)
                {
                    explosionRange = bomblebee.explosionRange;
                    explosionDamageableLayer = bomblebee.damageableLayer;
                    bomblebee.ReturnToPool();
                    hasBomblebee = true;
                    canExplodeOnTouch = false;
                    if(explosionDelay != 0)
                        GameController.instance.player.StartExplosion(explosionDelay);
                }
                else
                    bomblebee.Explode();
            }
            else if(hasBomblebee && canExplodeOnTouch)
            {
                GameController.instance.player.Explosion();
            }
        }
        if(isHatarang)
        {
            hatarangCollided = true;
        }
    }
    private void Jump()
    {
        //Vector3 targetPos = target.position;
        //float d = targetPos.x - transform.position.x;
        //float y0 = transform.position.y;
        //float y = targetPos.y;
        float v0 = 20f;
        //float tgalpha = (v0 * (-1f - Mathf.Sqrt(1 - ((2 * Physics.gravity.magnitude) / v0) * (y0 - y + ((Physics.gravity.magnitude * d * d) / (2 * v0)))))) / (Physics.gravity.magnitude * d);
        //float alpha = Mathf.Atan(tgalpha);
        //Vector3 velocity = new Vector3(v0 * Mathf.Cos(alpha), v0 * Mathf.Sin(alpha), 0);
        myRB.velocity = Vector3.up * v0;
        animPos.from = transform.localPosition.x;
        if (target != null)
        {
            animPos.to = transform.parent.InverseTransformPoint(target.position).x;
            animPos.duration = (2 * v0 / Physics.gravity.magnitude) + ((-v0 + Mathf.Sqrt(v0 * v0 - 2 * Physics.gravity.magnitude * (target.position.y - transform.position.y))) / Physics.gravity.magnitude);
            animPos.PlayForward();
        }
        animRot.PlayForward();
        isJumping = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        //if(collision.gameObject.tag == "Player")
        //{
        //    isPlayerOn = false;
        //}
    }

    void OnCollisionStay(Collision col)
    {
        if (col.collider.tag == "Player" && IsCollectable())
        {
            //Debug.Log("2");
            col.collider.GetComponent<PlayerController>().EnableShooting();
            gameObject.SetActive(false);
            shadowMode = false;
        }
        //if (col.collider.tag == "Player" && col.contacts[0].point.y > transform.position.y && Mathf.Abs(col.contacts[0].point.x - transform.position.x) < collider.size.x)
        //{
        //    col.collider.GetComponent<PlayerController>().AddVelocity(Vector3.up * playerJumpForce);
        //}
    }

    public bool IsCollectable()
    {
        return isCollectable /* && !isBoat */ && shadowAttractionPoint == null && !isUncollectable && !isJumping;
    }

    public void Block()
    {
        myRB.useGravity = false;
        myRB.isKinematic = true;
        if (hatarangRoutine != null)
            StopCoroutine(hatarangRoutine);
    }

    public void LaunchHatarang(Vector3 dir)
    {
        if(hatarangRoutine == null)
            hatarangRoutine = StartCoroutine(HatarangRoutine(dir));
    }

    public void ChangeLightColor(Color color)
    {
        lightRenderer.color = color;
    }

    public void ResetLightColor()
    {
        lightRenderer.color = lightStartColor;
    }

    //public void BoatMovement(Vector3 movementAmount)
    //{
    //    if (Physics.OverlapBox(collider.bounds.center + movementAmount, collider.bounds.extents, transform.rotation, obstacleLayer, QueryTriggerInteraction.Ignore).Length != 0)
    //        return;
    //    myRB.MovePosition(transform.position + movementAmount);
    //}

    public void ResetToStart()
    {
        transform.rotation = startRotation;
        isHatarang = false;
        lastIsCollectable = true;
        ResetLightColor();
    }

    public void CheckUpsideDown()
    {
        transform.localScale = new Vector3(startScale.x, startScale.y * (GameController.instance.UpsideDown? -1 : 1), startScale.z);
        boatScaleAnimation.from.y = startBoatAnimFromY * (GameController.instance.UpsideDown ? -1 : 1);
        boatScaleAnimation.to.y = startBoatAnimToY * (GameController.instance.UpsideDown ? -1 : 1);
        //Debug.Log("mi giro");
    }

    public void AssignVelocity(Vector3 velocity)
    {
        //Debug.Log("parto");
        myRB.velocity = velocity;
    }

    public bool IsGrounded(float spaceToGround)
    {
        bool rightGrounded = Physics.Raycast(collider.bounds.center - transform.up * collider.bounds.extents.y + transform.up * 0.1F + transform.right * collider.bounds.extents.x, -transform.up, spaceToGround + 0.1F, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        bool leftGrounded = Physics.Raycast(collider.bounds.center - transform.up * collider.bounds.extents.y + transform.up * 0.1F - transform.right * collider.bounds.extents.x, -transform.up, spaceToGround + 0.1F, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        bool isGrounded = rightGrounded || leftGrounded;
        //Debug.DrawRay(this.transform.position - transform.up * myBounds.extents.y + transform.up * 0.1F + transform.right * myBounds.extents.x, -transform.up * (maxDistanceFromGroundToJump + 0.1F), rightGrounded ? Color.green : Color.red, Time.fixedDeltaTime);
        //Debug.DrawRay(this.transform.position - transform.up * myBounds.extents.y + transform.up * 0.1F - transform.right * myBounds.extents.x, -transform.up * (maxDistanceFromGroundToJump + 0.1F), leftGrounded ? Color.green : Color.red, Time.fixedDeltaTime);
        return isGrounded;
    }

    public void ClampPosition(float bottom, float top, float left, float right)
    {
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, left, right), Mathf.Clamp(transform.position.y, bottom, top), transform.position.z);
    }

    public void FreezeRigidbodyRotation(bool value)
    {
        if (value)
        {
            myRB.freezeRotation = true;
        }
        else
        {
            myRB.constraints = constraints;
        }
    }

    private IEnumerator SetCollectable()
    {
        float time = 0;
        while(time < collectableDelay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        isCollectable = true;
        collider.enabled = true;
        collectableRoutine = null;
    }

    private IEnumerator EnableSound()
    {
        float time = 0;
        while (time <= delayBetweenSounds)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        canSound = true;
        enableSoundRoutine = null;
    }

    private IEnumerator HatarangRoutine(Vector3 dir)
    {
        isHatarang = true;
        hatarangCollided = false;
        myRB.useGravity = false;
        //myRB.isKinematic = true;
        float speed = GameController.instance.player.hatarangSpeed;
        float delay = GameController.instance.player.hatarangReturnDelay;
        float time = 0;
        transform.right = dir;
        while (time <= delay && !hatarangCollided)
        {
            yield return new WaitForEndOfFrame();
            transform.position += dir * speed * Time.deltaTime;
            transform.Rotate(Vector3.forward, speed * 5 * Time.deltaTime, Space.Self);
            time += Time.deltaTime;
        }
        myRB.velocity = Vector3.zero;
        myRB.angularVelocity = Vector3.zero;
        myRB.isKinematic = true;
        while(gameObject.activeSelf)
        {
            yield return new WaitForEndOfFrame();
            Vector3 playerdir = GameController.instance.player.transform.position - transform.position;
            playerdir.Normalize();
            transform.position += playerdir * speed * Time.deltaTime;
            transform.Rotate(Vector3.forward, speed * 5 * Time.deltaTime, Space.Self);
        }
        hatarangRoutine = null;
    }

    public void ManageDeath(bool reset)
    {
        foreach(GameObject go in objectsToDeactivateAtDeath)
        {
            go.SetActive(reset ? true : false);
            //mesh.material.SetFloat("Fresnel", reset ? 1f : 0f);
            mesh.material = reset ? baseMat : deathMaterial;
        }
    }

    public void OnFlippedLanding()
    {
        onFlippedLanding.Invoke();
        onFlippedLanding = null;
        isJumping = false;
        /*if (!vfxSSIsPlaying)
        {
            vfxShadowSmoke.Play();
            vfxSSIsPlaying = true;
        }*/
    }

    public void PlayHatFlipSound()
    {
        GameController.instance.audioManager.PlayGenericSound(hatFlipSound, gameObject);
    }

    //private IEnumerator BoatDisable()
    //{
    //    float time = 0;
    //    while (time <= 2)
    //    {
    //        yield return new WaitForEndOfFrame();
    //        time += Time.deltaTime;
    //    }

    //    boatScaleAnimation.PlayBackward();
    //    boatDisableRoutine = null;
    //}

    public void SetUncollectable(bool value)
    {
        isUncollectable = value;
        /*if(value)
        {
            myRB.isKinematic = true;
            mesh.transform.localPosition = new Vector3(mesh.transform.localPosition.x, mesh.transform.localPosition.y, uncollectableZ);
            gameObject.layer = uncollectableLayer;
        }
        else
        {
            myRB.isKinematic = false;
            mesh.transform.localPosition = new Vector3(mesh.transform.localPosition.x, mesh.transform.localPosition.y, startZ);
            gameObject.layer = startLayer;
        }*/
    }
}
