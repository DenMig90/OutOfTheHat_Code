using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;

public class CameraController : MonoBehaviour {

    [Range(0, 1)]
    public float smoothness = 0.5F;
    [Range(0,1)]
    public float verticalDeadZone;
    public AnimationCurve verticalDeadZoneCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
    public float sizeChangeSpeed = 0.5f;
    public float offsetChangeSpeed = 0.5f;
    public float globalSpeed = 0.5f;
    public bool useGlobalSpeed = true;
    public float startTargetChangeTime = 1;
    public Vector2 startOffset;
    public float startSize;
    public AnimationCurve offsetCurve;
    public AnimationCurve sizeCurve;
    public AnimationCurve targetCurve;
    public AnimationCurve restoreVerticalDeadzoneCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
    public Camera testCamera;
    public Camera hiddenCamera;
    public AnimationCurve shakeCurve;
    public AnimationCurve horizontalShakeCurve;
    public AnimationCurve verticalShakeCurve;
    public AnimationCurve flipCurve;
    public float shakeRotAmount;
    public float shakePosXAmount;
    public float shakePosYAmount;
    public float shakeDuration;
    public float flipDuration;
    public UnityEvent onStartShaking;
    public UnityEvent onEndShaking;
    public UnityEvent onStartFlipping;
    public UnityEvent onEndFlipping;
    public float stillDistance = 5f;
    public float stillDelay = 15f;
    public float stillSpeedIn = 2f;
    public float stillSpeedOut = 50f;
    [Tooltip("Più è alto e più il maintain y position sarà impreciso, più è basso e più la camera si muoverà durante i salti")]
    [Range(0, 1)]
    public float maintainYPosMaxSmoothness = 0.99F;
    [Range(0, 1)]
    public float blockVericalDeadzoneMaxSmoothness = 0.99f;

    [Header("FMOD Events")]
    [EventRef] public string flipSound;
    [EventRef] public string finalFlipSound;
    public bool finalFlip = false;

    public float SizeDelta
    {
        get { return myCamera.orthographicSize / startSize; }
    }

    public float Size
    {
        get { return myCamera.orthographicSize; }
    }

    [HideInInspector]
    public Vector2 offsetAdditive = Vector3.zero;


    private Transform target;
    private float size;
    private float previousSize;
    private float minSize;
    private float targetChangeTime;
    private float actualTargetChangeTime;
    private float startVerticalDeadzone;
    //private float startSize;
    //[SerializeField]
    private Vector2 offset;
    private Vector2 previousOffset;
    private Vector2 targetOffset;
    private Vector2 stopFollowingPos;
    private Camera myCamera;
    [SerializeField]
    private bool isStopped = false;
    [SerializeField]
    private bool blockPlayer = false;
    [SerializeField]
    private bool followPlayer = true;
    private Vector2 targetPos;
    private Coroutine shakeRoutine;
    private Coroutine flipCoroutine;
    private Coroutine restoreDeadzoneRoutine;
    private AnimationCurve startOffsetCurve;
    private AnimationCurve startSizeCurve;
    private AnimationCurve startTargetCurve;
    //private float maintainXPosUntil = 0f;
    private bool isMaintainingXPosition = false;
    private float xPosToMaintain = 0f;
    //private float maintainYPosUntil = 0f;
    private bool isMaintainingYPosition = false;
    private float yPosToMaintain = 0f;
    private Vector3 marginTop;
    private Vector3 marginBottom;
    private Vector3 marginRight;
    private Vector3 marginLeft;
    private UnityEvent onTargetReached=null;
    private Vector2 realTarget;
    private Vector2 targetObjectPos;
    private float stillStartTime = 0f;
    private Vector2 lastPosition;
    private float lastSize;
    private bool isZoomingOnStillPlayer = false;
    private float deadzoneMultiplier = 1f;
    private Vector3 shakePos = Vector3.zero;

    private void Awake()
    {
        myCamera = GetComponent<Camera>();
        GameController.instance.SetMainCamera(this);
        startSizeCurve = sizeCurve;
        myCamera.orthographicSize = startSize;
        hiddenCamera.orthographicSize = myCamera.orthographicSize;
        DistanceCamera(myCamera.orthographicSize);
        offset = startOffset;
        startOffsetCurve = offsetCurve;
        targetChangeTime = startTargetChangeTime;
        actualTargetChangeTime = 0;
        startTargetCurve = targetCurve;
        startVerticalDeadzone = verticalDeadZone;
        ChangeOffset(offset);
        blockPlayer = false;
        followPlayer = true;
        isStopped = false;
        followPlayer = true;
        isStopped = false;
        finalFlip = false;
        shakeRoutine = null;
        restoreDeadzoneRoutine = null;
        target = GameController.instance.player.cameraTarget.transform;
        //GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        //float myX = target.position.x + offset.x;
        //float myY = target.position.y + offset.y;
        //transform.position = new Vector3(myX, myY, transform.position.z);
    }

    // Use this for initialization
    void Start () {
        //startSize = size;
        //lastSavedSize = size;
        //lastSavedSizeChangeSpeed = sizeChangeSpeed;
        //Cursor.visible = false;
        //lastSavedOffset = offset;
        //lastSavedOffsetChangeSpeed = offsetChangeSpeed;
        stillStartTime = Time.time;
    }

    // Update is called once per frame
    void LateUpdate () {
        marginRight = myCamera.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height/2,0));
        marginLeft = myCamera.ScreenToWorldPoint(new Vector3(0, Screen.height/2, 0));
        marginTop = myCamera.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height, 0));
        marginBottom = myCamera.ScreenToWorldPoint(new Vector3(Screen.width/2, 0, 0));
        //Debug.Log(marginRight);
        Debug.DrawRay(marginRight, Vector3.right, Color.red, Time.deltaTime);
        Debug.DrawRay(marginTop, Vector3.up, Color.red, Time.deltaTime);
        Debug.DrawRay(marginLeft, Vector3.left, Color.red, Time.deltaTime);
        Debug.DrawRay(marginBottom, Vector3.down, Color.red, Time.deltaTime);

        CalculateMinSize();

        //Debug.DrawRay(myCamera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)), Vector3.left, Color.magenta, 10000);
        //Debug.DrawRay(myCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)), Vector3.left, Color.magenta, 10000);
        //Debug.DrawRay(myCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)), Vector3.left, Color.magenta, 10000);
        //Debug.DrawRay(myCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)), Vector3.left, Color.magenta, 10000);
        if (!isStopped)
        {
            float actualSize = (size > minSize) ? size : minSize;
            AnimationCurve actualSizeCurve = sizeCurve;
            float actualSizeChangeSpeed = (useGlobalSpeed) ? globalSpeed : sizeChangeSpeed;
            if(Time.time > stillStartTime + stillDelay && actualSize>=stillDistance)
            {
                actualSize = stillDistance;
                if(!isZoomingOnStillPlayer)
                {

                    previousSize = myCamera.orthographicSize;
                    isZoomingOnStillPlayer = true;

                    xPosToMaintain = Camera.main.WorldToScreenPoint(targetObjectPos).x;
                    yPosToMaintain = Camera.main.WorldToScreenPoint(targetObjectPos).y;
                    isMaintainingXPosition = true;
                    isMaintainingYPosition = true;
                }
            }


            float prevSizeDifference = Mathf.Abs(previousSize - actualSize);
            float actualSizeDifference = Mathf.Abs(myCamera.orthographicSize - actualSize);

            if ((Time.time <= stillStartTime + stillDelay) && actualSizeDifference < 0.1f && isZoomingOnStillPlayer)
            {
                isZoomingOnStillPlayer = false;
                previousSize = myCamera.orthographicSize;
            }
            if (isZoomingOnStillPlayer)
            {
                actualSizeCurve = startSizeCurve;
                if(Time.time > stillStartTime + stillDelay)
                    actualSizeChangeSpeed = stillSpeedIn;
                else
                    actualSizeChangeSpeed = stillSpeedOut;

            }
            //Debug.Log(actualSizeChangeSpeed);
            float mySize = Mathf.MoveTowards(myCamera.orthographicSize, actualSize, actualSizeChangeSpeed * Time.deltaTime * Mathf.Clamp(actualSizeCurve.Evaluate((prevSizeDifference == 0) ? 1 : (actualSizeDifference / prevSizeDifference)), 0.1f, 1f));
            

            //Debug.Log("roba");
            float distanceFromPrev = Vector2.Distance(previousOffset, targetOffset);
            float actualDistance = Vector2.Distance(offset, targetOffset);
            Vector2 oldOffset = offset;
            offset = Vector2.Lerp(offset, targetOffset, ((useGlobalSpeed) ? globalSpeed : offsetChangeSpeed) * Time.deltaTime * Mathf.Clamp(offsetCurve.Evaluate((distanceFromPrev==0)? 1 : (actualDistance/distanceFromPrev)),0.1f,1f));
            if (isMaintainingXPosition)
                offset.x = targetOffset.x;
            if (isMaintainingYPosition)
                offset.y = targetOffset.y;

            //offset += offsetAdditive;

            float targetPlayerX = (GameController.instance.UpsideDown) ? target.position.x - (offset.x + offsetAdditive.x) : target.position.x + (offset.x + offsetAdditive.x);
            float targetPlayerY = (GameController.instance.UpsideDown) ? target.position.y - (offset.y + offsetAdditive.y) : target.position.y + (offset.y + offsetAdditive.y);
            Vector2 myTargetPos = targetPos + ((GameController.instance.UpsideDown) ? -offset : offset);
            realTarget = CalculateTargetPosition(followPlayer ? myTargetPos : stopFollowingPos, followPlayer ? new Vector2(targetPlayerX, targetPlayerY) : myTargetPos, actualTargetChangeTime / targetChangeTime);
            targetObjectPos = CalculateTargetPosition(followPlayer ? targetPos : (Vector2)target.position, followPlayer ? (Vector2)target.position : targetPos, actualTargetChangeTime / targetChangeTime);
            if (actualTargetChangeTime>0)
                actualTargetChangeTime -= Time.deltaTime;
            else
            {
                if(onTargetReached!=null)
                    onTargetReached.Invoke();
                onTargetReached = null;
            }

            float minVerticalDeadzone = myCamera.ScreenToWorldPoint(new Vector3(0, Screen.height * ((1-verticalDeadZone)/2), 0)).y;
            //float maxVerticalDeadzone = myCamera.ScreenToWorldPoint(new Vector3(0, Screen.height * ((1 - verticalDeadZone) / 2) + Screen.height * verticalDeadZone, 0)).y;
            float maxVerticalDeadzoneDistance = Mathf.Abs(transform.position.y - minVerticalDeadzone);
            float targetVerticalDistance = Mathf.Abs(transform.position.y - realTarget.y);

            //if (realTarget.y >= minVerticalDeadzone && realTarget.y <= maxVerticalDeadzone)
            //    realTarget.y = transform.position.y;

            float smoothnessY = deadzoneMultiplier * (isMaintainingYPosition ? maintainYPosMaxSmoothness : 1f) * Mathf.Lerp(1f, smoothness, verticalDeadZoneCurve.Evaluate(Mathf.Clamp01(targetVerticalDistance / maxVerticalDeadzoneDistance)));
            //Debug.Log(smoothnessY);

            float myX;
            if (isMaintainingXPosition && (actualSizeDifference>0.1f || Time.time > stillStartTime + stillDelay))
            {
                float tempX = Camera.main.transform.position.x - (Camera.main.ScreenToWorldPoint(new Vector3(xPosToMaintain, transform.position.y, 0f)).x - targetObjectPos.x);

                myX = Mathf.Lerp(tempX, transform.position.x, smoothness);
                //ChangeOffsetX(myX - realTarget.x);
                targetOffset.x = tempX - targetObjectPos.x;
            }
            else
            {
                isMaintainingXPosition = false;
                myX = Mathf.Lerp(realTarget.x, transform.position.x, smoothness);
            }

            float myY;
            if (isMaintainingYPosition && (actualSizeDifference> 0.1f || Time.time > stillStartTime + stillDelay))
            {
                //Debug.Log("Camera position=" + Camera.main.transform.position.y + "PosToMaintein=" + Camera.main.ScreenToWorldPoint(new Vector3(transform.position.x, yPosToMaintain, 0f)).y + " PlayerPos=" + GameController.instance.player.transform.position.y);
                //Debug.Break();
                float tempY = Camera.main.transform.position.y - (Camera.main.ScreenToWorldPoint(new Vector3(transform.position.x, yPosToMaintain, 0f)).y - targetObjectPos.y);
                
                myY = Mathf.Lerp(tempY, transform.position.y, smoothnessY);
                //ChangeOffsetY(myY - (realTarget.y - offset.y));
                targetOffset.y = tempY - targetObjectPos.y;
                //Debug.Log(offset.y);
            }
            else
            {
                isMaintainingYPosition = false;
                myY = Mathf.Lerp(realTarget.y, transform.position.y, smoothnessY);
            }
                

            transform.position = new Vector3(myX, myY, transform.position.z) + shakePos;
            shakePos = Vector3.zero;
            myCamera.orthographicSize = mySize;
            hiddenCamera.orthographicSize = myCamera.orthographicSize;
        }
        if(blockPlayer)
        {
            GameController.instance.player.teleportHat.ClampPosition(marginBottom.y, marginTop.y, marginLeft.x, marginRight.x);
        }

        if(Vector2.Distance(targetObjectPos, lastPosition) > 0f || (lastSize != myCamera.orthographicSize && !isZoomingOnStillPlayer) || !followPlayer || GameController.instance.player.IsBlocked || GameController.instance.ActualState != GameState.InGame || GameController.instance.UpsideDown)
        {
            stillStartTime = Time.time;
        }
        lastPosition = targetObjectPos;
        lastSize = myCamera.orthographicSize;
    }

    public bool CheckAllowedPosition(Vector3 position)
    {
        if(blockPlayer)
        {
            bool allowed = position.x >= marginLeft.x && position.x <= marginRight.x && position.y <= marginTop.y && position.y >= marginBottom.y;
            return allowed;
        }
        return true;
    }

    public Vector3 GetAllowedPosition(Vector3 position)
    {
        if(blockPlayer)
        {
            return new Vector3(Mathf.Clamp(position.x, marginLeft.x, marginRight.x), Mathf.Clamp(position.y, marginBottom.y, marginTop.y), position.z);
        }
        return position;
    }

    private Vector2 CalculateTargetPosition(Vector2 start, Vector2 destination, float lerp)
    {
        Vector2 returnable;
        //Vector2 dir = destination - start;
        //returnable = destination + (dir * Mathf.Clamp01(lerp));
        returnable = Vector2.Lerp(start, destination, targetCurve.Evaluate(1f-lerp));
        return returnable;
    }

    private void CalculateMinSize()
    {
        minSize = 1;
        if (GameController.instance.player.teleportHat.gameObject.activeSelf)
        {
            Camera cam = testCamera;
            cam.transform.position = transform.position;
            cam.orthographic = myCamera.orthographic;
            cam.orthographicSize = minSize;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            bool inView = GeometryUtility.TestPlanesAABB(planes, GameController.instance.player.Collider.bounds) && GeometryUtility.TestPlanesAABB(planes, GameController.instance.player.teleportHat.Collider.bounds);
            //Debug.Log(inView);
            int cicles = 10000;
            while (!inView && cicles > 0)
            {
                minSize++;
                cam.orthographicSize = minSize;
                cicles--;
                planes = GeometryUtility.CalculateFrustumPlanes(cam);
                inView = GeometryUtility.TestPlanesAABB(planes, GameController.instance.player.Collider.bounds) && GeometryUtility.TestPlanesAABB(planes, GameController.instance.player.teleportHat.Collider.bounds);
            }
        }
        //Debug.Log(minSize);
    }

    public void Block(bool newValue)
    {
        isStopped = newValue;
    }

    public void StopFollowing()
    {
        targetPos = (Vector2)transform.position - offset;
        actualTargetChangeTime = targetChangeTime;
        followPlayer = false;
    }

    public void StopFollowing(Vector2 newTarget)
    {
        stopFollowingPos = transform.position;
        targetPos = newTarget;
        actualTargetChangeTime = targetChangeTime;
        followPlayer = false;
    }

    public void BlockPlayer(bool value)
    {
        blockPlayer = value;
    }

    public void Follow()
    {
        followPlayer = true;
        actualTargetChangeTime = targetChangeTime;
    }

    public void BlockVerticalDeadzone()
    {
        float duration = CalculateMaxDuration();
        //Debug.Log("Vertical Deadzone Block Duration: " + duration);
        //verticalDeadZone = 0;
        if(restoreDeadzoneRoutine != null)
        {
            StopCoroutine(restoreDeadzoneRoutine);
        }
        restoreDeadzoneRoutine = StartCoroutine(RestoreDeadzone(duration));
    }

    public void TargetChangeSpeed(float value)
    {
        targetChangeTime = value;
    }

    public void TargetChangeCurve(AnimationCurve value)
    {
        targetCurve = value;
    }

    public void DistanceCamera(float amount, AnimationCurve curve = null)
    {
        previousSize = myCamera.orthographicSize;
        size = amount;
        DistanceChangeCurve(curve != null ? curve : startSizeCurve);
    }

    public void DistanceChangeSpeed(float amount)
    {
        sizeChangeSpeed = amount;
    }

    public void DistanceChangeCurve(AnimationCurve curve)
    {
        sizeCurve = curve;
    }

    public void GlobalSpeed(float amount)
    {
        globalSpeed = amount;
    }

    public void OffsetChangeSpeed(float amount)
    {
        offsetChangeSpeed = amount;
    }
    
    public void MaintainPlayerXPosition()
    {
        float duration = CalculateMaxDuration();
        //maintainXPosUntil = Time.timeSinceLevelLoad + duration;
        isMaintainingXPosition = true;
        xPosToMaintain = Camera.main.WorldToScreenPoint(targetObjectPos).x;

    }

    public void MaintainPlayerYPosition()
    {
        float duration = CalculateMaxDuration();
        isMaintainingYPosition = true;
        //maintainYPosUntil = Time.timeSinceLevelLoad + duration;
        yPosToMaintain = Camera.main.WorldToScreenPoint(targetObjectPos).y;
    }

    public void Save()
    {
        CameraData save = GameController.instance.SaveData.cameraData;
        save.lastSavedSize = size;
        save.lastSavedSizeChangeSpeed = sizeChangeSpeed;
        Vector2 offsetToSave = targetOffset;
        if (isMaintainingXPosition)
            offsetToSave.x = offset.x;
        if (isMaintainingYPosition)
            offsetToSave.y = offset.y;
        save.lastSavedOffset = offsetToSave;
        save.lastSavedOffsetChangeSpeed = offsetChangeSpeed;
        save.lastSavedVerticalDeadzone = verticalDeadZone;
        //save.savedFollowPlayer = followPlayer;
        //save.savedStopped = isStopped;
        //save.savedBlockPlayer = blockPlayer;
    }

    public void LoadSaved()
    {
        ResetToStart();
        CameraData save = GameController.instance.SaveData.cameraData;
        size = save.lastSavedSize;
        myCamera.orthographicSize = size;
        hiddenCamera.orthographicSize = myCamera.orthographicSize;
        sizeChangeSpeed = save.lastSavedSizeChangeSpeed;
        targetOffset = save.lastSavedOffset;
        offset = save.lastSavedOffset;
        offsetChangeSpeed = save.lastSavedOffsetChangeSpeed;
        verticalDeadZone = save.lastSavedVerticalDeadzone;
        //followPlayer = save.savedFollowPlayer;
        //isStopped = save.savedStopped;
        //blockPlayer = save.savedBlockPlayer;
        ManageUpsideDown();
        //float myX = target.position.x + offset.x;
        //float myY = target.position.y + offset.y;
        //transform.position = new Vector3(myX, myY, transform.position.z);
    }

    public void ResetToStart()
    {
        myCamera.orthographicSize = startSize;
        hiddenCamera.orthographicSize = myCamera.orthographicSize;
        DistanceCamera(startSize);
        offset = startOffset;
        ChangeOffset(offset);
        blockPlayer = false;
        //followPlayer = true;
        isStopped = false;
        followPlayer = true;
        isStopped = false;
        finalFlip = false;
        offsetCurve = startOffsetCurve;
        sizeCurve = startSizeCurve;
        targetChangeTime = startTargetChangeTime;
        actualTargetChangeTime = 0;
        targetCurve = startTargetCurve;
        verticalDeadZone = startVerticalDeadzone;
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }
        if(flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
        }
        if (restoreDeadzoneRoutine != null)
        {
            StopCoroutine(restoreDeadzoneRoutine);
        }
        stillStartTime = Time.time;
        isZoomingOnStillPlayer = false;
        //float myX = target.position.x + offset.x;
        //float myY = target.position.y + offset.y;
        //transform.position = new Vector3(myX, myY, transform.position.z);
    }

    public void ManageUpsideDown()
    {
        transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, GameController.instance.UpsideDown ? 180 : 0);
    }

    public void ChangeOffsetX(float target, AnimationCurve curve = null)
    {
        ChangeOffset(new Vector2(target, targetOffset.y));
        ChangeOffsetCurve(curve != null ? curve : startOffsetCurve);
    }
    public void ChangeOffsetY(float target, AnimationCurve curve = null)
    {
        ChangeOffset(new Vector2(targetOffset.x, target));
        ChangeOffsetCurve(curve != null ? curve : startOffsetCurve);
    }

    public void ChangeOffsetXImmediate(float target)
    {
        offset = new Vector2(target, targetOffset.y);
        ChangeOffset(offset);
    }
    public void ChangeOffsetYImmediate(float target)
    {
        offset = new Vector2(targetOffset.x, target);
        ChangeOffset(offset);
    }

    public void ChangeOffset(Vector2 target, AnimationCurve curve = null)
    {
        if(target.x != targetOffset.x)
        {
            isMaintainingXPosition = false;
        }
        if(target.y != targetOffset.y)
        {
            isMaintainingYPosition = false;
        }
        targetOffset = target;
        previousOffset = offset;
        ChangeOffsetCurve(curve != null ? curve : startOffsetCurve);

        //isZoomingOnStillPlayer = false;
    }

    public void ChangeOffsetCurve(AnimationCurve curve)
    {
        offsetCurve = curve;
    }

    public void ChangeVerticalDeadzone(float amount)
    {
        verticalDeadZone = Mathf.Clamp01(amount);
    }

    public void StartFlipping()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
        }
        flipCoroutine = StartCoroutine(Flip());
    }

    public void StartShaking()
    {
        if(shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }
        shakeRoutine = StartCoroutine(Shake(shakeDuration, shakeCurve, horizontalShakeCurve, verticalShakeCurve, shakeRotAmount, shakePosXAmount, shakePosYAmount, true));
    }

    public void StartShakingNoMaintainPos()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }
        shakeRoutine = StartCoroutine(Shake(shakeDuration, shakeCurve, horizontalShakeCurve, verticalShakeCurve, shakeRotAmount, shakePosXAmount, shakePosYAmount, false));
    }

    public void StartShaking(float duration, AnimationCurve rotShakeCurve, AnimationCurve xPosShakeCurve, AnimationCurve yPosShakeCurve, float rotShakeAmount, float xPosShakeAmount, float yPosShakeAmount)
    {
        if(shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }
        shakeRoutine = StartCoroutine(Shake(duration, rotShakeCurve, xPosShakeCurve, yPosShakeCurve, rotShakeAmount, xPosShakeAmount, yPosShakeAmount, true));
    }

    public void StartShakingNoMaintainPos(float duration, AnimationCurve rotShakeCurve, AnimationCurve xPosShakeCurve, AnimationCurve yPosShakeCurve, float rotShakeAmount, float xPosShakeAmount, float yPosShakeAmount)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }
        shakeRoutine = StartCoroutine(Shake(duration, rotShakeCurve, xPosShakeCurve, yPosShakeCurve, rotShakeAmount, xPosShakeAmount, yPosShakeAmount, false));
    }

    public void SetFinalFlip(bool value)
    {
        finalFlip = value;
    }

    //private IEnumerator Shake()
    //{
    //    float lerp = 0;
    //    Quaternion startRotation = transform.localRotation;
    //    Quaternion minShake = startRotation * Quaternion.Euler(0, 0, -shakeRotAmount);
    //    Quaternion maxShake = startRotation * Quaternion.Euler(0, 0, shakeRotAmount);
    //    Vector3 startPos = transform.position;
    //    float minPosX = startPos.x - shakePosXAmount;
    //    float maxPosX = startPos.x + shakePosXAmount;
    //    float minPosY = startPos.y - shakePosYAmount;
    //    float maxPosY = startPos.y + shakePosYAmount;
    //    onStartShaking.Invoke();
    //    float posX;
    //    float posY;
    //    while (lerp < 1)
    //    {
    //        lerp += Time.unscaledDeltaTime / shakeDuration;
    //        transform.localRotation = Quaternion.Lerp(minShake, maxShake, shakeCurve.Evaluate(lerp));
    //        posX = Mathf.Lerp(minPosX, maxPosX, shakeCurve.Evaluate(lerp));
    //        posY = Mathf.Lerp(minPosY, maxPosY, verticalShakeCurve.Evaluate(lerp));
    //        transform.position = new Vector3(posX, posY, transform.position.z);
    //        yield return new WaitForEndOfFrame();
    //    }
    //    onEndShaking.Invoke();
    //    shakeRoutine = null;
    //}

    private IEnumerator Shake(float duration, AnimationCurve rotShakeCurve, AnimationCurve xPosShakeCurve, AnimationCurve yPosShakeCurve, float rotShakeAmount, float xPosShakeAmount, float yPosShakeAmount, bool maintainPos)
    {
        float lerp = 0;
        Quaternion startRotation = transform.localRotation;
        Quaternion minShake = startRotation * Quaternion.Euler(0, 0, -rotShakeAmount);
        Quaternion maxShake = startRotation * Quaternion.Euler(0, 0, rotShakeAmount);
        Vector3 startPos = maintainPos ? transform.position : Vector3.zero;
        //float minPosX = startPos.x - xPosShakeAmount;
        float maxPosX = startPos.x + xPosShakeAmount;
        //float minPosY = startPos.y - yPosShakeAmount;
        float maxPosY = startPos.y + yPosShakeAmount;
        onStartShaking.Invoke();
        float posX;
        float posY;
        while (lerp < 1)
        {
            //if (!maintainPos)
                //startPos = transform.position;
            lerp += Time.deltaTime / duration;
            transform.localRotation = Quaternion.Lerp(minShake, maxShake, rotShakeCurve.Evaluate(lerp));
            posX = Mathf.LerpUnclamped(startPos.x, maxPosX, xPosShakeCurve.Evaluate(lerp));
            posY = Mathf.LerpUnclamped(startPos.y, maxPosY, yPosShakeCurve.Evaluate(lerp));
            if (maintainPos)
                transform.position = new Vector3(posX, posY, transform.position.z);
            else
                shakePos = new Vector3(posX, posY, 0f);
            yield return new WaitForEndOfFrame();
        }
        onEndShaking.Invoke();
        shakeRoutine = null;
    }

    private IEnumerator Flip()
    {
        float lerp = 0;
        Quaternion startRotation = transform.localRotation;
        Quaternion minShake = startRotation * Quaternion.Euler(0, 0, -shakeRotAmount);
        Quaternion maxShake = startRotation * Quaternion.Euler(0, 0, shakeRotAmount);
        Vector3 startPos = transform.position;
        float minPosX = startPos.x - shakePosXAmount;
        float maxPosX = startPos.x + shakePosXAmount;
        float minPosY = startPos.y - shakePosYAmount;
        float maxPosY = startPos.y + shakePosYAmount;
        onStartFlipping.Invoke();
        PlayFlipSound();
        StartShaking();
        yield return new WaitWhile(() => shakeRoutine != null);
        Quaternion flipped = startRotation * Quaternion.Euler(0, 0, 180);
        while (lerp < 1)
        {
            lerp += Time.deltaTime / flipDuration;
            transform.localRotation = Quaternion.Lerp(startRotation, flipped, flipCurve.Evaluate(lerp));
            yield return new WaitForEndOfFrame();
        }
        onEndFlipping.Invoke();
    }

    private IEnumerator RestoreDeadzone(float duration)
    {
        float time = 0;
        float deadzone = verticalDeadZone;
        while(time < duration)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
            //verticalDeadZone = Mathf.Lerp(deadzone, deadzone, restoreVerticalDeadzoneCurve.Evaluate(time / duration));
            deadzoneMultiplier = Mathf.Lerp(1f, blockVericalDeadzoneMaxSmoothness, restoreVerticalDeadzoneCurve.Evaluate(time / duration));
        }
        restoreDeadzoneRoutine = null;
    }

    private float CalculateMaxDuration()
    {
        float sizeDuration = Mathf.Abs(myCamera.orthographicSize - size) / sizeChangeSpeed;
        float offsetDuration = Vector2.Distance(targetOffset, offset) / offsetChangeSpeed;
        return sizeDuration > offsetDuration ? sizeDuration : offsetDuration;
    }

    public void SetOnTargetReached(UnityEvent myEvent)
    {
        onTargetReached = myEvent;
    }

    private void PlayFlipSound()
    {
        GameController.instance.audioManager.PlayGenericSound(finalFlip? finalFlipSound : flipSound, GameController.instance.player.gameObject);
    }
}
