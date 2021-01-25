using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ActivateGameobjects : MonoBehaviour {
    public GameObject[] objectsToHide;
    [HideInInspector]
    public bool show = true;
	// Use this for initialization
	void Start () {
        show = true;
        Activate(show);
    }

    public void Activate(bool _show)
    {
        show = _show;
        foreach (GameObject obj in objectsToHide)
        {
            if(obj != null)
                obj.SetActive(show);
        }
    }
}