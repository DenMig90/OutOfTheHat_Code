using UnityEngine;
using System.Collections;

public class ShootingEnabler : MonoBehaviour {

    //public float rotationSpeed = 5F;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
	
	}

    void OnCollisionEnter(Collision col)
    {
        //Debug.Log("1");
        if(col.collider.tag=="Player")
        {
        //Debug.Log("2");
            col.collider.GetComponent<PlayerController>().EnableShooting();
            gameObject.SetActive(false);
        }
    }
}
