using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MentorTranformationBehaviour : MonoBehaviour {

    public Material saneMaterial;
    public float delay = 1.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Transformation()
    {
        Invoke("PerformTransformation", delay);
    }

    private void PerformTransformation()
    { 
        int childNr = transform.childCount;

        for(int i=0; i<childNr; i++)
        {
            if(transform.GetChild(i).GetComponent<Renderer>())
            {
                transform.GetChild(i).GetComponent<Renderer>().material = saneMaterial;
            }

        }
    }
}
