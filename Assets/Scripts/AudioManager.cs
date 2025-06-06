using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource woodHitWoodSource;
    public AudioSource pinHitGroundSource;
    public AudioSource stickHitGroundSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void PlayWoodHitWood()
    {
        woodHitWoodSource.Play();
    }
    public void PlayPinHitGround()
    {
        pinHitGroundSource.Play();
    }
    public void PlayStickHitGround()
    {
        stickHitGroundSource.Play();
    }
}
