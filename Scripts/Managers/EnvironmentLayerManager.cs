using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class LayerSelectorAttribute : PropertyAttribute
{
    public LayerSelectorAttribute()
    {

    }
}

[Serializable]
public class AdditionalData
{
    public string[] keywords;
    [LayerSelector]
    public int physicLayer = 0;
    public ComponentSelector[] componentsToAdd;

    public AdditionalData()
    {

    }
}

[System.Serializable]
public class EnvironmentLayerData
{
    public string suffix;
    public Transform parent;
    public float zValue;
    public int orderInLayer;
    public Material material;

    public bool addComponents;
    public AdditionalData[] additionalDatas;
}

[ExecuteInEditMode]
public class EnvironmentLayerManager : MonoBehaviour {
    public EnvironmentLayerData[] DataArray;

#if UNITY_EDITOR
    //[SerializeField]
    private int objectsCounter = 0;
    //private GameObject lastGO = null;
    // Use this for initialization
    public void OnEnable() {
        if (!EditorApplication.isPlaying)
        {
            List<GameObject> objectsInScene = new List<GameObject>();
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                    continue;

                //if (!EditorUtility.IsPersistent(go.transform.root.gameObject))
                //    continue;

                objectsInScene.Add(go);
            }
            objectsCounter = objectsInScene.Count;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            //lastGO = Selection.activeGameObject;
        }
    }

    void OnHierarchyChanged()
    {
        if (!EditorApplication.isPlaying)
        {
            //Debug.Log("cambiato");
            List<GameObject> objectsInScene = new List<GameObject>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                    continue;

                //if (!EditorUtility.IsPersistent(go.transform.root.gameObject))
                //    continue;

                objectsInScene.Add(go);
            }
            int counter = objectsInScene.Count;
            //if (lastGO != Selection.activeGameObject)
            //{
            if (counter > objectsCounter)
            {
                //Debug.Log("aggiunto");
                //Debug.Log(Selection.activeGameObject.name);
                foreach (EnvironmentLayerData data in DataArray)
                {
                    GameObject go = Selection.activeGameObject;
                    if (go != null && go.name.EndsWith(data.suffix))
                    {
                        go.transform.parent = data.parent;
                        go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, data.zValue);
                        //go.layer = data.layerNumber;
                        SpriteRenderer sprite = go.GetComponent<SpriteRenderer>();
                        if (sprite != null)
                        {
                            sprite.sortingOrder = data.orderInLayer;
                            if (data.material != null)
                            {
                                sprite.material = data.material;
                            }
                        }
                        if (data.addComponents)
                        {
                            if(!go.GetComponent<Collider2DOptimization.PolygonColliderOptimizer>())
                                go.AddComponent<Collider2DOptimization.PolygonColliderOptimizer> ();
                            if(!go.GetComponent<ExtrudeSprite>())
                                go.AddComponent<ExtrudeSprite>();
                            if (!go.GetComponent<SpriteDivider>())
                                go.AddComponent<SpriteDivider>();

                        }
                        if (data != null)
                        {
                            foreach (AdditionalData addData in data.additionalDatas)
                            {
                                bool hasKeyword = false;
                                foreach (string keyword in addData.keywords)
                                {
                                    if (go.name.Contains(keyword))
                                        hasKeyword = true;
                                }
                                if (hasKeyword)
                                {
                                    go.layer = addData.physicLayer;
                                    foreach (ComponentSelector component in addData.componentsToAdd)
                                    {
                                        go.AddComponent(component.GetSelectedComponent());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            objectsCounter = counter;
            //}
            //lastGO = Selection.activeGameObject;
        }
    }

        // Update is called once per frame
        void Update () {
		
	}
#endif
}
