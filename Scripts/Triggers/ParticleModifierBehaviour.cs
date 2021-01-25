using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleModifierBehaviour : MonoBehaviour {

    [Header("Numeric")]
    public bool changeStartLifetime = false;
    public bool changeStartSpeed = false;
    public bool changeGravityModifer = false;
    [Header("Activable")]
    public bool changeForceOverLifetime = false;
    public bool changeColorOverLifetime = false;
    public bool changeNoise = false;
    public bool changeRotationOverLifetime = false;
    public bool changeVelocityOverLifetime = false;

    [Header("Values")]
    public float startLifetime = 0;
    public float startSpeed = 0;
    public float gravityModifer = 1;
    public bool forceOverLifetime = false;
    public bool colorOverLifetime = false;
    public bool noise = false;
    public bool rotationOverLifetime = false;
    public bool velocityOverLifetime = false;

    private float _startLifetime = 0;
    private float _startSpeed = 0;
    private float _gravityModifer = 1;
    private bool _forceOverLifetime = false;
    private bool _colorOverLifetime = false;
    private bool _noise = false;
    private bool _rotationOverLifetime = false;
    private bool _velocityOverLifetime = false;

    private ParticleSystem particle;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
    }

    // Use this for initialization
    void Start()
    {
        ParticleSystem.MainModule main = particle.main;
        _startLifetime = main.startLifetime.constant;
        _startSpeed = main.startSpeed.constant;
        _gravityModifer = main.gravityModifier.constant;
        ParticleSystem.ForceOverLifetimeModule folm = particle.forceOverLifetime;
        _forceOverLifetime = folm.enabled;
        ParticleSystem.ColorOverLifetimeModule colm = particle.colorOverLifetime;
        _colorOverLifetime = colm.enabled;
        ParticleSystem.NoiseModule nm = particle.noise;
        _noise = nm.enabled;
        ParticleSystem.RotationOverLifetimeModule rolm = particle.rotationOverLifetime;
        _rotationOverLifetime = rolm.enabled;
        ParticleSystem.VelocityOverLifetimeModule volm = particle.velocityOverLifetime;
        _velocityOverLifetime = volm.enabled;
    }

    public void ResetToStart()
    {
        ParticleSystem.MainModule main = particle.main;
        main.startLifetime = _startLifetime;
        main.startSpeed = _startSpeed;
        main.gravityModifier = _gravityModifer;
        ParticleSystem.ForceOverLifetimeModule folm = particle.forceOverLifetime;
        folm.enabled = _forceOverLifetime;
        ParticleSystem.ColorOverLifetimeModule colm = particle.colorOverLifetime;
        colm.enabled = _colorOverLifetime;
        ParticleSystem.NoiseModule nm = particle.noise;
        nm.enabled = _noise;
        ParticleSystem.RotationOverLifetimeModule rolm = particle.rotationOverLifetime;
        rolm.enabled = _rotationOverLifetime;
        ParticleSystem.VelocityOverLifetimeModule volm = particle.velocityOverLifetime;
        volm.enabled = _velocityOverLifetime;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Modify()
    {
        ParticleSystem.MainModule main = particle.main;
        if (changeStartLifetime)
        {
            main.startLifetime = startLifetime;
        }
        if (changeStartSpeed)
        {
            main.startSpeed = startSpeed;
        }
        if (changeGravityModifer)
        {
            main.gravityModifier = gravityModifer;
        }
        if (changeForceOverLifetime)
        {
            ParticleSystem.ForceOverLifetimeModule folm = particle.forceOverLifetime;
            folm.enabled = forceOverLifetime;
        }
        if (changeColorOverLifetime)
        {
            ParticleSystem.ColorOverLifetimeModule colm = particle.colorOverLifetime;
            colm.enabled = colorOverLifetime;
        }
        if (changeNoise)
        {
            ParticleSystem.NoiseModule nm = particle.noise;
            nm.enabled = noise;
        }
        if (changeRotationOverLifetime)
        {
            ParticleSystem.RotationOverLifetimeModule rolm = particle.rotationOverLifetime;
            rolm.enabled = rotationOverLifetime;
        }
        if (changeVelocityOverLifetime)
        {
            ParticleSystem.VelocityOverLifetimeModule volm = particle.velocityOverLifetime;
            volm.enabled = velocityOverLifetime;
        }
    }
}
