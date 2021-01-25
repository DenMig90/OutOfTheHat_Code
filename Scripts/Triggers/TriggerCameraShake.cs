using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCameraShake : MonoBehaviour
{

    public float duration = 0f;
    public AnimationCurve xPosShakeCurve;
    public float xPosShakeAmount;
    public AnimationCurve yPosShakeCurve;
    public float yPosShakeAmount;
    public AnimationCurve rotShakeCurve;
    public float rotShakeAmount;
    public bool noMaintainPos = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartShake()
    {
        if(!noMaintainPos)
            GameController.instance.mainCamera.StartShaking(duration, rotShakeCurve, xPosShakeCurve, yPosShakeCurve, rotShakeAmount, xPosShakeAmount, yPosShakeAmount);
        else
            GameController.instance.mainCamera.StartShakingNoMaintainPos(duration, rotShakeCurve, xPosShakeCurve, yPosShakeCurve, rotShakeAmount, xPosShakeAmount, yPosShakeAmount);

    }
}
