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
        Debug.Log($"MechAIManager::ConductUnitTurn( {controlledMech} )");

        // Iterate through opposing units, find closest one(s)
        // Attack the nearest mech

        int nearestDistance = int.MaxValue;
        MechController targetMech = null;

        // For the moment, we are assuming that the controlled mech is always one of the computer player's
        foreach (MechController opposingMech in GameManager.Instance.MechFriendlies)
        {
            //Debug.Log($"MechAIManager :: Potential Target: {opposingMech}");

            if (GameManager.Instance.CheckForBlockingMech(controlledMech, opposingMech) != null)
            {
                continue;
            }

            int distance = HexGridManager.Instance.HexGrid.CubeDistance(controlledMech.GetCurrentHexTile(), opposingMech.GetCurrentHexTile());

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

        if (targetMech != null)
        {
            controlledMech.AttackTarget(targetMech);
        }
        else
        {
            GameManager.Instance.ConcludeCurrentTurn();
        }
    }
}