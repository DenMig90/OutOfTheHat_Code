using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;

[Serializable]
public class ComponentSelector
{
#if UNITY_EDITOR
    //public List<string> allComponents;
    public string selected = "";

    public ComponentSelector()
    {
        //allComponents = new List<string>();
        //GetAllComponents();
    }

    public Type GetSelectedComponent()
    {
        Type mytype = Type.GetType(selected);
        //Debug.Log(mytype);
        return mytype;
    }

    //public T[] GetAtPath<T>(string path)
    //{
    //    ArrayList al = new ArrayList();
    //    string[] subdirs = Directory.GetDirectories(Application.dataPath + "/" + path);
    //    foreach (string dir in subdirs)
    //    {
    //        string temp = dir.Replace("\\", "/");
    //        int index = temp.LastIndexOf("/");
    //        string local = temp.Substring(index);
    //        //Debug.Log(local);
    //        T[] fromsub = GetAtPath<T>(path + local);
    //        foreach (T obj in fromsub)
    //            al.Add(obj);
    //    }
    //    string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
    //    foreach (string fileName in fileEntries)
    //    {
    //        string temp = fileName.Replace("\\", "/");
    //        int index = temp.LastIndexOf("/");
    //        string localPath = "Assets/" + path;

    //        if (index > 0)
    //            localPath += temp.Substring(index);

    //        UnityEngine.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

    //        if (t != null)
    //            al.Add(t);
    //    }

    //    T[] result = new T[al.Count];

    //    for (int i = 0; i < al.Count; i++)
    //        result[i] = (T)al[i];

    //    return result;
    //}

    //public void GetAllComponents()
    //{
    //    MonoScript[] scripts = GetAtPath<MonoScript>("Scripts");

    //    allComponents.Clear();

    //    foreach (MonoScript script in scripts)
    //    {
    //        Type myclass = script.GetClass();
    //        //Debug.Log(script.name + " " + myclass);
    //        if (myclass == null)
    //            continue;
    //        //Debug.Log(script.name + " " + myclass + " " + myclass.IsSubclassOf(typeof(MonoBehaviour)));
    //        if (myclass.IsSubclassOf(typeof(MonoBehaviour)))
    //        {
    //            allComponents.Add(script.name);
    //        }
    //    }
    //    allComponents.Sort();
    //}
}

[ExecuteInEditMode]
public class ComponentAdder : MonoBehaviour {
    public ComponentSelector list;
    private static ComponentAdder instance;
    public List<string> allComponents;

    public static ComponentAdder Instance
    {
        get
        {
            if (instance == null)
                instance = (new GameObject("ComponentAdder", new Type[] { typeof(ComponentAdder) })).GetComponent<ComponentAdder>();
            return instance;
        }
    }

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
        }
    }

    // Use this for initialization
    void OnEnable () {
        if (allComponents == null)
            allComponents = new List<string>();
        GetAllComponents();
        //allScripts = new List<string>();

        //string scripts = ((MonoBehaviour)AssetDatabase.LoadAssetAtPath("Assets/Scripts", typeof(MonoScript))).name /* = Resources.FindObjectsOfTypeAll<MonoScript>() */;
        //Debug.Log(scripts.Length);
        //foreach (MonoScript script in scripts)
        //{
        //    if (script.GetClass() != null && script.GetClass().IsSubclassOf(typeof(MonoBehaviour)))
        //    {
        //    //if (script.GetType().Equals(typeof(MonoBehaviour)))
        //    //{
        //    allScripts.Add(script.name);
        //        Debug.Log(script.name);
        //    }
        //}

        //scripts = Resources.LoadAll<MonoScript>("Scripts");
        //Debug.Log(scripts.Length);
        //foreach (var script in scripts)
        //{
        //    // GetClass method returns the type of the script
        //    allScripts.Add(script.GetClass().Name);
        //}
        //allScripts.Add(scripts);
        //Object[] objects = Utils.GetAtPath<AudioClip>("/Sound/Music");
    }

    // Update is called once per frame
    void Update () {
		
	}

    public T[] GetAtPath<T>(string path)
    {
        ArrayList al = new ArrayList();
        string[] subdirs = Directory.GetDirectories(Application.dataPath + "/" + path);
        foreach (string dir in subdirs)
        {
            string temp = dir.Replace("\\", "/");
            int index = temp.LastIndexOf("/");
            string local = temp.Substring(index);
            //Debug.Log(local);
            T[] fromsub = GetAtPath<T>(path + local);
            foreach (T obj in fromsub)
                al.Add(obj);
        }
        string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
        foreach (string fileName in fileEntries)
        {
            string temp = fileName.Replace("\\", "/");
            int index = temp.LastIndexOf("/");
            string localPath = "Assets/" + path;

            if (index > 0)
                localPath += temp.Substring(index);

            UnityEngine.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

            if (t != null)
                al.Add(t);
        }

        T[] result = new T[al.Count];

        for (int i = 0; i < al.Count; i++)
            result[i] = (T)al[i];

        return result;
    }

    public void GetAllComponents()
    {
        MonoScript[] scripts = GetAtPath<MonoScript>("Scripts");

        allComponents.Clear();

        foreach (MonoScript script in scripts)
        {
            Type myclass = script.GetClass();
            //Debug.Log(script.name + " " + myclass);
            if (myclass == null)
                continue;
            //Debug.Log(script.name + " " + myclass + " " + myclass.IsSubclassOf(typeof(MonoBehaviour)));
            if (myclass.IsSubclassOf(typeof(MonoBehaviour)))
            {
                allComponents.Add(script.name);
            }
        }
        allComponents.Sort();
    }

    public List<string> GetAllComponentsInList()
    {
        MonoScript[] scripts = GetAtPath<MonoScript>("Scripts");

        List<string> returnable = new List<string>();

        foreach (MonoScript script in scripts)
        {
            Type myclass = script.GetClass();
            //Debug.Log(script.name + " " + myclass);
            if (myclass == null)
                continue;
            //Debug.Log(script.name + " " + myclass + " " + myclass.IsSubclassOf(typeof(MonoBehaviour)));
            if (myclass.IsSubclassOf(typeof(MonoBehaviour)))
            {
                returnable.Add(script.name);
            }
        }
        returnable.Sort();
        return returnable;
    }

    public void AddComponent()
    {
        Type mytype = Type.GetType(list.selected);
        Debug.Log(mytype);
        gameObject.AddComponent(mytype);
    }

    //public List<T> GetAssetList<T>(string path) where T : class
    //{
    //    string[] fileEntries = Directory.GetFiles(path);

    //    return fileEntries.Select(fileName =>
    //    {
    //        string assetPath = fileName.Substring(fileName.IndexOf("Assets"));
    //        assetPath = Path.ChangeExtension(assetPath, null);
    //        return UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));
    //    })
    //        .OfType<T>()
    //        .ToList();
    //}
#endif
}
