using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGlideStateMachineBehaviour : StateMachineBehaviour {

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int stateMachinePathHash)
    {

        GameController.instance.player.OnGlideStart();

    }
}
