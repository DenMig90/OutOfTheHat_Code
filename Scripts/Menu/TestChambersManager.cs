using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestChambersManager : MonoBehaviour {

    public int testchamber1Index;
    public int testchamber2Index;
    public int testchamber3Index;
    public int testchamber4Index;
    public int verticalIndex;

    private string saveDataKey = "SaveData";

    // Use this for initialization
    void Start () {
        Time.timeScale = 1;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadTestchamber1()
    {
        ClearSavedData();
        SceneManager.LoadScene(testchamber1Index, LoadSceneMode.Single);
    }

    public void LoadTestchamber2()
    {
        ClearSavedData();
        SceneManager.LoadScene(testchamber2Index, LoadSceneMode.Single);
    }

    public void LoadTestchamber3()
    {
        ClearSavedData();
        SceneManager.LoadScene(testchamber3Index, LoadSceneMode.Single);
    }

    public void LoadTestchamber4()
    {
        ClearSavedData();
        SceneManager.LoadScene(testchamber4Index, LoadSceneMode.Single);
    }

    public void LoadVertical()
    {
        ClearSavedData();
        SceneManager.LoadScene(verticalIndex, LoadSceneMode.Single);
    }

    public void ClearSavedData()
    {
        PlayerPrefs.DeleteKey(saveDataKey);
    }
}
