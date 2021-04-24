using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    public int firstDice;
    public int secondDice;
    public bool IsDouble;
    GameManger gameManager;
    public static bool NotCheckForThisTurn = true;

    private void Start(){
        gameManager = FindObjectOfType<GameManger>();
    }

    public DiceManager(int firstDice, int secondDice){
        this.firstDice = firstDice;
        this.secondDice = secondDice;
        if (firstDice == secondDice)
            IsDouble = true;
        else
            IsDouble = false;
    }

    // The function return true if all of steps has been done by the player at his turn;
    public bool IsPlayerDidAllSteps()
    {
        int count = 0;
        foreach(bool step in gameManager.DoneMove){
            if (step)
                count++;
        }
        if ((IsDouble && count == 4) || (!IsDouble && count == 2))
            return true;
        return false;
            
    }
}
