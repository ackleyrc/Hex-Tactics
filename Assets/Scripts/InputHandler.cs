using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private static InputHandler _Instance;
    public static InputHandler Instance { get { return _Instance; } }

    public event System.Action OnConductMove = delegate { };
    public event System.Action OnConductAttack = delegate { };

    public HighlightIndicator selectionHighlight;
    public HighlightIndicator actionHighlight;
    public HighlightIndicator warningHighlight;
    public float collateralDistanceThreshold = 0.80f;

    public List<HighlightIndicator> movementRangeHighlights;

    private Cube currentHexTileUnderMouse = new Cube (-1, -1);
    private MechController selectedMech = null;

    private bool movementHighlightsHidden = false;

    private Transform highlightsParent;

    private void Awake()
    {
        _Instance = this;

        highlightsParent = GameObject.Instantiate(new GameObject(), Vector3.zero, Quaternion.identity, this.transform).transform;
        highlightsParent.gameObject.name = "HighlightsParent";

        selectionHighlight = GameObject.Instantiate(selectionHighlight, highlightsParent) as HighlightIndicator;
        actionHighlight = GameObject.Instantiate(actionHighlight, highlightsParent) as HighlightIndicator;
        warningHighlight = GameObject.Instantiate(warningHighlight, highlightsParent) as HighlightIndicator;

        movementRangeHighlights = new List<HighlightIndicator>();

        // Start with 12 to begin with...
        for (int i = 0; i < 12; i++)
        {
            HighlightIndicator highlight = GameObject.Instantiate(selectionHighlight, highlightsParent) as HighlightIndicator;
            movementRangeHighlights.Add(highlight);
            highlight.DisplayAsMoveRangeIndicator();
            highlight.gameObject.SetActive(false);
        }

        Debug.Log($"InputHander :: Initially Created Movement Range Highlights: {movementRangeHighlights.Count}");
        movementHighlightsHidden = true;

        selectionHighlight.DisplayAsGenericHighlight();
        actionHighlight.DisplayAsDestinationIndicator(true, false);
        warningHighlight.DisplayAsBlockedLOSIndicator(false);

        selectionHighlight.gameObject.SetActive(false);
        actionHighlight.gameObject.SetActive(false);
        warningHighlight.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_Instance == this)
        {
            _Instance = null;
        }
    }

    private void Update()
    {
        if (TitleScreenGUI.Instance.IsDisplaying == true ||
            DifficultySelectionGUI.Instance.IsDisplaying == true ||
            PauseScreenGUI.Instance.IsDisplayed == true ||
            CustomScenarioGUI.Instance.IsDisplayed == true ||
            GameManager.Instance.CurrentPlayerTurn != GameManager.PlayerTurn.HUMAN_PLAYER ||
            GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.NONE)
        {
            // TODO: Avoid these invocations every frame
            selectionHighlight.gameObject.SetActive(false);
            actionHighlight.gameObject.SetActive(false);
            warningHighlight.gameObject.SetActive(false);

            if (movementHighlightsHidden == false)
            {
                HideMovementRange();
                movementHighlightsHidden = true;
            }

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

                                StopAllCoroutines();
                                StartCoroutine(ConductAttack(selectedMech, enemyMechClicked));
                                AudioManager.Instance.PlayActionConfirm();
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
                                float pathCost = HexGridManager.Instance.GetPathCost(path);
                                if (pathCost <= selectedMech.weightedDistanceRange)
                                {
                                    StopAllCoroutines();
                                    StartCoroutine(ConductMove(selectedMech, path));
                                    AudioManager.Instance.PlayActionConfirm();
                                }
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

    private IEnumerator ConductMove(MechController selectedMech, List<Cube> path)
    {
        OnConductMove?.Invoke();

        DialogueGUI.Instance.HandleHumanMechMoveDialogue(selectedMech, selectedMech.GetCurrentHexTile(), path[path.Count - 1]);

        yield return new WaitForSeconds(1.0f);

        selectedMech.TravelPath(path);
    }

    private IEnumerator ConductAttack(MechController selectedMech, MechController targetMech)
    {
        OnConductAttack?.Invoke();

        DialogueGUI.Instance.HandleHumanMechAttackDialogue(selectedMech, targetMech);

        yield return new WaitForSeconds(1.0f);

        selectedMech.AttackTarget(targetMech);
    }

    private void HandleInteractionUI(Cube hexCubeUnderMouse)
    {
        if (currentHexTileUnderMouse != hexCubeUnderMouse)
        {
            currentHexTileUnderMouse = hexCubeUnderMouse;

            warningHighlight.gameObject.SetActive(false);

            HexInfoGUI.Instance.UpdateInfoPanel(currentHexTileUnderMouse);

            if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse))
            {
                AudioManager.Instance.PlayHexHoverOver();
            }

            bool isDefended = HexGridManager.Instance.GetDefenseMultiplier(currentHexTileUnderMouse) != 1.0f;

            //Debug.Log($"InputHandler :: Selected Mech: {selectedMech?.MechName}");

            if (selectedMech == null)
            {
                if (movementHighlightsHidden == false)
                {
                    HideMovementRange();
                    movementHighlightsHidden = true;
                }

                if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse))
                {
                    selectionHighlight.gameObject.SetActive(true);
                    selectionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                    MechController friendlyMechUnderMouse = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);

                    //Debug.Log($"InputHandler :: Friendly Mech Under Mouse: {friendlyMechUnderMouse?.MechName} [{friendlyMechUnderMouse?.UnitIndex}]");

                    if (friendlyMechUnderMouse != null &&
                        GameManager.Instance.CurrentUnitIndex == friendlyMechUnderMouse.UnitIndex)
                    {
                        selectionHighlight.DisplayAsSelectIndicator(isSelected: false, isDefended);
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

                        if (movementHighlightsHidden == false)
                        {
                            HideMovementRange();
                            movementHighlightsHidden = true;
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

                            //Debug.Log($"InputHandler :: Blocking Hex? {blockingHex}");

                            if (blockingHex != null)
                            {
                                warningHighlight.DisplayAsBlockedLOSIndicator(isDefended);
                                warningHighlight.gameObject.SetActive(true);
                                warningHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(blockingHex);

                                actionHighlight.DisplayAsAttackIndicator(isValid: false, isDefended);
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
                                actionHighlight.DisplayAsAttackIndicator(isValid: true, isDefended);
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

                        if (movementHighlightsHidden == false)
                        {
                            HideMovementRange();
                            movementHighlightsHidden = true;
                        }

                        LineOfSightGUI.Instance.DisplayLinesOfSight(selectedMech.GetCurrentHexTile(), selectedMech);
                        MovePathGUI.Instance.HidePath();
                    }
                    else // if (friendlyMechUnderMouse == null && enemyMechUnderMouse == null)
                    {
                        if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
                        {
                            List<Cube> pathToHexUnderMouse = HexGridManager.Instance.GetPath(selectedMech.GetCurrentHexTile(), currentHexTileUnderMouse);
                            float pathCost = HexGridManager.Instance.GetPathCost(pathToHexUnderMouse);
                            bool isCompletePath = pathToHexUnderMouse != null && pathToHexUnderMouse.Count > 1;
                            bool isPathInRange = pathCost <= selectedMech.weightedDistanceRange;

                            if (isCompletePath == true)
                            {
                                actionHighlight.DisplayAsDestinationIndicator(isPathInRange, isDefended);
                            }
                            else
                            {
                                actionHighlight.DisplayAsNonTraversibleIndicator();
                            }

                            actionHighlight.gameObject.SetActive(true);
                            actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                            DisplayMovementRange(currentHexTileUnderMouse);
                            movementHighlightsHidden = false;

                            if (isPathInRange == true)
                            {
                                GameManager.Instance.actionPanelGUI.DisplayMovePending();
                                GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                            }
                            else
                            {
                                GameManager.Instance.actionPanelGUI.DisplayMoveEnabled();
                                GameManager.Instance.actionPanelGUI.DisplayAttackEnabled();
                            }

                            LineOfSightGUI.Instance.DisplayLinesOfSight(currentHexTileUnderMouse, selectedMech);

                            if (isCompletePath == true)
                            {
                                MovePathGUI.Instance.DisplayPath(pathToHexUnderMouse, isValid: isPathInRange);
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

                            if (movementHighlightsHidden == false)
                            {
                                HideMovementRange();
                                movementHighlightsHidden = true;
                            }

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

    private void DisplayMovementRange(Cube currentHexUnderMouse)
    {
        //Debug.Log($"InputHander::DisplayMovementRange( currentHexUnderMouse: {currentHexUnderMouse} )");

        int activeHighlightCount = 0;

        // Do not display movement range highlights over select mech's hex or potential destination
        foreach (Cube reachableCube in GameManager.Instance.GetCurrentUnitReachableRange())
        {
            if (reachableCube != selectedMech.GetCurrentHexTile() &&
                reachableCube != currentHexUnderMouse)
            {
                if (activeHighlightCount >= movementRangeHighlights.Count)
                {
                    HighlightIndicator highlight = GameObject.Instantiate(selectionHighlight, highlightsParent) as HighlightIndicator;
                    movementRangeHighlights.Add(highlight);
                    highlight.DisplayAsMoveRangeIndicator();
                }

                movementRangeHighlights[activeHighlightCount].gameObject.SetActive(true);
                movementRangeHighlights[activeHighlightCount].transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(reachableCube);
                activeHighlightCount++;
            }
        }

        //Debug.Log($"InputHander :: Active Highlights: {activeHighlightCount}");

        if (activeHighlightCount < movementRangeHighlights.Count)
        {
            for (int i = activeHighlightCount; i < movementRangeHighlights.Count; i++)
            {
                movementRangeHighlights[activeHighlightCount].gameObject.SetActive(false);
            }
        }
    }

    private void HideMovementRange()
    {
        Debug.Log($"InputHander::HideMovementRange()");

        foreach (HighlightIndicator highlight in movementRangeHighlights)
        {
            highlight.gameObject.SetActive(false);
        }
    }

    private void SelectMech(MechController mechToSelect)
    {
        //Debug.Log($"InputHandler::SelectMech( {mechToSelect?.MechName} )");

        bool isDefended = HexGridManager.Instance.GetDefenseMultiplier(mechToSelect.GetCurrentHexTile()) != 1.0f;

        selectedMech = mechToSelect;
        selectionHighlight.gameObject.SetActive(true);
        selectionHighlight.DisplayAsSelectIndicator(isSelected: true, isDefended);
        selectionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(mechToSelect.GetCurrentHexTile());

        actionHighlight.gameObject.SetActive(false);

        if (GameManager.Instance.CurrentActionPhase == GameManager.ActionPhase.PRIMARY)
        {
            DisplayMovementRange(currentHexTileUnderMouse);
            movementHighlightsHidden = false;
        }

        LineOfSightGUI.Instance.DisplayLinesOfSight(selectedMech.GetCurrentHexTile(), selectedMech);
        MovePathGUI.Instance.HidePath();
    }

    private void DeselectMech()
    {
        //Debug.Log($"InputHandler::DeselectMech()");

        selectedMech = null;

        if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse))
        {
            bool isDefended = HexGridManager.Instance.GetDefenseMultiplier(currentHexTileUnderMouse) != 1.0f;

            selectionHighlight.gameObject.SetActive(true);
            selectionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

            MechController friendlyMech = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);

            if (friendlyMech != null &&
                GameManager.Instance.CurrentUnitIndex == friendlyMech.UnitIndex)
            {
                selectionHighlight.DisplayAsSelectIndicator(isSelected: false, isDefended);
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

        if (movementHighlightsHidden == false)
        {
            HideMovementRange();
            movementHighlightsHidden = true;
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