using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public HighlightIndicator selectionHighlight;
    public HighlightIndicator actionHighlight;

    private Cube currentHexTileUnderMouse = new Cube (-1, -1);
    private MechController selectedMech = null;

    private void Awake()
    {
        selectionHighlight = GameObject.Instantiate(selectionHighlight) as HighlightIndicator;
        actionHighlight = GameObject.Instantiate(actionHighlight) as HighlightIndicator;

        selectionHighlight.DisplayAsGenericHighlight();
        actionHighlight.DisplayAsMoveIndicator();

        selectionHighlight.gameObject.SetActive(false);
        actionHighlight.gameObject.SetActive(false);
    }

    private void Update()
    {
        Cube hexCubeUnderMouse = HexGridManager.Instance.GetHexCubeUnderMouse();

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
                if (friendlyMechClicked != selectedMech)
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
                        Debug.Log($"InputHandler :: Click to Attack enemy mech at {hexCubeUnderMouse}");
                        selectedMech.AttackTarget(enemyMechClicked);
                    }
                    else // if (enemyMechClicked == null)
                    {
                        if (HexGridManager.Instance.IsHexCubeOnMap(hexCubeUnderMouse))
                        {
                            // TODO: Obtain path from selected mech to hex under mouse

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

        if (currentHexTileUnderMouse != hexCubeUnderMouse)
        {
            currentHexTileUnderMouse = hexCubeUnderMouse;

            if (selectedMech == null)
            {
                if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse))
                {
                    selectionHighlight.gameObject.SetActive(true);
                    selectionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

                    MechController friendlyMech = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);

                    if (friendlyMech != null)
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
            }
            else // if (selectedMech != null)
            {
                if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse))
                {
                    MechController friendlyMech = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);
                    MechController enemyMech = GameManager.Instance.GetEnemyMechAt(currentHexTileUnderMouse);

                    if (friendlyMech != null)
                    {
                        actionHighlight.gameObject.SetActive(false);
                    }
                    else if (enemyMech != null)
                    {
                        actionHighlight.DisplayAsAttackIndicator();
                        actionHighlight.gameObject.SetActive(true);
                        actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);
                    }
                    else
                    {
                        actionHighlight.DisplayAsMoveIndicator();
                        actionHighlight.gameObject.SetActive(true);
                        actionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);
                    }
                }
                else
                {
                    actionHighlight.gameObject.SetActive(false);
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
    }

    private void DeselectMech()
    {
        selectedMech = null;

        if (HexGridManager.Instance.IsHexCubeOnMap(currentHexTileUnderMouse))
        {
            selectionHighlight.gameObject.SetActive(true);
            selectionHighlight.transform.position = HexGridManager.Instance.GetHexCubeWorldPostion(currentHexTileUnderMouse);

            MechController friendlyMech = GameManager.Instance.GetFriendlyMechAt(currentHexTileUnderMouse);

            if (friendlyMech != null)
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
    }
}