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

        ConductUnitTurn_Version1(controlledMech);
    }

#region VERSION_1
    private void ConductUnitTurn_Version1(MechController controlledMech)
    {
        // TODO: Implement basic utility function evaluation
        //      For the moment, continue doing version 0
        ConductUnitTurn_Version0(controlledMech);

        Cube currenMechHex = controlledMech.GetCurrentHexTile();

        // We'll assume at first that the current mech's position is optimal until we begin evaluating utility scores of nearby hexes
        Cube bestReachableHexCube = currenMechHex;
        float bestReachableHexUtility = 0.0f;

        // Each key is a hex within the mech's movement range, each value the utility score for that hex
        // Each utility score is derived from various factors under consideration,
        //  including: distance to nearest cover, exposure to opponents, target opportunities

        Dictionary<Cube, float> hexUtilityScores = new Dictionary<Cube, float>();
        foreach (Cube reachableCube in HexGridManager.Instance.GetReachableHexes(currenMechHex, controlledMech.weightedDistanceRange))
        {
            hexUtilityScores.Add(reachableCube, 0.0f);
        }

        // Evaluate Defensive Proximity Utility scores
        float relevantRange = controlledMech.weightedDistanceRange * 2.0f;
        List<Cube> relevantDefensiveHexes = new List<Cube>();
        foreach (Cube defensiveHex in HexGridManager.Instance.GetDefensiveHexTiles())
        {
            if (HexGridManager.Instance.HexGrid.CubeDistance(currenMechHex, defensiveHex) < relevantRange)
            {
                relevantDefensiveHexes.Add(defensiveHex);
            }
        }

        Dictionary<Cube, float> distanceToNearestCover = HexGridManager.Instance.GetDistanceMap(currenMechHex, relevantDefensiveHexes, relevantRange);

        foreach (Cube cube in new List<Cube>(hexUtilityScores.Keys))
        {
            if (distanceToNearestCover.ContainsKey(cube) == false)
            {
                //Debug.Log($"MechAIManager :: Reachable Hex {cube} NOT in Distance Map: {distanceToNearestCover[cube]}");
                hexUtilityScores[cube] = 0.0f;
            }
            else
            {
                hexUtilityScores[cube] = Mathf.Clamp01(1.0f - (distanceToNearestCover[cube] / relevantRange));
                //Debug.Log($"MechAIManager :: Hex {cube} Defensive Proximity Distance: {distanceToNearestCover[cube]}");
                Debug.Log($"MechAIManager :: Hex {cube} Defensive Proximity Utility: {hexUtilityScores[cube]}");
            }
        }

        // Evaluate Exposure & Target Opportunity Utility scores
        float normalizedHealth = Mathf.Clamp01(controlledMech.health.CurrentHealth / (float)controlledMech.health.initialHealth);
        foreach (Cube cube in new List<Cube>(hexUtilityScores.Keys))
        {
            int numOpponentsExposedTo = 0;
            float sumTargetOpportunityUtility = 0.0f;

            // For the moment, we are assuming that the controlled mech is always one of the computer player's
            foreach (MechController opposingMech in GameManager.Instance.MechFriendlies)
            {
                if (opposingMech.health.CurrentHealth <= 0)
                {
                    continue;
                }

                if (GameManager.Instance.CheckForLineOfSight(fromHexTile: cube, opposingMech.GetCurrentHexTile()) != null)
                {
                    continue;
                }

                numOpponentsExposedTo++;

                float distanceToOpponent = HexGridManager.Instance.HexGrid.CubeDistance(cube, opposingMech.GetCurrentHexTile());
                float proximityUtility = 1.0f - Mathf.Clamp01(distanceToOpponent / (controlledMech.weightedDistanceRange * 2.0f)); // TODO: Set denominator to attack range
                float defenseMultipler = HexGridManager.Instance.GetDefenseMultiplier(opposingMech.GetCurrentHexTile());
                int tentativeDamage = Mathf.RoundToInt(2.0f * defenseMultipler); // For now, this should be 2 or 1
                float vulnerabilityUtility = 1.0f - ((opposingMech.health.CurrentHealth - tentativeDamage) / (float)opposingMech.health.initialHealth);
                sumTargetOpportunityUtility += (proximityUtility * vulnerabilityUtility);
            }
            
            // Note: including exposureUtility is way too evasive at the moment...
            // TODO: Test weighting exposureUtility by inverse proportion to controlled mech health
            //          and target opportunity utility in direct proportion to controlled mech health?

            //float exposureUtility = Mathf.Clamp01((numOpponentsExposedTo == 0 ? 1.0f : 0.5f / numOpponentsExposedTo) * normalizedHealth);
            //hexUtilityScores[cube] = (1.0f * hexUtilityScores[cube] + exposureUtility) / 2.0f;
            //Debug.Log($"MechAIManager :: Hex {cube} Opponents Exposed To: {numOpponentsExposedTo}");
            //Debug.Log($"MechAIManager :: Hex {cube} Exposure Utility: {exposureUtility}");

            float targetOpportunityUtility = Mathf.Clamp01(numOpponentsExposedTo == 0 ? 0.0f : sumTargetOpportunityUtility / numOpponentsExposedTo);
            hexUtilityScores[cube] = (1.0f * hexUtilityScores[cube] + targetOpportunityUtility) / 2.0f;
            Debug.Log($"MechAIManager :: Hex {cube} Sum Target Opportunity Utility: {sumTargetOpportunityUtility}");
            Debug.Log($"MechAIManager :: Hex {cube} Target Opportunity Utility: {targetOpportunityUtility}");

            Debug.Log($"MechAIManager :: Hex {cube} Overall Utility: {hexUtilityScores[cube]}");

            if (hexUtilityScores[cube] > bestReachableHexUtility)
            {
                bestReachableHexCube = cube;
                bestReachableHexUtility = hexUtilityScores[cube];
                Debug.Log($"MechAIManager :: NEW BEST Hex {cube}");
            }
        }

        // Acquiring a target after (optionally) moving should use the same/similar target opportunity evaluation
    }
    #endregion VERSION_1

    #region VERSION_0
    private void ConductUnitTurn_Version0(MechController controlledMech)
    {
        // Iterate through opposing units, find closest one(s)
        // Attack the nearest mech. Break tie with lowest health

        MechController targetMech = AcquireTarget_v0(controlledMech.GetCurrentHexTile());

        if (targetMech != null)
        {
            controlledMech.AttackTarget(targetMech);
        }
        else // if (targetMech == null)
        {
            Debug.Log($"MechAIManager :: AI Could NOT acquire target at CURRENT POSITION");

            if (AttempReposition_v0(controlledMech) == true)
            {
                controlledMech.OnMoveStopped += ContinueUnitTurn_v0;
            }
            else
            {
                GameManager.Instance.ConcludeCurrentTurn();
            }
        }
    }

    private bool AttempReposition_v0(MechController controlledMech)
    {
        Debug.Log($"MechAIManager::AttempReposition( {controlledMech.MechName} )");

        // For now, enemy mechs will not attempt to re-position if they cannot gain sight of an human player unit within one move action
        // At best, the furthest hexes will be as many hexes away as the mech's weighted distance limit (i.e. lowest movement cost is 1)
        for (int tileDistance = 1; tileDistance <= controlledMech.weightedDistanceRange; tileDistance++)
        {
            List<Cube> potentialHexes = HexGridManager.Instance.HexGrid.GetRing(controlledMech.GetCurrentHexTile(), tileDistance);

            Debug.Log($"MechAIManager :: Evaluating {potentialHexes.Count} nearby positions at distance of {tileDistance} tile(s)...");

            foreach (Cube potentialHex in potentialHexes)
            {
                if (HexGridManager.Instance.IsHexCubeOnMap(potentialHex) == true &&
                    GameManager.Instance.GetFriendlyMechAt(potentialHex) == false &&
                    GameManager.Instance.GetEnemyMechAt(potentialHex) == false)
                {
                    MechController potentialTargetMech = AcquireTarget_v0(fromHexTile: potentialHex);

                    if (potentialTargetMech != null)
                    {
                        List<Cube> path = HexGridManager.Instance.GetPath(controlledMech.GetCurrentHexTile(), potentialHex);

                        if (path != null && path.Count > 1)
                        {
                            if (HexGridManager.Instance.GetPathCost(path) <= controlledMech.weightedDistanceRange)
                            {
                                controlledMech.TravelPath(path);
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    private void ContinueUnitTurn_v0(MechController controlledMech)
    {
        Debug.Log($"MechAIManager::ContinueUnitTurn( {controlledMech.MechName} )");

        controlledMech.OnMoveStopped -= ContinueUnitTurn_v0;

        MechController targetMech = AcquireTarget_v0(controlledMech.GetCurrentHexTile());

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

    private MechController AcquireTarget_v0(Cube fromHexTile)
    {
        int nearestDistance = int.MaxValue;
        MechController targetMech = null;

        // For the moment, we are assuming that the controlled mech is always one of the computer player's
        foreach (MechController opposingMech in GameManager.Instance.MechFriendlies)
        {
            //Debug.Log($"MechAIManager :: Potential Target: {opposingMech.MechName}");

            if (opposingMech.health.CurrentHealth <= 0)
            {
                continue;
            }

            if (GameManager.Instance.CheckForLineOfSight(fromHexTile, opposingMech.GetCurrentHexTile()) != null)
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
#endregion VERSION_0
}