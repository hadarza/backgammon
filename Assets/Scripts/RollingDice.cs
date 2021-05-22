using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RollingDice : MonoBehaviour
{
    [SerializeField]
    Dice[] dices;

    [SerializeField]
    Vector3[] startPos;
    Vector3 force;
    AudioSource audioSource;
    [SerializeField]
    AudioClip rollingDice;
    [SerializeField]
    Button RollBtn;
    [SerializeField]
    RawImage Instruction;
    GameManger gameManger;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gameManger = FindObjectOfType<GameManger>();
    }
    public void FadeInInstruction()
    {
        LeanTween.moveY(Instruction.gameObject, 900f, 1f);
    }
    public void OnRollingDice()
    {
        foreach (GameObject text in gameManger.textBlackWhite)
            text.SetActive(false);
        foreach (GameObject diceUI in gameManger.diceCountUI)
            diceUI.SetActive(false);
        foreach (Dice d in gameManger.dices)
            d.isDiceLand = false;
        // set start position for both dices;
        for (int indexDice = 0; indexDice < dices.Length; indexDice++){
            switch (GameManger.PlayerTurn){
                case null:
                    // rolling first dice to decide who start the game
                    dices[indexDice].transform.position = startPos[indexDice];
                    force = indexDice == 0 ? new Vector3(40, 0, 200) : new Vector3(40, 0, -200);
                    MoveDice(indexDice);
                    break;
                case "White":
                    dices[indexDice].transform.position = startPos[0]+ indexDice * new Vector3(5,0,0);
                    print(dices[indexDice].transform.position);
                    force = new Vector3(40, 0, 200);
                    MoveDice(indexDice);
                    break;
                case "Black":
                    dices[indexDice].transform.position = startPos[1] + indexDice * new Vector3(5, 0, 0);
                    print(dices[indexDice].transform.position);
                    force = new Vector3(40, 0, -200);
                    
                    MoveDice(indexDice);
                    break;
                default:
                    break;
            }            
        }
    }
    public void MoveDice(int indexDice)
    {
        string currentPlayer;
        dices[indexDice].GetComponent<Rigidbody>().velocity = new Vector3(0, 2, 0);
        dices[indexDice].GetComponent<Rigidbody>().AddForce(force);
        dices[indexDice].transform.Rotate(new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)));
        audioSource.PlayOneShot(rollingDice);
        LeanTween.moveY(Instruction.gameObject, 1300f, 1f);
        if (GameManger.PlayerTurn == null)
            RollBtn.gameObject.SetActive(false);
        else{
            currentPlayer = GameManger.PlayerTurn == "Black" ? "שחור" : "לבן";
            gameManger.UIcurrentPlayer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = " תור שחקן: שחקן " + currentPlayer;
            ShowUIcurrentPlayer();
        }
        foreach (GameObject diceUI in gameManger.diceCountUI)
            diceUI.SetActive(false);
    }
    public void ShowUIcurrentPlayer()
    {
        gameManger.UIcurrentPlayer.SetActive(true);
    }
}
