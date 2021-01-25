using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class WaterfallBehaviour : MonoBehaviour {
    public float maxDistance = 10f;
    public Transform emitter;
    [EventRef] public string waterfallSound;

    private EventInstance soundInstance;
    private Collider col;
    private float parameter = 0;
    private float distanceToCheck = 100;
	// Use this for initialization
	void Awake () {
        parameter = 0;
        if (emitter == null)
        {
            emitter = new GameObject("Emitter").transform;
            emitter.parent = transform;
        }
        col = GetComponent<Collider>();
	}

    private void Start()
    {
        soundInstance = GameController.instance.audioManager.PlayGenericSound(waterfallSound, emitter.gameObject, "WaterfallProximity", parameter);
    }

    // Update is called once per frame
    void Update () {
        if (Mathf.Abs(transform.position.x - GameController.instance.player.transform.position.x) < distanceToCheck)
        {
            Vector3 closestPoint = col.bounds.ClosestPoint(GameController.instance.player.transform.position);
            emitter.position = new Vector3(closestPoint.x, closestPoint.y, GameController.instance.player.transform.position.z);
        }
        if (Vector3.Distance(emitter.position, GameController.instance.player.transform.position) <= maxDistance)
        {
            parameter = 1f - Vector3.Distance(emitter.position, GameController.instance.player.transform.position) / maxDistance;
            GameController.instance.audioManager.ChangeInstanceParameter(soundInstance, "WaterfallProximity", parameter);
        }
        else if(parameter != 0)
        {
            parameter = 0;
            GameController.instance.audioManager.ChangeInstanceParameter(soundInstance, "WaterfallProximity", parameter);
        }
	}
}
