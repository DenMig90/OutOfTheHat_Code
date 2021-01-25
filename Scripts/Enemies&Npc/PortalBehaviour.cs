using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalBehaviour : MonoBehaviour {
    public PortalBehaviour linkedPortal;

    [HideInInspector]
    public bool canTeleport;
    public new BoxCollider collider;
    public float minExitVelocity;

    private void Awake()
    {
        //collider = GetComponent<BoxCollider>();
    }

    // Use this for initialization
    void Start () {
        if (linkedPortal != null)
            linkedPortal.Link(this);
        canTeleport = true;
	}
	
	// Update is called once per frame
	void Update () {
		if(!canTeleport)
        {
            Collider[] collisions = Physics.OverlapBox(collider.bounds.center, collider.bounds.extents, transform.rotation, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            bool found = false;
            foreach(Collider col in collisions)
            {
                if (col.gameObject.tag == "Player" || col.gameObject.tag == "Teleport")
                    found = true;
            }
            if (!found)
                canTeleport = true;
        }
	}

    //public void ResetToStart()
    //{
    //    canTeleport = true;
    //}

    public void Link(PortalBehaviour portal)
    {
        if (linkedPortal != null)
            return;
        linkedPortal = portal;
    }

    private void OnTriggerEnter(Collider other)
    {
        if((other.gameObject.tag == "Player" || other.gameObject.tag == "Teleport") && canTeleport)
        {
            Vector3 distance = other.transform.position - transform.position;
            //Debug.Log(other.transform.position.y);
            other.transform.position = linkedPortal.transform.position + distance;
            //Debug.Log(other.transform.position.y);
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            float velocityMagn = rb.velocity.magnitude;
            Vector3 dir = -collider.transform.forward - rb.velocity.normalized;
            Vector3 velocity = linkedPortal.collider.transform.forward + dir;
            velocity = Quaternion.AngleAxis(180, linkedPortal.collider.transform.forward) * velocity;
            //Debug.Log(velocityMagn);
            if (velocityMagn < linkedPortal.minExitVelocity)
                velocity = velocity.normalized * linkedPortal.minExitVelocity;
            else
                velocity = velocity.normalized * velocityMagn;
            rb.velocity = velocity;
            linkedPortal.canTeleport = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Teleport")
        {
            canTeleport = true;
        }
    }
}
