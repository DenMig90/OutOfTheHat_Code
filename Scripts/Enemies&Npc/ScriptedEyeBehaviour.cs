using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptedEyeBehaviour : MonoBehaviour {

    public Animator anim;
    public float rotationRange = 45;

    [SerializeField]
    private bool isOpened = true;

    //private Quaternion baseRotation;
    private Vector3 baseDir;

	// Use this for initialization
	void Start () {
        //transform.Rotate(-Vector3.right, rotationRange / 2);
        //baseRotation = transform.rotation;
        baseDir = transform.up;
        //isOpened = false;
        if(anim!=null)
        anim.SetBool("EyeOpened", isOpened);
    }
	
	// Update is called once per frame
	void Update () {
        if(isOpened)
        {
            Quaternion targetRotation = transform.rotation;
            Vector3 look = GameController.instance.player.transform.position - transform.position;
            look.z = 0;

            //Quaternion q = Quaternion.LookRotation(look, transform.forward);
            //Debug.Log(Quaternion.Angle(q, baseRotation));
            //if (Quaternion.Angle(q, baseRotation) <= rotationRange)
            //    targetRotation = q;
            //transform.rotation = targetRotation;
            if (Vector3.Angle(baseDir, look) <= rotationRange)
            {
                float rot_z = Mathf.Atan2(look.y, look.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
            }
            //Debug.Log(Vector3.Angle(baseDir, look));
        }
	}

    public void Open()
    {
        isOpened = true;
        anim.SetBool("EyeOpened", isOpened);
        //Debug.Log(isOpened);
    }
}
