using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventProbability
{
    [Range(0,1)]
    public float probability;
    public UnityEvent eventToTrigger;
}

public class RandomTriggerBehaviour : MonoBehaviour {
    public float minWait;
    public float maxWait;
    public EventProbability[] events;
	// Use this for initialization
	void Start () {
		
	}

    private void OnEnable()
    {
        StartCoroutine(RandomLoop());
    }

    // Update is called once per frame
    void Update () {
		
	}

    private IEnumerator RandomLoop()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
            float maxProb = 0;
            foreach(EventProbability _event in events)
            {
                maxProb += _event.probability;
            }
            float random = Random.Range(0, maxProb);
            float cumulativeProb = 0;
            for (int i = 0; i < events.Length; i++)
            {
                if (random >= cumulativeProb && random < cumulativeProb + events[i].probability)
                {
                    events[i].eventToTrigger.Invoke();
                }
                cumulativeProb += events[i].probability;
            }
        }
    }
}
