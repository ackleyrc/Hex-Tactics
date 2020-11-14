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
        if (GameManager.Instance.CurrentPlayerTurn != GameManager.PlayerTurn.HUMAN_PLAYER ||
            GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.NONE)
        {
            selectionHighlight.gameObject.SetActive(false);
            actionHighlight.gameObject.SetActive(false);
            warningHighlight.gameObject.SetActive(false);

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

                    if (enemyMechClicked != null)
                    {
                        MechController blockingMech = GameManager.Instance.CheckForBlockingMech(selectedMech, enemyMechClicked);

                        if (blockingMech == null)
                        {
                            Debug.Log($"InputHandler :: Click to Attack enemy mech at {hexCubeUnderMouse}");
                            selectedMech.AttackTarget(enemyMechClicked);
                        }
                    }
                    else // if (enemyMechClicked == null)
                    {
                        if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY &&
                            HexGridManager.Instance.IsHexCubeOnMap(hexCubeUnderMouse) == true)
                        {
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
    }

    private void HandleInteractionUI(Cube hexCubeUnderMouse)
    {
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
                        MechController blockingMech = GameManager.Instance.CheckForBlockingMech(selectedMech, enemyMechUnderMouse);

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
        else
        {
            selectionHighlight.gameObject.SetActive(false);
            warningHighlight.gameObject.SetActive(false);
            actionHighlight.gameObject.SetActive(false);
        }
    }
}