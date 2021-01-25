using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class BoolEvent : UnityEvent<bool> { }

[RequireComponent(typeof(Text))]
public class TextToggle : MonoBehaviour {

    public string activeMessageKey;
    public string disactiveMessageKey;
    public BoolEvent onToggle;
    [SerializeField]
    private bool active;

    public bool Active
    {
        set
        {
            active = value;
            ManageText(Language.English);
        }
        get
        {
            return active;
        }
    }

    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        LocalizationManager.instance.AddChangeLanguageListener(ManageText);
    }

    private void OnDisable()
    {
        LocalizationManager.instance.RemoveChangeLanguageListener(ManageText);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Toggle()
    {
        active = !active;
        ManageText(Language.English);
        onToggle.Invoke(active);
    }

    public void ManageText(Language actual)
    {
        text.text = active ? LocalizationManager.instance.GetActualValue(activeMessageKey) : LocalizationManager.instance.GetActualValue(disactiveMessageKey);
    }
}
