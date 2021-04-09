using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    public int firstDice;
    public int secondDice;
    public bool IsDouble;

    public DiceManager(int firstDice,int secondDice)
    {
        this.firstDice = firstDice;
        this.secondDice = secondDice;
        if(firstDice == secondDice) {
            IsDouble = true;
        }
    }

}
