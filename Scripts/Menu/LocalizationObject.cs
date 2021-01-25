using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationObject : MonoBehaviour {

    public LanguagePackage pack;

    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    // Use this for initialization
    void Start () {
		
	}

    private void OnEnable()
    {
        LocalizationManager.instance.AddChangeLanguageListener(ManageLanguage);
        //LocalizationManager.instance.AddSavePackagesListener(OnSavePackage);
        //LocalizationManager.instance.AddLoadPackagesListener(OnLoadPackage);
        OnLoadPackage();
        Language actual = LocalizationManager.instance.ActualLanguage;
        ManageLanguage(actual);
    }

    private void OnDisable()
    {
        LocalizationManager.instance.RemoveChangeLanguageListener(ManageLanguage);
        //LocalizationManager.instance.RemoveSavePackagesListener(OnSavePackage);
        //LocalizationManager.instance.RemoveLoadPackagesListener(OnLoadPackage);
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void ManageLanguage(Language actual)
    {
        if (text == null)
            return;
        switch(actual)
        {
            case Language.English:
                text.text = pack.english;
                break;
            case Language.Italian:
                text.text = pack.italian;
                break;
            case Language.Spanish:
                text.text = pack.spanish;
                break;
            case Language.German:
                text.text = pack.german;
                break;
            case Language.French:
                text.text = pack.french;
                break;
        }
    }

    public void OnSavePackage()
    {
        LocalizationManager.instance.UpdatePackage(pack);
    }

    public void OnLoadPackage()
    {
        foreach(LanguagePackage package in LocalizationManager.instance.packages)
        {
            if(pack.key == package.key)
            {
                pack.english = package.english;
                pack.italian = package.italian;
                pack.spanish = package.spanish;
                pack.german = package.german;
                pack.french = package.french;
            }
        }
    }
}
