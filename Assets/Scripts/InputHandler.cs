using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public HighlightIndicator selectionHighlight;
    public HighlightIndicator actionHighlight;
    public HighlightIndicator warningHighlight;
    public float collateralDistanceThreshold = 0.80f;

    private Cube currentHexTileUnderMouse = new Cube (-1, -1);
    private MechController selectedMech = null;

    private void Awake()
    {
        selectionHighlight = GameObject.Instantiate(selectionHighlight) as HighlightIndicator;
        actionHighlight = GameObject.Instantiate(actionHighlight) as HighlightIndicator;
        warningHighlight = GameObject.Instantiate(warningHighlight) as HighlightIndicator;

        selectionHighlight.DisplayAsGenericHighlight();
        actionHighlight.DisplayAsMoveIndicator();
        warningHighlight.DisplayAsCollateralIndicator();

        selectionHighlight.gameObject.SetActive(false);
        actionHighlight.gameObject.SetActive(false);
        warningHighlight.gameObject.SetActive(false);
    }

    private void Update()
    {
        Cube hexCubeUnderMouse = HexGridManager.Instance.GetHexCubeUnderMouse();

        if (Input.GetMouseButtonUp(1) == true)
        {
            if (selectedMech != null)
            {
                DeselectMech();
            }
        }
        else if (Input.GetMouseButtonUp(0) == true)
        {
            MechController friendlyMechClicked = GameManager.Instance.GetFriendlyMechAt(hexCubeUnderMouse);

            if (friendlyMechClicked != null)
            {
                if (friendlyMechClicked != selectedMech)
                {
                    SelectMech(friendlyMechClicked);
                }
                else // if (friendlyMechClicked == selectedMech)
                {
                    DeselectMech();
                }
            }
            else // if (friendlyMechClicked == null)
            {
                if (selectedMech != null)
                {
                    MechController enemyMechClicked = GameManager.Instance.GetEnemyMechAt(hexCubeUnderMouse);

                    if (enemyMechClicked != null)
                    {
                        Debug.Log($"InputHandler :: Click to Attack enemy mech at {hexCubeUnderMouse}");
                        selectedMech.AttackTarget(enemyMechClicked);
                    }
                    else // if (enemyMechClicked == null)
                    {
                        if (HexGridManager.Instance.IsHexCubeOnMap(hexCubeUnderMouse))
                        {
                            // TODO: Obtain path from selected mech to hex under mouse

                            Debug.Log($"InputHandler :: Click to Move from {selectedMech.GetCurrentHexTile()} to {hexCubeUnderMouse}");

                            List<Cube> path = HexGridManager.Instance.GetPath(selectedMech.GetCurrentHexTile(), hexCubeUnderMouse);

                            if (path != null && path.Count > 1)
                            {
                                Debug.Log($"InputHanlder :: Obtained path of length: {path.Count}");
                                selectedMech.TravelPath(path);
                            }
                            else
                            {
                                Debug.Log($"InputHanlder :: Could NOT obtain Path");
                            }
                        }
                    }

                    DeselectMech();
                }
            }
        }

        if (currentHexTileUnderMouse != hexCubeUnderMouse)
        {
            currentHexTileUnderMouse = hexCubeUnderMouse;

            warningHighlight.gameObject.SetActive(false);

            if (selectedMech == null)
            {
                if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse))
                {
                    selectionHighlight.gameObject.SetActive(true);
                    selectionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                    MechController friendlyMech = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);

                    if (friendlyMech != null)
                    {
                        selectionHighlight.DisplayAsSelectIndicator(isSelected: false);
                    }
                    else
                    {
                        selectionHighlight.DisplayAsGenericHighlight();
                    }
                }
                else
                {
                    selectionHighlight.gameObject.SetActive(false);
                }
            }
            else // if (selectedMech != null)
            {
                if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse))
                {
                    MechController friendlyMech = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);
                    MechController enemyMech = GameManager.Instance.GetEnemyMechAt(currentHexTileUnderMouse);

                    if (friendlyMech != null)
                    {
                        if (friendlyMech != selectedMech)
                        {
                            actionHighlight.DisplayAsSelectIndicator(isSelected: false);
                            actionHighlight.gameObject.SetActive(true);
                            actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(friendlyMech.GetCurrentHexTile());
                        }
                        else
                        {
                            actionHighlight.gameObject.SetActive(false);
                        }
                    }
                    else if (enemyMech != null)
                    {
                        MechController blockingMech = CheckForBlockingMech(selectedMech, enemyMech);

                        Debug.Log($"InputHandler :: Blocking Mech? {blockingMech}");

                        if (blockingMech != null)
                        {
                            warningHighlight.DisplayAsCollateralIndicator();
                            warningHighlight.gameObject.SetActive(true);
                            warningHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(blockingMech.GetCurrentHexTile());
                        }

                        actionHighlight.DisplayAsAttackIndicator(isValid: blockingMech == null);
                        actionHighlight.gameObject.SetActive(true);
                        actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);
                    }
                    else
                    {
                        actionHighlight.DisplayAsMoveIndicator();
                        actionHighlight.gameObject.SetActive(true);
                        actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);
                    }
                }
                else
                {
                    actionHighlight.gameObject.SetActive(false);
                }
            }
        }
    }

    /// TODO? Update this function to obtain a list of mechs. Collateral damage may be inflicted at diminishing rates per mech, 
    /// with the collateral chance based on distance between the center of that mech's hex from the line connecting the start and end hexes.
    private MechController CheckForBlockingMech(MechController attackingMech, MechController targetMech)
    {
        List<Cube> cubesLine = Cube.Line(attackingMech.GetCurrentHexTile(), targetMech.GetCurrentHexTile());

        Vector3 attackerPos = HexGridManager.Instance.GetHexCubeWorldPostion(attackingMech.GetCurrentHexTile());
        Vector3 targetPos = HexGridManager.Instance.GetHexCubeWorldPostion(targetMech.GetCurrentHexTile());

        for (int i = 0; i < cubesLine.Count; i++)
        {
            if (i == 0 || i == cubesLine.Count - 1)
            {
                continue; // Ignore attacking mech at starting hex and target mech at ending hex
            }

            MechController blockingFriendlyMech = GameManager.Instance.GetFriendlyMechAt(cubesLine[i]);

            if (blockingFriendlyMech != null)
            {
                Vector3 blockerPos = HexGridManager.Instance.GetHexCubeWorldPostion(blockingFriendlyMech.GetCurrentHexTile());
                if (DistanceToLine(blockerPos, attackerPos, targetPos) <= collateralDistanceThreshold)
                {
                    //Debug.Log($"InputHandler :: Distance To Line: {DistanceToLine(blockerPos, attackerPos, targetPos)}");
                    return blockingFriendlyMech;
                }
            }

            MechController blockingEnemyMech = GameManager.Instance.GetEnemyMechAt(cubesLine[i]);

            if (blockingEnemyMech != null)
            {
                Vector3 blockerPos = HexGridManager.Instance.GetHexCubeWorldPostion(blockingEnemyMech.GetCurrentHexTile());
                if (DistanceToLine(blockerPos, attackerPos, targetPos) <= collateralDistanceThreshold)
                {
                    //Debug.Log($"InputHandler :: Distance To Line: {DistanceToLine(blockerPos, attackerPos, targetPos)}");
                    return blockingEnemyMech;
                }
            }
        }

        return null;
    }


    /// <summary>
    /// (If a is a point on the line, p is the query point, and n is a normalized vector for the line, 
    /// the distance to the line is given by the length of (a - p) - ((a - p) dot n) * n)
    /// </summary>
    private float DistanceToLine(Vector3 queryPoint, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 a = queryPoint;
        Vector3 p = lineStart;
        Vector3 n = (lineEnd - lineStart).normalized;
        float distanceToLine = ((a - p) - (Vector3.Dot(a - p, n) * n)).magnitude;
        float distanceToStart = Vector3.Distance(queryPoint, lineStart);
        float distanceToEnd = Vector3.Distance(queryPoint, lineEnd);
        return Mathf.Min(distanceToLine, distanceToStart, distanceToEnd);
    }

    private void SelectMech(MechController mechToSelect)
    {
        selectedMech = mechToSelect;
        selectionHighlight.gameObject.SetActive(true);
        selectionHighlight.DisplayAsSelectIndicator(isSelected: true);
        selectionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(mechToSelect.GetCurrentHexTile());

        actionHighlight.gameObject.SetActive(false);
    }

    private void DeselectMech()
    {
        selectedMech = null;

        if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse))
        {
            selectionHighlight.gameObject.SetActive(true);
            selectionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

            MechController friendlyMech = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);

            if (friendlyMech != null)
            {
                selectionHighlight.DisplayAsSelectIndicator(isSelected: false);
            }
            else
            {
                selectionHighlight.DisplayAsGenericHighlight();
            }
        }
        else
        {
            selectionHighlight.gameObject.SetActive(false);
        }

        actionHighlight.gameObject.SetActive(false);
    }
}