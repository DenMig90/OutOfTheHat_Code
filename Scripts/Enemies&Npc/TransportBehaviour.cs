using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportBehaviour : MonoBehaviour {

    //private const float removeFrequency = 0.1f;
    private List<ToMove> objectsToMove;
    private Vector3 lastPosition;
    private ActualPlatformBehaviour actualPlatform = null;
    private MovingPlatformController movingPlatform = null;

    //private List<int> indexesToRemove = new List<int>();
    //private float lastTimeRemove=0f;
    //private List<ToMove> toRemove = new List<ToMove>();

    // Use this for initialization
    void Start () {
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        objectsToMove = new List<ToMove>();
        lastPosition = transform.position;
        actualPlatform = GetComponent<ActualPlatformBehaviour>();
        movingPlatform = GetComponent<MovingPlatformController>();
    }

    void ResetToStart()
    {
        objectsToMove.Clear();
        //toRemove.Clear();
    }
	
	// Update is called once per frame
	void FixedUpdate () {


        // Muovo gli oggetti sulla piattaforma

        for(int i = 0; i < objectsToMove.Count; i++)
        {
            if (objectsToMove[i].rb.tag == "Teleport" && (!objectsToMove[i].rb.gameObject.activeSelf || !objectsToMove[i].collider.enabled))
            {
                objectsToMove.RemoveAt(i);
                i--;
            }
            else
            {
                if (objectsToMove[i].justAdded)
                    objectsToMove[i].justAdded = false;
                else
                    objectsToMove[i].rb.position += transform.position - lastPosition;
            }
        }

        /*foreach (ToMove remove in toRemove)
            objectsToMove.RemoveAll(x => x.rb == remove.rb);

        toRemove.Clear();*/

        lastPosition = transform.position;

        /*if (Time.timeSinceLevelLoad > lastTimeRemove + removeFrequency)
        {
            foreach (int index in indexesToRemove)
            {
                objectsToMove.RemoveAt(index);
            }
            indexesToRemove.Clear();
            lastTimeRemove = Time.timeSinceLevelLoad;
        }*/
    }

    public void AddPlayer()
    {
        //Debug.Log("AddPlayer 1");
        AddSomething(GameController.instance.player.gameObject);
        
        if(movingPlatform)
        {
            movingPlatform.TryBouncing();
        }
    }
    void OnCollisionEnter(Collision col)
    {

        if (col.collider.tag == "Teleport")
        {
            AddSomething(col.gameObject);
        }
    }

    void AddSomething(GameObject go)
    {
        //yield return new WaitForFixedUpdate();
        Rigidbody rb = go.GetComponent<Rigidbody>();
        Collider collider = go.GetComponent<Collider>();
        if (rb != null && collider != null)
        {
        //Debug.Log("AddPlayer 2");
            ToMove add = new ToMove();
            //rb.velocity = Vector3.zero;
            add.rb = rb;
            add.collider = collider;

            bool found = false;
            foreach (ToMove tm in objectsToMove)
            {
                if (tm.rb == add.rb)
                {
        //Debug.Log("player found");
                    found = true;
                    break;
                }
            }

            if (!found)
            {
        //Debug.Log("AddPlayer ok");
                objectsToMove.Add(add);
            }

            /*bool found = false;

            foreach(ToMove tm in objectsToMove)
            {
                if (tm.rb == add.rb)
                {
                    found = true;
                }
            }
                
            if(!found)
            {
                objectsToMove.Add(add);
            }*/
        }

        //			col.collider.transform.Translate(new Vector3(movement.x,movement.y,0));

    }

    public void RemovePlayer()
    {
        RemoveSomething(GameController.instance.player.gameObject);
        if(actualPlatform)
        {
            actualPlatform.CancelPlayerSaving();
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.collider.tag == "Teleport")
        {
            RemoveSomething(col.gameObject);

        }
    }

    void RemoveSomething(GameObject go)
    {
        //Debug.Log("tolgo");
        /*yield return new WaitForFixedUpdate();
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            for (int i = 0; i < objectsToMove.Count; i++)
            {
                if (objectsToMove[i].rb == rb)
                {
                    if(toRemove.FindAll(x => x.rb == rb).Count == 0)
                        toRemove.Add(i);
                }
            }
        }*/

        //yield return new WaitForFixedUpdate();
        Rigidbody rb = go.GetComponent<Rigidbody>();
        objectsToMove.RemoveAll(x => x.rb == rb);
        /*Collider collider = go.GetComponent<Collider>();
        if (rb != null && collider != null)
        {
            ToMove remove = new ToMove();
            //rb.velocity = Vector3.zero;
            remove.rb = rb;
            remove.collider = collider;
            
            if(toRemove.FindAll(x=> x.rb == remove.rb).Count == 0)
                toRemove.Add(remove);
        }*/
    }
}
