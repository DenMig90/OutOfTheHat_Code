using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldMentorIldeSMBehaviour : StateMachineBehaviour {

    public float minIdleDuration;
    public float maxIdleDuration;
    public float minAltDuration;
    public float maxAltDuration;

    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        //Debug.Log("entro");
        animator.GetComponent<OldMentorBehaviour>().StartLoop(animator);
        //animator.SetInteger("IdleIndex", Random.Range(minIdleIndex, maxIdleIndex + 1));
        //Debug.Log("idle");
    }

    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        animator.GetComponent<OldMentorBehaviour>().StopLoop();
        //animator.SetInteger("IdleIndex", Random.Range(minIdleIndex, maxIdleIndex + 1));
        //Debug.Log("idle");
    }
}
