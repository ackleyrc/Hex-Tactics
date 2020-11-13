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
        if (GameManager.Instance.CurrentPlayerTurn != GameManager.PlayerTurn.HUMAN_PLAYER)
        {
            return;
        }

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
                if (selectedMech == null &&
                    GameManager.Instance.CurrentUnitIndex == friendlyMechClicked.UnitIndex)
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
                        MechController blockingMech = CheckForBlockingMech(selectedMech, enemyMechClicked);

                        if (blockingMech == null)
                        {
                            Debug.Log($"InputHandler :: Click to Attack enemy mech at {hexCubeUnderMouse}");
                            selectedMech.AttackTarget(enemyMechClicked);
                        }
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

                    MechController friendlyMechUnderMouse = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);

                    if (friendlyMechUnderMouse != null &&
                        GameManager.Instance.CurrentUnitIndex == friendlyMechUnderMouse.UnitIndex)
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

                if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
                {
                    GameManager.Instance.actionPanelGUI.DisplayMoveEnabled();
                    GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                }
                else if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.SECONDARY)
                {
                    GameManager.Instance.actionPanelGUI.DisplayMoveDisabled();
                    GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                }
            }
            else // if (selectedMech != null)
            {
                if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse) == true)
                {
                    MechController friendlyMechUnderMouse = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);
                    MechController enemyMechUnderMouse = GameManager.Instance.GetEnemyMechAt(currentHexTileUnderMouse);

                    if (friendlyMechUnderMouse != null)
                    {
                        if (friendlyMechUnderMouse != selectedMech)
                        {
                            actionHighlight.DisplayAsGenericHighlight();
                            actionHighlight.gameObject.SetActive(true);
                            actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(friendlyMechUnderMouse.GetCurrentHexTile());
                        }
                        else
                        {
                            actionHighlight.gameObject.SetActive(false);
                        }

                        if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
                        {
                            GameManager.Instance.actionPanelGUI.DisplayMoveEnabled();
                            GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                        }
                        else if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.SECONDARY)
                        {
                            GameManager.Instance.actionPanelGUI.DisplayMoveDisabled();
                            GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                        }
                    }
                    else if (enemyMechUnderMouse != null)
                    {
                        MechController blockingMech = CheckForBlockingMech(selectedMech, enemyMechUnderMouse);

                        Debug.Log($"InputHandler :: Blocking Mech? {blockingMech}");

                        if (blockingMech != null)
                        {
                            warningHighlight.DisplayAsCollateralIndicator();
                            warningHighlight.gameObject.SetActive(true);
                            warningHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(blockingMech.GetCurrentHexTile());

                            actionHighlight.DisplayAsAttackIndicator(isValid: false);
                            actionHighlight.gameObject.SetActive(true);
                            actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                            if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
                            {
                                GameManager.Instance.actionPanelGUI.DisplayMoveEnabled();
                                GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                            }
                            else if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.SECONDARY)
                            {
                                GameManager.Instance.actionPanelGUI.DisplayMoveDisabled();
                                GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                            }
                        }
                        else // if (blockingMech == null)
                        {
                            actionHighlight.DisplayAsAttackIndicator(isValid: true);
                            actionHighlight.gameObject.SetActive(true);
                            actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                            if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
                            {
                                GameManager.Instance.actionPanelGUI.DisplayMovePending();
                                GameManager.Instance.actionPanelGUI.DisplayAttackPending();
                            }
                            else if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.SECONDARY)
                            {
                                GameManager.Instance.actionPanelGUI.DisplayMoveDisabled();
                                GameManager.Instance.actionPanelGUI.DisplayAttackPending();
                            }
                        }
                    }
                    else // if (friendlyMechUnderMouse == null && enemyMechUnderMouse == null)
                    {
                        if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
                        {
                            actionHighlight.DisplayAsMoveIndicator();
                            actionHighlight.gameObject.SetActive(true);
                            actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                            GameManager.Instance.actionPanelGUI.DisplayMovePending();
                            GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                        }
                        else if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.SECONDARY)
                        {
                            actionHighlight.DisplayAsGenericHighlight();
                            actionHighlight.gameObject.SetActive(true);
                            actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                            GameManager.Instance.actionPanelGUI.DisplayMoveDisabled();
                            GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                        }
                    }
                }
                else // if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse) == false)
                {
                    actionHighlight.gameObject.SetActive(false);

                    if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
                    {
                        GameManager.Instance.actionPanelGUI.DisplayMoveEnabled();
                        GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                    }
                    else if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.SECONDARY)
                    {
                        GameManager.Instance.actionPanelGUI.DisplayMoveDisabled();
                        GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                    }
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

            if (friendlyMech != null &&
                GameManager.Instance.CurrentUnitIndex == friendlyMech.UnitIndex)
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

        if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
        {
            GameManager.Instance.actionPanelGUI.DisplayMoveEnabled();
            GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
        }
        else if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.SECONDARY)
        {
            GameManager.Instance.actionPanelGUI.DisplayMoveDisabled();
            GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
        }
    }
}