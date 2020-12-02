using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechLaser : MonoBehaviour
{
    public Transform laserEffectOrigin;
    public float laserEffectMaxLength;
    public GameObject laserEffectPrefab;

    public AudioSource laserFireSoundEffect;
    public float laserFireMaxVolume;
    public float laserFireDecayDuration;
    public AudioSource laserSizzleSoundEffect;
    public float laserSizzleMaxVolume;
    public float laserSizzleDecayDuration;

    private GameObject laserEffectInstance;
    private Hovl_Laser LaserScript;
    private Hovl_Laser2 LaserScript2;

    private float laserFireDuration;
    private float laserFireElapsed = 0.0f;
    private bool isFiringLaser = false;

    private Transform currentTarget;

    public void FireLaser(Transform target, float duration)
    {
        currentTarget = target;
        Destroy(laserEffectInstance);
        Quaternion laserRotation = Quaternion.LookRotation(currentTarget.position - laserEffectOrigin.position, laserEffectOrigin.up);
        laserEffectInstance = Instantiate(laserEffectPrefab, laserEffectOrigin.position, laserEffectOrigin.rotation);
        laserEffectInstance.transform.parent = laserEffectOrigin;
        LaserScript = laserEffectInstance.GetComponent<Hovl_Laser>();
        LaserScript2 = laserEffectInstance.GetComponent<Hovl_Laser2>();

        laserFireElapsed = 0.0f;
        laserFireDuration = duration;

        isFiringLaser = true;

        StopAllCoroutines();
        StartCoroutine(StartLaserSFX());
    }

    private void Update()
    {
        if (isFiringLaser == true)
        {
            if (laserFireElapsed >= laserFireDuration)
            {
                if (LaserScript) LaserScript.DisablePrepare();
                if (LaserScript2) LaserScript2.DisablePrepare();
                Destroy(laserEffectInstance, 1.0f);

                isFiringLaser = false;
            }
            else
            {
                Quaternion laserRotation = Quaternion.LookRotation(currentTarget.position - laserEffectOrigin.position, laserEffectOrigin.up);
                laserEffectInstance.transform.rotation = laserRotation;
                laserFireElapsed += Time.deltaTime;
            }
        }
    }

    private IEnumerator StartLaserSFX()
    {
        laserFireSoundEffect.transform.position = laserEffectOrigin.position;
        laserSizzleSoundEffect.transform.position = currentTarget.position;

        laserFireSoundEffect.volume = laserFireMaxVolume;
        laserFireSoundEffect.Stop();
        laserFireSoundEffect.Play();

        laserSizzleSoundEffect.volume = laserSizzleMaxVolume;
        laserSizzleSoundEffect.Stop();
        laserSizzleSoundEffect.Play();

        while (isFiringLaser == true)
        {
            yield return null;
        }

        float laserStoppedTime = Time.time;

        while (Time.time - laserStoppedTime < laserFireDecayDuration ||
               Time.time - laserStoppedTime < laserSizzleDecayDuration)
        {
            float laserFireDecayNormalized = Mathf.Clamp01((Time.time - laserStoppedTime) / laserFireDecayDuration);
            float laserSizzleDecayNormalized = Mathf.Clamp01((Time.time - laserStoppedTime) / laserSizzleDecayDuration);

            laserFireSoundEffect.volume = Mathf.Lerp(laserFireMaxVolume, 0.0f, laserFireDecayNormalized);
            laserSizzleSoundEffect.volume = Mathf.Lerp(laserSizzleMaxVolume, 0.0f, laserSizzleDecayNormalized);

            yield return null;
        }
    }
}