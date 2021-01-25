using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenDebug : MonoBehaviour {

    private Text text;
    [HideInInspector]
    public static ScreenDebug instance; 

    private void Awake()
    {
        text = GetComponent<Text>();
        instance = this;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Log(string msg)
    {
        text.text = msg;
    }
}
