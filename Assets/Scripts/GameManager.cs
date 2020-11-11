using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public MechController mechFriendlyPrefab;
    public MechController mechEnemyPrefab;

    private List<MechController> mechFriendlies = new List<MechController>();
    private List<MechController> mechEnemies = new List<MechController>();

    private void Start()
    {
        MechController mechFriendly_01 = GameObject.Instantiate(mechFriendlyPrefab) as MechController;
        MechController mechFriendly_02 = GameObject.Instantiate(mechFriendlyPrefab) as MechController;
        mechFriendlies.Add(mechFriendly_01);
        mechFriendlies.Add(mechFriendly_02);
        mechFriendly_01.Initialize(new Cube(2, 1));
        mechFriendly_02.Initialize(new Cube(1, 2));

        MechController mechEnemy = GameObject.Instantiate(mechEnemyPrefab) as MechController;
        mechEnemies.Add(mechEnemy);
        mechEnemy.Initialize(new Cube(4, 2));
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
}