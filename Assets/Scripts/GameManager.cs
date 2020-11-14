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

    public MechNamesData mechNames;

    public MechController mechFriendlyPrefab;
    public MechController mechEnemyPrefab;

    public List<MechController> MechFriendlies { get; private set; } = new List<MechController>();
    public List<MechController> MechEnemies { get; private set; } = new List<MechController>();

    public UnitTurnGUI unitTurnGUI;
    public CurrentTurnGUI currentTurnGUI;
    public ActionPanelGUI actionPanelGUI;

    public Button endTurnButton;

    public enum PlayerTurn { NONE, HUMAN_PLAYER, COMPUTER_PLAYER }
    public PlayerTurn CurrentPlayerTurn { get; private set; }

    public enum ActionPhase { NONE, PRIMARY, SECONDARY }
    public ActionPhase CurrentActionPhase { get; private set; }

    public int CurrentUnitIndex { get; private set; }

    private const float COLLATERAL_DISTANCE_THRESHOLD = 0.80f;

    private void Start()
    {
        // TODO: Refactor initialization for better extensibility (e.g. of different counts of mechs on each side)

        MechController mechFriendly_01 = GameObject.Instantiate(mechFriendlyPrefab) as MechController;
        MechController mechFriendly_02 = GameObject.Instantiate(mechFriendlyPrefab) as MechController;
        MechFriendlies.Add(mechFriendly_01);
        MechFriendlies.Add(mechFriendly_02);
        mechFriendly_01.Initialize(new Cube(2, 1), 0, Allegiance.FRIENDLY, mechNames.FriendlyMechNames[0]);
        mechFriendly_02.Initialize(new Cube(1, 2), 1, Allegiance.FRIENDLY, mechNames.FriendlyMechNames[1]);

        MechController mechEnemy_01 = GameObject.Instantiate(mechEnemyPrefab) as MechController;
        MechController mechEnemy_02 = GameObject.Instantiate(mechEnemyPrefab) as MechController;
        MechEnemies.Add(mechEnemy_01);
        MechEnemies.Add(mechEnemy_02);
        mechEnemy_01.Initialize(new Cube(4, 3), 0, Allegiance.ENEMY, mechNames.EnemyMechNames[0]);
        mechEnemy_02.Initialize(new Cube(3, 2), 1, Allegiance.ENEMY, mechNames.EnemyMechNames[1]);

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

        actionPanelGUI.DisplayMoveEnabled();
        actionPanelGUI.DisplayAttackEnabled();

        unitTurnGUI.Initialize(new string[] { mechNames.FriendlyMechNames[0], mechNames.FriendlyMechNames[1] },
                               new string[] { mechNames.EnemyMechNames[0], mechNames.EnemyMechNames[1] },
                               Allegiance.FRIENDLY);
        unitTurnGUI.SetCurrentTurn(Allegiance.FRIENDLY, CurrentUnitIndex);

        endTurnButton.onClick.AddListener(HandleEndTurnButtonClicked);
    }

    public MechController CheckForBlockingMech(MechController attackingMech, MechController targetMech)
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

            MechController blockingFriendlyMech = GetFriendlyMechAt(cubesLine[i]);

            if (blockingFriendlyMech != null)
            {
                Vector3 blockerPos = HexGridManager.Instance.GetHexCubeWorldPostion(blockingFriendlyMech.GetCurrentHexTile());
                if (DistanceToLine(blockerPos, attackerPos, targetPos) <= COLLATERAL_DISTANCE_THRESHOLD)
                {
                    //Debug.Log($"GameManager :: Distance To Line: {DistanceToLine(blockerPos, attackerPos, targetPos)}");
                    return blockingFriendlyMech;
                }
            }

            MechController blockingEnemyMech = GetEnemyMechAt(cubesLine[i]);

            if (blockingEnemyMech != null)
            {
                Vector3 blockerPos = HexGridManager.Instance.GetHexCubeWorldPostion(blockingEnemyMech.GetCurrentHexTile());
                if (DistanceToLine(blockerPos, attackerPos, targetPos) <= COLLATERAL_DISTANCE_THRESHOLD)
                {
                    //Debug.Log($"GameManager :: Distance To Line: {DistanceToLine(blockerPos, attackerPos, targetPos)}");
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
        foreach (MechController mech in MechEnemies)
        {
            if (mech.GetCurrentHexTile() == hexTile)
            {
                return mech;
            }
        }

        return null;
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

        CurrentUnitIndex++;
        CurrentActionPhase = ActionPhase.PRIMARY;

        if (CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER &&
            CurrentUnitIndex >= MechFriendlies.Count)
        {
            CurrentUnitIndex = 0;
            CurrentPlayerTurn = PlayerTurn.COMPUTER_PLAYER;
            endTurnButton.gameObject.SetActive(false);
        }
        else if (CurrentPlayerTurn == PlayerTurn.COMPUTER_PLAYER &&
                 CurrentUnitIndex >= MechEnemies.Count)
        {
            CurrentUnitIndex = 0;
            CurrentPlayerTurn = PlayerTurn.HUMAN_PLAYER;
            endTurnButton.gameObject.SetActive(true);
        }

        //Debug.Log($"GameManager :: New Unit Index: {CurrentUnitIndex}");
        //Debug.Log($"GameManager :: New Player Turn: {CurrentPlayerTurn}");

        currentTurnGUI.DisplayTurn(CurrentPlayerTurn);
        unitTurnGUI.SetCurrentTurn(CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER ? Allegiance.FRIENDLY : Allegiance.ENEMY, CurrentUnitIndex);

        if (CurrentPlayerTurn == PlayerTurn.COMPUTER_PLAYER)
        {
            StartCoroutine(ConductEnemyTurn());
        }
    }

    private IEnumerator ConductEnemyTurn()
    {
        Debug.Log($"GameManager::ConductEnemyTurn()");

        yield return new WaitForSeconds(1.5f);

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
        Debug.Log($"GameManager::HandleMoveStopped()");

        CurrentPlayerTurn = mechStoppingMove.MechAllegiance == Allegiance.FRIENDLY ? PlayerTurn.HUMAN_PLAYER : PlayerTurn.COMPUTER_PLAYER;
        CurrentActionPhase = ActionPhase.SECONDARY;

        actionPanelGUI.DisplayMoveDisabled();
        actionPanelGUI.DisplayAttackEnabled();
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

        actionPanelGUI.DisplayMoveEnabled();
        actionPanelGUI.DisplayAttackEnabled();
    }
}