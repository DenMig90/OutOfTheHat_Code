using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunflowerBehaviour : MonoBehaviour
{
    public BomblebeeBehaviour bomblebeePrefab;
    [Tooltip("Direction is Blue arrow")]
    public Transform spawnPoint;
    public bool playAtStart = false;
    public float shootDelay;
    public int magazine;
    public float loadTime;

    private Coroutine shootRoutine;

    // Use this for initialization
    void Start()
    {
        //GameController.instance.AddGunflower(this);
        GameController.instance.AddOnDeathDelegate(ResetToStart);
        GameController.instance.AddOnNewGameDelegate(ResetToStart);
        if (playAtStart)
            shootRoutine=StartCoroutine(ShootRoutine());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetToStart()
    {
        if (shootRoutine != null)
        {
            StopCoroutine(shootRoutine);
            shootRoutine = null;
        }
        if (playAtStart)
            shootRoutine = StartCoroutine(ShootRoutine());
    }

    public void StartShooting()
    {
        if (shootRoutine == null)
            shootRoutine = StartCoroutine(ShootRoutine());
    }

    public void StopShooting()
    {
        if (shootRoutine != null)
            StopCoroutine(shootRoutine);
        shootRoutine = null;
    }

    private void Shoot()
    {
        BomblebeeBehaviour spawn = bomblebeePrefab.GetPooledInstance<BomblebeeBehaviour>();
        spawn.transform.position = spawnPoint.position;
        spawn.Rotate(spawnPoint.transform.rotation);
    }

    private IEnumerator ShootRoutine()
    {
        float time;
        int actualMagazine = magazine;
        float waitTime;
        while (true)
        {
            Shoot();
            actualMagazine--;
            if (actualMagazine > 0)
            {
                waitTime = shootDelay; 
            }
            else
            {
                waitTime = loadTime;
                actualMagazine = magazine;
            }
            time = 0;
            while (time <= waitTime)
            {
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
            }
        }
    }
}
