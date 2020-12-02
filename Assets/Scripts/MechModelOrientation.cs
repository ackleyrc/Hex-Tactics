using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechModelOrientation : MonoBehaviour
{
    public Transform mechModelTransform;
    public Animator modelAnimator;
    public Vector3 skewRotation;
    public float turnRadiansPerSecond;

    private Vector3 targetLookDirection;
    private Vector3 currentLookDirection;
    private bool isChangingLookDirection = false;

    private readonly Vector3 UP_BG_BASIS = Vector3.back;

    private void Awake()
    {
        currentLookDirection = Vector3.up;
        targetLookDirection = currentLookDirection;
        mechModelTransform.rotation = Quaternion.LookRotation(Quaternion.Euler(skewRotation) * currentLookDirection, Quaternion.Euler(skewRotation) * UP_BG_BASIS);
    }

    public void SetLookTarget(Vector3 relativeDirection)
    {
        //Debug.Log($"MechModelOrientation::SetLookTarget( {relativeDirection} )");

        if (Mathf.Approximately(relativeDirection.x, 0.0f) && Mathf.Approximately(relativeDirection.y, 0.0f))
        {
            Debug.LogWarning("MechModelOrientation :: Relative Direction vector must have absolute XY magnitude greater than Zero");
            return;
        }

        Vector3 targetZZeroed = new Vector3(relativeDirection.x, relativeDirection.y, 0.0f);
        targetLookDirection = targetZZeroed;

        //Debug.Log($"MechModelOrientation :: targetLookDirection: {targetLookDirection}");
    }

    public bool IsFacingDirection(Vector3 targetDirection)
    {
        //Debug.Log($"MechModelOrientation :: Dot(current, target) = {Vector3.Dot(currentLookDirection.normalized, targetDirection.normalized)}");
        return Mathf.Abs(Vector3.Dot(currentLookDirection.normalized, targetDirection.normalized) - 1.0f) < 0.001f;
    }

    private void Update()
    {
        if (IsFacingDirection(targetLookDirection) == false)
        {
            if (isChangingLookDirection == false)
            {
                //Debug.Log($"MechModelOrientation :: Beginning to rotate...");
                isChangingLookDirection = true;
                modelAnimator.SetBool("isTurning", true);
                modelAnimator.speed = 1.3f;
            }

            // Rotations to the exact opposite directions produce strange results, so we'll add a slight bias to resolve this...
            if (Mathf.Abs(Vector3.Dot(currentLookDirection.normalized, targetLookDirection.normalized) + 1.0f) < 0.001f)
            {
                //Debug.Log($"Rotating 180 degrees...");
                currentLookDirection = Vector3.RotateTowards(currentLookDirection, targetLookDirection, turnRadiansPerSecond * Time.deltaTime, float.MaxValue);
                mechModelTransform.rotation = Quaternion.LookRotation(Quaternion.Euler(skewRotation) * currentLookDirection, Quaternion.Euler(skewRotation) * UP_BG_BASIS);
            }
            else
            {
                currentLookDirection = Vector3.RotateTowards(currentLookDirection, targetLookDirection, turnRadiansPerSecond * Time.deltaTime, float.MaxValue);
                mechModelTransform.rotation = Quaternion.LookRotation(Quaternion.Euler(skewRotation) * currentLookDirection, Quaternion.Euler(skewRotation) * UP_BG_BASIS);
            }
        }
        else
        {
            if (isChangingLookDirection == true)
            {
                //Debug.Log($"MechModelOrientation :: Done Rotating!");
                isChangingLookDirection = false;
                modelAnimator.SetBool("isTurning", false);
            }
        }
    }
}