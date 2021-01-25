using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDividerCollector : MonoBehaviour {
    public int size;

    [HideInInspector]
    public int actual;
    [HideInInspector]
    public int target;
    [HideInInspector]
    public Coroutine routine;

    private SpriteDivider[] all;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CancelDividing()
    {
        StopCoroutine(routine);
        routine = null;
    }

    public void StartDividingAll()
    {
        all = FindObjectsOfType<SpriteDivider>();
        routine = StartCoroutine(DivideAll());
    }

    private IEnumerator DivideAll()
    {
        actual = 0;
        target = all.Length;
        foreach (SpriteDivider divider in all)
        {
            divider.size = size;
            divider.StartDivide();
            yield return new WaitWhile(() => divider.actual != divider.target);
            actual++;
            yield return null;
        }
        routine = null;
    }
}
