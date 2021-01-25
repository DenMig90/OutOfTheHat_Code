using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;
using TMPro;

public enum GameState
{
    MainMenu,
    InGame,
    Pause,
    SecretPage
}

[System.Serializable]
public class UnityEventBool : UnityEvent<bool> { }
[System.Serializable]
public class UnityEventFloat : UnityEvent<float> { }

public delegate void VoidDelegate();

[System.Serializable]
public class CustomVector3
{
    public float x, y, z;

    public CustomVector3(float _x,float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }
}

[System.Serializable]
public class ObjectActiveState
{
    public int id;
    public string name;
    public bool active;

    public ObjectActiveState(int _id, string _name, bool _active)
    {
        id = _id;
        name = _name;
        active = _active;
    }
}

[System.Serializable]
public class ObjectActiveStateList
{
    public List<ObjectActiveState> list;

    public ObjectActiveStateList()
    {
        list = new List<ObjectActiveState>();
    }

    public void Add(ObjectActiveState obj)
    {
        list.Add(obj);
    }

    public void Add(int _id, string _name, bool _state)
    {
        list.Add(new ObjectActiveState(_id, _name, _state));
    }

    public bool ContainsId(int id)
    {
        bool found = false;
        foreach(ObjectActiveState obj in list)
        {
            if (obj.id == id)
            {
                found = true;
                break;
            }
        }
        return found;
    }

    public bool ContainsName(string name)
    {
        bool found = false;
        foreach (ObjectActiveState obj in list)
        {
            if (obj.name == name)
            {
                found = true;
                break;
            }
        }
        return found;
    }

    public void Set(int id, bool state)
    {
        for(int i = 0; i < list.Count; i++)
        {
            if (list[i].id == id)
            {
                list[i].active = state;
                break;
            }
        }
    }

    public bool Get(int id)
    {
        bool returnable = false; ;
        foreach (ObjectActiveState obj in list)
        {
            if (obj.id == id)
            {
                ObjectActiveState _obj = list[list.IndexOf(obj)];
                returnable = _obj.active;
                break;
            }
        }
        return returnable;
    }

    public void Set(string name, bool state)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].name == name)
            {
                list[i].active = state;
                break;
            }
        }
    }

    public bool Get(string name)
    {
        bool returnable = false; ;
        foreach (ObjectActiveState obj in list)
        {
            if (obj.name == name)
            {
                ObjectActiveState _obj = list[list.IndexOf(obj)];
                returnable = _obj.active;
                break;
            }
        }
        return returnable;
    }
}

[System.Serializable]
public class PlayerData
{
    public Vector3 lastSavedPosition;
    public Vector3 respawnJumpDirection;
    public Vector3 savedLastGroundedPosition;
    public bool glideUnlocked;
    public bool hatUnlocked;
    public bool teleportTutorialEnabled;
    public bool changeIdle;
}

[System.Serializable]
public class CameraData
{
    public float lastSavedSize;
    public float lastSavedSizeChangeSpeed;
    public float lastSavedOffsetChangeSpeed;
    public float lastSavedVerticalDeadzone;
    public Vector2 lastSavedOffset;
    //public bool savedStopped;
    //public bool savedFollowPlayer;
    //public bool savedBlockPlayer;
}

[System.Serializable]
public class SaveData
{
    public PlayerData playerData;
    public CameraData cameraData;
    public ObjectActiveStateList objectActiveStates;
    public ObjectActiveStateList bunniesActiveStates;
    //public ObjectActiveStateList seedsActiveStates;
    public int lastCheckpoint;
    public int checkpointSeeds;
    public bool canCreateCheckpoint;
    public CustomVector3 customCheckpointPos;
    public bool upsideDown;
    public int difficulty;
    public string actualAmbience;
    public string actualMusic;

    public SaveData()
    {
        playerData = new PlayerData();
        cameraData = new CameraData();
        objectActiveStates = new ObjectActiveStateList();
        bunniesActiveStates = new ObjectActiveStateList();
        //seedsActiveStates = new ObjectActiveStateList();
    }
}

[System.Serializable]
public class SecretData
{
    public string key;
    public bool discovered;

    public SecretData()
    {

    }

    public SecretData(string _key, bool _discovered)
    {
        key = _key;
        discovered = _discovered;
    }
}

public class SecretSaveData
{
    public List<SecretData> secrets;

    public SecretSaveData()
    {
        secrets = new List<SecretData>();
    }
}

public class GameController : MonoBehaviour {
    [HideInInspector]
    public static GameController instance;
    public UnityEvent onNewGame;
    public UnityEvent onRestart;
    [HideInInspector]
    public bool assistedMode = false;

    public int ActualCheckpointSeeds
    {
        get { return actualCheckpointSeeds; }
        set
        {
            actualCheckpointSeeds = value;
            if(seedCounter != null)
            seedCounter.SeedNumberValue(actualCheckpointSeeds);
            CheckCheckpointReminder();
        }
    }

    public float CameraSizeDelta
    {
        get { return mainCamera.SizeDelta; }
    }

    public SaveData SaveData
    {
        get { return saveData; }
    }

    public GameState ActualState
    {
        get { return actualState; }
        set
        {
            actualState = value;
            switch (actualState)
            {
                case GameState.MainMenu:
                case GameState.Pause:
                case GameState.SecretPage:
                    Cursor.visible = true;
                    previousInputBlock = inputManager.InputBlocked;
                    inputManager.InputBlock(true);
                    break;
                case GameState.InGame:
                    Cursor.visible = false;
                    inputManager.InputBlock(previousInputBlock);
                    break;
            }
        }
    }

    public Vector3 Right { get { return mainCamera.transform.right; } }
    public Vector3 Left { get { return -mainCamera.transform.right; } }
    public Vector3 Up { get { return mainCamera.transform.up; } }
    public Vector3 Down { get { return -mainCamera.transform.up; } }

    public bool UpsideDown
    {
        set { upsideDown = value;
            Physics.gravity = upsideDown ? -startGravity : startGravity;
            onUpsideDownDelegates(); }
        get { return upsideDown; }
    }

    public bool AllSecretsUnlocked
    {
        get { return allSecretsUnlocked; }
    }
    public bool alternativeEndingUnlocked = false;

    public CameraController mainCamera;
    public PlayerController player;
    public LevelManager levelmanager;
    public TutorialManager tutorialManager;
    public AudioManager audioManager;
    public InputManager inputManager;
    public DifficultyManager difficultyManager;
    public CustomCheckpointBehaviour customCheckpointPrefab;
    public TutorialTriggerBehaviour checkpointReminder;
    //public LocalizationManager localizationManager;
    public GameObject secretPageContainer;
    public UnityEngine.UI.Image secretPageImage;
    public GameObject secretCounterContainer;
    public UnityEngine.UI.Text secretCounterText;
    public SeedsCounterBehaviour seedCounter;
    public UnityEvent onSeedStart;
    public UnityEvent onSeedEnd;
    public UnityEvent onSeedCannot;
    public UnityEvent onCheckpointCreation;
    public UnityEvent onSeedAdded;
    public GameObject devMode;
    public GameObject checkpointContainer;
    public GameObject cameraTriggerContainer;
    public GameObject[] secretsGO;
    public List<GameObject> gameObjectsToSave;
    public float createCheckpointDelay;
    public float minCustomCheckDistanceFromOthers = 10f;
    public float checkpointablePosCheckDelay = 1;
    public float checkpointReminderMinDistance = 20f;
    public LayerMask createCheckpointLayer;
    //public GameObject[] gameobjectsToReset;
    //public Collider[] collidersToReset;
    public bool startUpsideDown;
    [Header("FMOD Events")]
    [EventRef] public string cannotCheckpointSound;
    [EventRef] public string createCheckpointSound;
    [EventRef] public string openSecretSound;
    [EventRef] public string closeSecretSound;
    [SerializeField]
    private GameState actualState;

    private VoidDelegate onDeathDelegates;
    private VoidDelegate onNewGameDelegates;
    private VoidDelegate onSaveStateDelegates;
    private VoidDelegate onLoadStateDelegates;
    private VoidDelegate onUpsideDownDelegates;
    private VoidDelegate onDifficultyChangedDelegates;
    private List<HandBehaviour> hands;
    //private List<PlayerKiller> playerKillers;
    //private List<BreakablePlatformBehaviour> breakablePlatforms;
    //private List<TrapBehaviour> traps;
    //private List<MovingPlatformController> movingPlatforms;
    //private List<EaterTrapBehaviour> eaterTraps;
    //private List<GunflowerBehaviour> gunflowers;
    //private List<BomblebeeBehaviour> bomblebees;
    //private List<DestroyableBehaviour> destroyableObjects;
    //private List<MovableBehaviour> movableBehaviours;
    //private List<BunnyBehaviour> bunnies;
    //private List<BunnyBehaviour> bunniesTR;
    private List<Checkpoint> checkpoints;
    [SerializeField]
    private List<GameObject> cameraTriggers;
    //private List<SoundTriggerBehaviour> soundTriggers;
    [SerializeField]
    private Dictionary<string, SecretBehaviour> secrets;
    [SerializeField]
    private CustomCheckpointBehaviour customCheckpoint;
    //private bool[] gameobjectsToResetStates;
    //private bool[] collidersToResetStates;
    private bool allSecretsUnlocked;
    private bool canDevModeShortcut;
    private bool previousInputBlock;
    private bool upsideDown;
    private bool canCreateCheckpoint;
    private bool canRemind;
    [SerializeField]
    private int actualCheckpointSeeds;
    private Vector3 startGravity;
    private Vector2 previousCheckpointablePos;
    private Vector2 lastCheckpointablePos;
    private float lastCheckpointableCheckTime;
    [SerializeField]
    private SaveData saveData;
    private SecretSaveData secretSaveData;
    //private string assistedSaveKey = "AssistedMode";
    private string saveDataKey = "SaveDataNew";
    private string secretSaveKey = "SecretSaveData";
    private float oldTimeScale;

    private void Awake()
    {
        instance = this;
        hands = new List<HandBehaviour>();
        //playerKillers = new List<PlayerKiller>();
        //breakablePlatforms = new List<BreakablePlatformBehaviour>();
        //traps = new List<TrapBehaviour>();
        //movingPlatforms = new List<MovingPlatformController>();
        //eaterTraps = new List<EaterTrapBehaviour>();
        //gunflowers = new List<GunflowerBehaviour>();
        //bomblebees = new List<BomblebeeBehaviour>();
        //destroyableObjects = new List<DestroyableBehaviour>();
        //movableBehaviours = new List<MovableBehaviour>();
        //bunnies = new List<BunnyBehaviour>();
        //bunniesTR = new List<BunnyBehaviour>();
        //secretSaveData = new SecretSaveData();
        checkpoints = new List<Checkpoint>();
        //soundTriggers = new List<SoundTriggerBehaviour>();
        secrets = new Dictionary<string, SecretBehaviour>();
        for (int i = 0; i < checkpointContainer.transform.childCount; i++)
        {
            Checkpoint child = checkpointContainer.transform.GetChild(i).gameObject.GetComponent<Checkpoint>();
            if (child != null)
                checkpoints.Add(child);
        }


        cameraTriggers = new List<GameObject>();
        if (cameraTriggerContainer != null)
        {
            for (int i = 0; i < cameraTriggerContainer.transform.childCount; i++)
            {
                cameraTriggers.Add(cameraTriggerContainer.transform.GetChild(i).gameObject);
            }
        }

        if (cameraTriggers.Count > 0)
        {
            bool toorder = true;
            while (toorder)
            {
                toorder = false;
                for (int i = 0; i < cameraTriggers.Count - 1; i++)
                {
                    if (cameraTriggers[i].transform.position.x > cameraTriggers[i + 1].transform.position.x)
                    {
                        GameObject aux = cameraTriggers[i];
                        cameraTriggers[i] = cameraTriggers[i + 1];
                        cameraTriggers[i + 1] = aux;
                        toorder = true;
                    }
                }
            }
        }

        if(secretsGO.Length != 0)
        {
            foreach(GameObject go in secretsGO)
            {
                SecretBehaviour bh = go.GetComponent<SecretBehaviour>();
                if (bh != null)
                    AddSecret(bh);
            }
        }

        //gameObjectsToSave = new List<GameObject>();

        TutorialTriggerBehaviour[] tutorialObjs = Resources.FindObjectsOfTypeAll<TutorialTriggerBehaviour>();
        foreach(TutorialTriggerBehaviour obj in tutorialObjs)
        {
            if (obj.gameObject.hideFlags == HideFlags.NotEditable || obj.gameObject.hideFlags == HideFlags.HideAndDontSave)
                continue;
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(obj.gameObject.transform.root.gameObject))
                continue;
#endif
            if (obj == checkpointReminder)
                continue;

            if (!gameObjectsToSave.Contains(obj.gameObject))
                gameObjectsToSave.Add(obj.gameObject);
        }

        //gameobjectsToResetStates = new bool[gameobjectsToReset.Length];
        //for (int i = 0; i < gameobjectsToReset.Length; i++)
        //    gameobjectsToResetStates[i] = gameobjectsToReset[i].activeSelf;
        //collidersToResetStates = new bool[collidersToReset.Length];
        //for (int i = 0; i < collidersToReset.Length; i++)
        //    collidersToResetStates[i] = collidersToReset[i].enabled;
        startGravity = Physics.gravity;
    }

    // Use this for initialization
    void Start () {
        previousInputBlock = false;
        canCreateCheckpoint = true;
        canRemind = false;
        inputManager.InputBlock(false);
        UpsideDown = startUpsideDown;
        //inputManager.InputBlock(true);
        if (PlayerPrefs.HasKey("Restart"))
        {
            PlayerPrefs.DeleteKey("Restart");
            StartCoroutine(NewGameDelayed());
        }
        else
        {
            ActualState = GameState.MainMenu;
            levelmanager.LoadMainMenu();
        }
        //assistedMode = GetAssistedModeSave();
        if(secretPageContainer!= null)
            secretPageContainer.SetActive(false);
        saveData = GetSaveData();
        if (saveData != null)
        {
            //saveData = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(saveDataKey));
            //Debug.Log("load " +saveData.playerData.lastSavedPosition);
            //Debug.Log(saveData.customCheckpointPos);
            if (saveData.customCheckpointPos != null)
            {
                CustomVector3 cv3 = saveData.customCheckpointPos;
                Vector3 pos = new Vector3(cv3.x, cv3.y, cv3.z);
                //Debug.Log("(" + saveData.customCheckpointPos.x + "," + saveData.customCheckpointPos.y + "," + saveData.customCheckpointPos.z + ")" + " " + pos);
                customCheckpoint = InstantiateCustomCheckpoint(new Vector3(cv3.x,cv3.y,cv3.z));
            }
            LoadSavedData();
        }
        else
        {
            StartConfigSaveData();
            mainCamera.ManageUpsideDown();
        }
        secretSaveData = GetSecretsSave();
        if (secretSaveData != null)
        {
            //Debug.Log("chiave esiste");
            //secretSaveData = JsonUtility.FromJson<SecretSaveData>(PlayerPrefs.GetString(secretSaveKey));
            //Debug.Log(PlayerPrefs.GetString(secretSaveKey));
            //Debug.Log("load " +saveData.playerData.lastSavedPosition);
            foreach(SecretData data in secretSaveData.secrets)
            {
                SecretBehaviour secret;
                if(secrets.TryGetValue(data.key, out secret))
                {
                    //Debug.Log("found");
                    secret.Discovered = data.discovered;
                }
            }
        }
        else
        {
            secretSaveData = new SecretSaveData();
        }
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(saveData.customCheckpointPos);
        //Debug.Log(seedsCounterText.text);
        /*if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}*/

        //		if (Input.GetKeyDown (KeyCode.R)) {
        //			int i = Application.loadedLevel;
        //			Application.LoadLevel (i);
        //		}
        //
        //if (Input.GetKeyDown (KeyCode.Alpha1)) {
        //	Application.LoadLevel ("Tutorialv2");
        //}

        //if (Input.GetKeyDown (KeyCode.Alpha2)) {
        //	Application.LoadLevel ("Medio");
        //}

        //if (Input.GetKeyDown (KeyCode.Alpha3)) {
        //	Application.LoadLevel ("Difficile");
        //}

        if (inputManager.PauseInput)
        {
            SetPause(true);
        }
        else if (inputManager.ResumeInput)
        {
            if (actualState == GameState.Pause)
                SetPause(false);
            else if (actualState == GameState.SecretPage)
                HideSecretPage();
        }

        if (inputManager.CreateCheckpointInputPressed)
        {
            //Debug.Log("creo");
            bool canCheckpoint = canCreateCheckpoint;
            canCheckpoint &= !player.IsBlocked;
            canCheckpoint &= ActualCheckpointSeeds > 0;
            canCheckpoint &= player.IsGrounded;
            canCheckpoint &= CheckDistanceFromOtherCheckpoints();
            canCheckpoint &= !player.lastGroundObject.GetComponent<MovingPlatformController>() && !player.lastGroundObject.GetComponent<ActualPlatformBehaviour>();
            if (canCheckpoint && !seedCounter.Closing)
            {
                StartCoroutine(CreateCheckpointCoroutine());
            }
            else
            {
                onSeedCannot.Invoke();
                seedCounter.SeedCannot(!canCreateCheckpoint);
                PlayCannotCheckpointSound();
            }
        }

        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKey(KeyCode.D))
        {
            if (canDevModeShortcut)
            {
                if (devMode != null)
                    devMode.gameObject.SetActive(!devMode.gameObject.activeSelf);
                Cursor.visible = devMode.gameObject.activeSelf;
                canDevModeShortcut = false;
            }
        }
        else
        {
            canDevModeShortcut = true;
        }

        //if (Input.GetKeyDown(KeyCode.N))
        //    NewGame();

        //if (Input.GetKeyDown(KeyCode.C))
        //    Continue();

        //if (Input.GetKeyDown(KeyCode.F1))
        //    levelmanager.LoadMainMenu();

        //if (Input.GetKeyDown(KeyCode.Delete))
        //    ClearSecretSavedData();

        //if (Input.GetKeyDown(KeyCode.F))
        //    FlipWorld();
    }

    private void LateUpdate()
    {
        if (player.lastGroundObject == null)
            return;
        if (!player.lastGroundObject.GetComponent<MovingPlatformController>() && !player.lastGroundObject.GetComponent<ActualPlatformBehaviour>() && (Time.timeSinceLevelLoad >= (lastCheckpointableCheckTime + checkpointablePosCheckDelay)))
        {
            GetCheckpointablePosition();
        }
    }

    private void GetCheckpointablePosition()
    {
        previousCheckpointablePos = lastCheckpointablePos;
        lastCheckpointablePos = new Vector2(player.LastGroundedPosition.x, player.LastGroundedPosition.y);
        lastCheckpointableCheckTime = Time.timeSinceLevelLoad;
    }

    public void AddHand(HandBehaviour hand)
    {
        hands.Add(hand);
    }

    public bool AllHandsReturned()
    {
        bool all = true;
        foreach(HandBehaviour hand in hands)
        {
            if (!hand.Returned)
                all = false;
        }
        return all;
    }

    //public void AddPlayerKiller(PlayerKiller killer)
    //{
    //    playerKillers.Add(killer);
    //}

    //public void AddBreakablePlatform(BreakablePlatformBehaviour platform)
    //{
    //    breakablePlatforms.Add(platform);
    //}

    //public void RemoveBreakablePlatform(BreakablePlatformBehaviour platform)
    //{
    //    breakablePlatforms.Remove(platform);
    //}

    //public void AddTrap(TrapBehaviour trap)
    //{
    //    traps.Add(trap);
    //}

    //public void AddMovingPlatform(MovingPlatformController platform)
    //{
    //    movingPlatforms.Add(platform);
    //}

    //public void AddEaterTrap(EaterTrapBehaviour eaterTrap)
    //{
    //    eaterTraps.Add(eaterTrap);
    //}

    //public void AddBomblebee(BomblebeeBehaviour bomblebee)
    //{
    //    if(!bomblebees.Contains(bomblebee))
    //        bomblebees.Add(bomblebee);
    //}

    //public void AddGunflower(GunflowerBehaviour gunflower)
    //{
    //    gunflowers.Add(gunflower);
    //}

    //public void RemoveBomblebee(BomblebeeBehaviour bomblebee)
    //{
    //    if (bomblebees.Contains(bomblebee))
    //        bomblebees.Remove(bomblebee);
    //}

    //public void AddDestroyable(DestroyableBehaviour destroyable)
    //{
    //    destroyableObjects.Add(destroyable);
    //}

    //public void AddMovable(MovableBehaviour movable)
    //{
    //    movableBehaviours.Add(movable);
    //}

    //public void AddBunny(BunnyBehaviour bunny)
    //{
    //    bunnies.Add(bunny);
    //}

    //public void AddBunnyTR(BunnyBehaviour bunny)
    //{
    //    bunniesTR.Add(bunny);
    //}

    //public void AddSoundTrigger(SoundTriggerBehaviour sound)
    //{
    //    soundTriggers.Add(sound);
    //}

    //public void RemoveSoundTrigger(SoundTriggerBehaviour sound)
    //{
    //    soundTriggers.Remove(sound);
    //}

    public void AddOnDeathDelegate(VoidDelegate action)
    {
        onDeathDelegates += action;
    }

    public void RemoveOnDeathDelegate(VoidDelegate action)
    {
        onDeathDelegates -= action;
    }

    public void AddOnNewGameDelegate(VoidDelegate action)
    {
        onNewGameDelegates += action;
    }

    public void RemoveOnNewGameDelegate(VoidDelegate action)
    {
        onNewGameDelegates -= action;
    }

    public void AddOnSaveStateDelegate(VoidDelegate action)
    {
        onSaveStateDelegates += action;
    }

    public void RemoveOnSaveStateDelegate(VoidDelegate action)
    {
        onSaveStateDelegates -= action;
    }

    public void AddOnLoadStateDelegate(VoidDelegate action)
    {
        onLoadStateDelegates += action;
    }

    public void RemoveOnLoadStateDelegate(VoidDelegate action)
    {
        onLoadStateDelegates -= action;
    }

    public void AddOnUpsideDown(VoidDelegate action)
    {
        onUpsideDownDelegates += action;
    }

    public void RemoveOnUpsideDown(VoidDelegate action)
    {
        onUpsideDownDelegates -= action;
    }

    public void AddOnDifficultyChanged(VoidDelegate action)
    {
        onDifficultyChangedDelegates += action;
    }

    public void RemoveOnDifficultyChanged(VoidDelegate action)
    {
        onDifficultyChangedDelegates -= action;
    }

    public void TriggerChangeDifficulty()
    {
        onDifficultyChangedDelegates();
    }

    public void AddSecret(SecretBehaviour secret)
    {
        if(!secrets.ContainsKey(secret.Key))
            secrets.Add(secret.Key, secret);
    }

    public void SetMainCamera(CameraController camera)
    {
        mainCamera = camera;
    }

    public void SetTutorialManager(TutorialManager tutorial)
    {
        tutorialManager = tutorial;
    }

    //public void SetLocalizationManager(LocalizationManager _new)
    //{
    //    localizationManager = _new;
    //}        

    public void OnDeath()
    {
        //tutorialManager.ResetToStart();

        onDeathDelegates();
        //Debug.Log("muoio");
        Vector3 checkpointReminderPos = new Vector3(previousCheckpointablePos.x, previousCheckpointablePos.y, 0);
        canRemind = true;
        if (customCheckpoint != null)
        {
            if (Vector3.Distance(checkpointReminderPos, customCheckpoint.transform.position) < checkpointReminderMinDistance)
                canRemind = false;
        }
        if (canRemind)
        {
            foreach (Checkpoint checkpoint in checkpoints)
            {
                if (Vector3.Distance(checkpointReminderPos, checkpoint.transform.position) < checkpointReminderMinDistance)
                    canRemind = false;
            }
        }
        if (canRemind && checkpointReminder != null)
        {
            checkpointReminder.transform.position = checkpointReminderPos;
        }
        CheckCheckpointReminder();
        //checkpointReminder.ResetToStart();
        //checkpointReminder.gameObject.SetActive(true);

        //audioManager.ResetToStart();

        //foreach (PlayerKiller killer in playerKillers)
        //{
        //    killer.ResetToStart();
        //}

        //foreach(BreakablePlatformBehaviour platfrom in breakablePlatforms)
        //{
        //    platfrom.ResetToStart();
        //}

        //foreach (TrapBehaviour trap in traps)
        //{
        //    trap.ResetToStart();
        //}

        //foreach (MovingPlatformController platform in movingPlatforms)
        //{
        //    platform.ResetToStart();
        //}

        //foreach (EaterTrapBehaviour eaterTrap in eaterTraps)
        //{
        //    eaterTrap.ResetToStart();
        //}

        //foreach (GunflowerBehaviour gunflower in gunflowers)
        //{
        //    gunflower.ResetToStart();
        //}

        //foreach (BomblebeeBehaviour bomblebee in bomblebees)
        //{
        //    bomblebee.ResetToStart();
        //}

        //foreach (DestroyableBehaviour destroyable in destroyableObjects)
        //{
        //    destroyable.ResetToStart();
        //}

        //foreach (MovableBehaviour movable in movableBehaviours)
        //{
        //    movable.ResetToStart();
        //}

        //foreach (BunnyBehaviour bunny in bunnies)
        //{
        //    bunny.ResetToStart();
        //}

        //foreach (BunnyBehaviour bunny in bunnies)
        //{
        //    bunny.ResetToStart();
        //}

        //foreach (SoundTriggerBehaviour sound in soundTriggers)
        //{
        //    sound.ResetToStart();
        //}

        LoadSavedData();

        GetCheckpointablePosition();
        inputManager.DirectionBlock(false);

        //mainCamera.Follow();
        //mainCamera.Block(false);
    }

    //   public void NextLevel(){
    //	int i = Application.loadedLevel;
    //	Application.LoadLevel( i +1 );
    //}



    //public void ReloadLevel(){
    //	int i = Application.loadedLevel;
    //	Application.LoadLevel(i);
    //}

    //public void LoadLevel1(){
    //	int i = Application.loadedLevel;
    //       Application.LoadLevel(0);
    //}

    public void StartInteraction(UnityAction action)
    {
        player.AddInteraction(action);
        inputManager.SetInteraction(true);
        tutorialManager.StartTutorial(TutorialAction.Interaction);
    }

    public void EndInteraction(UnityAction action)
    {
        player.RemoveInteraction(action);
        inputManager.SetInteraction(false);
        tutorialManager.StopTutorial(TutorialAction.Interaction);
    }

    private void CheckCheckpointReminder()
    {
        if(checkpointReminder != null)
        checkpointReminder.gameObject.SetActive(canRemind && actualCheckpointSeeds > 0 && canCreateCheckpoint);
    }

    public void AddCheckpointSeeds(int seeds)
    {
        ActualCheckpointSeeds += seeds;
        onSeedAdded.Invoke();
        seedCounter.SeedAdded();
    }

    public void BlockCheckpointCreation(bool value)
    {
        canCreateCheckpoint = !value;
        CheckCheckpointReminder();
    }

    public void OnSecretDiscovered()
    {
        bool all = true;
        allSecretsUnlocked = false;
        foreach (SecretBehaviour secret in secrets.Values)
        {
            if (!secret.Discovered)
                all = false;
        }
        if (all)
        {
            allSecretsUnlocked = true;
            Debug.Log("All Secrets discovered");
        }
    }

    public void ShowSecretPage(Sprite page)
    {
        ActualState = GameState.SecretPage;
        secretPageContainer.SetActive(true);
        seedCounter.Disable();
        secretPageImage.sprite = page;
        PlayOpenSecretSound();
        ScaleTime(0);
    }

    public void ShowSecretCounter()
    {
        int discovered = 0;
        foreach (SecretBehaviour secret in secrets.Values)
        {
            if (secret.Discovered)
                discovered++;
        }
        secretCounterText.text = discovered + "/" + secrets.Values.Count;
        secretCounterContainer.gameObject.SetActive(true);
    }

    public void HideSecretPage()
    {
        ActualState = GameState.InGame;
        secretPageImage.GetComponent<AnimateScale>().PlayBackward();
        PlayCloseSecretSound();
        ScaleTime(1);
        //secretPageContainer.SetActive(false);
    }

    public void ScaleTime(float value)
    {
        oldTimeScale = Time.timeScale;
        Time.timeScale = value;
        //Debug.Log("cambio tempo " + value);
    }

    public void Restart()
    {
        audioManager.DestroyAllInstances();
        ClearSavedData();
        ClearSecretSavedData();
        //levelmanager.UnloadMainMenu();
        ScaleTime(1);
        //onRestart.Invoke();
        PlayerPrefs.SetInt("Restart", 1);
        levelmanager.ReloadLevel();
    }

    public void NewGame()
    {
        ActualState = GameState.InGame;
        levelmanager.UnloadMainMenu();
        if (player.HasStarted)
            onRestart.Invoke();
        ClearSavedData();
        ClearSecretSavedData();
        if(customCheckpoint != null)
            Destroy(customCheckpoint.gameObject);
        //player.wakeUpAtStart = false;
        //player.ResetToStart();
        //mainCamera.ResetToStart();
        //audioManager.ResetToStart();
        //tutorialManager.ResetToStart();

        inputManager.ResetToStart();

        onNewGameDelegates();
        StartConfigSaveData();

        UpsideDown = startUpsideDown;
        mainCamera.ManageUpsideDown();
        actualCheckpointSeeds = 0;
        canRemind = false;

        foreach (SecretBehaviour secret in secrets.Values)
        {
            secret.gameObject.SetActive(true);
        }

        //foreach (Checkpoint checkpoint in checkpoints)
        //{
        //    checkpoint.ResetToStart();
        //}
        //foreach (BunnyBehaviour bunny in bunniesTR)
        //{
        //    bunny.ResetToBeginning();
        //}
        //foreach (PlayerKiller killer in playerKillers)
        //{
        //    killer.ResetToStart();
        //}

        //foreach (BreakablePlatformBehaviour platfrom in breakablePlatforms)
        //{
        //    platfrom.ResetToStart();
        //}

        //foreach (TrapBehaviour trap in traps)
        //{
        //    trap.ResetToStart();
        //}

        //foreach (MovingPlatformController platform in movingPlatforms)
        //{
        //    platform.ResetToStart();
        //}

        //foreach (EaterTrapBehaviour eaterTrap in eaterTraps)
        //{
        //    eaterTrap.ResetToStart();
        //}

        //foreach (BunnyBehaviour bunny in bunnies)
        //{
        //    bunny.ResetToStart();
        //}

        //foreach (SoundTriggerBehaviour sound in soundTriggers)
        //{
        //    sound.ResetToStart();
        //}
        //for (int i = 0; i < gameobjectsToReset.Length; i++)
        //    gameobjectsToReset[i].SetActive(gameobjectsToResetStates[i]);
        //for (int i = 0; i < collidersToReset.Length; i++)
        //    collidersToReset[i].enabled = collidersToResetStates[i];
        onNewGame.Invoke();
        inputManager.ForceDirection(true);
        player.WakeUp();
        ScaleTime(1);
    }

    public void Continue()
    {
        ActualState = GameState.InGame;
        levelmanager.UnloadMainMenu();
        //LoadSavedData();
    }

    public void SetPause(bool value)
    {
        if(value)
        {
            //Debug.Log("pauso");
            ActualState = GameState.Pause;
            player.CancelLaunch();
            levelmanager.LoadPauseMenu();
            ScaleTime(0);
        }
        else
        {
            //Debug.Log("unpauso");
            ActualState = GameState.InGame;
            levelmanager.UnloadPauseMenu();
            ScaleTime(oldTimeScale);
        }
        audioManager.PauseAllSounds(value);
    }

    public void LoadSavedData()
    {
        UpsideDown = saveData.upsideDown;
        ActualCheckpointSeeds = saveData.checkpointSeeds;
        canCreateCheckpoint = saveData.canCreateCheckpoint;
        foreach (GameObject obj in gameObjectsToSave)
        {
            if (obj != null)
            {
                bool state = obj.activeSelf;
                state = saveData.objectActiveStates.Get(GetHierarchyName(obj));
                obj.SetActive(state);
            }
            else
            {
                Debug.LogWarning("There is a missing object in \"gameobjectsToSave\"");
            }
        }
        onLoadStateDelegates();
        mainCamera.LoadSaved();
        player.LoadSaved();
        difficultyManager.ChangeDifficulty((Difficulty)saveData.difficulty);
    }

    public void AssistedMode(bool value)
    {
        //assistedMode = value;
        //UpdateAssistedModeSave();
        difficultyManager.ChangeDifficulty(value ? Difficulty.Easy : Difficulty.Normal);
        saveData.difficulty = (int)difficultyManager.ActualDifficulty;
        UpdateSaveData();
        WriteSavings();
    }

    public void ClearSavedData()
    {
        PlayerPrefs.DeleteKey(saveDataKey);
    }

    public void StartConfigSaveData()
    {
        saveData = new SaveData();
        player.Save(player.transform.position, player.transform.up);
        mainCamera.Save();
        saveData.lastCheckpoint = -1;
        saveData.checkpointSeeds = 0;
        saveData.customCheckpointPos = null;
        saveData.upsideDown = UpsideDown;
        saveData.canCreateCheckpoint = canCreateCheckpoint;
        saveData.difficulty = (int)difficultyManager.ActualDifficulty;
        foreach (GameObject obj in gameObjectsToSave)
        {
            if(obj == null)
            {
                Debug.LogError("Objects to save contains an empty value");
                continue;
            }
            bool state = obj.activeSelf;
            saveData.objectActiveStates.Add(obj.GetInstanceID(), GetHierarchyName(obj), state);
        }
    }

    private void WriteSavings()
    {
        PlayerPrefs.Save();
    }

    //private void UpdateAssistedModeSave()
    //{
    //    PlayerPrefs.SetInt(assistedSaveKey, assistedMode ? 0 : 1);
    //}

    private void UpdateSaveData()
    {
        PlayerPrefs.SetString(saveDataKey, JsonUtility.ToJson(saveData));
        //Debug.Log(JsonUtility.ToJson(saveData));
    }

    private void UpdateSecretsSave()
    {
        PlayerPrefs.SetString(secretSaveKey, JsonUtility.ToJson(secretSaveData));
    }

    //private bool GetAssistedModeSave()
    //{
    //    bool returnable;
    //    returnable = PlayerPrefs.GetInt(assistedSaveKey, assistedMode ? 0 : 1) == 0;
    //    return returnable;
    //}

    private SaveData GetSaveData()
    {
        SaveData returnable = null;
        if (PlayerPrefs.HasKey(saveDataKey))
            returnable = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(saveDataKey));
        return returnable;
    }

    private SecretSaveData GetSecretsSave()
    {
        SecretSaveData returnable = null;
        if (PlayerPrefs.HasKey(secretSaveKey))
            returnable = JsonUtility.FromJson<SecretSaveData>(PlayerPrefs.GetString(secretSaveKey));
        return returnable;
    }

    public void SetActualCheckpoint(Checkpoint actual, Vector3 playerPos, Vector3 respawnDir, bool hasToSave)
    {
        int actualIndex = 0;
        //Debug.Log("chiamato");
        if (checkpoints.Count == 0)
            return;
        //saveData.lastCheckpoint = 0;
        //Debug.Log(actual.gameObject.name);
        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (checkpoints[i] == actual)
                actualIndex = i;
        }
        //Debug.Log(actualIndex);
        if (actualIndex == saveData.lastCheckpoint)
            return;
        //Debug.Log("salvo");
        saveData.lastCheckpoint = actualIndex;
        SetCheckpoint(playerPos, respawnDir, hasToSave);
    }

    public void SetCheckpoint(Vector3 playerPos, Vector3 respawnDir, bool hasToSave)
    {
        saveData.checkpointSeeds = actualCheckpointSeeds;
        saveData.upsideDown = UpsideDown;
        saveData.canCreateCheckpoint = canCreateCheckpoint;
        saveData.difficulty = (int)difficultyManager.ActualDifficulty;
        player.Save(playerPos, respawnDir);
        mainCamera.Save();
        foreach (GameObject obj in gameObjectsToSave)
        {
            bool state = obj.activeSelf;
            if (saveData.objectActiveStates.ContainsName(GetHierarchyName(obj)))
                saveData.objectActiveStates.Set(GetHierarchyName(obj),state);
            else
                saveData.objectActiveStates.Add(obj.GetInstanceID(), GetHierarchyName(obj), state);
        }

        onSaveStateDelegates();

        if (hasToSave)
        {
            UpdateSaveData();
            //Debug.Log(JsonUtility.ToJson(saveData));
            //Debug.Log("save " +saveData.playerData.lastSavedPosition);
            WriteSavings();
        }
    }

    public void ClearSecretSavedData()
    {
        PlayerPrefs.DeleteKey(secretSaveKey);
    }

    public bool CheckDistanceFromOtherCheckpoints()
    {
        Vector3 pos = player.plantingPosition.position;
        if (customCheckpoint != null && Vector3.Distance(customCheckpoint.transform.position, pos) < minCustomCheckDistanceFromOthers)
            return false;
        foreach(Checkpoint cp in checkpoints)
        {
            if (cp.gameObject.activeSelf && Vector3.Distance(cp.transform.position, pos) < minCustomCheckDistanceFromOthers)
                return false;
        }
        return true;
    }

    public void CreateCustomCheckpoint()
    {
        ActualCheckpointSeeds--;
        if (customCheckpoint != null)
            Destroy(customCheckpoint.gameObject);
        Vector3 pos = player.plantingPosition.position;
        //RaycastHit hit;
        //if (Physics.Raycast(pos, Down, out hit, player.Collider.bounds.extents.y*2f, player.groundLayer, QueryTriggerInteraction.Ignore))
        //{
        //    pos = hit.point;
        //    Debug.Log("hittato");
        //}
        customCheckpoint = InstantiateCustomCheckpoint(pos);
        customCheckpoint.Grow();
        PlayCreateCheckpointSound();
        saveData.customCheckpointPos = new CustomVector3(customCheckpoint.transform.position.x, customCheckpoint.transform.position.y, customCheckpoint.transform.position.z);
        //Debug.Log(customCheckpoint.transform.position + " " + "(" + saveData.customCheckpointPos.x + "," + saveData.customCheckpointPos.y + "," + saveData.customCheckpointPos.z +")");
        SetCheckpoint(customCheckpoint.playerRespawn.position, customCheckpoint.playerRespawn.up, true);
    }

    private CustomCheckpointBehaviour InstantiateCustomCheckpoint(Vector3 pos)
    {
        customCheckpoint = Instantiate(customCheckpointPrefab, pos, Quaternion.identity);

        RaycastHit hit;
        if(Physics.Raycast(pos + Vector3.up,-customCheckpoint.transform.up,out hit))
        {
            BouncingAnimation mushroom = null;
            mushroom = hit.collider.GetComponent<BouncingAnimation>();

            if (mushroom)
            {
                mushroom.AddObjectOnMushroom(customCheckpoint.gameObject);
            }
        }

        return customCheckpoint;
    }

    public void SaveSecrets()
    {
        foreach(SecretBehaviour secret in secrets.Values)
        {
            bool contains = false;
            foreach(SecretData data in secretSaveData.secrets)
            {
                if(data.key == secret.Key)
                {
                    contains = true;
                    data.discovered = secret.Discovered;
                    break;
                }
            }
            if (!contains)
            {
                secretSaveData.secrets.Add(new SecretData(secret.Key, secret.Discovered));
            }
        }
        UpdateSecretsSave();
        WriteSavings();
    }

    public void NextCheckpoint()
    {
        if (saveData.lastCheckpoint >= checkpoints.Count - 1)
            return;
        int check = saveData.lastCheckpoint+1;
        //saveData.lastCheckpoint++;

        if (cameraTriggers.Count > 0)
        {
            int index = 0;
            for (int i = 0; i < cameraTriggers.Count; i++)
            {
                if (cameraTriggers[i].transform.position.x > checkpoints[check].transform.position.x)
                    break;
                index = i;
            }
            bool size = false;
            bool offsetX = false;
            bool offsetY = false;
            bool offsetSpeed = false;
            bool sizeSpeed = false;
            string sizeStr = "DistanceCamera";
            string offsetXStr = "ChangeOffsetY";
            string offsetYStr = "ChangeOffsetX";
            string offsetSpeedStr = "OffsetChangeSpeed";
            string sizeSpeedStr = "DistanceChangeSpeed";

            List<UnityEvent> events = new List<UnityEvent>();

            for (int i = index; i>= 0; i--)
            {
                UnityEvent eventTr = cameraTriggers[i].GetComponent<TriggerEvent>().eventsToTrigger;
                events.Add(eventTr);
                for(int j = 0; j < eventTr.GetPersistentEventCount(); j++)
                {
                    if (eventTr.GetPersistentMethodName(j) == sizeStr)
                        size = true;
                    if (eventTr.GetPersistentMethodName(j) == offsetXStr)
                        offsetX = true;
                    if (eventTr.GetPersistentMethodName(j) == offsetYStr)
                        offsetY = true;
                    if (eventTr.GetPersistentMethodName(j) == offsetSpeedStr)
                        offsetSpeed = true;
                    if (eventTr.GetPersistentMethodName(j) == sizeSpeedStr)
                        sizeSpeed = true;
                }
                if (size && offsetX && offsetY && offsetSpeed && sizeSpeed)
                    break;
            }

            for(int i = events.Count-1; i >= 0; i--)
            {
                events[i].Invoke();
            }
        }

        //Debug.Log(actualCheckpoint);
        player.enabled = false;
        player.transform.position = new Vector3(checkpoints[check].transform.position.x, checkpoints[check].transform.position.y, player.transform.position.z);
        player.enabled = true;
        SetActualCheckpoint(checkpoints[check], checkpoints[check].playerRespawn.position, checkpoints[check].playerRespawn.up, !checkpoints[check].isFake);
    }

    public void PreviousCheckpoint()
    {
        if (saveData.lastCheckpoint == 0)
            return;
        int check = saveData.lastCheckpoint-1;

        if (cameraTriggers.Count > 0)
        {
            int index = 0;
            for (int i = 0; i < cameraTriggers.Count; i++)
            {
                if (cameraTriggers[i].transform.position.x > checkpoints[check].transform.position.x)
                    break;
                index = i;
            }
            bool size = false;
            bool offsetX = false;
            bool offsetY = false;
            bool offsetSpeed = false;
            bool sizeSpeed = false;
            string sizeStr = "DistanceCamera";
            string offsetXStr = "ChangeOffsetY";
            string offsetYStr = "ChangeOffsetX";
            string offsetSpeedStr = "OffsetChangeSpeed";
            string sizeSpeedStr = "DistanceChangeSpeed";

            List<UnityEvent> events = new List<UnityEvent>();

            for (int i = index; i >= 0; i--)
            {
                UnityEvent eventTr = cameraTriggers[i].GetComponent<TriggerEvent>().eventsToTrigger;
                events.Add(eventTr);
                for (int j = 0; j < eventTr.GetPersistentEventCount(); j++)
                {
                    if (eventTr.GetPersistentMethodName(j) == sizeStr)
                        size = true;
                    if (eventTr.GetPersistentMethodName(j) == offsetXStr)
                        offsetX = true;
                    if (eventTr.GetPersistentMethodName(j) == offsetYStr)
                        offsetY = true;
                    if (eventTr.GetPersistentMethodName(j) == offsetSpeedStr)
                        offsetSpeed = true;
                    if (eventTr.GetPersistentMethodName(j) == sizeSpeedStr)
                        sizeSpeed = true;
                }
                if (size && offsetX && offsetY && offsetSpeed && sizeSpeed)
                    break;
            }

            for (int i = events.Count - 1; i >= 0; i--)
            {
                events[i].Invoke();
            }
        }

        player.enabled = false;
        player.transform.position = new Vector3(checkpoints[check].transform.position.x, checkpoints[check].transform.position.y, player.transform.position.z);
        player.enabled = true;
        SetActualCheckpoint(checkpoints[check], checkpoints[check].playerRespawn.position, checkpoints[check].playerRespawn.up, !checkpoints[check].isFake);
    }

    public void UnlockAltEnding()
    {
        alternativeEndingUnlocked = true;
    }

    public void FlipWorld()
    {
        mainCamera.StartFlipping();
    }

    public void InvertGravity()
    {
        UpsideDown = !UpsideDown;
    }

    private IEnumerator CreateCheckpointCoroutine()
    {
        //Debug.Log(seedCounterOpened);
        onSeedStart.Invoke();
        seedCounter.SeedStart();
        player.StartPlanting();
        float time = 0;
        bool broken = false;
        while (time < createCheckpointDelay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
            seedCounter.CheckpointSliderValue(time / createCheckpointDelay);
            if(inputManager.CreateCheckpointInputReleased || !player.IsGrounded)
            {
                broken = true;
                break;
            }
        }
        if (!broken)
        {
            CreateCustomCheckpoint();
            onCheckpointCreation.Invoke();
        }
        seedCounter.CheckpointSliderValue(0);
        //seedCounterClosing = true;
        onSeedEnd.Invoke();
        seedCounter.SeedEnd();
        player.EndPlanting();
    }

    private IEnumerator NewGameDelayed()
    {
        yield return new WaitForSeconds(1);
        NewGame();
    }

    public void PlayCannotCheckpointSound()
    {
        audioManager.PlayGenericSound(cannotCheckpointSound, player.gameObject);
    }

    public void PlayCreateCheckpointSound()
    {
        audioManager.PlayGenericSound(createCheckpointSound, player.gameObject);
    }

    public void PlayOpenSecretSound()
    {
        audioManager.PlayGenericSound(openSecretSound, player.gameObject);
    }

    public void PlayCloseSecretSound()
    {
        audioManager.PlayGenericSound(closeSecretSound, player.gameObject);
    }

    private string GetHierarchyName(GameObject obj)
    {
        string name = obj.name;
        if (obj.transform.parent != null)
        {
            name = GetHierarchyName(obj.transform.parent.gameObject) + "." + name;
        }
        return name;
    }

    private void OnDestroy()
    {
        Physics.gravity = startGravity;
    }
}


