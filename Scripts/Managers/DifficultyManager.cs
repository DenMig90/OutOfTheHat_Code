using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty
{
    Easy = 1,
    Normal = 2,
    None = 0
}

public class DifficultyManager : MonoBehaviour {
    [HideInInspector]
    public DifficultyScriptableObject difficultyData;

    [SerializeField]
    private DifficultyScriptableObject easyData;
    [SerializeField]
    private DifficultyScriptableObject normalData;

    [SerializeField]
    private Difficulty actualDifficulty;

    public Difficulty ActualDifficulty { get { return actualDifficulty; } }
    public int NumberOfDifficulties { get { return System.Enum.GetNames(typeof(Difficulty)).Length - 1; } }

    private Difficulty prevDifficulty;

	// Use this for initialization
	void Start () {
        ManageDifficultyData();
        prevDifficulty = actualDifficulty;
        GameController.instance.difficultyManager = this;
	}

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    // Update is called once per frame
    void Update () {
		if(prevDifficulty != actualDifficulty)
        {
            ChangeDifficulty(actualDifficulty);
        }
	}

    private void ManageDifficultyData()
    {
        switch (actualDifficulty)
        {
            case Difficulty.Easy:
                difficultyData = easyData;
                break;
            case Difficulty.Normal:
                difficultyData = normalData;
                break;
        }
    }

    public void ChangeDifficulty(Difficulty new_difficulty)
    {
        actualDifficulty = new_difficulty;
        ManageDifficultyData();
        prevDifficulty = actualDifficulty;
        GameController.instance.TriggerChangeDifficulty();
    }

    public void ChangeDifficulty(float value)
    {
        Difficulty difficulty = (Difficulty)((value * (NumberOfDifficulties-1)) + 1);
        //Debug.Log((value * NumberOfDifficulties) + 1);
        ChangeDifficulty(difficulty);
    }
}
