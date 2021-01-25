using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SecretBehaviour : MonoBehaviour {

    [SerializeField] private string key;
    [SerializeField] private bool discovered = false;
    [SerializeField] private Sprite page;
    [SerializeField] private UnityEvent onOpen;
    [SerializeField] private new SpriteRenderer renderer;
    [SerializeField] private Sprite discoveredSprite;
    [SerializeField] private Sprite notDiscroveredSprite;

    public bool Discovered
    {
        get { return discovered; }
        set
        {
            discovered = value;
            if(renderer!=null)
                renderer.sprite = discovered ? discoveredSprite : notDiscroveredSprite;
            if (discovered)
            {
                GameController.instance.OnSecretDiscovered();
            }
        }
    }

    public string Key { get { return key; } }

    private void Awake()
    {
        if (renderer == null)
            renderer = GetComponent<SpriteRenderer>();
        //GameController.instance.AddSecret(this);
    }

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {

    }

    public void AddInteraction()
    {
        if (!discovered)
        {
            GameController.instance.StartInteraction(Open);
        }
    }

    public void RemoveInteraction()
    {
        if (!discovered)
        {
            GameController.instance.EndInteraction(Open);
        }
    }

    public void Open()
    {
        RemoveInteraction();
        if (page != null)
            GameController.instance.ShowSecretPage(page);
        onOpen.Invoke();
        Discovered = true;
        //gameObject.SetActive(false);
        GameController.instance.SaveSecrets();
    }
}
