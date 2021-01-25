using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFramesAnimation : MonoBehaviour {

    public List<Sprite> frames;
    public int fps = 30;

    private new SpriteRenderer renderer;
    private Coroutine routine;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    private void OnEnable() {
        routine = StartCoroutine(Animation());
	}

    private void OnDisable()
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void Load(string location)
    {
        Sprite[] files = Resources.LoadAll<Sprite>(location);
        if (frames == null)
            frames = new List<Sprite>();
        else
            frames.Clear();
        foreach (Sprite file in files)
        {
            frames.Add(file);
        }
    }

    private IEnumerator Animation()
    {
        foreach(Sprite frame in frames)
        {
            renderer.sprite = frame;
            yield return new WaitForSeconds(1f / fps);
        }
        routine = null;
    }
}
