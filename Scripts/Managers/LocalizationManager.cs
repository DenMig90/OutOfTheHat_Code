using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public delegate void LanguageDelegate(Language new_language);
//public delegate void VoidDelegate();

public enum Language
{
    English,
    Italian,
    Spanish,
    German,
    French
}

[System.Serializable]
public class LanguagePackage
{
    public string key;
    public string english;
    public string italian;
    public string spanish;
    public string german;
    public string french;
}

public class LocalizationManager : MonoBehaviour {

    public static LocalizationManager instance;
    public List<LanguagePackage> packages;

    [SerializeField]
    private Language actualLanguage;
    private Language previousLanguage;
    private LanguageDelegate onChangeLanguage;
    //private VoidDelegate onSavePackages;
    //private VoidDelegate onLoadPackages;
    private string path = "Assets/Localization/LocalizationPackage.json";

    public Language ActualLanguage { get { return actualLanguage; } }

    public LocalizationManager()
    {
        instance = this;
    }

    private void Awake()
    {
        //GameController.instance.SetLocalizationManager(this);
    }

    // Use this for initialization
    void Start () {
        //onLoadPackages();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void LateUpdate()
    {
        if (previousLanguage != actualLanguage)
        {
            onChangeLanguage(actualLanguage);
            previousLanguage = actualLanguage;
        }
    }

    public void SavePackages()
    {
        //packages.Clear();
        //onSavePackages();
        if (!File.Exists(path))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                foreach(LanguagePackage pack in packages)
                    writer.WriteLine(JsonUtility.ToJson(pack));
            }
        }
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        if(!(UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject) == null && UnityEditor.PrefabUtility.GetPrefabObject(gameObject) != null))
        UnityEditor.PrefabUtility.ReplacePrefab(gameObject, UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject), UnityEditor.ReplacePrefabOptions.ConnectToPrefab);
#endif
    }

    public void LoadPackages()
    {
        //Debug.Log("richiamato");
        if (File.Exists(path))
        {
            //Debug.Log("il file c'è");
            packages.Clear();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    //Debug.Log("leggo");
                    while (reader.Peek() >= 0)
                    {
                        packages.Add(JsonUtility.FromJson<LanguagePackage>(reader.ReadLine()));
                        //Debug.Log("letta una riga");
                    }
                    //backupSaveData = saveDataSlots.backupSaveData;
                    //saveData = saveDataSlots.saveData;
                }
            }
        }
    }

    public void AddChangeLanguageListener(LanguageDelegate action)
    {
        onChangeLanguage += action;
    }

    public void RemoveChangeLanguageListener(LanguageDelegate action)
    {
        onChangeLanguage -= action;
    }

    public void UpdatePackage(LanguagePackage pack)
    {
        bool contains = false;
        foreach (LanguagePackage package in packages)
        {
            if (pack.key == package.key)
            {
                contains = true;
                package.english = pack.english;
                package.italian = pack.italian;
                package.spanish = pack.spanish;
                package.german = pack.german;
                package.french = pack.french;
            }
        }
        if (!contains)
            packages.Add(pack);
    }

    public LanguagePackage LoadPackage(string key)
    {
        LanguagePackage returnable = new LanguagePackage();
        foreach (LanguagePackage package in packages)
        {
            if (key == package.key)
            {
                returnable = package;
            }
        }
        return returnable;
    }

    public string GetActualValue(string key)
    {
        string returnable = "UNDEFINED";
        foreach (LanguagePackage package in packages)
        {
            if (key == package.key)
            {
                switch (actualLanguage)
                {
                    case Language.English:
                        returnable = package.english;
                        break;
                    case Language.Italian:
                        returnable = package.italian;
                        break;
                    case Language.Spanish:
                        returnable = package.spanish;
                        break;
                    case Language.German:
                        returnable = package.german;
                        break;
                    case Language.French:
                        returnable = package.french;
                        break;
                }
            }
        }
        return returnable;
    }

    //public void AddSavePackagesListener(VoidDelegate action)
    //{
    //    onSavePackages += action;
    //}

    //public void RemoveSavePackagesListener(VoidDelegate action)
    //{
    //    onSavePackages -= action;
    //}

    //public void AddLoadPackagesListener(VoidDelegate action)
    //{
    //    onLoadPackages += action;
    //}

    //public void RemoveLoadPackagesListener(VoidDelegate action)
    //{
    //    onLoadPackages -= action;
    //}
}
