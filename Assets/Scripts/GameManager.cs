using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

    /// <summary> Boolean parameter indicates whether the human player has won </summary>
    public event System.Action<bool> OnWinLoseConditionMet = delegate { };

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

    private enum MapQuadrant { LOWER_LEFT, LOWER_RIGHT, UPPER_LEFT, UPPER_RIGHT }

    private List<Cube> team1StartHexes;
    private List<Cube> team2StartHexes;

    private int currentSeed = -1;

    private void Start()
    {
        endTurnButton.onClick.AddListener(HandleEndTurnButtonClicked);

        Initialize();
    }

    private void Initialize(int seed = -1, List<Cube> team1StartHexes = null, List<Cube> team2StartHexes = null)
    {
        Debug.Log($"GameManager::Initialize()");

        // TODO: Refactor initialization for better extensibility (e.g. of different counts of mechs on each side)

        if (seed == -1 || seed != currentSeed)
        {
            currentSeed = (seed == -1 ? Random.Range(0, 10000) : seed);

            HexGridManager.Instance.GenerateMap(currentSeed);
        }

        if (team1StartHexes == null || team1StartHexes.Count != 2 ||
            team2StartHexes == null || team2StartHexes.Count != 2)
        {
            System.Random rnd = new System.Random();
            List<MapQuadrant> quadrants = new List<MapQuadrant> { MapQuadrant.LOWER_LEFT, MapQuadrant.LOWER_RIGHT, MapQuadrant.UPPER_LEFT, MapQuadrant.UPPER_RIGHT };
            List<MapQuadrant> selectedQuadrants = quadrants.OrderBy(q => rnd.Next()).Take(2).ToList();

            this.team1StartHexes = GetStartHexesForQuadrant(selectedQuadrants[0], 2);
            this.team2StartHexes = GetStartHexesForQuadrant(selectedQuadrants[1], 2);
        }

        if (this.team1StartHexes.Count >= 2 &&
            this.team2StartHexes.Count >= 2)
        {
            MechController mechFriendly_01 = GameObject.Instantiate(mechFriendlyPrefab) as MechController;
            MechController mechFriendly_02 = GameObject.Instantiate(mechFriendlyPrefab) as MechController;
            MechFriendlies.Add(mechFriendly_01);
            MechFriendlies.Add(mechFriendly_02);
            mechFriendly_01.Initialize(this.team1StartHexes[0], 0, Allegiance.FRIENDLY, mechDetails.GetName(Allegiance.FRIENDLY, 0));
            mechFriendly_02.Initialize(this.team1StartHexes[1], 1, Allegiance.FRIENDLY, mechDetails.GetName(Allegiance.FRIENDLY, 1));

            MechController mechEnemy_01 = GameObject.Instantiate(mechEnemyPrefab) as MechController;
            MechController mechEnemy_02 = GameObject.Instantiate(mechEnemyPrefab) as MechController;
            MechEnemies.Add(mechEnemy_01);
            MechEnemies.Add(mechEnemy_02);
            mechEnemy_01.Initialize(this.team2StartHexes[0], 0, Allegiance.ENEMY, mechDetails.GetName(Allegiance.ENEMY, 0));
            mechEnemy_02.Initialize(this.team2StartHexes[1], 1, Allegiance.ENEMY, mechDetails.GetName(Allegiance.ENEMY, 1));

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
    }

    private List<Cube> GetStartHexesForQuadrant(MapQuadrant quadrant, int numStartHexes)
    {
        Debug.Log($"GameManager::GetStartHexesForQuadrant( {quadrant} , {numStartHexes} )");

        List<Cube> startHexes = new List<Cube>();

        int width = HexGridManager.Instance.MapWidth;
        int length = HexGridManager.Instance.MapLength;

        OffsetCoord startCoord = new OffsetCoord(-1, -1);

        // Get the approximate center of the quadrant
        switch (quadrant)
        {
            case MapQuadrant.LOWER_LEFT:
                int llCol = Mathf.FloorToInt(width * 0.25f);
                int llRow = Mathf.FloorToInt(length * 0.25f);
                startCoord = new OffsetCoord(llCol, llRow);
                break;
            case MapQuadrant.LOWER_RIGHT:
                int lrCol = Mathf.FloorToInt(width * 0.75f);
                int lrRow = Mathf.FloorToInt(length * 0.25f);
                startCoord = new OffsetCoord(lrCol, lrRow);
                break;
            case MapQuadrant.UPPER_LEFT:
                int ulCol = Mathf.FloorToInt(width * 0.25f);
                int ulRow = Mathf.FloorToInt(length * 0.75f);
                startCoord = new OffsetCoord(ulCol, ulRow);
                break;
            case MapQuadrant.UPPER_RIGHT:
                int urCol = Mathf.FloorToInt(width * 0.75f);
                int urRow = Mathf.FloorToInt(length * 0.75f);
                startCoord = new OffsetCoord(urCol, urRow);
                break;
        }

        if (startCoord.col != -1 && startCoord.row != -1)
        {
            Cube startCube = startCoord.ToCube();

            // Find an initial traversible hex that has at least (numStartHexes - 1) other reachable hexes around it
            int maxRadius = Mathf.FloorToInt(Mathf.Max(width, length) * 0.25f);
            for (int i = 0; i < maxRadius; i++)
            {
                foreach (Cube cubeInStartingRing in HexGridManager.Instance.HexGrid.GetRing(startCube, i))
                {
                    if (HexGridManager.Instance.CanTravelOverHex(cubeInStartingRing) == false)
                    {
                        continue;
                    }

                    if (IsHexInQuadrant(cubeInStartingRing, quadrant, width, length) == false)
                    {
                        continue;
                    }

                    HashSet<Cube> reachable = HexGridManager.Instance.GetReachableHexes(cubeInStartingRing, maxRadius * 2); // 2x max radius accounts for terrain that is half movement
                    List<Cube> reachableValid = reachable.Where(c => IsHexInQuadrant(c, quadrant, width, length) && IsHexInsideMapBorder(c, width, length)).ToList();

                    if (reachableValid.Count > numStartHexes)
                    {
                        System.Random rnd = new System.Random();
                        startHexes = reachableValid.OrderBy(c => rnd.Next()).Take(numStartHexes).ToList();

                        Debug.Log($"GameManager :: Start Hexes: {string.Join(",", startHexes)}");

                        return startHexes;
                    }
                }
            }
        }

        return startHexes;
    }

    private bool IsHexInQuadrant(Cube cube, MapQuadrant quadrant, int width, int length)
    {
        OffsetCoord coord = cube.ToOffsetCoord();

        if (coord.row < Mathf.FloorToInt(length * 0.5f))
        {
            if (coord.col < Mathf.FloorToInt(width * 0.5f))
            {
                return quadrant == MapQuadrant.LOWER_LEFT;
            }
            else
            {
                return quadrant == MapQuadrant.LOWER_RIGHT;
            }
        }
        else
        {
            if (coord.col < Mathf.FloorToInt(width * 0.5f))
            {
                return quadrant == MapQuadrant.UPPER_LEFT;
            }
            else
            {
                return quadrant == MapQuadrant.UPPER_RIGHT;
            }
        }
    }

    private bool IsHexInsideMapBorder(Cube cube, int width, int length)
    {
        OffsetCoord coord = cube.ToOffsetCoord();

        return coord.row > 0 && coord.row < length - 1 && coord.col > 0 && coord.col < width - 1;
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
        Initialize(currentSeed, team1StartHexes, team2StartHexes);
    }

    public void NewStart()
    {
        Debug.Log($"GameManager::NewStart()");

        ClearGameState();
        Initialize(currentSeed);
    }

    public void NewMap()
    {
        Debug.Log($"GameManager::NewMap()");

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

        AudioManager.Instance.PlayUnitTurnChange();

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

        OnWinLoseConditionMet?.Invoke(humanPlayerWon);
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