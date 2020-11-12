using UnityEngine;
using UnityEngine.UI;

public class CurrentTurnGUI : MonoBehaviour
{
    public Image background;
    public Text turnText;

    public Color playerColor;
    public Color enemyColor;

    public void DisplayTurn(GameManager.PlayerTurn playerTurn)
    {
        if (playerTurn == GameManager.PlayerTurn.HUMAN_PLAYER)
        {
            background.color = playerColor;
            turnText.text = "YOUR TURN";
            this.gameObject.SetActive(true);
        }
        else if (playerTurn == GameManager.PlayerTurn.COMPUTER_PLAYER)
        {
            background.color = enemyColor;
            turnText.text = "ENEMY TURN";
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}