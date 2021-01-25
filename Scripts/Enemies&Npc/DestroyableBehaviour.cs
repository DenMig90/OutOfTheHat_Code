using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DestroyableBehaviour : MonoBehaviour {

    public int maxLifePoints = 1;
    public UnityEvent onDestroy;
    public UnityEvent onReset;

    private Collider col;
    //private Renderer[] renderers;
    private int lifePoints;

    private void Awake()
    {
        col = GetComponent<Collider>();
        //renderers = GetComponentsInChildren<Renderer>();
    }

    // Use this for initialization
    void Start () {
        ResetToStart();
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ResetToStart()
    {
        lifePoints = maxLifePoints;
        onReset.Invoke();
        //gameObject.SetActive(true);
        //foreach(Renderer renderer in renderers)
        //    renderer.enabled = true;
        col.enabled = true;
    }

    public void Damage(int value)
    {
        lifePoints -= value;
        if (lifePoints < 0)
            lifePoints = 0;
        if(lifePoints == 0)
        {
            onDestroy.Invoke();
            //gameObject.SetActive(false);
            //foreach (Renderer renderer in renderers)
            //    renderer.enabled = false;
            col.enabled = false;
        }
    }
}
