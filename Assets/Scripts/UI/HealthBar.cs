using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image[] healthPoints;
    public Text healthText;

    private void Start()
    {
        UpdateHealthBar(1.0f);
    }

    public void UpdateHealthBar(float healthNormalized)
    {
        //Debug.Log($"HealthBar::UpdateHealthBar( {healthNormalized} )");

        int reNormNumerator = (int)(healthNormalized * healthPoints.Length) - (healthPoints.Length % 2);
        int reNormDenominator = healthPoints.Length - (healthPoints.Length % 2);

        //Debug.Log($"Health Renormalized: {reNormNumerator} / {reNormDenominator}");

        float healthRenormalized = reNormNumerator / (float)reNormDenominator;

        //Debug.Log($"Health Renormalized: {healthRenormalized}");

        float r = healthRenormalized < 0.5f ? 1.0f : Mathf.Clamp01((1.0f - healthRenormalized) / 0.5f);
        float g = healthRenormalized < 0.5f ? Mathf.Clamp01(healthRenormalized / 0.5f) : 1.0f;
        float b = 0.25f;
        Color healthColor = new Color(r, g, b);

        //Debug.Log($"Health Points: {healthNormalized * healthPoints.Length}");
        //Debug.Log($"Health Color: {healthColor}");

        for (int i = 0; i < healthPoints.Length; i++)
        {
            healthPoints[i].color = i >= Mathf.RoundToInt(healthNormalized * healthPoints.Length) ? Color.black : healthColor;
        }

        healthText.text = $"{Mathf.RoundToInt(healthNormalized * healthPoints.Length)} HP ";
    }
}