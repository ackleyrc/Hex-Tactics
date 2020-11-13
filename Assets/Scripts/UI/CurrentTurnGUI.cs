using UnityEngine;
using UnityEngine.UI;

public class CurrentTurnGUI : MonoBehaviour
{
    public HUDColorPalette colorPalette;

    public Image background;
    public Text turnText;

    public void DisplayTurn(GameManager.PlayerTurn playerTurn)
    {
        if (playerTurn == GameManager.PlayerTurn.HUMAN_PLAYER)
        {
            background.color = colorPalette.FriendlyUnitColor;
            turnText.text = "YOUR TURN";
            this.gameObject.SetActive(true);
        }
        else if (playerTurn == GameManager.PlayerTurn.COMPUTER_PLAYER)
        {
            background.color = colorPalette.EnemyUnitColor;
            turnText.text = "ENEMY TURN";
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}