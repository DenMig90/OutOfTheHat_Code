using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FMOD.Studio;
using FMODUnity;

public class MenuManager : MonoBehaviour
{
    public int testchamberSceneIndex;
    public Button[] menuButtons;
    public Button confirmNoButton;
    public GameObject confirmDialog;
    [EventRef] public string menuOpenSound;
    [EventRef] public string menuCloseSound;
    [EventRef] public string menuScrollSound;

    private GameObject previousSelected;

    private void Start()
    {
        //SceneManager.SetActiveScene(gameObject.scene);
    }

    public void Restart()
    {
        GameController.instance.Restart();
    }

    public void NewGame(bool check)
    {
        if (check)
        {
            if (PlayerPrefs.HasKey("SaveDataNew"))
            {
                OpenConfirmDialog(true);
                return;
            }
        }
    
        if (GameController.instance.ActualState == GameState.Pause)
            GameController.instance.SetPause(false);
        GameController.instance.NewGame();
        //Debug.Log("NewGame");
        //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }

    public void Continue()
    {
        GameController.instance.Continue();
        //Debug.Log("Continue");
        //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

    }

    public void OpenConfirmDialog(bool value)
    {
        if(value)
        {
            previousSelected = EventSystem.current.currentSelectedGameObject;
        }
        foreach(Button btn in menuButtons)
        {
            btn.interactable = !value;
        }
        confirmDialog.SetActive(value);
        if(value)
        {
            EventSystem.current.SetSelectedGameObject(confirmNoButton.gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(previousSelected);
        }
    }

    public void Resume()
    {
        //GameController.instance.Continue();
        //Debug.Log("Continue");
        GameController.instance.SetPause(false);

    }

    public void Kickstarter()
    {
        //Debug.Log("Kickstarter");
        Application.OpenURL("http://discord.gg/EjHJvFJ");
    }

    public void AssistedMode(bool value)
    {
        GameController.instance.AssistedMode(value);
    }

    public void LoadTestchamberMenu()
    {
        SceneManager.LoadScene(testchamberSceneIndex, LoadSceneMode.Single);
    }

    public void Quit()
    {
       // Debug.Log("Quit");
        Application.Quit();
    }

    public void PlayMenuOpenSound()
    {
        GameController.instance.audioManager.PlayGenericSound(menuOpenSound);
    }

    public void PlayMenuCloseSound()
    {
        GameController.instance.audioManager.PlayGenericSound(menuCloseSound);
    }

    public void PlayMenuScrollSound()
    {
        GameController.instance.audioManager.PlayGenericSound(menuScrollSound);
    }
}
