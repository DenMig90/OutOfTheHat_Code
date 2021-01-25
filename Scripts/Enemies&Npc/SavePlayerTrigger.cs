using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePlayerTrigger : MonoBehaviour {

    public MovingPlatformController platform;

    private PlayerController player;
    private Rigidbody playerRB;
    // Use this for initialization
    void Start () {

        player = GameController.instance.player;
        playerRB = GameController.instance.player.GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(player)
            {
                ActualPlatformBehaviour lastPlatform = player.lastGroundObject.GetComponent<ActualPlatformBehaviour>();
                MovingPlatformController savingPlatform;
                if (lastPlatform)
                {
                    savingPlatform = lastPlatform.target;
                }
                else
                {
                    savingPlatform = platform;
                }

                if (savingPlatform)
                { 
                    if (savingPlatform.savePlayer)
                    {
                        if (savingPlatform.SavePlayer())
                        {
                            playerRB.velocity = Vector3.zero;
                            return;
                        }
                    }
                }
            }
        }
    }
}
