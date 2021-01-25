using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeedsCounterBehaviour : MonoBehaviour {

    public Image background;
    public TextMeshProUGUI seedsCounterText;
    public Image checkpointCreationSlider;
    public AnimateCollector labelAnimColl;

    public bool Opened
    {
        get { return seedCounterOpened; }
    }

    public bool Closing
    {
        get { return seedCounterClosing; }
    }

    public bool Disabled
    {
        set
        {
            disabled = value;
            background.material.SetFloat("_DesaturationFactor", disabled ? 1 : 0);
        }
    }

    private bool seedCounterOpened;
    private bool seedCounterClosing;
    private bool disabled;

    // Use this for initialization
    void Start () {
        seedCounterOpened = false;
        seedCounterClosing = false;
        background.gameObject.SetActive(false);
        labelAnimColl.AnimationReset("Apparition");
        Disabled = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SeedStart()
    {
        if (seedCounterOpened)
            return;
        background.gameObject.SetActive(true);
        Disabled = false;
        labelAnimColl.PlayForward("FadeIn");
    }

    public void SeedEnd()
    {
        labelAnimColl.PlayForward("FadeOut");
    }

    public void SeedCannot(bool blocked)
    {
        if (disabled != blocked)
            Disabled = blocked;
        if (seedCounterOpened)
            return;
        background.gameObject.SetActive(true);
        labelAnimColl.PlayForward(disabled? "ApparitionDisabled" : "Apparition");
    }

    public void SeedAdded()
    {
        if (seedCounterOpened)
            return;
        background.gameObject.SetActive(true);
        Disabled = false;
        labelAnimColl.PlayForward("Apparition");
    }

    public void SeedNumberValue(float value)
    {
        seedsCounterText.text = value.ToString();
    }

    public void CheckpointSliderValue(float value)
    {
        checkpointCreationSlider.fillAmount = value;
    }

    public void OnOpenSeedCounter()
    {
        seedCounterOpened = true;
    }

    public void OnCloseSeedCounter()
    {
        seedCounterOpened = false;
        seedCounterClosing = false;
    }

    public void OnSeedCounterClosing()
    {
        seedCounterClosing = true;
    }

    public void Disable()
    {
        background.gameObject.SetActive(false);
        labelAnimColl.AnimationReset("Apparition");
    }
}
