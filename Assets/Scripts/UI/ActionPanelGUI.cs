using UnityEngine;

public class ActionPanelGUI : MonoBehaviour
{
    public ActionPanelElementGUI moveAction;
    public ActionPanelElementGUI attackAction;
    public GameObject skipMoveNotice;

    public void DisplayMoveEnabled()
    {
        moveAction.DisplayAsEnabled();

        skipMoveNotice.gameObject.SetActive(false);
    }

    public void DisplayMoveDisabled()
    {
        moveAction.DisplayAsDisabled();

        skipMoveNotice.gameObject.SetActive(false);
    }

    public void DisplayMovePending(bool displaySkipNotice = false)
    {
        moveAction.DisplayAsPending();

        skipMoveNotice.SetActive(displaySkipNotice);
    }

    public void DisplayAttackEnabled()
    {
        attackAction.DisplayAsEnabled();
    }

    public void DisplayAttackDisabled()
    {
        attackAction.DisplayAsDisabled();
    }

    public void DisplayAttackPending()
    {
        attackAction.DisplayAsPending();
    }
}