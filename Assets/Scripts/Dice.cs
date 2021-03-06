using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{
	#region Public Variables
	// Orignal Dice
	public GameObject[] orignalDice;
	//dice resultant number..
	public int diceCount;
	//Can Throw Dice
	public bool isDiceLand = true;
    public int indexDice;

    #endregion
    #region Private Variables
    Ray ray;
	RaycastHit hit;
    #endregion
	GameManger gameManager;
    string TextToShow;
    #region serializeField Variables

    [SerializeField]
    Button RollBtn;
    [SerializeField]
    RawImage Instruction;
    
    #endregion
    void Start()
	{
		GetComponent<Rigidbody>().solverIterations = 250;
        gameManager = FindObjectOfType<GameManger>();

    }
    public void Update()
    {
        if(GameManger.PlayerTurn != null) {
            if (isDiceLand && GameManger.canRoll){
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit)){
                    if (Input.GetMouseButtonDown(0)) {
                        if (hit.collider.name == "dice" || hit.collider.name == "dice2")
                        {
                            foreach (GameObject text in gameManager.textBlackWhite)
                                text.SetActive(false);
                            foreach (GameObject diceUI in gameManager.diceCountUI)
                                diceUI.SetActive(false);
                            gameManager.indexCountMove = 0; //resart countMovemnts 

                            // restart at rolling the dices again.
                            for (int i = 0; i < 4; i++)
                                gameManager.IndexPutAccordingToDice[i] = 0;

                            // make 4 steps to be able to move
                            for (int i = 0; i < gameManager.DoneMove.Length; i++)
                                gameManager.DoneMove[i] = false;

                            gameManager.RollFirstTime = true;
                            isDiceLand = false;
                            GameManger.canRoll = false;
                            gameManager.SumMovements.gameObject.GetComponent<RollingDice>().OnRollingDice();
                        }
                    }
                }
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        // if dice collide with the boardGame
     //  if (collision.gameObject.tag == "Board" || collision.gameObject.tag == "Stone")
       //     StartCoroutine(Wait4sec());
    }

    public void OnCollisionStay(Collision collision)
    {
        if (gameManager.RollForPlayerStartGame){
                StartCoroutine(Wait4sec());

        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3f);
        int startGame = gameManager.GetPlayerStartGame(gameManager.dices[0].diceCount, gameManager.dices[1].diceCount);
        if (GameManger.PlayerTurn == null)
        {
            switch (startGame)
            {
                case 0:
                    startGame = -1;
                    TextToShow = "תיקו! הגרל קובייה מחדש";
                    RollBtn.gameObject.SetActive(true);

                    break;
                case 1:
                    startGame = -1;
                    GameManger.PlayerTurn = "Black";
                    TextToShow = "שחקן 1 - שחור מתחיל את המשחק, הגרל קוביה";
                    foreach (Dice d in gameManager.dices)
                        d.isDiceLand = true;
                    RollBtn.gameObject.SetActive(false);
                    
                    break;
                case 2:
                    startGame = -1;
                    GameManger.PlayerTurn = "White";
                    TextToShow = "שחקן 2 - לבן מתחיל את המשחק, הגרל קובייה";
                    foreach (Dice d in gameManager.dices)
                        d.isDiceLand = true;
                    RollBtn.gameObject.SetActive(false);
                   
                    break;
            }
            Instruction.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = TextToShow;
            LeanTween.moveY(Instruction.gameObject, 900f, 1f);
            foreach (GameObject text in gameManager.textBlackWhite)
                text.SetActive(true);
        }
        ShowVisibleDiceUI();
        
        if(GameManger.PlayerTurn != null) {
            if (gameManager.IsBothDicesLandAndRoll()){
                if (!gameManager.SumMovements.IsPlayerDidAllSteps()){
                    // after rolling check if there is an option to move a stone, while we don't have anything onPlayerTrapped Array.
                    if (gameManager.CantMove()){
                        if(!gameManager.isAllPlayersCanRemoved(gameManager.BoardGame, GameManger.PlayerTurn))
                            gameManager.ShowErrorPassTurn("אין ביכולתך להזיז אבנים לפי הקוביות הנתונות ולכן התור עובר ליריב");
                        else{
                            int[] indexDices = { 0, 0, 0, 0 };
                            int index = 0;
                            foreach (int diceCount in gameManager.dicesCount){
                                if (diceCount > 0){
                                    indexDices[index] = gameManager.GetIndexCountOnRemovingStones(GameManger.PlayerTurn, index);
                                    if (gameManager.BoardGame[indexDices[index]].Count > 0){
                                        if (gameManager.BoardGame[indexDices[index]].Peek().PlayerType == GameManger.PlayerTurn){
                                            gameManager.ShowMessagePassTurn = false;
                                            break;
                                        }else{
                                            if (OnSelected.SelectedPlayer != null)
                                                gameManager.TakeCareOnNotOnStackAsDice(indexDice, OnSelected.SelectedPlayer.PlayerType, OnSelected.SelectedPlayer);
                                            if (gameManager.RectanglesShowTakeOut[0].activeInHierarchy || gameManager.RectanglesShowTakeOut[1].activeInHierarchy)
                                                break;
                                        }
                                    }else{
                                        if(OnSelected.SelectedPlayer !=null)
                                            gameManager.TakeCareOnNotOnStackAsDice(indexDice, OnSelected.SelectedPlayer.PlayerType, OnSelected.SelectedPlayer);
                                        if (gameManager.RectanglesShowTakeOut[0].activeInHierarchy || gameManager.RectanglesShowTakeOut[1].activeInHierarchy)
                                            break;
                                    }
                                }
                                index++;
                            }
                            if (gameManager.ShowMessagePassTurn)
                                gameManager.ShowErrorPassTurn("אין ביכולתך להזיז אף אבן ולכן התור עובר ליריב");
                        }
                    }
                }
            }
        }
    }

    // show visible UI for finding dice 
    public void ShowVisibleDiceUI()
    {
        if (gameManager.dices[indexDice].diceCount >= 1){
            gameManager.diceCountUI[indexDice].GetComponent<Image>().sprite = gameManager.diceSides[gameManager.dices[indexDice].diceCount - 1];
            foreach (GameObject diceUI in gameManager.diceCountUI)
                diceUI.SetActive(true);
        }
    }


    IEnumerator Wait4sec()
    {
        yield return new WaitForSeconds(4f);

        diceCount = 0;
        this.isDiceLand = true;
        // get dice count according to the angle of the vectors
        diceCount = GetDiceCount();
        for (int i = 0; i < orignalDice.Length; i++)
            //	orignalDice[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            gameManager.SumMovements = gameManager.UpdateCurrentDiceManager();
        gameManager.UpdateBufferOfDices();
        StartCoroutine(Wait());
        // check if there is an optional move, If not, show a message and pass turn
        //  if (!gameManager.ThereIsOptionalMove())
        //    gameManager.panelTurnpass.SetActive(true);
        //switch (gameManager.ThereIsOptionalMove())

        //{
        //    case GameManger.Move.NoPlayTurnPass:
        //        gameManager.highestDice = 0;
        //        gameManager.panelTurnpass.gameObject.SetActive(true);
        //        // show a message on display to tell the player that the turn pass
        //        gameManager.PassTurn();
        //        break;
        //    case GameManger.Move.PlayOneDice:
        //        // check posibilty to step by the highest dice
        //        gameManager.highestDice = Mathf.Max(gameManager.dices[0].diceCount, gameManager.dices[1].diceCount);
        //        break;
        //    case GameManger.Move.PlayTwoDice:
        //        gameManager.highestDice = 0;
        //        break;
        //}
    }

    

    // find dice count according the angle between 2 vectors 
    public int GetDiceCount(){
		if (Vector3.Dot(transform.forward, Vector3.up) > 0.6f)
			diceCount = 2;
		else if (Vector3.Dot(-transform.forward, Vector3.up) > 0.6f)
			diceCount = 4;
		else if (Vector3.Dot(transform.up, Vector3.up) > 0.6f)
			diceCount = 3;
		else if (Vector3.Dot(-transform.up, Vector3.up) > 0.6f)
			diceCount = 1;
		else if (Vector3.Dot(transform.right, Vector3.up) > 0.6f)
			diceCount = 5;
		else if (Vector3.Dot(-transform.right, Vector3.up) > 0.6f)
			diceCount = 6;
		return diceCount;
	}
}
