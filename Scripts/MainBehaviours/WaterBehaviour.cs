using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBehaviour : MonoBehaviour {

    public float currentSpeed;

    //public Vector3 CurrentDirection { get { return transform.right; } }
    [SerializeField]
    private bool playerOn;
    //private HatBehaviour boat;

	// Use this for initialization
	void Start () {
        //boat = null;
        playerOn = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //if (boat != null && !boat.gameObject.activeSelf)
        //    boat = null;
        if (playerOn && GameController.instance.player.IsDead)
            playerOn = false;
        if (playerOn)
        {
            GameController.instance.player.Rigidbody.MovePosition(GameController.instance.player.Rigidbody.position + transform.right * currentSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if (GameController.instance.player.IsBoat)
            {
                playerOn = true;
                GameController.instance.player.transform.up = transform.up;
            }
            else
            {
                GameController.instance.player.StartKill();
                playerOn = false;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {

        if (collision.gameObject.tag == "Player")
        {
            playerOn = false;
        }
    }

}
