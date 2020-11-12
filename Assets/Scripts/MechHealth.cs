using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechHealth : MonoBehaviour
{
    public int initialHealth = 5;
    public HealthBar healthBar;

    public int CurrentHealth { get; private set; }

    private void Awake()
    {
        CurrentHealth = initialHealth;
    }

    public void InflictDamage(int damagePoints)
    {
        Debug.Log($"MechHealth::InflictDamage( {damagePoints} )");

        CurrentHealth -= damagePoints;
        healthBar.UpdateHealthBar(Mathf.Clamp01((float)CurrentHealth / (float)initialHealth));
    }
}