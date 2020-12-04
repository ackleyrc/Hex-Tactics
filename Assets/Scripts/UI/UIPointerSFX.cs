using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIPointerSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public Selectable selectable;

    public bool pointerEnter;
    public bool pointerClick;
    public bool pointerDown;
    public bool pointerUp;

    [HideInInspector] // Comment this out if you want to experiment with adjusting this value in play mode
    public float pointerUpThreshold = 0.2f; // How long to wait after mouse down to play pointer up

    private bool mouseWasDown = false; // Tells us if the mouse was first down in the previous frame
    private bool mouseWasPressed = false; // Tells us if the mouse was pressed in the previous frame

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) == true)
        {
            mouseWasDown = true;
            StartCoroutine(ResetMouseWasDownBool());
        }

        if (Input.GetMouseButton(0) == true)
        {
            mouseWasPressed = true;
        }
        else if (mouseWasPressed == true)
        {
            StartCoroutine(ResetMouseWasPressedBool());
        }
    }

    private void OnDisable()
    {
        mouseWasDown = false;
        mouseWasPressed = false;
    }

    private IEnumerator ResetMouseWasDownBool()
    {
        // Effectively, prevent PointerUp from playing SFX unless some time has passed since the mouse button was initially pressed down
        yield return new WaitForSeconds(pointerUpThreshold);
        mouseWasDown = false;
    }

    private IEnumerator ResetMouseWasPressedBool()
    {
        yield return null; // Wait Until Next Frame
        mouseWasPressed = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pointerEnter == true && selectable.interactable == true)
        {
            if (eventData.dragging == false && Input.GetMouseButton(0) == false && mouseWasDown == false)
            {
                AudioManager.Instance.PlayHoverOver();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (pointerClick == true && selectable.interactable == true)
        {
            AudioManager.Instance.PlayClick();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pointerDown == true && selectable.interactable == true)
        {
            AudioManager.Instance.PlayClick();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (pointerUp == true && selectable.interactable == true && mouseWasDown == false)
        {
            AudioManager.Instance.PlayClick();
        }
    }
}