using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//[RequireComponent(typeof(SpriteRenderer))]
public class AnimateColor : Animate<Color> {

    public bool getChildren = false;

    protected new SpriteRenderer renderer;
    protected Image image;
    protected new Light light;
    protected Text text;
    protected TextMeshProUGUI textmesh;
    protected MeshRenderer meshRenderer;

    protected SpriteRenderer[] renderers;
    protected Image[] images;
    protected Light[] lights;
    protected Text[] texts;
    protected TextMeshProUGUI[] textmeshes;
    protected MeshRenderer[] meshRenderers;

    protected Color color
    {
        get
        {
            if (renderer != null)
                return renderer.color;
            if (meshRenderer != null)
                return meshRenderer.material.color;
            if (image != null)
                return image.color;
            if (text != null)
                return text.color;
            if (textmesh != null)
                return textmesh.color;
            if (light != null)
                return light.color;
            return Color.white;
        }
        set
        {
            if (getChildren)
            {
                foreach (SpriteRenderer comp in renderers)
                    comp.color = value;
                foreach (MeshRenderer comp in meshRenderers)
                    comp.material.color = value;
                foreach (Image comp in images)
                    comp.color = value;
                foreach (Text comp in texts)
                    comp.color = value;
                foreach (TextMeshProUGUI comp in textmeshes)
                    comp.color = value;
                foreach (Light comp in lights)
                    comp.color = value;
            }
            else
            {
                if (renderer != null)
                    renderer.color = value;
                else if (meshRenderer != null)
                    meshRenderer.material.color = value;
                else if (image != null)
                    image.color = value;
                else if (text != null)
                    text.color = value;
                else if (textmesh != null)
                    textmesh.color = value;
                else if (light != null)
                    light.color = value;
                else
                    Debug.LogError("Gameobject " + gameObject.name + " has no allowed component");
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        renderer = GetComponent<SpriteRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
        image = GetComponent<Image>();
        light = GetComponent<Light>();
        text = GetComponent<Text>();
        textmesh = GetComponent<TextMeshProUGUI>();

        renderers = GetComponentsInChildren<SpriteRenderer>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        images = GetComponentsInChildren<Image>();
        lights = GetComponentsInChildren<Light>();
        texts = GetComponentsInChildren<Text>();
        textmeshes = GetComponentsInChildren<TextMeshProUGUI>();
    }

    protected override void AnimateValues(Color _from, Color _to, float lerp)
    {
        //Color color;
        //color = renderer.color;
        color = Color.Lerp(_from, _to, lerp);
    }
    
}
