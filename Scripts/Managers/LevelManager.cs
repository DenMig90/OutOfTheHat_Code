using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    public int gameIndex = 0;
    public int mainMenuIndex = 0;
    public int pauseMenuIndex = 2;
    public int endingIndex = 3;

    private AsyncOperation asyncGame;

    // Use this for initialization
    void Start () {
        if(GameController.instance != null)
            GameController.instance.levelmanager = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadMainMenu()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(mainMenuIndex, LoadSceneMode.Additive);
        StartCoroutine(ActivateScene(async, mainMenuIndex));
    }

    public void UnloadMainMenu()
    {
        if(SceneManager.GetSceneByBuildIndex(mainMenuIndex).isLoaded)
            SceneManager.UnloadSceneAsync(mainMenuIndex);
        SceneManager.SetActiveScene(gameObject.scene);
    }

    public void LoadPauseMenu()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(pauseMenuIndex, LoadSceneMode.Additive);
        StartCoroutine(ActivateScene(async, pauseMenuIndex));
        //SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(pauseMenuIndex));
    }

    public void UnloadPauseMenu()
    {
        if (SceneManager.GetSceneByBuildIndex(pauseMenuIndex).isLoaded)
            SceneManager.UnloadSceneAsync(pauseMenuIndex);
        SceneManager.SetActiveScene(gameObject.scene);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(gameIndex);
    }

    public void LoadActiveScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartLoadGameBG()
    {
        asyncGame = SceneManager.LoadSceneAsync(gameIndex, LoadSceneMode.Single);
        asyncGame.allowSceneActivation = false;
    }

    public void ActiveGameBG()
    {
        if (asyncGame == null)
            return;

        asyncGame.allowSceneActivation = true;
    }

    public void LoadEnding(float delay)
    {
        StartCoroutine(LoadSceneDelayed(delay));
    }

    public void ReloadLevel()
    {
        StartCoroutine(ReloadLevelCoroutine());
    }

    private IEnumerator LoadSceneDelayed(float delay)
    {
        float time = 0;
        while(time < delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        SceneManager.LoadScene(endingIndex, LoadSceneMode.Single);
        GameController.instance.ClearSavedData();
    }

    private IEnumerator ActivateScene(AsyncOperation async, int index)
    {
        while(!async.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(index));
    }

    private IEnumerator ReloadLevelCoroutine()
    {
        UnloadMainMenu();
        UnloadPauseMenu();
        yield return null;
        LoadActiveScene();
    }
}