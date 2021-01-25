using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public enum TrapState
{
    Idle,
    Triggered,
    Return
}

public class TrapBehaviour : MonoBehaviour {

    public float delay;
    public float attackSpeed;
    public float returnDelay;
    public float returnSpeed;
    public Transform startPos;
    public Transform endPos;
    public Transform disappearPos;
    public Transform playerCaughtAnchor;
    public Animator clawAnim;
    public SpriteRenderer armRenderer;
    public LineRenderer armExtension;
    public Transform armStartPoint;
    public AnimateScale clawCloseAnim;
    [EventRef] public string grabSound;
    [HideInInspector]
    public bool canTrigger;
    public bool canUpdate;

    public bool PlayerCaught
    {
        get
        {
            return playerCaught;
        }
    }

    public bool IsAttacking
    {
        get
        {
            return isAttacking;
        }
    }
    //private Vector3 returnPos;
    //[SerializeField]
    //private TrapState state;

    private Coroutine attackRoutine;
    private bool playerCaught;
    private bool isAttacking;

    // Use this for initialization
    void Start () {
        //state = TrapState.Idle;
        //startPos = transform.position;
        transform.position = startPos.position;
        //GameController.instance.AddTrap(this);
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        attackRoutine = null;
        playerCaught = false;
        canTrigger = true;
        isAttacking = false;
	}
	
	// Update is called once per frame
	void Update () {
        //switch (state)
        //{
        //    case TrapState.Triggered:
        //        transform.position += -transform.up * attackSpeed * Time.deltaTime;
        //        break;
        //    case TrapState.Return:
        //        //transform.position = Vector3.Lerp(returnPos, startPos, Time.deltaTime*returnSpeed);
        //        Vector3 dir = startPos - transform.position;
        //        transform.position += dir.normalized * returnSpeed * Time.deltaTime;
        //        //Debug.Log(DistanceLineSegmentPoint(transform.position + transform.up * returnSpeed * Time.deltaTime, transform.position, startPos));
        //        if (Vector3.Distance(startPos, transform.position) < returnSpeed * Time.deltaTime)
        //        {
        //            transform.position = startPos;
        //            state = TrapState.Idle;
        //        }
        //        break;
        //}
        
	}

    public void LateUpdate()
    {
        if(canUpdate)
        {
            if(armExtension != null)
            {
                if (Vector3.Distance(startPos.position, endPos.position) > Vector3.Distance(armStartPoint.position, endPos.position))
                {
                    armExtension.enabled = true;
                    armExtension.SetPosition(0, armExtension.transform.InverseTransformPoint(startPos.position));
                    armExtension.SetPosition(1, armExtension.transform.InverseTransformPoint(armStartPoint.position));
                }
                else
                {
                    armExtension.enabled = false;
                }

            }
        }
    }

    public void ResetToStart()
    {
        //state = TrapState.Idle;
        if(attackRoutine!= null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
        transform.position = startPos.position;
        playerCaught = false;
        canTrigger = true;
        isAttacking = false;
        clawAnim.Rebind();
    }

    // Distance to point (p) from line segment (end points a b)
    float DistanceLineSegmentPoint(Vector3 a, Vector3 b, Vector3 p)
    {
        // If a == b line segment is a point and will cause a divide by zero in the line segment test.
        // Instead return distance from a
        if (a == b)
            return Vector3.Distance(a, p);

        // Line segment to point distance equation
        Vector3 ba = b - a;
        Vector3 pa = a - p;
        return (pa - ba * (Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba))).magnitude;
    }

    public void Trigger()
    {
        if (attackRoutine == null)
        {
            attackRoutine = StartCoroutine(Attack());
        }
        
        //if(state == TrapState.Idle)
        //    state = TrapState.Triggered;
    }

    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log(collision.gameObject.name);
        //if (state == TrapState.Triggered)
        //{
        //    //returnPos = transform.position;
        //    state = TrapState.Return;
        //}
        if (collision.gameObject.tag == "Player" && !playerCaught && isAttacking)
        {
            GameController.instance.player.Capture();
            GameController.instance.mainCamera.Block(true);
            playerCaught = true;
            GameController.instance.player.transform.position = playerCaughtAnchor.position;
            clawAnim.ResetTrigger("ClawGrow");
            clawAnim.SetTrigger("ClawClose");
        }

        if(collision.gameObject.tag == "Destroyable" && !playerCaught && isAttacking)
        {
            DestroyableBehaviour obj = collision.gameObject.GetComponent<DestroyableBehaviour>();
            obj.Damage(1);
        }
    }

    public void PlayGrabSound()
    {
        GameController.instance.audioManager.PlayGenericSound(grabSound, clawAnim.gameObject);
    }

    private IEnumerator Attack()
    {
        float time = 0;
        float distance = Vector3.Distance(startPos.position, endPos.position);
        float attackTime = distance / attackSpeed;
        float returnTime = distance / returnSpeed;
        canTrigger = false;
        clawCloseAnim.AnimationReset();
        while(time < delay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.fixedDeltaTime;
        }
        time = 0;
        isAttacking = true;
        PlayGrabSound();
        clawAnim.SetTrigger("ClawGrow");
        while (time < attackTime)
        {
            transform.position = Vector3.Lerp(startPos.position, endPos.position, time / attackTime);
            yield return new WaitForFixedUpdate();
            if (playerCaught)
                break;
            time += Time.fixedDeltaTime;
        }
        isAttacking = false;
        float distanceCoveredPerc = time / attackTime;
        if (distanceCoveredPerc > 1)
            distanceCoveredPerc = 1;
        float remainingDistancePerc = 1 - distanceCoveredPerc;
        time = 0;
        while (time < returnDelay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.fixedDeltaTime;
        }
        //Debug.Log(distanceCoveredPerc);
        //Debug.Log(remainingDistancePerc);
        time = remainingDistancePerc * returnTime;
        bool disappeared = false;
        float prevDistance = 1000f;
        float actualDistance = 0;
        while (time < returnTime)
        {
            transform.position = Vector3.Lerp(endPos.position, startPos.position, time / returnTime);
            if (playerCaught)
                GameController.instance.player.transform.position = playerCaughtAnchor.position;
            if(disappearPos != null)
            {
                actualDistance = Vector3.Distance(transform.position, new Vector3(disappearPos.position.x, disappearPos.position.y, transform.position.z));
                if((actualDistance == 0 || actualDistance >= prevDistance) && !disappeared)
                {
                    //Debug.Log("chiudo");
                    //clawAnim.SetTrigger("ClawDisappear");
                    clawCloseAnim.PlayForward();
                    disappeared = true;
                }
                prevDistance = actualDistance;
            }
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        if(playerCaught)
            GameController.instance.player.Kill();
        canTrigger = true;
        attackRoutine = null;
    }
}
