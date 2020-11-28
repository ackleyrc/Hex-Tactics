using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Allegiance { NONE, FRIENDLY, ENEMY }

public class GameManager : MonoBehaviour
{
#region SINGLETON_MGMT
    private static GameManager _Instance;
    public static GameManager Instance { get { return _Instance; } }

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

    public MechEntityDetailsData mechDetails;

    public MechController mechFriendlyPrefab;
    public MechController mechEnemyPrefab;

    public List<MechController> MechFriendlies { get; private set; } = new List<MechController>();
    public List<MechController> MechEnemies { get; private set; } = new List<MechController>();

    public UnitTurnGUI unitTurnGUI;
    public CurrentTurnGUI currentTurnGUI;
    public ActionPanelGUI actionPanelGUI;

    public Button endTurnButton;
    public CanvasGroup endTurnGroup;

    public enum PlayerTurn { NONE, HUMAN_PLAYER, COMPUTER_PLAYER }
    public PlayerTurn CurrentPlayerTurn { get; private set; }

    public enum ActionPhase { NONE, PRIMARY, SECONDARY }
    public ActionPhase CurrentActionPhase { get; private set; }

    public int CurrentUnitIndex { get; private set; }

    private const float COLLATERAL_DISTANCE_THRESHOLD = 0.80f;
    private const float TERRAIN_BLOCKING_LOS_THRESHOLD = 1.20f;

    public int InstanceIndex { get; private set; }
    
    // We cache this for displaying a unit's movement range so we do not have to re-evaluate it every time the cursor moves between hexes
    private HashSet<Cube> currentUnitReachableRange = new HashSet<Cube>();

    private void Start()
    {
        endTurnButton.onClick.AddListener(HandleEndTurnButtonClicked);

        Initialize();
    }

    private void Initialize()
    {
        Debug.Log($"GameManager::Initialize()");

        // TODO: Refactor initialization for better extensibility (e.g. of different counts of mechs on each side)

        MechController mechFriendly_01 = GameObject.Instantiate(mechFriendlyPrefab) as MechController;
        MechController mechFriendly_02 = GameObject.Instantiate(mechFriendlyPrefab) as MechController;
        MechFriendlies.Add(mechFriendly_01);
        MechFriendlies.Add(mechFriendly_02);
        mechFriendly_01.Initialize(new Cube(0, 6), 0, Allegiance.FRIENDLY, mechDetails.GetName(Allegiance.FRIENDLY, 0));
        mechFriendly_02.Initialize(new Cube(1, 7), 1, Allegiance.FRIENDLY, mechDetails.GetName(Allegiance.FRIENDLY, 1));

        MechController mechEnemy_01 = GameObject.Instantiate(mechEnemyPrefab) as MechController;
        MechController mechEnemy_02 = GameObject.Instantiate(mechEnemyPrefab) as MechController;
        MechEnemies.Add(mechEnemy_01);
        MechEnemies.Add(mechEnemy_02);
        mechEnemy_01.Initialize(new Cube(5, 4), 0, Allegiance.ENEMY, mechDetails.GetName(Allegiance.ENEMY, 0));
        mechEnemy_02.Initialize(new Cube(7, 3), 1, Allegiance.ENEMY, mechDetails.GetName(Allegiance.ENEMY, 1));

        foreach (MechController friendlyMech in MechFriendlies)
        {
            friendlyMech.OnMoveStarted += HandleMoveStarted;
            friendlyMech.OnMoveStopped += HandleMoveStopped;
            friendlyMech.OnAttackStarted += HandleAttackStarted;
            friendlyMech.OnAttackStopped += HandleAttackStopped;
        }

        foreach (MechController enemyMech in MechEnemies)
        {
            enemyMech.OnMoveStarted += HandleMoveStarted;
            enemyMech.OnMoveStopped += HandleMoveStopped;
            enemyMech.OnAttackStarted += HandleAttackStarted;
            enemyMech.OnAttackStopped += HandleAttackStopped;
        }

        CurrentPlayerTurn = PlayerTurn.HUMAN_PLAYER;
        CurrentUnitIndex = 0;
        CurrentActionPhase = ActionPhase.PRIMARY;

        endTurnGroup.alpha = 1.0f;
        endTurnGroup.interactable = true;
        endTurnGroup.blocksRaycasts = true;

        actionPanelGUI.DisplayMoveEnabled();
        actionPanelGUI.DisplayAttackEnabled();

        unitTurnGUI.Initialize(new string[] { mechDetails.GetName(Allegiance.FRIENDLY, 0), mechDetails.GetName(Allegiance.FRIENDLY, 1) },
                               new string[] { mechDetails.GetName(Allegiance.ENEMY, 0), mechDetails.GetName(Allegiance.ENEMY, 1) },
                               Allegiance.FRIENDLY);

        CacheReachableRange();
        DisplayCurrentTurn();
    }

    private void ClearGameState()
    {
        Debug.Log($"GameManager::ClearGameState()");

        foreach (MechController friendlyMech in MechFriendlies)
        {
            friendlyMech.OnMoveStarted -= HandleMoveStarted;
            friendlyMech.OnMoveStopped -= HandleMoveStopped;
            friendlyMech.OnAttackStarted -= HandleAttackStarted;
            friendlyMech.OnAttackStopped -= HandleAttackStopped;

            Destroy(friendlyMech.gameObject);
        }

        MechFriendlies.Clear();

        foreach (MechController enemyMech in MechEnemies)
        {
            enemyMech.OnMoveStarted -= HandleMoveStarted;
            enemyMech.OnMoveStopped -= HandleMoveStopped;
            enemyMech.OnAttackStarted -= HandleAttackStarted;
            enemyMech.OnAttackStopped -= HandleAttackStopped;

            Destroy(enemyMech.gameObject);
        }

        MechEnemies.Clear();
    }

    public void ResetGame()
    {
        Debug.Log($"GameManager::ResetGame()");

        ClearGameState();
        Initialize();
    }

    public Cube CheckForLineOfSight(Cube fromHexTile, Cube toHexTile, MechController ignoreMech = null)
    {
        List<Cube> cubesLine = Cube.Line(fromHexTile, toHexTile);

        Vector3 attackerPos = HexGridManager.Instance.GetHexCubeWorldPostion(fromHexTile);
        Vector3 targetPos = HexGridManager.Instance.GetHexCubeWorldPostion(toHexTile);

        for (int i = 0; i < cubesLine.Count; i++)
        {
            HexTerrainType terrain = HexGridManager.Instance.GetHexTerrainType(cubesLine[i]);
            bool terrainAllowsLOS = HexGridManager.Instance.terrainData.AllowsLineOfSight(terrain);

            if (terrainAllowsLOS == false)
            {
                if (i == 0 || i == cubesLine.Count - 1)
                {
                    return cubesLine[i];
                }
                else
                {
                    Vector3 blockerPos = HexGridManager.Instance.GetHexCubeWorldPostion(cubesLine[i]);
                    if (DistanceToLine(blockerPos, attackerPos, targetPos) <= TERRAIN_BLOCKING_LOS_THRESHOLD)
                    {
                        //Debug.Log($"GameManager :: Distance To Line: {DistanceToLine(blockerPos, attackerPos, targetPos)}");
                        return cubesLine[i];
                    }
                }
            }

            if (i == 0 || i == cubesLine.Count - 1)
            {
                continue; // Ignore attacking mech at starting hex and target mech at ending hex
            }

            MechController blockingFriendlyMech = GetFriendlyMechAt(cubesLine[i]);

            if (blockingFriendlyMech != null &&
                blockingFriendlyMech != ignoreMech)
            {
                Vector3 blockerPos = HexGridManager.Instance.GetHexCubeWorldPostion(blockingFriendlyMech.GetCurrentHexTile());
                if (DistanceToLine(blockerPos, attackerPos, targetPos) <= COLLATERAL_DISTANCE_THRESHOLD)
                {
                    //Debug.Log($"GameManager :: Distance To Line: {DistanceToLine(blockerPos, attackerPos, targetPos)}");
                    return cubesLine[i];
                }
            }

            MechController blockingEnemyMech = GetEnemyMechAt(cubesLine[i]);

            if (blockingEnemyMech != null &&
                blockingEnemyMech != ignoreMech)
            {
                Vector3 blockerPos = HexGridManager.Instance.GetHexCubeWorldPostion(blockingEnemyMech.GetCurrentHexTile());
                if (DistanceToLine(blockerPos, attackerPos, targetPos) <= COLLATERAL_DISTANCE_THRESHOLD)
                {
                    //Debug.Log($"GameManager :: Distance To Line: {DistanceToLine(blockerPos, attackerPos, targetPos)}");
                    return cubesLine[i];
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

    public MechController GetFriendlyMechAt(Cube hexTile)
    {
        foreach (MechController mech in MechFriendlies)
        {
            if (mech.GetCurrentHexTile() == hexTile)
            {
                return mech;
            }
        }

        return null;
    }

    public MechController GetEnemyMechAt(Cube hexTile)
    {
        //Debug.Log($"GameManager::GetEnemyMechAt( {hexTile} )");

        foreach (MechController mech in MechEnemies)
        {
            //Debug.Log($"GameManager :: mech: {mech.MechName}");
            if (mech.GetCurrentHexTile() == hexTile)
            {
                return mech;
            }
        }

        return null;
    }

    private void DisplayCurrentTurn()
    {
        //Debug.Log($"GameManager::DisplayCurrentTurn()");

        currentTurnGUI.DisplayTurn(CurrentPlayerTurn);
        unitTurnGUI.SetCurrentTurn(CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER ? Allegiance.FRIENDLY : Allegiance.ENEMY, CurrentUnitIndex);

        foreach (MechController friendlyMech in MechFriendlies)
        {
            if (CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER &&
                CurrentUnitIndex == friendlyMech.UnitIndex)
            {
                friendlyMech.nameGUI.DisplayAsCurrentUnit(true);
            }
            else
            {
                friendlyMech.nameGUI.DisplayAsCurrentUnit(false);
            }
        }

        foreach (MechController enemyMech in MechEnemies)
        {
            if (CurrentPlayerTurn == PlayerTurn.COMPUTER_PLAYER &&
                CurrentUnitIndex == enemyMech.UnitIndex)
            {
                enemyMech.nameGUI.DisplayAsCurrentUnit(true);
            }
            else
            {
                enemyMech.nameGUI.DisplayAsCurrentUnit(false);
            }
        }
    }

    public HashSet<Cube> GetCurrentUnitReachableRange()
    {
        return currentUnitReachableRange;
    }

    private void CacheReachableRange()
    {
        currentUnitReachableRange.Clear();

        if (CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER)
        {
            if (CurrentUnitIndex >= 0 && CurrentUnitIndex < MechFriendlies.Count)
            {
                MechController currentMech = MechFriendlies[CurrentUnitIndex];
                currentUnitReachableRange = HexGridManager.Instance.GetReachableHexes(currentMech.GetCurrentHexTile(), currentMech.weightedDistanceRange);
            }
        }
        else if (CurrentPlayerTurn == PlayerTurn.COMPUTER_PLAYER)
        {
            if (CurrentUnitIndex >= 0 && CurrentUnitIndex < MechEnemies.Count)
            {
                MechController currentMech = MechEnemies[CurrentUnitIndex];
                currentUnitReachableRange = HexGridManager.Instance.GetReachableHexes(currentMech.GetCurrentHexTile(), currentMech.weightedDistanceRange);
            }
        }

        Debug.Log($"GameManager :: Cached Reachable Hexes: {currentUnitReachableRange.Count}");
        /*
        foreach (Cube cube in currentReachableRange)
        {
            Debug.Log($"GameManager :: Reachable Hex: {cube}");
        }
        */
    }

    private void HandleEndTurnButtonClicked()
    {
        Debug.Log($"GameManager::HandleEndTurnButtonClicked()");

        ConcludeCurrentTurn();
    }

    public void ConcludeCurrentTurn()
    {
        Debug.Log($"GameManager::ConcludeCurrentTurn()");

        //Debug.Log($"GameManager :: Current Unit Index: {CurrentUnitIndex}");
        //Debug.Log($"GameManager :: Current Player Turn: {CurrentPlayerTurn}");

        if (HasHumanPlayerWon() == true)
        {
            StartCoroutine(HandleEndOfGame(humanPlayerWon: true));
            return;
        }
        else if (HasComputerPlayerWon() == true)
        {
            StartCoroutine(HandleEndOfGame(humanPlayerWon: false));
            return;
        }

        CurrentUnitIndex++;
        CurrentActionPhase = ActionPhase.PRIMARY;

        if (CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER &&
            CurrentUnitIndex >= MechFriendlies.Count)
        {
            CurrentUnitIndex = 0;
            CurrentPlayerTurn = PlayerTurn.COMPUTER_PLAYER;

            endTurnGroup.alpha = 0.0f;
            endTurnGroup.interactable = false;
            endTurnGroup.blocksRaycasts = false;
        }
        else if (CurrentPlayerTurn == PlayerTurn.COMPUTER_PLAYER &&
                 CurrentUnitIndex >= MechEnemies.Count)
        {
            CurrentUnitIndex = 0;
            CurrentPlayerTurn = PlayerTurn.HUMAN_PLAYER;

            endTurnGroup.alpha = 1.0f;
            endTurnGroup.interactable = true;
            endTurnGroup.blocksRaycasts = true;
        }

        Debug.Log($"GameManager :: New Unit Index: {CurrentUnitIndex}");
        Debug.Log($"GameManager :: New Player Turn: {CurrentPlayerTurn}");

        CacheReachableRange();
        DisplayCurrentTurn();

        if (CurrentPlayerTurn == PlayerTurn.COMPUTER_PLAYER)
        {
            actionPanelGUI.gameObject.SetActive(false);

            StartCoroutine(ConductEnemyTurn());
        }
        else if (CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER)
        {
            actionPanelGUI.gameObject.SetActive(true);

            if (MechFriendlies[CurrentUnitIndex].health.CurrentHealth <= 0)
            {
                StartCoroutine(SkipTurn());
            }
        }
    }

    private bool HasComputerPlayerWon()
    {
        foreach (MechController friendlyMech in MechFriendlies)
        {
            if (friendlyMech.health.CurrentHealth > 0)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasHumanPlayerWon()
    {
        foreach (MechController enemyMech in MechEnemies)
        {
            if (enemyMech.health.CurrentHealth > 0)
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator HandleEndOfGame(bool humanPlayerWon)
    {
        GameManager.Instance.CurrentActionPhase = GameManager.ActionPhase.NONE;

        endTurnGroup.alpha = 0.0f;
        endTurnGroup.interactable = false;
        endTurnGroup.blocksRaycasts = false;

        actionPanelGUI.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.0f);
        
        WinConditionGUI.Instance.Display(humanPlayerWon);
    }

    private IEnumerator SkipTurn()
    {
        yield return new WaitForSeconds(1.5f);

        ConcludeCurrentTurn();
    }

    private IEnumerator ConductEnemyTurn()
    {
        Debug.Log($"GameManager::ConductEnemyTurn()");

        //Debug.Log($"GameManager :: Current Unit Index: {CurrentUnitIndex}");
        //Debug.Log($"GameManager :: Current Player Turn: {CurrentPlayerTurn}");
        //Debug.Log($"GameManager :: Enemy Mechs: {MechEnemies?.Count}");

        MechController currentEnemyMech = MechEnemies[CurrentUnitIndex];

        if (currentEnemyMech.health.CurrentHealth > 0)
        {
            MechAIManager.Instance.ConductUnitTurn(currentEnemyMech);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);

            ConcludeCurrentTurn();
        }
    }

    private void HandleMoveStarted(MechController mechStartingMove)
    {
        Debug.Log($"GameManager::HandleMoveStarted()");

        CurrentPlayerTurn = PlayerTurn.NONE;
        CurrentActionPhase = ActionPhase.NONE;
    }

    private void HandleMoveStopped(MechController mechStoppingMove)
    {
        Debug.Log($"GameManager::HandleMoveStopped( {mechStoppingMove.MechName} )");

        CurrentPlayerTurn = mechStoppingMove.MechAllegiance == Allegiance.FRIENDLY ? PlayerTurn.HUMAN_PLAYER : PlayerTurn.COMPUTER_PLAYER;
        CurrentActionPhase = ActionPhase.SECONDARY;

        //Debug.Log($"GameManager :: CurrentPlayerTurn: {CurrentPlayerTurn}");
        //Debug.Log($"GameManager :: CurrentActionPhase: {CurrentActionPhase}");
        //Debug.Log($"GameManager :: CurrentUnitIndex: {CurrentUnitIndex}");

        if (CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER)
        {
            actionPanelGUI.DisplayMoveDisabled();
            actionPanelGUI.DisplayAttackEnabled();
        }
    }

    private void HandleAttackStarted(MechController mechStartingAttack)
    {
        Debug.Log($"GameManager::HandleAttackStarted()");

        CurrentPlayerTurn = PlayerTurn.NONE;
        CurrentActionPhase = ActionPhase.NONE;
    }

    private void HandleAttackStopped(MechController mechStoppingAttack)
    {
        Debug.Log($"GameManager::HandleAttackStopped()");

        CurrentPlayerTurn = mechStoppingAttack.MechAllegiance == Allegiance.FRIENDLY ? PlayerTurn.HUMAN_PLAYER : PlayerTurn.COMPUTER_PLAYER;
        ConcludeCurrentTurn();

        if (CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER)
        {
            actionPanelGUI.DisplayMoveEnabled();
            actionPanelGUI.DisplayAttackEnabled();
        }
    }
}