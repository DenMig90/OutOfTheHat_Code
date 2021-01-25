using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCheckpointBehaviour : MonoBehaviour {

    public Animator anim;
    [Tooltip("Direction is green arrow")]
    public Transform playerRespawn;

    [SerializeField]
    private bool canShake;

    // Use this for initialization
    void Awake () {
        canShake = true;
        anim.SetBool("IsCustom", true);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Shake()
    {
        if (canShake)
        {
            canShake = false;
            anim.ResetTrigger("ShakeRX");
            anim.ResetTrigger("ShakeSX");
            if (GameController.instance.player.transform.position.x < transform.position.x)
                anim.SetTrigger("ShakeRX");
            else
                anim.SetTrigger("ShakeSX");
        }
    }

    public void Grow()
    {
        anim.SetTrigger("Grow");
        canShake = false;
        anim.ResetTrigger("ShakeRX");
        anim.ResetTrigger("ShakeSX");
    }

    public void EnableShake()
    {
        canShake = true;
    }
}
