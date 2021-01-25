using UnityEngine;
using System.Collections;

public class TeleportDestroyer : MonoBehaviour {

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.tag != "Teleporter")
            return;

        col.collider.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag != "Teleporter")
            return;

        col.gameObject.SetActive(false);
    }
}
