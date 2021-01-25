using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonControlBehaviour : MonoBehaviour {
    public Sprite keyboardSprite;
    public Sprite controllerSprite;

    private Image image;
    private bool isJoystick;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    // Use this for initialization
    void Start () {
        image.sprite = isJoystick ? controllerSprite : keyboardSprite;
    }
	
	// Update is called once per frame
	void Update () {
		if(isJoystick != GameController.instance.inputManager.IsJoystick)
        {
            isJoystick = GameController.instance.inputManager.IsJoystick;
            image.sprite = isJoystick ? controllerSprite : keyboardSprite;
        }
	}
}
