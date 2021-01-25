using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveTriggerBehaviour : MonoBehaviour {

    public float speed;
    public bool horizontally;
    public bool vertically;

    private Coroutine routine;

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
        routine = StartCoroutine(Movement());
    }

    private IEnumerator Movement()
    {
        if (speed == 0)
            yield break;
        PlayerController player = GameController.instance.player;
        Vector3 target = transform.position;
        while ((horizontally && (player.Rigidbody.position.x != transform.position.x)) || (vertically && (player.Rigidbody.position.y != transform.position.y)))
        {
            //Debug.Log("muovo");
            target.z = player.Rigidbody.position.z;
            if (!vertically)
                target.y = player.Rigidbody.position.y;
            if (!horizontally)
                target.x = player.Rigidbody.position.x;
            player.Rigidbody.position = Vector3.MoveTowards(player.Rigidbody.position, target, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        routine = null;
    }
}
