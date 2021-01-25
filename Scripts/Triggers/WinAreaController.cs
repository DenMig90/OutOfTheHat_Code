using UnityEngine;
using System.Collections;

public class WinAreaController : MonoBehaviour {

    public GameObject winGUI;
	public GameObject player;
    public Animator anim;

	IEnumerator OnTriggerEnter(Collider col)
    {
		if(col.tag=="Player" )
        {
            
            anim.SetTrigger("Open");

            yield return new WaitForSeconds(3);
            winGUI.SetActive(true);
            player.GetComponent<PlayerController>().enabled = false;
        }

//		player.GetComponent<PlayerController>().enabled = false;


    }
}
