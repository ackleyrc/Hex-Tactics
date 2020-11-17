using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePathGUI : MonoBehaviour
{
#region SINGLETON_MGMT
    private static MovePathGUI _Instance;
    public static MovePathGUI Instance { get { return _Instance; } }

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

    public LineRenderer lineRenderer;
    public Vector3 offset;
    public float lineWidth = 0.08f;
    public int sortingOrder = 2;

    public void DisplayPath(List<Cube> path, bool isValid)
    {
        lineRenderer.startColor = isValid ? Color.white : Color.black;
        lineRenderer.endColor = isValid ? Color.white : Color.black;
        lineRenderer.sortingOrder = sortingOrder;
        
        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, offset + HexGridManager.Instance.GetHexCubeWorldPostion(path[i]));
        }

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.gameObject.SetActive(true);
    }

    public void HidePath()
    {
        lineRenderer.gameObject.SetActive(false);
    }
}