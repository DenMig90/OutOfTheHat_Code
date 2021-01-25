using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

//[System.Serializable]
//public class UnityEventFloat : UnityEvent<float> { }

public class AnimateColorDistance : MonoBehaviour {

    public Color nearColor;
    public Color farColor;
    public float maxDistance;
    [Tooltip("Near - Far")]
    public AnimationCurve curve;
    public Transform target;
    public UnityEvent onEnter;
    public UnityEvent onExit;
    public UnityEventFloat onStay;

    [Header("Additive")]
    public bool additiveEnabled;
    public float modifier;

    protected new SpriteRenderer renderer;
    protected Image image;
    protected new Light light;
    protected Text text;
    protected bool isInside;

    protected Color color
    {
        get
        {
            if (renderer != null)
                return renderer.color;
            if (image != null)
                return image.color;
            if (text != null)
                return text.color;
            if (light != null)
                return light.color;
            return Color.white;
        }
        set
        {
            if (renderer != null)
                renderer.color = value;
            else if (image != null)
                image.color = value;
            else if (text != null)
                text.color = value;
            else if (light != null)
                light.color = value;
            else
                Debug.LogError("Gameobject " + gameObject.name + " has no allowed component");
        }
    }

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        light = GetComponent<Light>();
        text = GetComponent<Text>();
    }

    // Use this for initialization
    void Start () {
        if(additiveEnabled)
        {
            nearColor = color;
            farColor = new Color(nearColor.r + (modifier / 255), nearColor.g + (modifier / 255), nearColor.b + (modifier / 255), nearColor.a);
        }
        isInside = false;
        if (target == null)
            target = GameController.instance.player.transform;
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 myPos = transform.position;
        Vector3 targetPos = target.transform.position;
        myPos.z = 0;
        targetPos.z = 0;
        float distance = Vector3.Distance(myPos, targetPos);
        color = Color.Lerp(nearColor, farColor,  curve.Evaluate(distance / maxDistance));
        if(distance > maxDistance)
        {
            if (isInside)
            {
                onExit.Invoke();
            }
            isInside = false;
        }
        else if(distance <= maxDistance)
        {
            if (!isInside)
            {
                onEnter.Invoke();
            }
            onStay.Invoke(distance / maxDistance);
            isInside = true;
        }
	}
}
