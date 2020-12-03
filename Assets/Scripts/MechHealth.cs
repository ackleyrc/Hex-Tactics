using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechHealth : MonoBehaviour
{
    public int initialHealth = 5;
    public HealthBar healthBar;
    public Animator modelAnimator;
    public GameObject deathSmokePrefab;
    public Vector3 deathSmokeMaxScale;
    public float deathSmokeScalingDuration;

    public AudioData audioData;
    public AudioSource deathCollapseSoundEffect;
    public float deathCollapseMaxVolume;
    public float deathCollapseDecayStart;
    public float deathCollapseDecayDuration;
    public AudioSource deathCrashSoundEffect;

    public int CurrentHealth { get; private set; }

    private float deathSmokeScalingElapsed;

    private void Awake()
    {
        CurrentHealth = initialHealth;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void InflictDamage(int damagePoints)
    {
        //Debug.Log($"MechHealth::InflictDamage( {damagePoints} )");

        CurrentHealth -= damagePoints;
        healthBar.UpdateHealthBar(Mathf.Clamp01((float)CurrentHealth / (float)initialHealth));

        if (CurrentHealth <= 0.0f)
        {
            modelAnimator.speed = 1.0f;
            modelAnimator.SetBool("isDead", true);
            GameObject deathSmoke = GameObject.Instantiate(deathSmokePrefab, this.transform);
            deathSmoke.transform.position = modelAnimator.transform.position;
            deathSmoke.transform.rotation = modelAnimator.transform.rotation;
            deathSmoke.transform.localScale = Vector3.zero;
            StartCoroutine(HandleDeathCollapseSoundEffect());
            StartCoroutine(ScaleUpSmoke(deathSmoke));
        }
    }

    private IEnumerator HandleDeathCollapseSoundEffect()
    {
        Debug.Log($"MechHealth::HandleDeathCollapseSoundEffect()");

        deathCollapseSoundEffect.Stop();
        deathCollapseSoundEffect.clip = audioData.GetRandomDeathCollapse();
        deathCollapseSoundEffect.Play();

        yield return new WaitForSeconds(deathCollapseDecayStart);

        Clip deathCrash = audioData.GetRandomDeathCrash();
        deathCrashSoundEffect.clip = deathCrash.AudioClip;
        deathCrashSoundEffect.time = deathCrash.StartTime;
        deathCrashSoundEffect.volume = deathCrash.MaxVolume;
        deathCrashSoundEffect.pitch = deathCrash.Pitch;
        deathCrashSoundEffect.Play();

        Debug.Log($"MechHealth :: Start Death Collapse SFX Decay...");

        float decayElapsed = 0;

        while (decayElapsed < deathCollapseDecayDuration)
        {
            decayElapsed += Time.deltaTime;
            float decayNormalized = Mathf.Clamp01(decayElapsed / deathCollapseDecayDuration);
            deathCollapseSoundEffect.volume = Mathf.Lerp(deathCollapseMaxVolume, 0.0f, decayNormalized);
            yield return null;
        }

        deathCollapseSoundEffect.Stop();

        Debug.Log($"MechHealth :: Stop Death Collapse SFX");
    }

    private IEnumerator ScaleUpSmoke(GameObject deathSmoke)
    {
        while (deathSmokeScalingElapsed < deathSmokeScalingDuration)
        {
            deathSmokeScalingElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(deathSmokeScalingElapsed / deathSmokeScalingDuration);
            deathSmoke.transform.localScale = Vector3.Lerp(Vector3.zero, deathSmokeMaxScale, t);
            yield return null;
        }
    }
}