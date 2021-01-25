using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerAnimator : MonoBehaviour {

    public float delay;
    public string triggerName;
    public UnityEvent onStart;
    public Animator anim;

    private Coroutine routine;

    private void Awake()
    {
        if(anim == null)
            anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start () {
        routine = null;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Trigger()
    {
        if(routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(TriggerEvent());
    }

    public void ResetToStart()
    {
        if (routine != null)
            StopCoroutine(routine);
        anim.Rebind();
    }

    private IEnumerator TriggerEvent()
    {
        float time = 0;
        while (time < delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        anim.SetTrigger(triggerName);
        onStart.Invoke();
        routine = null;
    }
}
