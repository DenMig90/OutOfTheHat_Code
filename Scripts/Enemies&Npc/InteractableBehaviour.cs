using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum InteractableType
{
    Activable,
    Switchable,
    Pressable
}

public class InteractableBehaviour : MonoBehaviour {
    public InteractableType type;
    public UnityEventBool onActiveChange;
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    [SerializeField]
    private bool isActive;

    private bool IsActive
    {
        get { return isActive; }
        set
        {
            isActive = value;
            onActiveChange.Invoke(isActive);
            if (isActive)
                onActivate.Invoke();
            else
                onDeactivate.Invoke();
        }
    }

	// Use this for initialization
	void Start () {
        ResetToStart();
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
    }
	
	// Update is called once per frame
	void Update () {
		if(isActive)
        {
            if (!GameController.instance.player.teleportHat.gameObject.activeSelf)
                IsActive = false;
        }
	}

    public void ResetToStart()
    {
        IsActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Teleport")
        {
            GameController.instance.player.teleportHat.Block();
            switch (type)
            {
                case InteractableType.Activable:
                case InteractableType.Pressable:
                    if (!IsActive)
                    {
                        IsActive = true;
                    }
                    break;
                case InteractableType.Switchable:
                    IsActive = !IsActive;
                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Teleport")
        {
            switch (type)
            {
                case InteractableType.Pressable:
                    if (IsActive)
                    {
                        IsActive = false;
                    }
                    break;
            }
        }
    }
}
