using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class InputManager : MonoBehaviour {

    public float triggerDeadZone = 0.3f;
    public float movementDeadZone = 0.7f;
    public float aimDeadZone = 0.7f;
    public float aimHeightRaiser = 3f;
    public float mouseVerticalSensivity = 0.5f;
    public float stickVerticalSensivity = 0.5f;
    public float aimFramesToStore = 3;

    public bool IsJoystick
    {
        get
        {
            return isJoystick;
        }
    }

    public bool PauseInput
    {
        get
        {
            if (GameController.instance.ActualState != GameState.InGame)
                return false;
            bool returnable = pauseInput;
            //pauseInput = false;
            return returnable;
        }
    }

    public bool ResumeInput
    {
        get
        {
            if (GameController.instance.ActualState != GameState.Pause && GameController.instance.ActualState != GameState.SecretPage)
                return false;
            bool returnable = resumeInput;
            //resumeInput = false;
            return returnable;
        }
    }

    public bool MovementInput
    {
        get
        {
            if (scriptedMovement)
                return scriptedMovementAmount != 0;
            if (inputBlocked)
                return false;
            if (isJoystick)
                return Mathf.Abs(movementInput) > movementDeadZone;
            else
                return movementInput != 0;
        }
    }

    public bool VerticalMovementInput
    {
        get
        {
            if (inputBlocked)
                return false;
            if (isJoystick)
                return Mathf.Abs(verticalMovementInput) > movementDeadZone;
            else
                return verticalMovementInput != 0;
        }
    }

    public bool AimInput
    {
        get
        {
            if (inputBlocked)
                return false;
            if(isJoystick)
                return Mathf.Abs(aimInput) > aimDeadZone;
            else
            {
                return true;
            }
        }
    }

    public float MovementInputAmount
    {
        get
        {
            if (scriptedMovement)
                return scriptedMovementAmount;
            if (inputBlocked)
                return 0;
            return movementInput;
        }
    }

    public float VerticalMovementInputAmount
    {
        get
        {
            if (inputBlocked)
                return 0;
            return verticalMovementInput;
        }
    }

    public float AimAngleSine
    {
        get
        {
            if (inputBlocked)
                return 0;
            return (Mathf.Asin(lastAimInputStored.y) - (Mathf.PI/4)) / (Mathf.PI / 4);
        }
    }

    public bool JumpInput
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = jumpInput;
            //jumpInput = false;
            return returnable;
        }
    }

    public bool JumpInputReleased
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = jumpInputReleased;
            //jumpInput = false;
            return returnable;
        }
    }

    public bool TeleportInputPressed
    {
        get
        {
            if (inputBlocked || interaction)
                return false;
            bool returnable = teleportInputPressed;
            //teleportInputPressed = false;
            return returnable;
        }
    }

    public bool TeleportInputReleased
    {
        get
        {
            if (inputBlocked || interaction)
                return false;
            bool returnable = teleportInputReleased;
            //teleportInputReleased = false;
            return returnable;
        }
    }

    public bool ThrowInputPressed
    {
        get
        {
            if (inputBlocked || interaction)
                return false;
            bool returnable = throwInputPressed;
            //throwInputPressed = false;
            return returnable;
        }
    }

    public bool ThrowInputReleased
    {
        get
        {
            if (inputBlocked || interaction)
                return false;
            bool returnable = throwInputReleased;
            //throwInputReleased = false;
            return returnable;
        }
    }

    public bool ThrowInput
    {
        get
        {
            if (inputBlocked || interaction)
                return false;
            bool returnable = throwInput;
            //throwInput = false;
            return returnable;
        }
    }

    public bool CancelInput
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = cancelInput;
            //cancelInput = false;
            return returnable;
        }
    }

    public bool GlideInput
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = glideInput;
            //glideInput = false;
            return returnable;
        }
    }

    public bool DragInput
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = dragInput;
            //glideInput = false;
            return returnable;
        }
    }

    public bool HideInput
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = hideInput;
            //glideInput = false;
            return returnable;
        }
    }

    public bool HideInputPressed
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = hideInputPressed;
            //glideInput = false;
            return returnable;
        }
    }

    public bool HideInputReleased
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = hideInputReleased;
            //glideInput = false;
            return returnable;
        }
    }

    public bool BohatInputPressed
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = bohatInputPressed;
            //glideInput = false;
            return returnable;
        }
    }

    public bool BohatInputReleased
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = bohatInputReleased;
            //glideInput = false;
            return returnable;
        }
    }

    public bool BohatInput
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = bohatInput;
            //glideInput = false;
            return returnable;
        }
    }

    public bool HatarangInputPressed
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = hatarangInputPressed;
            //glideInput = false;
            return returnable;
        }
    }

    public bool InteractionInputPressed
    {
        get
        {
            if (inputBlocked || !interaction)
                return false;
            bool returnable = interactionInputPressed;
            //glideInput = false;
            return returnable;
        }
    }

    public bool HatarangInputReleased
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = hatarangInputReleased;
            //glideInput = false;
            return returnable;
        }
    }

    public bool CreateCheckpointInputPressed
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = createCheckpointInputPressed;
            return returnable;
        }
    }

    public bool CreateCheckpointInputReleased
    {
        get
        {
            if (inputBlocked)
                return false;
            bool returnable = createCheckpointInputReleased;
            return returnable;
        }
    }

    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // WARNING!!! ACHTUNG!!! Gli input dell'aim sono rielaborati all'interno di questo manager 
    // per fornire un vettore di direzione della mira relativa al world space.
    // Questo è stato fatto per unificare la gestione di mouse e controller
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public Vector3 FirstAimInputStored
    {
        get
        {
            Vector3 returnable;

            //if (inputBlocked || directionBlocked)
            //    returnable = new Vector3(blockedDir, 1, 0);
            //else
                returnable = firstAimInputStored;
            returnable *= (GameController.instance.Right.x > 0) ? 1 : -1;
            return returnable;
        }
    }
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // WARNING!!! ACHTUNG!!! Gli input dell'aim sono rielaborati all'interno di questo manager 
    // per fornire un vettore di direzione della mira relativa al world space.
    // Questo è stato fatto per unificare la gestione di mouse e controller
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public Vector3 LastAimInputStored
    {
        get
        {
            Vector3 returnable;
            //if (inputBlocked || directionBlocked)
            //    returnable = new Vector3(blockedDir,1,0);
            //else
                returnable = lastAimInputStored;
            returnable *= (GameController.instance.Right.x > 0) ? 1 : -1;
            return returnable;
        }
    }

    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // WARNING!!! ACHTUNG!!! Gli input dell'aim sono rielaborati all'interno di questo manager 
    // per fornire un vettore di direzione della mira relativa al world space.
    // Questo è stato fatto per unificare la gestione di mouse e controller
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public Vector3 FirstAimInputStoredTranslated
    {
        get
        {
            Vector3 returnable;
            //if (inputBlocked || directionBlocked)
            //    returnable = new Vector3(blockedDir, 0, 0);
            //else
                returnable = firstAimInputStoredTranslated;
            returnable *= (GameController.instance.Right.x > 0) ? 1 : -1;
            return returnable;
        }
    }
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // WARNING!!! ACHTUNG!!! Gli input dell'aim sono rielaborati all'interno di questo manager 
    // per fornire un vettore di direzione della mira relativa al world space.
    // Questo è stato fatto per unificare la gestione di mouse e controller
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public Vector3 LastAimInputStoredTranslated
    {
        get
        {
            Vector3 returnable;
            //if (inputBlocked || directionBlocked)
            //    returnable = new Vector3(blockedDir, 0, 0);
            //else
                returnable = lastAimInputStoredTranslated;
            returnable *= (GameController.instance.Right.x > 0) ? 1 : -1;
            return returnable;
        }
    }

    public bool InputBlocked
    {
        get
        {
            return inputBlocked;
        }
    }

    public bool DirectionBlocked
    {
        get
        {
            return directionBlocked;
        }
    }

    private bool isJoystick;
    private bool leftTriggerUp;
    private bool alreadyLeftUp;
    private bool rightTriggerUp;
    private bool alreadyRightUp;
    private bool leftTriggerDown;
    private bool alreadyLeftDown;
    private bool rightTriggerDown;
    private bool alreadyRightDown;
    private bool rightTrigger;
    private bool leftTrigger;
    private bool pauseInput;
    private bool resumeInput;
    private float movementInput;
    private float verticalMovementInput;
    private float aimInput;
    private bool jumpInput;
    private bool jumpInputReleased;
    private bool teleportInputPressed;
    private bool teleportInputReleased;
    private bool throwInputPressed;
    private bool throwInputReleased;
    private bool throwInput;
    private bool cancelInput;
    private bool glideInput;
    private bool dragInput;
    private bool hideInput;
    private bool hideInputPressed;
    private bool hideInputReleased;
    private bool bohatInputPressed;
    private bool bohatInputReleased;
    private bool bohatInput;
    private bool hatarangInputPressed;
    private bool hatarangInputReleased;
    private bool createCheckpointInputPressed;
    private bool createCheckpointInputReleased;
    private bool interactionInputPressed;
    private List<Vector2> aimInputStored;
    private List<Vector2> aimInputStoredTranslated;
    private Vector3 firstAimInputStored;
    private Vector3 lastAimInputStored;
    private Vector3 lastAimInputStoredNotNormalized;
    private Vector3 firstAimInputStoredTranslated;
    private Vector3 lastAimInputStoredTranslated;
    private float blockedDir;
    private float scriptedMovementAmount;
    private int frameToWaitBI;
    //private float lastAimY;
    [SerializeField]
    private bool inputBlocked;
    [SerializeField]
    private bool directionBlocked;
    private bool scriptedMovement;
    [SerializeField]
    private bool interaction;
    private bool newInputBlocked;
    private bool newDirectionBlocked;
    private bool newInteraction;

    private Player player;

    private void Awake()
    {
        aimInputStored = new List<Vector2>();
        aimInputStoredTranslated = new List<Vector2>();
        GameController.instance.inputManager = this;
        player = ReInput.players.GetPlayer(0);
        newInputBlocked = inputBlocked;
        newDirectionBlocked = directionBlocked;
    }

    public void ResetToStart()
    {
        interaction = false;
        newInteraction = false;
        directionBlocked = false;
        newDirectionBlocked = false;
        inputBlocked = false;
        newInputBlocked = false;
        scriptedMovement = false;
    }

    // Use this for initialization
    void Start () {
        if (aimFramesToStore == 0)
            aimFramesToStore = 1;
        lastAimInputStoredNotNormalized = new Vector3(1, 1, 0);
        blockedDir = GameController.instance.startUpsideDown ? -1 : 1;
        interaction = false;
        newInteraction = false;
        //lastAimY = 1;
        //inputBlocked = false;
    }
	
	// Update is called once per frame
	void Update () {
        //if(Input.GetKeyDown(KeyCode.K))
        //{
        //    ForceDirection(false);
        //}
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    ForceDirection(true);
        //}
        //isJoystick = false;
        ////Input.GetJoystickNames().Length > 0 && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]);
        ////Debug.Log(Input.GetJoystickNames().Length);
        //foreach (string joystick in Input.GetJoystickNames())
        //{
        //    if (!string.IsNullOrEmpty(joystick))
        //        isJoystick = true;
        //}

#if UNITY_EDITOR || UNITY_STANDALONE
        Controller controller = player.controllers.GetLastActiveController();
        if (controller != null)
        {
            //ScreenDebug.instance.Log(controller.type.ToString());
            switch (controller.type)
            {
                case ControllerType.Keyboard:
                    // Do something for keyboard
                    isJoystick = false;
                    break;
                case ControllerType.Joystick:
                    // Do something for joystick
                    isJoystick = true;
                    break;
                case ControllerType.Mouse:
                    // Do something for mouse
                    isJoystick = false;
                    break;
                case ControllerType.Custom:
                    // Do something custom controller
                    isJoystick = true;
                    break;
            }
        }
#else
        isJoystick = true;
#endif

        //Debug.Log(isJoystick);

        pauseInput = player.GetButtonDown("Start");
        resumeInput = player.GetButtonDown("Back");
        movementInput = /*Input.GetAxis("Horizontal")*/ player.GetAxis("Horizontal");
        verticalMovementInput = player.GetAxis("VerticalMov");
        aimInput = player.GetAxis("Vertical");
        jumpInput = player.GetButtonDown("Jump");
        jumpInputReleased = player.GetButtonUp("Jump");
        teleportInputPressed = /*(Input.GetButtonDown("Fire1") ||*/ player.GetButtonDown("Teleport") /*leftTriggerDown )*/;
        teleportInputReleased = /*Input.GetButtonUp("Fire1") ||*/ player.GetButtonUp("Teleport");
        throwInputPressed = /*(Input.GetButtonDown("Fire1") ||*/ player.GetButtonDown("Throw") /*rightTriggerDown)*/;
        throwInputReleased = /*(Input.GetButtonUp("Fire1") ||*/ player.GetButtonUp("Throw");
        throwInput = /*(Input.GetButton("Fire1") ||*/ player.GetButton("Throw");
        cancelInput = player.GetButtonDown("Cancel") /* || leftTriggerDown*/;
        glideInput = player.GetButton("Glide") /*|| leftTrigger*/;
        dragInput = player.GetButton("Drag");
        hideInput = player.GetButton("Hide");
        hideInputPressed = player.GetButtonDown("Hide");
        hideInputReleased = player.GetButtonUp("Hide");
        bohatInputPressed = player.GetButtonDown("Bohat")/* || rightTriggerDown*/;
        bohatInputReleased = player.GetButtonUp("Bohat")/* || rightTriggerUp*/;
        bohatInput = player.GetButton("Bohat")/* || rightTrigger*/;
        hatarangInputPressed = player.GetButtonDown("Hatarang");
        hatarangInputReleased = player.GetButtonUp("Hatarang");
        createCheckpointInputPressed = player.GetButtonDown("CreateCheckpoint");
        createCheckpointInputReleased = player.GetButtonUp("CreateCheckpoint");
        interactionInputPressed = player.GetButtonDown("Interaction");

        //if (!controller)
        //{
        //    rightTriggerUp = false;
        //    leftTriggerUp = false;
        //    rightTriggerDown = false;
        //    leftTriggerDown = false;
        //    rightTrigger = false;
        //    leftTrigger = false;
        //}
        //else
        //{
        //    if (!alreadyRightUp && Input.GetAxis("Fire1") < triggerDeadZone)
        //    {
        //        rightTriggerUp = true;
        //        alreadyRightUp = true;
        //        //Debug.Log("destro su");
        //    }
        //    else if (alreadyRightUp)
        //    {
        //        rightTriggerUp = false;
        //        if (Input.GetAxis("Fire1") > triggerDeadZone)
        //        {
        //            alreadyRightUp = false;
        //        }
        //    }
        //    if (!alreadyRightDown && Input.GetAxis("Fire1") > triggerDeadZone)
        //    {
        //        rightTriggerDown = true;
        //        alreadyRightDown = true;
        //        //Debug.Log("destro giu");
        //    }
        //    else if (alreadyRightDown)
        //    {
        //        rightTriggerDown = false;
        //        if (Input.GetAxis("Fire1") < triggerDeadZone)
        //        {
        //            alreadyRightDown = false;
        //        }
        //    }
        //    if (!alreadyLeftUp && Input.GetAxis("Fire2") < triggerDeadZone)
        //    {
        //        leftTriggerUp = true;
        //        alreadyLeftUp = true;
        //    }
        //    else if (alreadyLeftUp)
        //    {
        //        leftTriggerUp = false;
        //        if (Input.GetAxis("Fire2") > triggerDeadZone)
        //        {
        //            alreadyLeftUp = false;
        //        }
        //    }
        //    if (!alreadyLeftDown && Input.GetAxis("Fire2") > triggerDeadZone)
        //    {
        //        leftTriggerDown = true;
        //        alreadyLeftDown = true;
        //    }
        //    else if (alreadyLeftDown)
        //    {
        //        leftTriggerDown = false;
        //        if (Input.GetAxis("Fire2") < triggerDeadZone)
        //        {
        //            alreadyLeftDown = false;
        //        }
        //    }

        //    rightTrigger = Input.GetAxis("Fire1") > triggerDeadZone;
        //    leftTrigger = Input.GetAxis("Fire2") > triggerDeadZone;
        //}

        if (isJoystick)
        {
            Vector2 lastAim;
            Vector2 lastAimTranslated;
            float aimX = 1f;
            /*if (Mathf.Abs(Input.GetAxis("Horizontal")) > aimingDeadZone)
            {
                lastAim = new Vector2((Input.GetAxis("Horizontal") > 0 ? 1 : -1), 1).normalized;
            }
            else if (aimInputStored.Count > 0)
                lastAim = new Vector2(aimInputStored[aimInputStored.Count - 1].x, aimInputStored[aimInputStored.Count - 1].y);
            else
               lastAim = new Vector2(1, 1).normalized;*/
            if(directionBlocked || inputBlocked)
            {
                aimX = blockedDir;
            }
            else if (GameController.instance.inputManager.MovementInput)
            {
                aimX = player.GetAxis("Horizontal") >= 0 ? 1 : -1;
            }
            else if (aimInputStored.Count > 0)
            {
                aimX = aimInputStored[aimInputStored.Count - 1].x >= 0 ? 1 : -1;
            }
            lastAim = new Vector2(aimX, Mathf.Pow(player.GetAxis("Vertical") + 1f, aimHeightRaiser)).normalized;
            lastAimTranslated = new Vector2(aimX, player.GetAxis("Vertical")).normalized;
            //float aimY = 1f;
            //aimY = lastAimY;
            //if (AimInput)
            //{
            //    aimY += Input.GetAxis("Vertical") * stickVerticalSensivity;
            //    aimY = Mathf.Clamp(aimY, 0, Mathf.Pow(2, aimHeightRaiser));
            //}
            //lastAim = new Vector2(aimX, aimY).normalized;
            //lastAimY = aimY;

            //Debug.Log(lastAim);

            //lastAim.y = (Input.GetAxis("Vertical") + 1f) / 2f;
            //lastAim.Normalize();

            /*if (Mathf.Abs(Input.GetAxis("HorizontalRight")) > triggerDeadZone || Mathf.Abs(Input.GetAxis("VerticalRight")) > triggerDeadZone)
                lastAim = new Vector2(Input.GetAxis("HorizontalRight"), Input.GetAxis("VerticalRight"));
            else if (aimInputStored.Count > 0)
                lastAim = new Vector2(aimInputStored[aimInputStored.Count - 1].x, aimInputStored[aimInputStored.Count - 1].y);
            else
                lastAim = new Vector2(1, 1).normalized;
            //teleportVelocityV3 = new Vector3(Input.GetAxis("HorizontalRight"), Input.GetAxis("VerticalRight"), 0);
            */
            while (aimInputStored.Count >= (aimFramesToStore - 1) && aimFramesToStore != 0)
            {
                aimInputStored.RemoveAt(0);
                aimInputStoredTranslated.RemoveAt(0);
            }
            aimInputStored.Add(lastAim);
            aimInputStoredTranslated.Add(lastAimTranslated);
            lastAimInputStored = new Vector3(lastAim.x, lastAim.y, 0);
            lastAimInputStoredTranslated = new Vector3(lastAimTranslated.x, lastAimTranslated.y, 0);
        }
        else
        {
            float aimX = 1f /*(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, 0)).x >= transform.position.x) ? 1 : -1*/;
            if (directionBlocked || inputBlocked)
            {
                aimX = blockedDir;
            }
            else if (Mathf.Abs(player.GetAxis("Horizontal")) > 0)
            {
                aimX = player.GetAxis("Horizontal") >= 0 ? 1 : -1;
            }
            else
            {
                aimX = lastAimInputStoredNotNormalized.x >= 0 ? 1 : -1;
            }
            float aimY = lastAimInputStoredNotNormalized.y;
            if (player.GetAxis("Mouse Y") != 0)
            {
                aimY += player.GetAxis("Mouse Y") * mouseVerticalSensivity;
                aimY = Mathf.Clamp(aimY, 0, Mathf.Pow(2, aimHeightRaiser));
            }
            lastAimInputStoredNotNormalized = new Vector3(aimX, aimY, 0);
            lastAimInputStored = lastAimInputStoredNotNormalized.normalized;
            //previewVelocityV3 = (Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - transform.position);
        }

        if (isJoystick)
        {
            if (aimInputStored.Count > 0)
            {
                firstAimInputStored = Vector3.Normalize(new Vector3(aimInputStored[0].x, aimInputStored[0].y, 0));
                firstAimInputStoredTranslated = Vector3.Normalize(new Vector3(aimInputStoredTranslated[0].x, aimInputStoredTranslated[0].y, 0));
            }
            else
            {
                firstAimInputStored = Vector3.Normalize(new Vector3(1, 1, 0));
            }
        }
        else
            firstAimInputStored = lastAimInputStored;

        if(!inputBlocked && !directionBlocked)
        {
            blockedDir = lastAimInputStored.x;
        }
    }

    private void LateUpdate()
    {
        if (frameToWaitBI > 0)
        {
            frameToWaitBI--;
        }
        else
        {
            inputBlocked = newInputBlocked;
            //directionBlocked = newDirectionBlocked;
        }
        interaction = newInteraction;
    }

    public void ResetAimInputStored()
    {
        bool right = true;
        if(isJoystick)
        {
            right = lastAimInputStored.x >= 0 ? true : false;
            aimInputStored.Clear();
            aimInputStored.Add(new Vector3(right ? 1 : -1, 1, 0).normalized);
            aimInputStoredTranslated.Clear();
            aimInputStoredTranslated.Add(new Vector3(right ? 1 : -1, 0, 0).normalized);
            //lastAimY = 1;
        }
        else
        {
            right = lastAimInputStoredNotNormalized.x >= 0 ? true : false;
            lastAimInputStoredNotNormalized = new Vector3(right ? 1 : -1, 1, 0);
            lastAimInputStored = lastAimInputStoredNotNormalized.normalized;
        }
    }

    public void ClearAimInputStored()
    {
        bool right = lastAimInputStoredNotNormalized.x >= 0 ? true : false; ;
        aimInputStored.Clear();
        aimInputStoredTranslated.Clear();
        lastAimInputStoredNotNormalized = new Vector3(right ? 1 : -1, 1, 0);
        lastAimInputStored = lastAimInputStoredNotNormalized.normalized;
        Vector3 temp = firstAimInputStored;
        Vector3 tempTrans = firstAimInputStoredTranslated;
        aimInputStored.Add(new Vector2(temp.x, temp.y));
        aimInputStoredTranslated.Add(new Vector2(tempTrans.x, tempTrans.y));
    }

    public void InputBlock(bool value)
    {
        newInputBlocked = value;
        frameToWaitBI = 1;
    }

    public void DirectionBlock(bool value)
    {
        directionBlocked = value;
        newDirectionBlocked = directionBlocked;
        //frameToWaitBI = 1;
    }

    public void ScriptedMovement(float amount)
    {
        scriptedMovement = true;
        scriptedMovementAmount = amount;
    }

    public void StopScriptedMovement()
    {
        scriptedMovement = false;
    }

    public void ForceDirection(bool right)
    {
        blockedDir = right ? 1 : -1;
        if (isJoystick)
        {
            aimInputStored[aimInputStored.Count - 1] =new Vector2( right ? 1 : -1, aimInputStored[aimInputStored.Count - 1].y);
            aimInputStoredTranslated[aimInputStoredTranslated.Count - 1] = new Vector2(right ? 1 : -1, aimInputStoredTranslated[aimInputStoredTranslated.Count - 1].y);
        }
        else
        {
            lastAimInputStoredNotNormalized = new Vector3(right ? 1 : -1, lastAimInputStoredNotNormalized.y, 0);
            lastAimInputStored = lastAimInputStoredNotNormalized.normalized;
        }
    }

    public void BlockedDir(float dir)
    {
        blockedDir = dir;
        blockedDir = Mathf.Clamp(blockedDir, -1, 1);
    }

    public void SetInteraction(bool value)
    {
        newInteraction = value;
    }
}
