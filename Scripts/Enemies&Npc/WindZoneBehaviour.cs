using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[System.Serializable]
public class ParticleData
{
    public float shapeWidth;
    public float lifeTime;
    public float speed;
    public float rateOverTime;
    public float startSize;
}

public class WindZoneBehaviour : MonoBehaviour {

    public float windForce = 5;
    public float baseWindForce = 50;
    public float maxWindForce = 100;
    public float onTime;
    public float offTime;

    public ParticleSystem vfxParent;
    public ParticleSystem vfxWind;
    public List<ParticleSystem> vfxSmoke;
    public float sizeUnitForSmoke = 2;
    public float widthToAffectSmokeSize = 3;

    public ParticleData windData;
    public ParticleData smokeData;

    public float maxDistanceFromPlayerToSound = 20;
    [EventRef] public string windSound;
    [EventRef] public string windSoundStop;
    public GameObject windZoneSoundEmitterTransform;

    private Collider col;
    private new Renderer renderer;
    private EventInstance soundInstance;
    private bool soundAllowed;
    private bool isActive = true;
    //private List<ParticleSystem> particles;
    //private float entranceVelocity;
    //private Vector3 direction;
    //private Rigidbody otherRB;

    private bool IsActive
    {
        set
        {
            isActive = value;
            if(isActive)
            {
                StartWindSound();
            }
            else
            {
                StopWindSound();
            }
        }
        get
        {
            return isActive;
        }
    }

    private void Awake()
    {
        col = GetComponent<Collider>();
        renderer = GetComponent<Renderer>();
        SetParticlesData();
        //particles = new List<ParticleSystem>();
        //particles[0] = vfxSmoke[0];
        //particles[1] = vfxWind;
        //particlesData = new ParticleData[particles.Count];
        //for(int i = 0; i < particles.Count; i++)
        //{
        //    particlesData[i] = new ParticleData();
        //    particlesData[i].shapeWidth = particles[i].shape.radius;
        //    particlesData[i].lifeTime = particles[i].main.startLifetime.constant;
        //    particlesData[i].speed = particles[i].main.startSpeed.constant;
        //}
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(Timer());
        AdjustTransform();
        AdjustParticle();
        //CreateWindSound();
        IsActive = true;
    }
	
	// Update is called once per frame
	void Update () {
        //if (IsActive)
        //    CheckPlayerDistance();

        if (col.enabled && windZoneSoundEmitterTransform != null)
        {
            Vector3 closestPoint = col.bounds.ClosestPoint(GameController.instance.player.transform.position);
            windZoneSoundEmitterTransform.transform.position = new Vector3(closestPoint.x, closestPoint.y, 0);
        }
        //ChangeSoundVolumeBasedOnSpeed();
        //RuntimeManager.AttachInstanceToGameObject(soundInstance, GetComponent<Transform>(), GetComponent<Rigidbody>());
        //if (otherRB != null)
        //{
        //    otherRB.position = otherRB.position+ direction * entranceVelocity * Time.deltaTime;
        //}
        //AdjustParticle();
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.tag == "Teleport")
    //    {
    //        otherRB = other.GetComponent<Rigidbody>();
    //        entranceVelocity = otherRB.velocity.magnitude;
    //        //Debug.Log(entranceVelocity);
    //        direction = otherRB.velocity.x > 0 ? transform.right : -transform.right;
    //        otherRB.velocity = Vector3.zero;
    //        otherRB.isKinematic = true;
    //        otherRB.useGravity = false;
    //        //float x = transform.up.x*windForce;
    //        //float y = windForce*transform.up.y;
            
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (otherRB != null && other.gameObject == otherRB.gameObject)
    //    {
    //        otherRB.isKinematic = false;
    //        otherRB.useGravity = true;
    //        otherRB.velocity = direction * entranceVelocity;
    //        otherRB = null;
    //        //float x = transform.up.x*windForce;
    //        //float y = windForce*transform.up.y;

    //    }
    //}

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Teleport" || other.gameObject.tag == "Player" || other.gameObject.tag =="OldMentor")
        {
            float x = transform.up.x * windForce;
            float y = windForce * transform.up.y;
            //Debug.Log(y);
            if (other.gameObject.tag == "Teleport" && !GameController.instance.player.teleportHat.IsHatarang)
            {
                Rigidbody otherRB = GameController.instance.player.teleportHat.Rigidbody;
                
                if (otherRB != null)
                {
                    otherRB.AddForce(new Vector3(x, y, 0), ForceMode.Acceleration);
                }
            }
            else if(other.gameObject.tag == "Player")
            {
                PlayerController otherPC = GameController.instance.player;
                if(otherPC != null)
                {
                    otherPC.Ascend(new Vector3(x, y, 0));
                }
            }
            else if (other.gameObject.tag == "OldMentor")
            {
                OldMentorBehaviour otherMB = other.GetComponent<OldMentorBehaviour>();
                if (otherMB != null)
                {
                    otherMB.Ascend(new Vector3(x, y, 0));
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            GameController.instance.player.IsInWind = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameController.instance.player.IsInWind = false;
        }
    }

    public void SetParticlesData()
    {
        windData.shapeWidth = vfxWind.shape.radius;
        windData.lifeTime = vfxWind.main.startLifetime.constant;
        windData.speed = vfxWind.main.startSpeed.constant;
        windData.rateOverTime = vfxWind.emission.rateOverTime.constant;
        smokeData.shapeWidth = vfxSmoke[0].shape.radius;
        smokeData.lifeTime = vfxSmoke[0].main.startLifetime.constant;
        smokeData.speed = vfxSmoke[0].main.startSpeed.constant;
        smokeData.startSize = vfxSmoke[0].main.startSizeX.constant;
    }

    public void AdjustParticle()
    {
        vfxParent.transform.localPosition = new Vector3(vfxParent.transform.localPosition.z, -transform.localScale.y / 2f, vfxParent.transform.localPosition.z);

        ParticleSystem.ShapeModule windShape = vfxWind.shape;
        ParticleSystem.MainModule windMain = vfxWind.main;
        ParticleSystem.EmissionModule windEmis = vfxWind.emission;
        float windDistance = windData.lifeTime * windData.speed;
        windDistance *= transform.localScale.y;
        windShape.radius = windData.shapeWidth * transform.localScale.x;
        windMain.startSpeed = windData.speed * windForce / baseWindForce;
        windMain.startLifetime = windDistance / windMain.startSpeed.constant;
        windEmis.rateOverTime = windData.rateOverTime * transform.localScale.x;

        int neededSmokes = (int)(transform.localScale.x / sizeUnitForSmoke);
        if (neededSmokes < 1)
            neededSmokes = 1;

        if (neededSmokes > vfxSmoke.Count)
        {
            for(int i = vfxSmoke.Count; i < neededSmokes; i++)
            {
                ParticleSystem newSmoke = Instantiate(vfxSmoke[0].gameObject, vfxParent.transform).GetComponent<ParticleSystem>();
                vfxSmoke.Add(newSmoke);
            }
        }
        else if (neededSmokes < vfxSmoke.Count)
        {
            for (int i = vfxSmoke.Count; i > neededSmokes; i--)
            {
                ParticleSystem part = vfxSmoke[vfxSmoke.Count - 1];
                vfxSmoke.Remove(part);
                Destroy(part.gameObject);
            }
        }

        for (int i = 0; i < vfxSmoke.Count; i++)
        {
            float step = 1f / (float)vfxSmoke.Count;
            //Debug.Log(step);
            float x = (vfxSmoke.Count % 2 == 0) ? (i+0.5f - (int)(vfxSmoke.Count / 2)) * step : (i - (int)(vfxSmoke.Count / 2)) * step;
            //Debug.Log(x);
            vfxSmoke[i].transform.position = transform.TransformPoint(new Vector3(x, 0, 0));
            Vector3 locPos = vfxSmoke[i].transform.localPosition;
            locPos.y = 0;
            locPos.z = 0;
            vfxSmoke[i].transform.localPosition = locPos;
            ParticleSystem.ShapeModule shape = vfxSmoke[i].shape;
            ParticleSystem.MainModule main = vfxSmoke[i].main;
            float distance = smokeData.lifeTime * smokeData.speed;
            distance *= transform.localScale.y;
            shape.radius = smokeData.shapeWidth;
            main.startSpeed = smokeData.speed * windForce / baseWindForce;
            main.startLifetime = distance / main.startSpeed.constant;
            if (transform.localScale.x < widthToAffectSmokeSize)
            {
                main.startSizeX = smokeData.startSize * transform.localScale.x / widthToAffectSmokeSize;
            }
        }
    }

    private void AdjustTransform()
    {
        if (transform.localPosition != Vector3.zero)
        {
            transform.parent.position = transform.position;
            transform.localPosition = Vector3.zero;
        }
        if (transform.localEulerAngles.z != 0)
        {
            transform.parent.Rotate(Vector3.forward, transform.localEulerAngles.z);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
        }
    }

    private void CheckPlayerDistance()
    {
        if (Mathf.Abs(GameController.instance.player.transform.position.x - transform.position.x) < maxDistanceFromPlayerToSound)
        {
            if (!soundAllowed)
            {
                StartWindSound();
                //soundAllowed = true;
            }
        }
        else
        {
            if (soundAllowed)
            {
                //soundAllowed = false;
                StopWindSound();
            }
        }
    }

    public void CreateWindSound()
    {
        //soundInstance = GameController.instance.audioManager.PlayLoopSound(windSound, gameObject, "AirFlowSpeed", Mathf.Clamp01((windForce-baseWindForce)/(maxWindForce-baseWindForce)));
        //soundAllowed = true;
    }

    public void StartWindSound()
    {
        //soundInstance.setPaused(false);
        soundInstance = GameController.instance.audioManager.PlayLoopSound(windSound, windZoneSoundEmitterTransform, "AirFlowSpeed", Mathf.Clamp01((windForce - baseWindForce) / (maxWindForce - baseWindForce)));
        //soundInstance = GameController.instance.audioManager.PlayLoopSound(windSound, windZoneCameraPlaneProjection, "AirFlowSpeed", Mathf.Clamp01((windForce - baseWindForce) / (maxWindForce - baseWindForce)));

        soundAllowed = true;
        //Debug.Log("suono");
    }

    public void StopWindSound()
    {
        //soundInstance.setPaused(true);
        //GameController.instance.audioManager.ChangeInstanceParameter(soundInstance, "Stop", 1);
        //soundInstance = GameController.instance.audioManager.PlayGenericSound(windSoundStop, gameObject);
        GameController.instance.audioManager.DestroyInstanceFaded(soundInstance);
        //soundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        soundAllowed = false;
        //soundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        //soundInstance.release();
        //Debug.Log("no suono");
    }

    public void ChangeSoundVolumeBasedOnDistance()
    {
        Vector3 playerPos = GameController.instance.player.transform.position;
        Vector3 myPos = transform.position;
        float distance = Vector3.Distance(playerPos, myPos);
        //Debug.Log(Mathf.Clamp01(distance / maxDistanceFromPlayerToSound));
        //float value;
        //Debug.Log(soundInstance.getParameterByName("AirFlowProximity", out value));
        //Debug.Log(value);
        GameController.instance.audioManager.ChangeInstanceParameter(soundInstance, "AirFlowProximity", Mathf.Clamp01(distance / maxDistanceFromPlayerToSound));
    }

    private IEnumerator Timer()
    {
        while(gameObject.activeSelf && onTime != 0 && offTime != 0)
        {
            if (onTime != 0)
            {
                col.enabled = true;
                renderer.enabled = true;
                vfxParent.Play();
                IsActive = true;
                yield return new WaitForSeconds(onTime);
            }
            if (offTime != 0)
            {
                col.enabled = false;
                renderer.enabled = false;
                vfxParent.Stop();
                IsActive = false;
                yield return new WaitForSeconds(offTime);
            }
        }
    }
}
