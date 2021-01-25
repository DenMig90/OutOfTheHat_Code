using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using UnityEngine.Events;
using FMOD.Studio;

[RequireComponent(typeof(AnimateScale))]
[RequireComponent(typeof(AnimateRotationAround))]
public class BouncingAnimation : MonoBehaviour {

    public string colliderTag;
    public float rotationAmountEnter;
    public float rotationAmountExit;
    public float velocityThresholdEnter;
    public float velocityThresholdExit;
    public int playerPositionHistoryLenght=10;
    public GameObject[] objectsOnMushroom;
    [Header("FMOD Events")]
    [EventRef] public string bounceSound;
    public UnityEvent onLanding;


    private struct ObjOnMPh
    {
        public GameObject obj;
        public GameObject placeholder;
        public Vector3 offset;

        public ObjOnMPh(GameObject _obj, GameObject _placeholder, Vector3 _offset)
        {
            obj = _obj;
            placeholder = _placeholder;
            offset = _offset;
        }
    }
    private AnimateScale animScale;
    private AnimateRotationAround animRot;
    private float duration;
    private float lastBounceTime = 0f;
    private Collision lastCollision;
    private Coroutine exitCoroutine=null;
    private Transform playerTransform;
    private Vector3[] playerPositionHistory;
    private float angleRange;
    private List<ObjOnMPh> objectsOnMushroomPlaceholder = new List<ObjOnMPh>();

    // Use this for initialization
    void Start () {
        animScale = GetComponent<AnimateScale>();
        animRot = GetComponent<AnimateRotationAround>();
        duration = animScale.duration > animRot.duration ? animScale.duration : animRot.duration;
        playerTransform = GameController.instance.player.GetComponent<Transform>();
        angleRange = GameController.instance.player.GetComponent<PlayerController>().maxJumpSlope;
        playerPositionHistory = new Vector3[playerPositionHistoryLenght];

        foreach (GameObject go in objectsOnMushroom)
        {
            AddObjectOnMushroom(go);
        }
    }

    private void Update()
    {
        List<ObjOnMPh> toRemove = new List<ObjOnMPh>();
        foreach(ObjOnMPh oph in objectsOnMushroomPlaceholder)
        {
            if (oph.obj == null)
            {
                toRemove.Add(oph);
                continue;
            }

            oph.obj.transform.position = oph.placeholder.transform.position + (oph.placeholder.transform.rotation * oph.offset);
            oph.obj.transform.rotation = oph.placeholder.transform.rotation;
        }

        foreach(ObjOnMPh oph in toRemove)
        {
            objectsOnMushroomPlaceholder.Remove(oph);
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        for(int i=playerPositionHistoryLenght-1;i>=1;i--)
        {
            playerPositionHistory[i] = playerPositionHistory[i - 1];
        }
        playerPositionHistory[0] = playerTransform.position;
        playerPositionHistory[0].x = 0;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag != "Teleport")
        {
            Bounce(collision, false);
            if (exitCoroutine != null)
            {
                StopCoroutine(exitCoroutine);
                exitCoroutine = null;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag != "Teleport")
        {
            if (exitCoroutine != null)
            {
                StopCoroutine(exitCoroutine);
                exitCoroutine = null;
            }

            exitCoroutine = StartCoroutine(DelayedOnCollisionExit(lastCollision));
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        lastCollision = collision;
    }

    private void Bounce(Collision collision, bool isExit)
    {
        if (Time.time - lastBounceTime < duration)
            return;


        float velocity = (playerPositionHistory[0] - playerPositionHistory[playerPositionHistoryLenght - 1]).magnitude / (Time.fixedDeltaTime * playerPositionHistoryLenght);
        //Debug.Log(velocity);

        float velocityThreshold = isExit ? velocityThresholdExit : velocityThresholdEnter;
        {
            float normalAngle = Vector3.SignedAngle(collision.contacts[0].normal, -GameController.instance.Up,Vector3.forward);
            if (normalAngle > 180) normalAngle -= 180;
            
            if (Mathf.Abs(normalAngle) < angleRange)
            {
                if (!isExit)
                {
                    onLanding.Invoke();
                    PlayBounceSound();
                }

                PerformBounce(((isExit)?rotationAmountExit:rotationAmountEnter) * normalAngle, false);
            }

        }
    }

    public void PerformBounce(float angle, bool isHand)
    {
        if(isHand)
            onLanding.Invoke();

        animScale.PlayForward();

        animRot.from = 0;
        animRot.to = angle;

        animRot.PlayForward();

        lastBounceTime = Time.time;
    }

    private IEnumerator DelayedOnCollisionExit(Collision collision)
    {
        for (int i = 0; i < playerPositionHistoryLenght; i++)
            yield return new WaitForFixedUpdate();

        Bounce(collision, true);
        exitCoroutine = null;
    }

    public void AddObjectOnMushroom(GameObject go)
    {
        GameObject obj = new GameObject();
        RaycastHit hit;
        if (Physics.Raycast(go.transform.position + Vector3.up, -go.transform.up, out hit))
        {
            if (hit.collider != GetComponent<Collider>())
            {
                Debug.LogWarning("1: The object " + go.name + " on the mushroom is not actually on the mushroom " + gameObject.name);
            }
            else
            {
                GameObject placeholder = GameObject.Instantiate(obj, hit.point, Quaternion.Euler(0f, 0f, 0f));
                Vector3 offset = go.transform.position - placeholder.transform.position;
                placeholder.transform.parent = transform;
                objectsOnMushroomPlaceholder.Add(new ObjOnMPh(go, placeholder, offset));
            }
        }
        else
        {
            Debug.LogWarning("2: The object " + go.name + " on the mushroom is not actually on the mushroom " + gameObject.name);
        }
    }

    public void PlayBounceSound()
    {
        GameController.instance.audioManager.PlayGenericSound(bounceSound, gameObject);
    }
}
