using UnityEngine;

public class MechLaser : MonoBehaviour
{
    public Transform laserEffectOrigin;
    public float mlaserEffectMaxLength;
    public GameObject laserEffectPrefab;

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
}