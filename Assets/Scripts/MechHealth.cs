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

    public int CurrentHealth { get; private set; }

    private float deathSmokeScalingElapsed;

    private void Awake()
    {
        CurrentHealth = initialHealth;
    }

    public void InflictDamage(int damagePoints)
    {
        //Debug.Log($"MechHealth::InflictDamage( {damagePoints} )");

        CurrentHealth -= damagePoints;
        healthBar.UpdateHealthBar(Mathf.Clamp01((float)CurrentHealth / (float)initialHealth));

        if (CurrentHealth <= 0.0f)
        {
            modelAnimator.SetBool("isDead", true);
            GameObject deathSmoke = GameObject.Instantiate(deathSmokePrefab, this.transform);
            deathSmoke.transform.position = modelAnimator.transform.position;
            deathSmoke.transform.rotation = modelAnimator.transform.rotation;
            deathSmoke.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleUpSmoke(deathSmoke));
        }
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