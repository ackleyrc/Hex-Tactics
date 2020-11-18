using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechController : MonoBehaviour
{
    public MechNameGUI nameGUI;
    public MechHealth health;
    public MechLaser laserWeapon;
    public MechModelOrientation modelOrientation;
    public Animator modelAnimator;
    public Transform firingTarget;
    public float walkingSpeed = 1.0f;
    public float runningSpeed = 2.0f;
    public float weaponFireDuration = 2.0f;

    private Queue<Cube> currentPath = new Queue<Cube>();
    private Cube latestHexTile;
    private float distanceTravelled = 0.0f;
    private bool isWalking = false;
    private bool isRunning = false;

    private MechController currentAttackTarget;
    private float weaponFireElapsed = 0.0f;
    private bool isFiringWeapon = false;

    private enum MechState { NONE, TRAVELLING, ATTACKING }
    private MechState currentState = MechState.NONE;

    public int UnitIndex { get; private set; }
    public string MechName { get; private set; }
    public Allegiance MechAllegiance { get; private set; }

    public event Action<MechController> OnMoveStarted = delegate { };
    public event Action<MechController> OnMoveStopped = delegate { };
    public event Action<MechController> OnAttackStarted = delegate { };
    public event Action<MechController> OnAttackStopped = delegate { };

    public void Initialize(Cube startHex, int unitIndex, Allegiance allegiance, string mechName)
    {
        latestHexTile = startHex;
        this.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(startHex);

        this.UnitIndex = unitIndex;
        this.MechName = mechName;
        this.MechAllegiance = allegiance;

        nameGUI.SetName(this.MechName, allegiance);
    }

    public Cube GetCurrentHexTile()
    {
        return latestHexTile;
    }

    public void TravelPath(List<Cube> path)
    {
        Debug.Log($"MechController::SetPath( {path.Count} )");

        currentPath = new Queue<Cube>();
        foreach (Cube cube in path)
        {
            currentPath.Enqueue(cube);
        }

        distanceTravelled = 0.0f;
        latestHexTile = currentPath.Dequeue();
        this.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(latestHexTile);

        Cube nextHexTile = currentPath.Peek();
        Vector3 nextHexTilePos = HexGridManager.Instance.GetHexCubeWorldPostion(nextHexTile);
        Vector3 prevHexTilePos = HexGridManager.Instance.GetHexCubeWorldPostion(latestHexTile);
        Vector3 directionToNextTile = nextHexTilePos - prevHexTilePos;

        //Debug.Log($"MechController :: prevHexTilePos {prevHexTilePos} nextHexTilePos {nextHexTilePos} directionToNextTile {directionToNextTile}");

        modelOrientation.SetLookTarget(directionToNextTile);

        currentState = MechState.TRAVELLING;

        OnMoveStarted?.Invoke(this);
    }

    public void AttackTarget(MechController mechTarget)
    {
        Debug.Log($"MechController::AttackTarget( {mechTarget} )");

        Vector3 targetHexTilePos = HexGridManager.Instance.GetHexCubeWorldPostion(mechTarget.GetCurrentHexTile());
        Vector3 currentHexTilePos = HexGridManager.Instance.GetHexCubeWorldPostion(latestHexTile);
        Vector3 directionToTarget = targetHexTilePos - currentHexTilePos;

        modelOrientation.SetLookTarget(directionToTarget);
        currentAttackTarget = mechTarget;
        isFiringWeapon = false;
        weaponFireElapsed = 0.0f;
        currentState = MechState.ATTACKING;

        OnAttackStarted?.Invoke(this);
    }

    private void Update()
    {
        switch (currentState)
        {
            case MechState.TRAVELLING:
                TravellingUpdate();
                break;
            case MechState.ATTACKING:
                AttackingUpdate();
                break;
        }
    }

    private void AttackingUpdate()
    {
        Vector3 targetHexTilePos = HexGridManager.Instance.GetHexCubeWorldPostion(currentAttackTarget.GetCurrentHexTile());
        Vector3 currentHexTilePos = HexGridManager.Instance.GetHexCubeWorldPostion(latestHexTile);
        Vector3 directionToTarget = targetHexTilePos - currentHexTilePos;

        if (isFiringWeapon == false)
        {
            // If mech is now facing toward target, fire weapon (otherwise wait)...
            if (modelOrientation.IsFacingDirection(directionToTarget) == true)
            {
                Debug.Log($"MechController :: Begin Firing Laser...");
                laserWeapon.FireLaser(currentAttackTarget.firingTarget, weaponFireDuration);
                modelAnimator.SetBool("isFiringLaser", true);
                isFiringWeapon = true;
            }
        }
        else // if (isFiringWeapon == true)
        {
            if (weaponFireElapsed >= weaponFireDuration)
            {
                Debug.Log($"MechController :: DONE Firing Laser");
                modelAnimator.SetBool("isFiringLaser", false);
                currentAttackTarget.health.InflictDamage(1);
                currentState = MechState.NONE;

                OnAttackStopped?.Invoke(this);
            }
            else
            {
                weaponFireElapsed += Time.deltaTime;
            }
        }
    }

    private void TravellingUpdate()
    {
        if (currentPath.Count > 0)
        {
            Cube nextHexTile = currentPath.Peek();
            Vector3 nextHexTilePos = HexGridManager.Instance.GetHexCubeWorldPostion(nextHexTile);
            Vector3 prevHexTilePos = HexGridManager.Instance.GetHexCubeWorldPostion(latestHexTile);
            Vector3 directionToNextTile = nextHexTilePos - prevHexTilePos;

            // If mech is already facing toward the next destination (otherwise wait)...
            if (modelOrientation.IsFacingDirection(directionToNextTile) == true)
            {
                Cube hexCubeUnderMech = HexGridManager.Instance.GetHexCubeForWorldPosition(transform.position);
                float movementCost = HexGridManager.Instance.GetTerrainMovementCost(hexCubeUnderMech);

                if (movementCost > 1.0f && isWalking == false)
                {
                    Debug.Log($"MechController :: Has Begun Walking...");

                    isWalking = true;
                    modelAnimator.SetBool("isWalking", true);

                    isRunning = false;
                    modelAnimator.SetBool("isRunning", false);
                }
                else if (movementCost == 1.0f && isRunning == false)
                {
                    isWalking = false;
                    modelAnimator.SetBool("isWalking", false);

                    isRunning = true;
                    modelAnimator.SetBool("isRunning", true);
                }

                distanceTravelled += (movementCost > 1.0f ? walkingSpeed : runningSpeed) * Time.deltaTime;
                Vector3 currentPathSegment = nextHexTilePos - prevHexTilePos;
                float currentPathSegmentLength = currentPathSegment.magnitude;

                // If there's more distance left to travel on the current path segment...
                if (distanceTravelled < currentPathSegmentLength)
                {
                    float currLerp = Mathf.Clamp01(distanceTravelled / currentPathSegmentLength);
                    this.transform.position = Vector3.Lerp(prevHexTilePos, nextHexTilePos, currLerp);
                }
                // Otherwise, we've crossed over into the next path segment...
                else
                {
                    distanceTravelled -= currentPathSegmentLength;
                    latestHexTile = currentPath.Dequeue();

                    // If we've reached the end of the full path...
                    if (currentPath.Count == 0)
                    {
                        this.transform.position = nextHexTilePos; // still assigned to position of last tile in path
                        modelAnimator.SetBool("isWalking", false);
                        isWalking = false;
                        modelAnimator.SetBool("isRunning", false);
                        isRunning = false;

                        Debug.Log($"MechController :: Final Path Destination Reached!");
                        currentState = MechState.NONE;

                        OnMoveStopped?.Invoke(this);
                    }
                    // Otherwise, there's more path left...
                    else
                    {
                        // Update previous and next hex tile variables
                        nextHexTile = currentPath.Peek();
                        nextHexTilePos = HexGridManager.Instance.GetHexCubeWorldPostion(nextHexTile);
                        prevHexTilePos = HexGridManager.Instance.GetHexCubeWorldPostion(latestHexTile);
                        directionToNextTile = nextHexTilePos - prevHexTilePos;

                        // If mech is already rotated to face the proper direction...
                        if (modelOrientation.IsFacingDirection(directionToNextTile) == true)
                        {
                            // Translate mech remainder of distance to avoid hitches between collinear tiles
                            currentPathSegment = nextHexTilePos - prevHexTilePos;
                            currentPathSegmentLength = currentPathSegment.magnitude;
                            float currLerp = Mathf.Clamp01(distanceTravelled / currentPathSegmentLength);
                            this.transform.position = Vector3.Lerp(prevHexTilePos, nextHexTilePos, currLerp);
                        }
                        // Otherwise, wait for mech to rotate toward next direction before resuming walk
                        else
                        {
                            modelOrientation.SetLookTarget(directionToNextTile);
                            modelAnimator.SetBool("isWalking", false);
                            isWalking = false;
                            modelAnimator.SetBool("isRunning", false);
                            isRunning = false;

                            Debug.Log($"MechController :: Wait to change direction...");
                        }
                    }
                }
            }
        }
    }
}