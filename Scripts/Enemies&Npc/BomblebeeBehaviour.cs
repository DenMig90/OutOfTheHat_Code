using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BomblebeeState
{
    Flying,
    Aiming,
    Charging
}

public class BomblebeeBehaviour : PooledObject {

    public float speed;
    public float chargeSpeed;
    public float rotationSpeed;
    public float distanceToFollowPlayer;
    public float distanceToCharge;
    public float chargeDelay;
    public float delayToExplosion;
    public float explosionRange;
    public ExplosionBehaviour explosionPrefab;
    public LayerMask playerLayer;
    public LayerMask damageableLayer;

    private Vector3 customForward;
    private Rigidbody rb;
    private BomblebeeState status;
    private Vector3 lastSeenPlayerPos;
    private Coroutine chargeRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void Start () {
        if (distanceToCharge > distanceToFollowPlayer)
            distanceToCharge = distanceToFollowPlayer;
	}

    private void OnEnable()
    {
        if (delayToExplosion != 0)
        {
            Invoke("Explode", delayToExplosion);
        }
        customForward = transform.forward;
        transform.rotation = Quaternion.LookRotation(customForward, Vector3.forward);
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        status = BomblebeeState.Flying;
    }

    private void OnDisable()
    {
        GameController.instance.RemoveOnDeathDelegate(ResetToStart);
        GameController.instance.RemoveOnNewGameDelegate(ResetToStart);
        if (chargeRoutine != null)
            StopCoroutine(chargeRoutine);
        CancelInvoke("Explode");
        //GameController.instance.RemoveBomblebee(this);
    }

    // Update is called once per frame
    void Update () {
        float distance = Vector3.Distance(GameController.instance.player.transform.position, transform.position);
        Vector3 playerDir = GameController.instance.player.transform.position - transform.position;
        bool canSee = Physics.Raycast(transform.position, playerDir, distanceToFollowPlayer, playerLayer, QueryTriggerInteraction.Ignore) && !Physics.Raycast(transform.position, playerDir, distance, ~playerLayer, QueryTriggerInteraction.Ignore);
        if (/*distance < distanceToFollowPlayer &&*/ canSee && !GameController.instance.player.IsHidden)
        {
            lastSeenPlayerPos = GameController.instance.player.transform.position;
            if(status == BomblebeeState.Flying && distance < distanceToCharge)
            {
                status = BomblebeeState.Aiming;
                chargeRoutine = StartCoroutine(ChargeDelayed());
            }
        }

        if(status == BomblebeeState.Aiming || (status == BomblebeeState.Flying && canSee))
        {
            Vector3 dir = lastSeenPlayerPos - transform.position;
            float step = rotationSpeed * Time.deltaTime;

            Vector3 newDir = Vector3.RotateTowards(customForward, dir, step, 0.0f);
            //Debug.DrawRay(transform.position, newDir, Color.red);
            customForward = newDir;
            customForward.z = 0;
            transform.rotation = Quaternion.LookRotation(customForward, Vector3.forward);
        }
        float actualSpeed = 0;
        if (status == BomblebeeState.Charging)
            actualSpeed = chargeSpeed;
        else if (status == BomblebeeState.Flying)
            actualSpeed = speed;
        transform.Translate(customForward * actualSpeed * Time.deltaTime, Space.World);
	}

    public void ResetToStart()
    {
        ReturnToPool();
    }

    public void Rotate(Quaternion rotation)
    {
        transform.rotation = rotation;
        customForward = transform.forward;
    }

    public void Explode()
    {
        Collider[] toDamage = Physics.OverlapSphere(transform.position, explosionRange, damageableLayer);
        foreach (Collider col in toDamage)
        {
            //Vector3 colDir = col.gameObject.transform.position - transform.position;
            //float distance = Vector3.Distance(col.gameObject.transform.position, transform.position);
            //if (!Physics.Raycast(transform.position, colDir, distance, ~damageableLayer, QueryTriggerInteraction.Ignore))
            //{
                if (col.gameObject.tag == "Player")
                    GameController.instance.player.StartKill();
                if (col.gameObject.tag == "Destroyable")
                {
                    DestroyableBehaviour obj = col.gameObject.GetComponent<DestroyableBehaviour>();
                    obj.Damage(1);
                }
            if (col.gameObject.tag == "Eater")
            {
                EaterTrapBehaviour obj = col.gameObject.GetComponent<EaterTrapBehaviour>();
                obj.Kill();
            }
            //}
        }
        ExplosionBehaviour spawn = explosionPrefab.GetPooledInstance<ExplosionBehaviour>();
        spawn.transform.position = transform.position;
        spawn.animScale.to = new Vector3(explosionRange * 2f, explosionRange * 2f, explosionRange * 2f);
        CancelInvoke("Explode");
        ReturnToPool();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Teleport")
        {
            return;
        }
        Explode();
    }

    private IEnumerator ChargeDelayed()
    {
        float time = 0;
        while (time < chargeDelay)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        status = BomblebeeState.Charging;
        chargeRoutine = null;
    }
}
