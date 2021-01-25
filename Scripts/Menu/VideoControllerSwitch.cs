using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VideoControllerSwitch : MonoBehaviour {
#if UNITY_SWITCH
    private string MoviePath;
    public GameObject lumaObject;
    public GameObject chromaObject;
    private AsyncOperation nextLevel;

    UnityEngine.Switch.SwitchVideoPlayer video;
    UnityEngine.Switch.SwitchFMVTexture lumaTex;
    UnityEngine.Switch.SwitchFMVTexture chromaTex;

    void OnMovieEvent(int FMVevent)
    {
        Debug.Log("script has received FMV event :" + (UnityEngine.Switch.SwitchVideoPlayer.Event)FMVevent);
    }
    // Use this for initialization
    void Start () {
        MoviePath = Application.streamingAssetsPath + "/NaN_logo_p.mp4";
        video = new UnityEngine.Switch.SwitchVideoPlayer(OnMovieEvent);

        int width = 1980;
        int height = 1080;
        {   // if the resolution has already known, this should be removed so that the redundant file access could be suppressed.
            video.GetTrackInfo(MoviePath, out width, out height);
        }
        {
            // when you didn't call this, system would guesse a container type with the file extension. 
            video.SetContainerType(UnityEngine.Switch.SwitchVideoPlayer.ContainerType.Mpeg4);
        }

        lumaTex = new UnityEngine.Switch.SwitchFMVTexture();
        lumaTex.Create(width, height, UnityEngine.Switch.SwitchFMVTexture.Type.R8);
        chromaTex = new UnityEngine.Switch.SwitchFMVTexture();
        chromaTex.Create(width / 2, height / 2, UnityEngine.Switch.SwitchFMVTexture.Type.R8G8);
        video.Init(lumaTex, chromaTex);

        var quad = GameObject.Find("Quad");
        quad.GetComponent<Renderer>().material.mainTexture = lumaTex.GetTexture();
        quad.GetComponent<Renderer>().material.SetTexture("_ChromaTex", chromaTex.GetTexture());

        if (lumaObject != null)
        {
            lumaObject.GetComponent<Renderer>().material.mainTexture = lumaTex.GetTexture();
        }
        if (chromaObject != null)
        {
            chromaObject.GetComponent<Renderer>().material.mainTexture = chromaTex.GetTexture();
        }

        bool result = video.Play(MoviePath);
        Debug.Log("result of starting to play : " + result);
        nextLevel = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
        nextLevel.allowSceneActivation = false;
    }
	
	// Update is called once per frame
	void Update () {
        UnityEngine.Profiling.Profiler.BeginSample("fmv_update");

        UnityEngine.Profiling.Profiler.EndSample();
    }

    void OnPreRender()
    {
        video.Update();
        float normalized_progress = video.GetCurrentTime() / video.GetVideoLength();
        if(normalized_progress >=1)
        {
            video.Stop();
            nextLevel.allowSceneActivation = true;
        }
    }
#endif
}
