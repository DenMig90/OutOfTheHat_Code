using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MenuManager))]
public class PauseMenuManager : MonoBehaviour {

    public TextToggle assistedModeToggle;

    //private float oldTimeScale;

    private MenuManager menuManager;

    private void Awake()
    {
        menuManager = GetComponent<MenuManager>();
    }

    // Use this for initialization
    void Start () {
        //oldTimeScale = Time.timeScale;
        //Time.timeScale = 0f;
        menuManager.PlayMenuOpenSound();
        assistedModeToggle.Active = (GameController.instance != null) ? GameController.instance.difficultyManager.ActualDifficulty == Difficulty.Easy : false;
    }
	
	// Update is called once per frame
	void OnDestroy () {
        //Time.timeScale = oldTimeScale;
        menuManager.PlayMenuCloseSound();
    }
}
