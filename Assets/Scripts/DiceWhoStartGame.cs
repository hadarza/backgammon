using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceWhoStartGame : MonoBehaviour
{

    // Array of dice sides sprites to load from Resources folder
    private Sprite[] diceSides;

    // Reference to sprite renderer to change sprites
    private Image rend;

    Ray ray;
    RaycastHit hit;
    UITextTypeWriter textType;

    static bool IsTie;
    public static bool WasTie; // change to true at tie. not change. Help to change string at the second tie "שוב תיקו.."
    public static bool Player1Roll = true;  /* make it static , so no matter if the component is remove it won't reset to true 
                                            every time and will get change to false just once */
    public static int[] Rollresult = new int[2];

    static bool DidntFinishRolling = false;

    GameObject showStartPlayer;
    private void Start(){
        textType = FindObjectOfType<UITextTypeWriter>();

        // get from textType to PlayerStarter object , can't do with Find cause PlayerStarter is inactive.
        showStartPlayer = textType.transform.parent.GetChild(3).gameObject;
        // Assign Renderer component
        rend = GetComponent<Image>();

        // Load dice sides sprites to array from DiceSides subfolder of Resources folder
        diceSides = Resources.LoadAll<Sprite>("DiceSides/");
        gameObject.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(CallCor);
    }

    public void CallCor(){
        StartCoroutine("RollTheDice");
    }

    // If you left click over the dice then RollTheDice coroutine is started

    // Coroutine that rolls the dice
    private IEnumerator RollTheDice()
    {
        // indicate that the dice is rolled and can't be rolled again until it will finish this current rolling.
        DidntFinishRolling = true;

        // Variable to contain random dice side number.
        // It needs to be assigned. Let it be 0 initially
        int randomDiceSide = 0;

        // Final side or value that dice reads in the end of coroutine
        int finalSide = 0;

        // Loop to switch dice sides ramdomly
        // before final side appears. 20 itterations here.
        for (int i = 0; i <= 20; i++)
        {
            // Pick up random value from 0 to 5 (All inclusive)
            randomDiceSide = Random.Range(0, 6);

            // Set sprite to upper face of dice from array according to random value
            rend.sprite = diceSides[randomDiceSide];

            // Pause before next itteration
            yield return new WaitForSeconds(0.05f);
        }

        // Assigning final side so you can compare the results of first player vs second player.
        finalSide = randomDiceSide + 1;

        // Show final dice value in Console
        Debug.Log("dice "+finalSide);
        DidntFinishRolling = false;
        if (Player1Roll){
            if (IsTie){
                // after a tie , change from player 1 roll.. to second player roll..
                IsTie = false;
                textType.OnPlayerRoll(20);
            }
            else{
                // change for player 1 roll.. to player 2 roll..
                textType.OnPlayerRoll(34);
            }
            Player1Roll = false;

            // update dice result for first player.
            Rollresult[0] = finalSide;
        }
        else{
            // update dice result for second player.
            Rollresult[1] = finalSide;

            if (Rollresult[1] > Rollresult[0]){
                // show player 2 start the game
                showStartPlayer.SetActive(true);
                showStartPlayer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "שחקן 2 מתחיל!";

                GameManger.PlayerTurn = "Black"; //Black - Player 2

                // remove the option to roll the dice again.
                Destroy(this);

            }else if (Rollresult[0] > Rollresult[1]) {
                // show player 1 start the game
                showStartPlayer.SetActive(true);
                showStartPlayer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "שחקן 1 מתחיל!";
                GameManger.PlayerTurn = "White"; //white - Player 1
                // remove the option to roll the dice again.
                Destroy(this);               
            }
            else
            {
                // at a tie
                print("NOT DECIDED.");
                // both need to roll again.
                if (!WasTie)
                {
                    textType.OnPlayerRoll(40);
                    WasTie = true;
                }
                else
                {
                    textType.OnPlayerRoll(79);
                }
                Player1Roll = true;
                IsTie = true;
            }
        }
    }
}
