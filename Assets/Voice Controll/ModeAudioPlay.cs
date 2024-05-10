using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeAudioPlay : MonoBehaviour
{
    // Start is called before the first frame update

    public AudioSource idleSource;
    public AudioSource driveSource;
    public AudioSource armSource;
    public AudioSource emergencySource;
    public AudioSource screwSource;
    public AudioSource unScrewSource;


    public void PlayIdle()
    {
        idleSource.Play();
    }

    public void PlayDrive()
    {
        driveSource.Play();
    }

    public void PlayArm()
    {
        armSource.Play();
    }

    public void PlayEmergency()
    {
        emergencySource.Play();
    }

    public void PlayScrew()
    {
        screwSource.Play();
    }

    public void PlayUnScrew()
    {
        unScrewSource.Play();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
