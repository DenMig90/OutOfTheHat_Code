
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine.Video;
#endif
using UnityEngine.SceneManagement;

public class VideoController : MonoBehaviour {
#if UNITY_EDITOR || UNITY_STANDALONE
    #region PublicVariables
    //public TimeLineWindowUtilities timeLineUtilities;

    //public UILabel subtitlesLabel;

    //public GameObject loadingUi;
    //public TweenAlpha loadingTween;
    #endregion

    #region HideInInspectorVariables
    #endregion

    #region SerializeFieldVariables
    #endregion

    #region ProtectedVariables
    #endregion

    #region PrivateVariables
    //private List<Subtitles> subtitles = new List<Subtitles>();

    //private TweenAlpha subtitlestween;

    private VideoPlayer videoPlayer;
    private AsyncOperation nextLevel;
    private bool started;
    #endregion

    #region Properties
    public VideoClip GetVideoClip
    {
        get
        {
            VideoPlayer player = transform.GetComponent<VideoPlayer>();
            return player.clip;
        }
    }

    public float CurrentTime
    {
        get
        {
            VideoPlayer player = transform.GetComponent<VideoPlayer>();
            float time = (float)player.frame / (float)player.frameRate;
            return time;
        }
        set
        {            
            VideoPlayer player = transform.GetComponent<VideoPlayer>();
            long currentFrame = (long)value * (long)player.frameRate;
            player.frame = currentFrame;
        }
    }

    //public float SetTimeLine
    //{
    //    set
    //    {
    //        timeLineUtilities.currentTime = value;
    //    }
    //}

    //public bool IsPaused
    //{
    //    get
    //    {
    //        return timeLineUtilities.isPaused;
    //    }
    //    set
    //    {
    //        timeLineUtilities.isPaused = value;
    //    }
    //}
    #endregion

    #region MonobehaviourMethods
    private void Awake()
    {
        //timeLineUtilities.isPaused = false;

        //subtitles = timeLineUtilities.subtitles;
        //ListSort(subtitles);

        //subtitlesLabel.text = "";

        //subtitlestween = subtitlesLabel.transform.GetComponent<TweenAlpha>();
        //subtitlestween.enabled = false;

        videoPlayer = GetComponent<VideoPlayer>();

        //for (int i = 0; i < subtitles.Count; i++)
        //{
        //    subtitles[i].isActiveInScene = false;
        //}

        started = false;
    }

    private void Start()
    {
        //GameState.Instance.CurrentState = State.Cutscene;
        //if (timeLineUtilities.subtitles == null)
        //{
        //    timeLineUtilities.subtitles = new List<Subtitles>();
        //}
        nextLevel = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
        nextLevel.allowSceneActivation = false;
        //GameState.Instance.LevelManager.PreloadNextLevel();

        //GetComponent<AudioSource>().volume = GameState.Instance.AudioManager.fxVolume;

        StartCoroutine(CheckIfEnd());
    }

    private void Update()
    {        
        //if(!IsPaused)
        //    timeLineUtilities.currentTime = CurrentTime;
        //else        
        //    CurrentTime = timeLineUtilities.currentTime;

        //CheckSubtitles();
    }
    #endregion

    #region PublicMethods
    #endregion

    #region EditorMethods
    //public void AddSubtitles(float newStartTime , float newDuration)
    //{
    //    Subtitles newSubtitles = new Subtitles();
    //    newSubtitles.name = "SubtitlesName";
    //    newSubtitles.text = "Insert Subtitles Here";
    //    newSubtitles.startTime = newStartTime;
    //    newSubtitles.duration = newDuration;
    //    newSubtitles.isOpened = true;

    //    timeLineUtilities.subtitles.Add(newSubtitles);
    //}
    #endregion

    #region VirtualMethods
    #endregion

    #region OverrideMethods
    #endregion

    #region PrivateMethods
    //private void ListSort(List<Subtitles> list)
    //{
    //    Subtitles tempSub = new Subtitles();

    //    for (int i = 0; i < list.Count; i++)
    //    {
    //        for (int j = 0; j < list.Count; j++)
    //        {
    //            if (list[j].startTime > list[i].startTime)
    //            {
    //                tempSub = list[j];
    //                list[j] = list[i];
    //                list[i] = tempSub;
    //            }
    //        }
    //    }
    //}

    //private void CheckSubtitles()
    //{
    //    for (int i = 0; i < subtitles.Count; i++)
    //    {
    //        if (timeLineUtilities.currentTime >= subtitles[i].startTime && timeLineUtilities.currentTime < subtitles[i].startTime + subtitles[i].duration && !subtitles[i].isActiveInScene)
    //        {
    //            ActiveSubtitles(subtitles[i]);
    //        }

    //        else if (timeLineUtilities.currentTime >= subtitles[i].startTime + subtitles[i].duration && subtitles[i].isActiveInScene)
    //        {
    //            DisableSubtitles(subtitles[i]);
    //        }
    //    }
    //}

    //private void ActiveSubtitles(Subtitles sub)
    //{
    //    sub.isActiveInScene = true;
    //    subtitlesLabel.text = sub.text;

    //    subtitlestween.enabled = true;
    //    subtitlestween.PlayForward();
    //}

    //private void DisableSubtitles(Subtitles sub)
    //{
    //    sub.isActiveInScene = false;
    //    subtitlestween.PlayReverse();
    //}
    #endregion

    #region CoroutineMethods
    //private IEnumerator StartSubtitles(Subtitles sub)
    //{
    //    subtitlesLabel.text = sub.text;

    //    subtitlestween.enabled = true;
    //    subtitlestween.PlayForward();

    //    yield return new WaitForSeconds(sub.duration);

    //    subtitlestween.PlayReverse();

    //    yield return null;
    //}


    //private void LoadNextLevel()
    //{
    //    GameState.Instance.LevelManager.LoadNextLevel();
    //}

    public void EndVideo()
    {
        videoPlayer.Stop();
        //loadingUi.SetActive(true);
        //loadingTween.ResetToBeginning();
        //loadingTween.PlayForward();
        nextLevel.allowSceneActivation = true;
        //GameState.Instance.LevelManager.LaunchPreloadedLevel();
    }

    private IEnumerator CheckIfEnd()
    {
#if UNITY_EDITOR

        videoPlayer.Play();

        yield return new WaitForSeconds(1);
        float time = 0;
        //Debug.Log(videoPlayer.clip.length);
        while (time < videoPlayer.clip.length || nextLevel.progress < 0.9f)
        {
            yield return new WaitForEndOfFrame() ;
            time += Time.deltaTime;
        }

        yield return new WaitForEndOfFrame();


        EndVideo();
#endif

#if UNITY_STANDALONE
        started = false;

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return new WaitForEndOfFrame();
        }

        videoPlayer.Play();

        yield return new WaitForEndOfFrame();

        started = true;

        yield return new WaitForEndOfFrame();

        while (videoPlayer.isPlaying && started)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        EndVideo();
#endif
    }
    #endregion

    #region EventListenerMethods
    #endregion
#endif
}