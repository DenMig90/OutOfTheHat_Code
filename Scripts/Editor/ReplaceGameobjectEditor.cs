using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ReplaceGameObjectEditor : ScriptableWizard
{
    public string toReplaceName;
    public GameObject substitutePrefab;

    [MenuItem("Den/Replace GameObjects")]
    static void CreateWindow()
    {
        ScriptableWizard.DisplayWizard<ReplaceGameObjectEditor>("Replace Gameobjects", "Replace");
    }

    void OnWizardUpdate()
    {
        helpString = "Replace all the Gameobjects in scene which have the name that contains 'toReplaceName' string with an instance of 'substitutePrefab'";
        if (!substitutePrefab)
        {
            errorString = "Please assign a substitutePrefab";
            isValid = false;
        }
        else if(toReplaceName == "")
        {
            errorString = "Please assign a toReplaceName";
            isValid = false;
        }
        else
        {
            errorString = "";
            isValid = true;
        }
    }

    void OnWizardCreate()
    {
        List<Transform> replaces = new List<Transform>();

        foreach (Transform go in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave || go.hideFlags == HideFlags.HideInHierarchy)
                continue;

            if (!EditorUtility.IsPersistent(go.transform.root.gameObject))
                continue;

            if(go.name.Contains(toReplaceName))
                replaces.Add(go);
        }

        foreach (Transform t in replaces)
        {
            GameObject newObject;
            newObject = (GameObject)PrefabUtility.InstantiatePrefab(substitutePrefab);
            newObject.transform.position = t.position;
            newObject.transform.rotation = t.rotation;
            newObject.transform.parent = t.parent;

            DestroyImmediate(t.gameObject);

        }
        replaces.Clear();
    }
}