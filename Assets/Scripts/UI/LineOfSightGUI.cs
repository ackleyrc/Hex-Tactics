using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightGUI : MonoBehaviour
{
#region SINGLETON_MGMT
    private static LineOfSightGUI _Instance;
    public static LineOfSightGUI Instance { get { return _Instance; } }

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

    public LineRenderer[] lineRenderers;
    public Vector3 offset;
    public float lineWidth = 0.03f;
    public int defaultSortingOrder = 3;

    public void DisplayLinesOfSight(Cube fromHexTile, MechController ignoreMech = null)
    {
        int lineCount = 0;
        foreach (MechController enemyMech in GameManager.Instance.MechEnemies)
        {
            if (enemyMech.health.CurrentHealth > 0)
            {
                bool isLOSBlocked = GameManager.Instance.CheckForLineOfSight(fromHexTile, enemyMech.GetCurrentHexTile(), ignoreMech) != null;
                lineRenderers[lineCount].startColor = isLOSBlocked ? Color.gray : Color.red;
                lineRenderers[lineCount].endColor = isLOSBlocked ? Color.gray : Color.red;
                lineRenderers[lineCount].sortingOrder = isLOSBlocked ? defaultSortingOrder : defaultSortingOrder + 1;

                lineRenderers[lineCount].SetPosition(0, offset + HexGridManager.Instance.GetHexCubeWorldPostion(fromHexTile));
                lineRenderers[lineCount].SetPosition(1, offset + HexGridManager.Instance.GetHexCubeWorldPostion(enemyMech.GetCurrentHexTile()));

                lineRenderers[lineCount].startWidth = lineWidth;
                lineRenderers[lineCount].endWidth = lineWidth;

                lineRenderers[lineCount].gameObject.SetActive(true);

                lineCount++;
            }
        }

        for (int i = lineCount; i < lineRenderers.Length; i++)
        {
            lineRenderers[i].gameObject.SetActive(false);
        }
    }

    public void HideLinesOfSight()
    {
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            lineRenderers[i].gameObject.SetActive(false);
        }
    }
}