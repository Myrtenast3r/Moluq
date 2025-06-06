using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Player Related")]
    public AudioSource throwStickSource;
    public AudioSource chargePowerSource;
    private bool isChargingPower = false;

    [Header("Object Related")]
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

    #region Player Related Play Functions
    public void PlayThrowStick()
    {
        throwStickSource.Play();
    }
    public void PlayChargePower()
    {
        if (!isChargingPower)
        {
            chargePowerSource.Play();
            isChargingPower = true;
        }

        float pitchIncreaseRate = 1f;
        float maxPitch = 3.0f;

        chargePowerSource.pitch = Mathf.Clamp(chargePowerSource.pitch + pitchIncreaseRate * Time.deltaTime, 1.0f, maxPitch);
    }

    public void StopChargePower() 
    {
        isChargingPower = false; 
        chargePowerSource.Stop();
        chargePowerSource.pitch = 1.0f;
    }

    #endregion

    #region Object Related Play Functions
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
    #endregion
}
