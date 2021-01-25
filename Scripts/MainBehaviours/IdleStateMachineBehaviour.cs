using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleStateMachineBehaviour : StateMachineBehaviour {

    public int minIdleIndex = 0;
    public int maxIdleIndex = 1;

    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {

        animator.SetInteger("IdleIndex", Random.Range(minIdleIndex, maxIdleIndex+1));
        //Debug.Log("idle");
    }
}
