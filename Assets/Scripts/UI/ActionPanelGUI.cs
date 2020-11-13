using UnityEngine;

public class ActionPanelGUI : MonoBehaviour
{
    public ActionPanelElementGUI moveAction;
    public ActionPanelElementGUI attackAction;

    public void DisplayMoveEnabled()
    {
        moveAction.DisplayAsEnabled();
    }

    public void DisplayMoveDisabled()
    {
        moveAction.DisplayAsDisabled();
    }

    public void DisplayMovePending()
    {
        moveAction.DisplayAsPending();
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