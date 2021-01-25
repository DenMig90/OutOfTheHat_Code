using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class JumpTriggerBehaviour : MonoBehaviour {

    public float force = 30;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 pos = transform.position;
        Vector3 vel = transform.up * force;
        Vector3 lastPos = pos;
        int i = 1;
        i++;

        while (i < 1000)
        {
            vel = vel + Physics.gravity * Time.fixedDeltaTime;
            pos = pos + vel * Time.fixedDeltaTime;
            pos.z = 0;
            if (Physics.Raycast(lastPos, (pos - lastPos).normalized, (pos - lastPos).magnitude, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                break;
            }
            Debug.DrawLine(lastPos, pos, Color.yellow, Time.deltaTime);
            //line.SetPosition(i, new Vector3(pos.x, pos.y, 0));
            //line.SetPosition(i, new Vector3(pos.x, pos.y, 0));
            lastPos = pos;
            i++;
        }
    }
}
