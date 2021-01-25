using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using FMOD;
using UnityEngine.SceneManagement;
using Debug = FMOD.Debug;

[System.Serializable]
public class SoundParameters
{
    public string paramName;
    public float paramValue;

    public SoundParameters(string name, float value)
    {
        paramName = name;
        paramValue = value;
    }
}

public class AudioManager : MonoBehaviour {

    #region PublicVariables

    //[EventRef] [SerializeField] private string battle, menuHover, menuClick, level;

    public float startFadeInDuration = 0.5f;
    [EventRef] [SerializeField] private string ambienceSound;
    [EventRef] [SerializeField] private string ambienceMusic;

    //[EventRef] [SerializeField] private string fury;


    private EventInstance musicInstance;
    private EventInstance ambienceInstance;
    //private EventInstance battleInstance;
    //private EventInstance furyInstance;

    private Rigidbody player;
    //private Transform playerTransform;
    //private float musicPar = 0;
    private List<EventInstance> allInstances;
    private Dictionary<EventInstance, bool> pauseStates;
    private float actualPitch = 0;
    private const float actualPitchScale = 0f;
    private string actualAmbience;
    private string actualMusic;
    private bool started;

    public float masterVolume = 1, musicVolume = 1, fxVolume = 1;

    //private bool musicBattle;
    #endregion
    
    #region PrivateVariables

    private GameObject menuListener;
    
    #endregion

    #region MonobehaviourMethods
    private void Awake()
    {
        musicVolume = PlayerPrefs.GetFloat("musicVolume", 1);
        fxVolume = PlayerPrefs.GetFloat("fxVolume", 1);
        masterVolume = PlayerPrefs.GetFloat("masterVolume", 1);
        GameController.instance.audioManager = this;
        allInstances = new List<EventInstance>();
        pauseStates = new Dictionary<EventInstance, bool>();
        StartCoroutine(AllSoundsFadeIn(startFadeInDuration));
        //GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        GameController.instance.AddOnSaveStateDelegate(OnSave);
        GameController.instance.AddOnLoadStateDelegate(OnLoad);
        actualAmbience = ambienceSound;
        actualMusic = ambienceMusic;
        started = false;
    }

    public void OnSave()
    {
        GameController.instance.SaveData.actualAmbience = actualAmbience;
        GameController.instance.SaveData.actualMusic = actualMusic;
    }

    public void OnLoad()
    {
        if (started)
        {
            if(actualAmbience != GameController.instance.SaveData.actualAmbience)
                ChangeAmbience(GameController.instance.SaveData.actualAmbience);
            if(actualMusic != GameController.instance.SaveData.actualMusic)
                ChangeMusic(GameController.instance.SaveData.actualMusic, false);
        }
        else
        {
            actualAmbience = GameController.instance.SaveData.actualAmbience;
            actualMusic = GameController.instance.SaveData.actualMusic;
        }
    }

    private void Start()
    {
        actualPitch = ((Time.timeScale-1f)* actualPitchScale) +1f;
        ambienceInstance = PlayGenericSound(actualAmbience, GameController.instance.player.gameObject);
        musicInstance = PlayGenericSound(actualMusic, GameController.instance.player.gameObject);
        musicInstance.setVolume(musicVolume * masterVolume);
        player = GameController.instance.player.GetComponent<Rigidbody>();
        //playerTransform = GameController.instance.player.transform;
        //if (FindObjectOfType<MusicSelector>())
        //{
        //    if (FindObjectOfType<MusicSelector>().sceneMusic != "")
        //    {
        //        level = FindObjectOfType<MusicSelector>().sceneMusic;
        //        isHarbor = FindObjectOfType<MusicSelector>().isHarbor;
        //    }

        //    if (FindObjectOfType<MusicSelector>().sceneBackground != "")
        //        ambience = FindObjectOfType<MusicSelector>().sceneBackground;
        //}
        //if (GameState.Instance.CurrentState == State.InGame)
        //{
        //    if (GameState.Instance.Player != null)
        //    {
        //        player = GameState.Instance.Player.GetComponent<Rigidbody>();
        //        playerTransform = player.transform;
        //    }
        //}
        //if (GameState.Instance.CurrentState == State.MainMenu)
        //{
        //    if (level != "")
        //    {
        //        StudioListener listener = Camera.main.gameObject.AddComponent<StudioListener>();
        //        menuListener = listener.gameObject;

        //        //UnityEngine.Debug.Log(listener.gameObject);

        //        musicInstance = PlayGenericSound(level, Camera.main.gameObject, "", 0);
        //        musicInstance.setVolume(musicVolume * masterVolume);
        //    }
        //}
        //else if (SceneManager.GetActiveScene().buildIndex >= GameState.Instance.LevelManager.FirstLevelIndex + 1)
        //{

        //    if (ambience != "")
        //    {
        //        ambienceInstance = RuntimeManager.CreateInstance(ambience);
        //        ambienceInstance.start();
        //        RuntimeManager.AttachInstanceToGameObject(ambienceInstance, playerTransform, player);
        //        ambienceInstance.setVolume(musicVolume * masterVolume);

        //    }
        //    if (level != "")
        //    {               
        //        musicInstance = PlayGenericSound(level, playerTransform.gameObject, "Battle", 0);
        //        musicInstance.setVolume(musicVolume * masterVolume);
        //    }

        //}
        //SceneManager.activeSceneChanged += OnChangeScene;    
        started = true;
    }


    //private void OnChangeScene(Scene first, Scene newScene)
    //{
    //    StartCoroutine(WaitOnLoad(first, newScene));
    //}

    private void OnEnable()
    {
        //EventManager.StartListening(Events.changeState, OnChangeState);
        //EventManager.StartListening(Events.reset, OnReset);
    }
    private void OnDisable()
    {
        //EventManager.StopListening(Events.changeState, OnChangeState);
        //EventManager.StopListening(Events.reset, OnReset);
    }

    private void Update()
    {
        if (actualPitch != ((Time.timeScale - 1f) * actualPitchScale) + 1f)
        {
            actualPitch = ((Time.timeScale - 1f) * actualPitchScale) + 1f;
            foreach (EventInstance instance in allInstances)
                instance.setPitch(actualPitch);
        }
    }

    #endregion

    #region PublicMethods
    public void PauseAllSounds(bool value)
    {
        foreach (EventInstance instance in allInstances)
        {
            if(value)
            {
                bool state;
                instance.getPaused(out state);
                pauseStates.Add(instance, state);
                instance.setPaused(value);
                //UnityEngine.Debug.Log(instance.handle);
            }
            else
            {
                bool state = true;
                //UnityEngine.Debug.Log(pauseStates.ContainsKey(instance.GetHashCode()));
                pauseStates.TryGetValue(instance, out state);
                instance.setPaused(state);
            }
        }
        if(!value)
            pauseStates.Clear();
    }
    //public void OnSelectMenu()
    //{
    //    if (menuListener == null && GameState.Instance.CurrentState != State.MainMenu)
    //    {

    //        menuListener = GameState.Instance.Player.gameObject;
    //    }
    //    else if (menuListener == null)
    //    {
    //        menuListener = FindObjectOfType<StudioListener>().gameObject;
    //    }

    //    if (menuHover != "")
    //        PlayGenericSound(menuHover, menuListener, "Menu", 0);
    //}

    //public void OnClickMenu()
    //{
    //    if(menuListener == null && GameState.Instance.CurrentState != State.MainMenu)
    //        menuListener = GameState.Instance.Player.gameObject;
    //    else if (menuListener == null)
    //    {
    //        menuListener = FindObjectOfType<StudioListener>().gameObject;
    //    }
    //    if (menuHover != "")
    //        PlayGenericSound(menuHover, menuListener, "Menu", 1);
    //}

    /// <summary>
    /// Save all the volumes inside a playerPrefs
    /// </summary>
    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.SetFloat("fxVolume", fxVolume);
        PlayerPrefs.SetFloat("masterVolume", masterVolume);
        PlayerPrefs.Save();
    }

    public void MusicVolume(float newVolume)
    {
        musicVolume = newVolume;
        //if (GameState.Instance.CurrentState == State.PauseMenu)
        //{
            if (musicInstance.hasHandle())
                musicInstance.setVolume(musicVolume * masterVolume);
            if (ambienceInstance.hasHandle())
                ambienceInstance.setVolume(musicVolume * masterVolume);
        //}
    }

    public void FxVolume(float newVolume)
    {
        fxVolume = newVolume;
    }

    public void GeneralVolume(float newVolume)
    {
        masterVolume = newVolume;
        if (musicInstance.hasHandle())
            musicInstance.setVolume(musicVolume * masterVolume);
        if (ambienceInstance.hasHandle())
            ambienceInstance.setVolume(musicVolume * masterVolume);
    }

    public void SetGlobalParameter(string paramName, float paramValue)
    {
        RuntimeManager.StudioSystem.setParameterByName(paramName, paramValue);
    }

    public EventInstance PlayLoopSound(string eventToPlay, GameObject _object)
    {
        if (eventToPlay != "")
        {
            return PlayLoopSound(eventToPlay, _object, "", 0);
        }
        else
        {
            UnityEngine.Debug.Log("This instance is empty");
            return new EventInstance();
        }
    }

    public EventInstance PlayLoopSound(string eventToPlay, GameObject _object, string paramName, float paramValue)
    {
        if (eventToPlay != "")
        {


            EventInstance instance = RuntimeManager.CreateInstance(eventToPlay);
            allInstances.Add(instance);

            RuntimeManager.AttachInstanceToGameObject(instance, _object.transform, player);
            if (paramName != "")
                instance.setParameterByName(paramName, paramValue);
            //UnityEngine.Debug.Log((fxVolume * masterVolume).ToString());
            instance.setVolume(fxVolume * masterVolume);
            instance.start();


            return instance;
        }
        else
        {
            UnityEngine.Debug.Log("This instance is empty");
            return new EventInstance();
        }
    }

    public EventInstance PlayGenericSound(string eventToPlay, GameObject _object)
    {
        if (eventToPlay != "")
        {
            return PlayGenericSound(eventToPlay, _object, "", 0);
        }
        else
        {
            //UnityEngine.Debug.Log("This instance is empty");
            return new EventInstance();
        }
    }

    public EventInstance PlayGenericSound(string eventToPlay)
    {
        if (eventToPlay != "")
        {
            return PlayGenericSound(eventToPlay, player.gameObject, "", 0);
        }
        else
        {
            //UnityEngine.Debug.Log("This instance is empty");
            return new EventInstance();
        }
    }

    public EventInstance PlayGenericSound(string eventToPlay, GameObject _object, string paramName, float paramValue)
    {
        if (eventToPlay != "")
        {


            EventInstance instance = RuntimeManager.CreateInstance(eventToPlay);
            allInstances.Add(instance);

            RuntimeManager.AttachInstanceToGameObject(instance, _object.transform, player);
            //if (paramName != "")
            //{
            //    UnityEngine.Debug.Log(instance.setParameterByName(paramName, paramValue).ToString());
            //}
            //UnityEngine.Debug.Log((fxVolume * masterVolume).ToString());
            instance.setVolume(fxVolume * masterVolume);
            instance.start();
            StartCoroutine(WaitSound(instance));


            return instance;
        }
        else
        {
            //UnityEngine.Debug.Log("This instance is empty");
            return new EventInstance();
        }
    }

    public EventInstance PlayGenericSound(string eventToPlay, GameObject _object, SoundParameters[] parameters)
    {
        if (eventToPlay != "")
        {


            EventInstance instance = RuntimeManager.CreateInstance(eventToPlay);
            allInstances.Add(instance);

            RuntimeManager.AttachInstanceToGameObject(instance, _object.transform, player);
            foreach (SoundParameters parameter in parameters)
            {
                if (parameter.paramName != "")
                    instance.setParameterByName(parameter.paramName, parameter.paramValue);
            }
            //UnityEngine.Debug.Log((fxVolume * masterVolume).ToString());
            instance.setVolume(fxVolume * masterVolume);
            instance.start();
            StartCoroutine(WaitSound(instance));
            

            return instance;
        }
        else
        {
            //UnityEngine.Debug.Log("This instance is empty");
            return new EventInstance();
        }        
    }

    public EventInstance PlayGenericSound(string eventToPlay, SoundParameters[] parameters)
    {
        if (eventToPlay != "")
        {


            EventInstance instance = RuntimeManager.CreateInstance(eventToPlay);
            allInstances.Add(instance);

            RuntimeManager.AttachInstanceToGameObject(instance, player.transform, player);
            foreach (SoundParameters parameter in parameters)
            {
                if (parameter.paramName != "")
                    instance.setParameterByName(parameter.paramName, parameter.paramValue);
            }
            //UnityEngine.Debug.Log((fxVolume * masterVolume).ToString());
            instance.setVolume(fxVolume * masterVolume);
            instance.start();
            StartCoroutine(WaitSound(instance));


            return instance;
        }
        else
        {
            //UnityEngine.Debug.Log("This instance is empty");
            return new EventInstance();
        }
    }

    public EventInstance PlaySnapshot(string eventToPlay, GameObject _object)
    {
        EventInstance instance = RuntimeManager.CreateInstance(eventToPlay);
        allInstances.Add(instance);

        RuntimeManager.AttachInstanceToGameObject(instance, _object.transform, player);
        instance.setVolume(fxVolume);
        instance.start();
        
        return instance;
    }

    public void ChangeInstanceParameter(EventInstance instance, string paramName, float paramValue)
    {
        if(paramName != "")
        {
            instance.setParameterByName(paramName, paramValue);
            PLAYBACK_STATE control;
            instance.getPlaybackState(out control);
            if (control != PLAYBACK_STATE.PLAYING)
                instance.start();
        }
    }

    public void ChangeInstanceParameters(EventInstance instance, SoundParameters[] parameters)
    {
        foreach (SoundParameters param in parameters)
        {
            if (param.paramName != "")
            {
                instance.setParameterByName(param.paramName, param.paramValue);
                PLAYBACK_STATE control;
                instance.getPlaybackState(out control);
                if (control != PLAYBACK_STATE.PLAYING)
                    instance.start();
            }
        }
    }

    public void ChangeMusic(string musicToPlay, bool oneshot)
    {
        if (actualMusic == musicToPlay)
            return;

        DestroyInstance(musicInstance);
        if (oneshot)
            actualMusic = "";
        else
            actualMusic = musicToPlay;
        musicInstance = PlayGenericSound(musicToPlay, GameController.instance.player.gameObject);
        musicInstance.setVolume(musicVolume * masterVolume);
    }

    public void ChangeAmbience(string ambienceToPlay)
    {
        UnityEngine.Debug.Log("cambio ambience");
        DestroyInstance(ambienceInstance);
        actualAmbience = ambienceToPlay;
        ambienceInstance = PlayGenericSound(ambienceToPlay, GameController.instance.player.gameObject);
        ambienceInstance.setVolume(musicVolume * masterVolume);
    }

    public void ChangeMusic(string musicToPlay, SoundParameters[] parameters, bool oneshot)
    {
        if (actualMusic == musicToPlay)
            return;

        DestroyInstance(musicInstance);
        if (oneshot)
            actualMusic = "";
        else
            actualMusic = musicToPlay;
        musicInstance = PlayGenericSound(musicToPlay, GameController.instance.player.gameObject, parameters);
        musicInstance.setVolume(musicVolume * masterVolume);
    }

    public void ChangeAmbience(string ambienceToPlay, SoundParameters[] parameters)
    {
        DestroyInstance(ambienceInstance);
        actualAmbience = ambienceToPlay;
        ambienceInstance = PlayGenericSound(ambienceToPlay, GameController.instance.player.gameObject, parameters);
        ambienceInstance.setVolume(musicVolume * masterVolume);
    }

    public void ChangeMusicParameters(SoundParameters[] parameters)
    {
        if (actualMusic == "")
            return;
        ChangeInstanceParameters(musicInstance, parameters);
        musicInstance.setVolume(musicVolume * masterVolume);
    }

    public void StopMusic()
    {
        DestroyInstanceFaded(musicInstance);
        actualMusic = "";
    }

    public void ResetToStart()
    {
        /*ambienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);*/
        DestroyAllInstances();
        ChangeMusic(ambienceMusic, false);
        ChangeAmbience(ambienceSound);
        //while(allInstances.Count > 0)
        //    DestroyInstance(allInstances[0]);
    }

    private void OnDestroy()
    {

    }

    public void DestroyInstance(EventInstance instanceToDestroy)
    {
        if (instanceToDestroy.hasHandle())
        {
            allInstances.Remove(instanceToDestroy);
            instanceToDestroy.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instanceToDestroy.release();
        }
    }

    public void DestroyInstance(EventInstance instanceToDestroy, bool snapshot)
    {
        allInstances.Remove(instanceToDestroy);
        instanceToDestroy.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instanceToDestroy.release();

    }

    public void DestroyAllInstances()
    {
        foreach(EventInstance i in allInstances)
        {
            i.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            i.release();
        }

        allInstances.Clear();
    }

    public void DestroyInstanceFaded(EventInstance instanceToDestroy)
    {
        if (!instanceToDestroy.hasHandle())
        {
            UnityEngine.Debug.LogError("Passed instance is null!");
            return;
        }
        allInstances.Remove(instanceToDestroy);
        instanceToDestroy.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instanceToDestroy.release();
    }

    public void DestroyInstanceFaded(EventInstance instanceToDestroy, bool snapshot)
    {
        allInstances.Remove(instanceToDestroy);
        instanceToDestroy.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instanceToDestroy.release();
    }

    public void ReleaseInstance(EventInstance instanceToDestroy)
    {
        allInstances.Remove(instanceToDestroy);
        instanceToDestroy.release();
    }

    public void ChangeMusicValues(string parameterName, float value)
    {
        musicInstance.setParameterByName(parameterName, value);
    }
    //public void ChangeBattleValue(string parameterName, float value)
    //{
    //    battleInstance.setParameterByName(parameterName, value);
    //}

    public void ChangeAmbienceValues(string parameterName, float value)
    {
        ambienceInstance.setParameterByName(parameterName, value);
    }


    //public void LevelMusic()
    //{
    //    if(!isHarbor && level != "" && musicInstance.hasHandle())
    //    {
    //        ChangeMusicValues("Battle", 0);
    //    }     
    //}

    //public void BattleMusic()
    //{
    //    if (!isHarbor && level != "" && musicInstance.hasHandle())
    //    {
    //        ChangeMusicValues("Battle", 1);

    //    }
    //}

    //public void MortarMusic()
    //{
    //    if (isHarbor && level != "")
    //    {
    //        ChangeMusicValues("Battle", 1);
    //    }

    //}

    //public void FuryMusic(float time)
    //{
    //    ChangeMusicValues("Fury", 1);
    //    furyInstance = PlayGenericSound(fury, player.gameObject, "Fury", 1);
    //    furyInstance.setVolume(musicVolume * masterVolume);
    //    StartCoroutine(FuryWait(time, furyLerp));
    //}

    #endregion

    #region PrivateMethods


    #endregion

    #region CoroutineMethods

    //IEnumerator FuryStart(float lerpTime)
    //{
    //    float c = 0;
    //    while (c > 0.99f)
    //    {
    //        //c = Mathf.Lerp(1, 0, furyLerp);
    //        yield return new WaitForEndOfFrame();
    //        c -= furyLerp / 100;
    //        ChangeMusicValues("Fury", c);

    //    }

    //}
    //IEnumerator FuryWait(float time, float lerpTime)
    //{

    //    yield return new WaitForSeconds(time);

    //    ChangeMusicValues("Fury", 0);
    //    furyInstance.setParameterValue("Fury", 0);
    //    furyInstance.stop(STOP_MODE.IMMEDIATE);
    //    furyInstance.clearHandle();
    //}

    IEnumerator WaitSound(EventInstance instance)
    {
        PLAYBACK_STATE control;
        instance.getPlaybackState(out control);
        while (control != PLAYBACK_STATE.STOPPING)
        {
            yield return new WaitForEndOfFrame();
            instance.getPlaybackState(out control);
        }
        DestroyInstance(instance);
    }

    private IEnumerator AllSoundsFadeIn(float duration)
    {
        float time = 0;
        float tempMasterVol = 0;
        while(time <= duration)
        {
            tempMasterVol = (time / duration) * masterVolume;
            foreach (EventInstance instance in allInstances)
            {
                if (instance.Equals(musicInstance) || instance.Equals(ambienceInstance))
                {
                    instance.setVolume(musicVolume * tempMasterVol);
                }
                else
                {
                    instance.setVolume(fxVolume * tempMasterVol);
                }
            }
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        foreach (EventInstance instance in allInstances)
        {
            if (instance.Equals(musicInstance) || instance.Equals(ambienceInstance))
            {
                instance.setVolume(musicVolume * masterVolume);
            }
            else
            {
                instance.setVolume(fxVolume * masterVolume);
            }
        }
    }

    //IEnumerator WaitOnLoad(Scene first, Scene newScene)
    //{

    //    DestroyInstance(musicInstance);
    //    DestroyInstance(battleInstance);
    //    DestroyInstance(ambienceInstance);
    //    bool foundMusic = false;


    //    yield return new WaitForSeconds(0.2f);
    //    print("son qui " + GameState.Instance.CurrentState);

    //    if (FindObjectOfType<MusicSelector>())
    //    {
    //        if (FindObjectOfType<MusicSelector>().sceneMusic != "")
    //        {
    //            level = FindObjectOfType<MusicSelector>().sceneMusic;
    //            isHarbor = FindObjectOfType<MusicSelector>().isHarbor;
    //        }
    //        else level = "";

    //        if (FindObjectOfType<MusicSelector>().sceneBackground != "")
    //            ambience = FindObjectOfType<MusicSelector>().sceneBackground;
    //        else ambience = "";
    //        foundMusic = true;
    //    }

    //    if (foundMusic)
    //    {
    //        if (GameState.Instance.CurrentState == State.InGame)
    //        {


    //            player = GameState.Instance.Player.GetComponent<Rigidbody>();
    //            playerTransform = player.transform;
    //            if (newScene.buildIndex >= GameState.Instance.LevelManager.FirstLevelIndex + 1)
    //            {
    //                if (level != "")
    //                {

    //                    musicInstance = PlayGenericSound(level, Camera.main.gameObject, "Battle", 0);
    //                    musicInstance.setVolume((musicVolume * masterVolume));
    //                }

    //                if (ambience != "")
    //                {
    //                    ambienceInstance = RuntimeManager.CreateInstance(ambience);
    //                    ambienceInstance.start();
    //                    ambienceInstance.setVolume((musicVolume * masterVolume));
    //                }
    //            }
    //        }

    //        if (GameState.Instance.CurrentState == State.MainMenu)
    //        {
    //            print("son qui 2 " + GameState.Instance.CurrentState);

    //            StudioListener listener = Camera.main.gameObject.AddComponent<StudioListener>();
    //            menuListener = listener.gameObject;
    //            //UnityEngine.Debug.Log(listener.gameObject);

    //            if (FindObjectOfType<MusicSelector>())
    //            {
    //                if (FindObjectOfType<MusicSelector>().sceneMusic != "")
    //                {
    //                    level = FindObjectOfType<MusicSelector>().sceneMusic;
    //                    isHarbor = FindObjectOfType<MusicSelector>().isHarbor;

    //                    //musicInstance = PlayGenericSound(level, Camera.main.gameObject, "", 0);
    //                }
    //                else level = "";
    //            }

    //            musicInstance = PlayGenericSound(level, Camera.main.gameObject, "", 0);
    //            musicInstance.setVolume(musicVolume * masterVolume);
    //        }
    //    }
    //    else
    //    {
    //        print("son qui " + GameState.Instance.CurrentState);

    //        if (FindObjectOfType<MusicSelector>())
    //        {
    //            if (FindObjectOfType<MusicSelector>().sceneMusic != "")
    //            {
    //                level = FindObjectOfType<MusicSelector>().sceneMusic;
    //                isHarbor = FindObjectOfType<MusicSelector>().isHarbor;
    //            }
    //            else level = "";

    //            if (FindObjectOfType<MusicSelector>().sceneBackground != "")
    //                ambience = FindObjectOfType<MusicSelector>().sceneBackground;
    //            else ambience = "";
    //            foundMusic = true;
    //        }

    //        if (foundMusic)
    //        {
    //            if (GameState.Instance.CurrentState == State.InGame)
    //            {


    //                player = GameState.Instance.Player.GetComponent<Rigidbody>();
    //                playerTransform = player.transform;
    //                if (newScene.buildIndex >= GameState.Instance.LevelManager.FirstLevelIndex + 1)
    //                {
    //                    if (level != "")
    //                    {

    //                        musicInstance = PlayGenericSound(level, Camera.main.gameObject, "Battle", 0);
    //                        musicInstance.setVolume((musicVolume * masterVolume));
    //                    }

    //                    if (ambience != "")
    //                    {
    //                        ambienceInstance = RuntimeManager.CreateInstance(ambience);
    //                        ambienceInstance.start();
    //                        ambienceInstance.setVolume((musicVolume * masterVolume));
    //                    }
    //                }
    //            }

    //            if (GameState.Instance.CurrentState == State.MainMenu)
    //            {
    //                print("son qui 2 " + GameState.Instance.CurrentState);

    //                StudioListener listener = Camera.main.gameObject.AddComponent<StudioListener>();
    //                menuListener = listener.gameObject;
    //                //UnityEngine.Debug.Log(listener.gameObject);

    //                if (FindObjectOfType<MusicSelector>())
    //                {
    //                    if (FindObjectOfType<MusicSelector>().sceneMusic != "")
    //                    {
    //                        level = FindObjectOfType<MusicSelector>().sceneMusic;
    //                        isHarbor = FindObjectOfType<MusicSelector>().isHarbor;

    //                        //musicInstance = PlayGenericSound(level, Camera.main.gameObject, "", 0);
    //                    }
    //                    else level = "";
    //                }

    //                musicInstance = PlayGenericSound(level, Camera.main.gameObject, "", 0);
    //                musicInstance.setVolume(musicVolume * masterVolume);
    //            }
    //        }
    //    }
    //}

    //IEnumerator WaitOnLoad()
    //{

    //    DestroyInstance(musicInstance);
    //    DestroyInstance(battleInstance);
    //    DestroyInstance(ambienceInstance);

    //    yield return new WaitForSeconds(0.2f);
    //    if (FindObjectOfType<MusicSelector>())
    //    {
    //        if (FindObjectOfType<MusicSelector>().sceneMusic != "")
    //        {
    //            level = FindObjectOfType<MusicSelector>().sceneMusic;
    //            isHarbor = FindObjectOfType<MusicSelector>().isHarbor;
    //        }
    //        else level = "";

    //        if (FindObjectOfType<MusicSelector>().sceneBackground != "")
    //            ambience = FindObjectOfType<MusicSelector>().sceneBackground;
    //        else ambience = "";

    //    }

    //    if (GameState.Instance.CurrentState == State.InGame)
    //    {
    //        player = GameState.Instance.Player.GetComponent<Rigidbody>();
    //        playerTransform = player.transform;
    //        if (GameState.Instance.LevelManager.ActualLevelIndex >= GameState.Instance.LevelManager.FirstLevelIndex + 1)
    //        {
    //            if (level != "")
    //            {

    //                musicInstance = PlayGenericSound(level, GameState.Instance.Player.gameObject, "Battle", 0);
    //                musicInstance.setVolume((musicVolume * masterVolume));

    //            }
    //            if (ambience != "")
    //            {
    //                ambienceInstance = RuntimeManager.CreateInstance(ambience);
    //                ambienceInstance.start();
    //                ambienceInstance.setVolume((musicVolume * masterVolume));
    //            }
    //        }
    //    }

    //    if (GameState.Instance.CurrentState == State.MainMenu)
    //    {
    //        print("son qui 2 " + GameState.Instance.CurrentState);

    //        StudioListener listener = Camera.main.gameObject.AddComponent<StudioListener>();
    //        menuListener = listener.gameObject;
    //        //UnityEngine.Debug.Log(listener.gameObject);

    //        if (FindObjectOfType<MusicSelector>())
    //        {
    //            if (FindObjectOfType<MusicSelector>().sceneMusic != "")
    //            {
    //                level = FindObjectOfType<MusicSelector>().sceneMusic;
    //                isHarbor = FindObjectOfType<MusicSelector>().isHarbor;
    //            }
    //            else level = "";
    //        }


    //        musicInstance = PlayGenericSound(level, Camera.main.gameObject, "", 0);
    //        musicInstance.setVolume(musicVolume * masterVolume);

    //    }
    //}

    //IEnumerator WaitEndBattle()
    //{
    //    yield return new WaitForSeconds(0.1f);
    //    musicInstance.setParameterByName("End_of_battle", 0f);
    //}

    //public IEnumerator OnDeath()
    //{
    //    float t = 0;
    //    float c = 100;
    //    float d;
    //    musicInstance.getVolume(out c, out d);
    //    UnityEngine.Debug.Log(c);
    //    while (t < 2.09f)
    //    {

    //        musicInstance.setVolume(c - 10);
    //        yield return new WaitForFixedUpdate();
    //        t += Time.deltaTime;
    //    }
    //    musicInstance.setVolume(c);
    //}
    #endregion

    #region EventListenerMethods
    //private void OnChangeState(object sender)
    //{
    //    /*switch (GameState.Instance.CurrentState)
    //    {
    //        case State.GameOver:
    //            DestroyInstance(musicInstance);
    //            DestroyInstance(battleInstance);

    //            break;
    //        case State.Loading:
    //            DestroyInstance(musicInstance);
    //            DestroyInstance(battleInstance);

    //            break;
    //    }*/
    //}

    //private void OnReset(object sender)
    //{
    //    if (musicInstance.hasHandle())
    //        musicInstance.setParameterByName("Battle", 0);
    //    //StartCoroutine(WaitOnLoad());
    //}
    #endregion
}
