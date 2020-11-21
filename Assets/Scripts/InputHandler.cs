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
        actionHighlight.DisplayAsMoveIndicator(true);
        warningHighlight.DisplayAsBlockedLOSIndicator();

        selectionHighlight.gameObject.SetActive(false);
        actionHighlight.gameObject.SetActive(false);
        warningHighlight.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PauseScreenGUI.Instance.IsDisplayed == true ||
            GameManager.Instance.CurrentPlayerTurn != GameManager.PlayerTurn.HUMAN_PLAYER ||
            GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.NONE)
        {
            selectionHighlight.gameObject.SetActive(false);
            actionHighlight.gameObject.SetActive(false);
            warningHighlight.gameObject.SetActive(false);

            HexInfoGUI.Instance.Hide();

            return;
        }

        Cube hexCubeUnderMouse = HexGridManager.Instance.GetHexCubeUnderMouse();
        
        HandleMouseClick(hexCubeUnderMouse);
        HandleInteractionUI(hexCubeUnderMouse);
    }

    private void HandleMouseClick(Cube hexCubeUnderMouse)
    {
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

                    //Debug.Log($"InputHandler :: enemyMechClicked: {enemyMechClicked?.MechName}");

                    if (enemyMechClicked != null)
                    {
                        if (enemyMechClicked.health.CurrentHealth > 0)
                        {
                            Cube blockingHex = GameManager.Instance.CheckForLineOfSight(selectedMech.GetCurrentHexTile(), enemyMechClicked.GetCurrentHexTile());

                            if (blockingHex == null)
                            {
                                Debug.Log($"InputHandler :: Click to Attack enemy mech at {hexCubeUnderMouse}");
                                selectedMech.AttackTarget(enemyMechClicked);
                            }
                        }
                    }
                    else // if (enemyMechClicked == null)
                    {
                        if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY &&
                            HexGridManager.Instance.IsHexCubeOnMap(hexCubeUnderMouse) == true &&
                            HexGridManager.Instance.CanTravelOverHex(hexCubeUnderMouse) == true)
                        {
                            Debug.Log($"InputHandler :: Click to Move from {selectedMech.GetCurrentHexTile()} to {hexCubeUnderMouse}");

                            List<Cube> path = HexGridManager.Instance.GetPath(selectedMech.GetCurrentHexTile(), hexCubeUnderMouse);

                            if (path != null && path.Count > 1)
                            {
                                //Debug.Log($"InputHanlder :: Obtained path of length: {path.Count}");
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
    }

    private void HandleInteractionUI(Cube hexCubeUnderMouse)
    {
        if (currentHexTileUnderMouse != hexCubeUnderMouse)
        {
            currentHexTileUnderMouse = hexCubeUnderMouse;

            warningHighlight.gameObject.SetActive(false);

            HexInfoGUI.Instance.UpdateInfoPanel(currentHexTileUnderMouse);

            //Debug.Log($"InputHandler :: Selected Mech: {selectedMech?.MechName}");

            if (selectedMech == null)
            {
                if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse))
                {
                    selectionHighlight.gameObject.SetActive(true);
                    selectionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                    MechController friendlyMechUnderMouse = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);

                    //Debug.Log($"InputHandler :: Friendly Mech Under Mouse: {friendlyMechUnderMouse?.MechName} [{friendlyMechUnderMouse?.UnitIndex}]");

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

                LineOfSightGUI.Instance.HideLinesOfSight();
                MovePathGUI.Instance.HidePath();
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

                        LineOfSightGUI.Instance.DisplayLinesOfSight(selectedMech.GetCurrentHexTile(), selectedMech);
                        MovePathGUI.Instance.HidePath();
                    }
                    else if (enemyMechUnderMouse != null)
                    {
                        if (enemyMechUnderMouse.health.CurrentHealth <= 0)
                        {
                            warningHighlight.DisplayAsDeadIndicator();
                            warningHighlight.gameObject.SetActive(true);
                            warningHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(enemyMechUnderMouse.GetCurrentHexTile());

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
                        else // if (enemyMechUnderMouse.health.CurrentHealth > 0)
                        {
                            Cube blockingHex = GameManager.Instance.CheckForLineOfSight(selectedMech.GetCurrentHexTile(), enemyMechUnderMouse.GetCurrentHexTile());

                            Debug.Log($"InputHandler :: Blocking Hex? {blockingHex}");

                            if (blockingHex != null)
                            {
                                warningHighlight.DisplayAsBlockedLOSIndicator();
                                warningHighlight.gameObject.SetActive(true);
                                warningHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(blockingHex);

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
                            else // if (blockingHex == null)
                            {
                                actionHighlight.DisplayAsAttackIndicator(isValid: true);
                                actionHighlight.gameObject.SetActive(true);
                                actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                                if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
                                {
                                    GameManager.Instance.actionPanelGUI.DisplayMovePending(displaySkipNotice: true);
                                    GameManager.Instance.actionPanelGUI.DisplayAttackPending();
                                }
                                else if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.SECONDARY)
                                {
                                    GameManager.Instance.actionPanelGUI.DisplayMoveDisabled();
                                    GameManager.Instance.actionPanelGUI.DisplayAttackPending();
                                }
                            }
                        }

                        LineOfSightGUI.Instance.DisplayLinesOfSight(selectedMech.GetCurrentHexTile(), selectedMech);
                        MovePathGUI.Instance.HidePath();
                    }
                    else // if (friendlyMechUnderMouse == null && enemyMechUnderMouse == null)
                    {
                        if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
                        {
                            List<Cube> pathToHexUnderMouse = HexGridManager.Instance.GetPath(selectedMech.GetCurrentHexTile(), currentHexTileUnderMouse);
                            bool isCompletePath = pathToHexUnderMouse != null && pathToHexUnderMouse.Count > 1;

                            actionHighlight.DisplayAsMoveIndicator(isValid: isCompletePath);
                            actionHighlight.gameObject.SetActive(true);
                            actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                            GameManager.Instance.actionPanelGUI.DisplayMovePending();
                            GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();

                            LineOfSightGUI.Instance.DisplayLinesOfSight(currentHexTileUnderMouse, selectedMech);

                            if (isCompletePath == true)
                            {
                                MovePathGUI.Instance.DisplayPath(pathToHexUnderMouse, true);
                            }
                            else
                            {
                                MovePathGUI.Instance.HidePath();
                            }
                        }
                        else if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.SECONDARY)
                        {
                            actionHighlight.DisplayAsGenericHighlight();
                            actionHighlight.gameObject.SetActive(true);
                            actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                            GameManager.Instance.actionPanelGUI.DisplayMoveDisabled();
                            GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();

                            LineOfSightGUI.Instance.DisplayLinesOfSight(selectedMech.GetCurrentHexTile(), selectedMech);
                            MovePathGUI.Instance.HidePath();
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

                    LineOfSightGUI.Instance.DisplayLinesOfSight(selectedMech.GetCurrentHexTile(), selectedMech);
                    MovePathGUI.Instance.HidePath();
                }
            }
        }
    }

    private void SelectMech(MechController mechToSelect)
    {
        //Debug.Log($"InputHandler::SelectMech( {mechToSelect?.MechName} )");

        selectedMech = mechToSelect;
        selectionHighlight.gameObject.SetActive(true);
        selectionHighlight.DisplayAsSelectIndicator(isSelected: true);
        selectionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(mechToSelect.GetCurrentHexTile());

        actionHighlight.gameObject.SetActive(false);

        LineOfSightGUI.Instance.DisplayLinesOfSight(selectedMech.GetCurrentHexTile(), selectedMech);
        MovePathGUI.Instance.HidePath();
    }

    private void DeselectMech()
    {
        //Debug.Log($"InputHandler::DeselectMech()");

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
        warningHighlight.gameObject.SetActive(false);

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
        else
        {
            selectionHighlight.gameObject.SetActive(false);
            warningHighlight.gameObject.SetActive(false);
            actionHighlight.gameObject.SetActive(false);
        }

        LineOfSightGUI.Instance.HideLinesOfSight();
        MovePathGUI.Instance.HidePath();
    }
}