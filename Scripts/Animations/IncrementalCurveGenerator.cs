using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncrementalCurveGenerator : MonoBehaviour {
    public AnimationCurve curve;
    public int steps;
    public bool crescent;

    //private bool prevCrescent;

	// Use this for initialization
	void Start () {
        Generate();
        //prevCrescent = crescent;
	}
	
	// Update is called once per frame
	void Update () {
        //if ((curve.keys.Length != (steps + 1)) || (prevCrescent != crescent))
        //{
        //    Generate();
        //    prevCrescent = crescent;
        //}
	}

    public void Generate()
    {
        while(curve.length > 0)
        {
            curve.RemoveKey(0);
        }
        float[] array = new float[steps];
        for (int i = 0; i < steps; i++)
        {
            array[i] = i + 1;
        }
        float total = 0;
        for (int i = 0; i < steps; i++)
        {
            total += array[i];
        }

        float incremental = 0;
        float value = 1;
        float time = 0;
        time = (incremental / total);
        if (!crescent)
            time = 1 - time;
        curve.AddKey(new Keyframe(time, value, 0, 0));
        for (int i = 0; i < steps; i++)
        {
            incremental += array[i];
            if (value == 0)
                value = 1;
            else
                value = 0;
            //value *= -1;
            time = (incremental / total);
            if (!crescent)
                time = 1 - time;
            curve.AddKey(new Keyframe(time, value, 0, 0));
        }
        //Debug.Log("genero");
    }
}
