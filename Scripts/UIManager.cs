using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject gameController;
    public Text turnTextBox;
    private TurnPhase currentPhase;

    void Update()
    {
        currentPhase = gameController.GetComponent<GameController>().currentPhase;
        if (currentPhase == TurnPhase.playerTurn)
            turnTextBox.text = "Player turn";
        else if (currentPhase == TurnPhase.enemyTurn)
            turnTextBox.text = "Enemy turn";
        else if (currentPhase == TurnPhase.victory)
            turnTextBox.text = "Victory!";
        else if (currentPhase == TurnPhase.defeat)
            turnTextBox.text = "Defeat";
        else if (currentPhase == TurnPhase.tie)
            turnTextBox.text = "Tie";
    }
}
