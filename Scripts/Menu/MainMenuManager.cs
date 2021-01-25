using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MenuManager))]
public class MainMenuManager : MonoBehaviour {

    public UnityEvent onLoaded;
    public GameObject newGameButton;
    public GameObject continueButton;
    public TextToggle assistedModeToggle;

    private MenuManager menuManager;

    private void Awake()
    {
        menuManager = GetComponent<MenuManager>();
    }

    private void Start()
    {
        //SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        //SceneManager.sceneLoaded += OnLevelLoaded;
        onLoaded.Invoke();
        if (!PlayerPrefs.HasKey("SaveDataNew"))
        {
            continueButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            EventSystem.current.SetSelectedGameObject(newGameButton);
        }
        assistedModeToggle.Active = (GameController.instance != null) ? GameController.instance.difficultyManager.ActualDifficulty == Difficulty.Easy : false;
    }

    private void OnDestroy()
    {
        menuManager.PlayMenuCloseSound();
    }

    //private void OnLevelLoaded(Scene s, LoadSceneMode lsm)
    //{
    //    if (s.buildIndex == SceneManager.)
    //    {
    //        //Debug.Log("Loaded");

    //    }
    //}
}
