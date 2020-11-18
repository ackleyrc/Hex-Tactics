using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechAIManager : MonoBehaviour
{
#region SINGLETON_MGMT
    private static MechAIManager _Instance;
    public static MechAIManager Instance { get { return _Instance; } }

    private void Awake()
    {
        _Instance = this;
    }

    private void OnDestroy()
    {
        if (_Instance == this)
        {
            _Instance = null;
        }
    }
#endregion SINGLETON_MGMT

    public void ConductUnitTurn(MechController controlledMech)
    {
        Debug.Log($"MechAIManager::ConductUnitTurn( {controlledMech.MechName} )");

        // Iterate through opposing units, find closest one(s)
        // Attack the nearest mech

        MechController targetMech = AcquireTarget(controlledMech.GetCurrentHexTile());

        if (targetMech != null)
        {
            controlledMech.AttackTarget(targetMech);
        }
        else // if (targetMech == null)
        {
            Debug.Log($"MechAIManager :: AI Could NOT acquire target at CURRENT POSITION");

            if (AttempReposition(controlledMech) == true)
            {
                controlledMech.OnMoveStopped += ContinueUnitTurn;
            }
            else
            {
                GameManager.Instance.ConcludeCurrentTurn();
            }
        }
    }

    private bool AttempReposition(MechController controlledMech)
    {
        Debug.Log($"MechAIManager::AttempReposition( {controlledMech.MechName} )");

        // Evaluate positions up to 4 tiles away
        for (int tileDistance = 1; tileDistance <= 4; tileDistance++)
        {
            List<Cube> potentialHexes = HexGridManager.Instance.HexGrid.GetRing(controlledMech.GetCurrentHexTile(), tileDistance);

            Debug.Log($"MechAIManager :: Evaluating {potentialHexes.Count} nearby positions at distance of {tileDistance} tile(s)...");

            foreach (Cube potentialHex in potentialHexes)
            {
                if (HexGridManager.Instance.IsHexCubeOnMap(potentialHex) == true &&
                    GameManager.Instance.GetFriendlyMechAt(potentialHex) == false &&
                    GameManager.Instance.GetEnemyMechAt(potentialHex) == false)
                {
                    MechController potentialTargetMech = AcquireTarget(fromHexTile: potentialHex);

                    if (potentialTargetMech != null)
                    {
                        List<Cube> path = HexGridManager.Instance.GetPath(controlledMech.GetCurrentHexTile(), potentialHex);

                        if (path != null && path.Count > 1)
                        {
                            controlledMech.TravelPath(path);
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private void ContinueUnitTurn(MechController controlledMech)
    {
        Debug.Log($"MechAIManager::ContinueUnitTurn( {controlledMech.MechName} )");

        controlledMech.OnMoveStopped -= ContinueUnitTurn;

        MechController targetMech = AcquireTarget(controlledMech.GetCurrentHexTile());

        if (targetMech != null)
        {
            controlledMech.AttackTarget(targetMech);
        }
        else // if (targetMech == null)
        {
            Debug.LogWarning($"MechAIManager :: AI Could NOT acquire target at NEW POSITION???");

            // Prior evaluation should have already determined that a target could be acquired here...
            //      If that is not the case, something has gone wrong, and we will simply conclude the turn here
            GameManager.Instance.ConcludeCurrentTurn();
        }
    }

    private MechController AcquireTarget(Cube fromHexTile)
    {
        int nearestDistance = int.MaxValue;
        MechController targetMech = null;

        // For the moment, we are assuming that the controlled mech is always one of the computer player's
        foreach (MechController opposingMech in GameManager.Instance.MechFriendlies)
        {
            //Debug.Log($"MechAIManager :: Potential Target: {opposingMech.MechName}");

            if (GameManager.Instance.CheckForLineOfSight(fromHexTile, opposingMech.GetCurrentHexTile()) != null)
            {
                continue;
            }

            if (opposingMech.health.CurrentHealth <= 0)
            {
                continue;
            }

            int distance = HexGridManager.Instance.HexGrid.CubeDistance(fromHexTile, opposingMech.GetCurrentHexTile());

            //Debug.Log($"MechAIManager :: Potential Target Distance: {distance} Current Nearest Distance: {nearestDistance}");

            if (distance < nearestDistance)
            {
                targetMech = opposingMech;
                nearestDistance = distance;

                //Debug.Log($"MechAIManager :: New Target (Closer): {distance}");
            }
            else if (distance == nearestDistance)
            {
                if (opposingMech.health.CurrentHealth < targetMech.health.CurrentHealth)
                {
                    targetMech = opposingMech;

                    //Debug.Log($"MechAIManager :: New Target (Lower Health): {distance}");
                }
            }
        }

        return targetMech;
    }
}