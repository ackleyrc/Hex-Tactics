using UnityEngine;

public class UnitTurnGUI : MonoBehaviour
{
    public UnitTurnElementGUI[] unitTurns;

    private int friendlyUnitCount;
    private int enemyUnitCount;
    private Allegiance firstTurn;

    public void Initialize(string[] friendlyUnitNames, string[] enemyUnitNames, Allegiance firstTurn)
    {
        this.friendlyUnitCount = friendlyUnitNames.Length;
        this.enemyUnitCount = enemyUnitNames.Length;
        this.firstTurn = firstTurn;

        int unitTurnCount = 0;

        if (firstTurn == Allegiance.FRIENDLY)
        {
            for (int i = 0; i < friendlyUnitNames.Length; i++)
            {
                unitTurns[unitTurnCount].gameObject.SetActive(true);
                unitTurns[unitTurnCount].SetName(Allegiance.FRIENDLY, friendlyUnitNames[i]);
                unitTurns[unitTurnCount].SetCurrentTurn(false);
                unitTurnCount++;
            }
        }

        for (int i = 0; i < enemyUnitNames.Length; i++)
        {
            unitTurns[unitTurnCount].gameObject.SetActive(true);
            unitTurns[unitTurnCount].SetName(Allegiance.ENEMY, enemyUnitNames[i]);
            unitTurns[unitTurnCount].SetCurrentTurn(false);
            unitTurnCount++;
        }
        
        if (firstTurn != Allegiance.FRIENDLY)
        {
            for (int i = 0; i < friendlyUnitNames.Length; i++)
            {
                unitTurns[unitTurnCount].gameObject.SetActive(true);
                unitTurns[unitTurnCount].SetName(Allegiance.FRIENDLY, friendlyUnitNames[i]);
                unitTurns[unitTurnCount].SetCurrentTurn(false);
                unitTurnCount++;
            }
        }
        
        if (unitTurnCount < unitTurns.Length)
        {
            for (int i = unitTurnCount; i < unitTurns.Length; i++)
            {
                Debug.Log($"UnitTurnGUI :: unitTurns[{i}] [{unitTurns[i].gameObject}]");
                unitTurns[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetCurrentTurn(Allegiance allegiance, int unitIndex)
    {
        Debug.Log($"UnitTurnGUI::SetCurrentTurn( {allegiance} , {unitIndex} )");

        int unitOtherCount = allegiance == Allegiance.FRIENDLY ? enemyUnitCount : friendlyUnitCount;
        int unitTurnIndex = allegiance == firstTurn ? unitIndex : unitOtherCount + unitIndex;

        Debug.Log($"UnitTurnGUI :: Unit Other Count: {unitOtherCount} // Unit Turn Index: {unitTurnIndex}");

        for (int i = 0; i < unitTurns.Length; i++)
        {
            unitTurns[i].SetCurrentTurn(i == unitTurnIndex);
        }
    }
}