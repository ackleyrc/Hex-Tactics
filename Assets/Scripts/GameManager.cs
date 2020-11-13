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

    private List<MechController> mechFriendlies = new List<MechController>();
    private List<MechController> mechEnemies = new List<MechController>();

    public UnitTurnGUI unitTurnGUI;
    public CurrentTurnGUI currentTurnGUI;
    public Button endTurnButton;

    public enum PlayerTurn { NONE, HUMAN_PLAYER, COMPUTER_PLAYER }
    public PlayerTurn CurrentPlayerTurn { get; private set; }

    public int CurrentUnitIndex { get; private set; }

    private void Start()
    {
        MechController mechFriendly_01 = GameObject.Instantiate(mechFriendlyPrefab) as MechController;
        MechController mechFriendly_02 = GameObject.Instantiate(mechFriendlyPrefab) as MechController;
        mechFriendlies.Add(mechFriendly_01);
        mechFriendlies.Add(mechFriendly_02);
        mechFriendly_01.Initialize(new Cube(2, 1), 0, Allegiance.FRIENDLY, mechNames.FriendlyMechNames[0]);
        mechFriendly_02.Initialize(new Cube(1, 2), 1, Allegiance.FRIENDLY, mechNames.FriendlyMechNames[1]);

        MechController mechEnemy = GameObject.Instantiate(mechEnemyPrefab) as MechController;
        mechEnemies.Add(mechEnemy);
        mechEnemy.Initialize(new Cube(4, 3), 0, Allegiance.ENEMY, mechNames.EnemyMechNames[0]);

        CurrentPlayerTurn = PlayerTurn.HUMAN_PLAYER;
        CurrentUnitIndex = 0;

        unitTurnGUI.Initialize(new string[] { mechNames.FriendlyMechNames[0], mechNames.FriendlyMechNames[1] },
                               new string[] { mechNames.EnemyMechNames[0] },
                               Allegiance.FRIENDLY);
        unitTurnGUI.SetCurrentTurn(Allegiance.FRIENDLY, CurrentUnitIndex);

        endTurnButton.onClick.AddListener(HandleEndTurnButtonClicked);
    }

    public MechController GetFriendlyMechAt(Cube hexTile)
    {
        foreach (MechController mech in mechFriendlies)
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
        foreach (MechController mech in mechEnemies)
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

    private IEnumerator FinishEnemyTurn()
    {
        yield return new WaitForSeconds(5.0f);

        ConcludeCurrentTurn();
    }

    private void ConcludeCurrentTurn()
    {
        CurrentUnitIndex++;

        if (CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER &&
            CurrentUnitIndex >= mechFriendlies.Count)
        {
            CurrentUnitIndex = 0;
            CurrentPlayerTurn = PlayerTurn.COMPUTER_PLAYER;
            endTurnButton.gameObject.SetActive(false);

            StartCoroutine(FinishEnemyTurn());
        }
        else if (CurrentPlayerTurn == PlayerTurn.COMPUTER_PLAYER &&
                 CurrentUnitIndex >= mechEnemies.Count)
        {
            CurrentUnitIndex = 0;
            CurrentPlayerTurn = PlayerTurn.HUMAN_PLAYER;
            endTurnButton.gameObject.SetActive(true);
        }

        currentTurnGUI.DisplayTurn(CurrentPlayerTurn);
        unitTurnGUI.SetCurrentTurn(CurrentPlayerTurn == PlayerTurn.HUMAN_PLAYER ? Allegiance.FRIENDLY : Allegiance.ENEMY, CurrentUnitIndex);
    }
}