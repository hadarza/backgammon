﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManger : MonoBehaviour
{
    public List<Stack<Player>> BoardGame;
    [SerializeField] Player[] playerDefault;
    [SerializeField] Material source;

    [SerializeField] GameObject[] objectIntro;
    [SerializeField] GameObject[] objectsForGame;
    public GameObject panelTurnpass;
    public Vector3[] Vector3ArrayTrappedBlackStones;
    public Vector3[] Vector3ArrayTrappedWhiteStones;

    public Vector3[] Vector3TakeOutBlackStones;
    public Vector3[] Vector3TakeOutWhiteStones;


    public List<Player> BlackStonesTakeOut;
    public List<Player> WhiteStonesTakeOut;
    public GameObject[] RectanglesShowTakeOut;
    public const int BOARD_TRIANGLES = 24;

    public static string PlayerTurn = null;
    public static Material newMaterialSelected;

    public Dice[] dices;

    public Triangle[] triangles;

    public List<Player> onPlayerBlack;
    public List<Player> onPlayerWhite;

    public static GameObject LastSelected = null;
    public Material NormalColor;

    public int[] IndexPutAccordingToDice = { 0, 0, 0, 0 };

    public bool CanMoveStonesOnBoard = true;

    public DiceManager SumMovements;

    public static bool canRoll = true;

    public bool[] DoneMove = { false, false, false, false };

    public int indexCountMove = 0;

    public bool ReplaceOneByOne;
    int countPossibleForHighestDice;
    public int[] dicesCount;

    public enum Move { PlayTwoDice, PlayOneDice, NoPlayTurnPass }
    public Move move;

    public bool RollFirstTime = false;
    public bool RollForPlayerStartGame = false;

    public GameObject[] diceCountUI;
    public Sprite[] diceSides;
    public GameObject UIcurrentPlayer;

    public GameObject[] textBlackWhite;
    public Player currentObjectPossibleToMovement;

    public Canvas CanvasLoading;

    private void Awake()
    {
        BoardGame = new List<Stack<Player>>();
    }
    private void Start()
    {
        diceSides = Resources.LoadAll<Sprite>("DiceSides/");
        move = Move.PlayTwoDice;
        canRoll = true;
        newMaterialSelected = new Material(source);
        PushStacksToBoard(); // push into an array a null stack. every stack will indicate a traingle in the board

        triangles = new Triangle[BOARD_TRIANGLES];
        Transform TriangleFatherTransform = GameObject.Find("Traingles").transform;
        for (int i = 0; i < TriangleFatherTransform.childCount; i++)
        {
            triangles[i] = TriangleFatherTransform.GetChild(i).gameObject.GetComponent<Triangle>();

            if (triangles[i].gameObject.name.Length > 9) // for triangle12 for example - get 12 
                triangles[i].TriangleIndex = int.Parse(triangles[i].gameObject.name.Substring(triangles[i].gameObject.name.Length - 2, 2));
            else
                triangles[i].TriangleIndex = int.Parse(triangles[i].gameObject.name.Substring(triangles[i].gameObject.name.Length - 1));
        }
        Vector3TakeOutBlackStones = UpdateVector3TakeOutStones(Vector3TakeOutBlackStones);
        Vector3TakeOutWhiteStones = UpdateVector3TakeOutStones(Vector3TakeOutWhiteStones);
    }


    public void DidRollForPlayerStart()
    {
        RollForPlayerStartGame = true;
    }

    public Vector3[] UpdateVector3TakeOutStones(Vector3[] stones)
    {
        for(int i=1;i<15;i++)
        {
            stones[i] = stones[0] - new Vector3(0, 0, 0.2f) * i;
        }
        return stones;
    }
    // push to board the default stones at the begining of the game
    void PushStacksToBoard()
    {
        for (int boardIndex = 0; boardIndex < BOARD_TRIANGLES; boardIndex++)
        {
            BoardGame.Add(new Stack<Player>());
        }
        AddDefaultBoard();
    }

    /* pass on all 24 traingles at board and add into the BoardGame[traingleIndex] the players 
   that exist at the begining of the game on the board.
   (players are organize in a stack (Last in, first out - LIFO). */
    void AddDefaultBoard()
    {
        for (int boardIndex = 0; boardIndex < BOARD_TRIANGLES; boardIndex++)
        {
            for (int playerIndex = 0; playerIndex < playerDefault.Length; playerIndex++)
            {
                /* if player index from playerDefault array is equal to board traingle index,
                than add to the stack the player*/
                if (playerDefault[playerIndex].indexTriangle - 1 == boardIndex)
                {
                    BoardGame[boardIndex].Push(playerDefault[playerIndex]);
                }
            }
        }
    }

    //the function help to show and hide objects at starting game
    public void OnStartGame()
    {
        // after finding who is the player that start the game , call this function
        foreach (GameObject objectToHide in objectIntro) { objectToHide.SetActive(false); }
        foreach (GameObject objectToShow in objectsForGame) { objectToShow.SetActive(true); }
    }

    // get the player who start the game
    public int GetPlayerStartGame(int diceCount1, int diceCount2)
    {
        if (diceCount1 < diceCount2)
            return 2; // second player start game
        else if (diceCount1 > diceCount2)
            return 1; // first player start game
        else return 0; // not decided - Tie. need to roll again.
    }

    // the function return true if both dices are roll and land on the board, else return false
    public bool IsBothDicesLandAndRoll()
    {

        foreach (Dice d in dices)
        {
            if (!d.isDiceLand)
                return false;
        }
        return true;
    }

    // function update DiceManager object
    public DiceManager UpdateCurrentDiceManager()
    {
        SumMovements.firstDice = dices[0].diceCount;
        SumMovements.secondDice = dices[1].diceCount;
        SumMovements.IsDouble = SumMovements.firstDice == SumMovements.secondDice ? SumMovements.IsDouble = true : SumMovements.IsDouble = false;

        return SumMovements;
   }

    public void ChangeColorToCurrentPlayer(Player p)
    {
        /* if the player wan to change his choise on selecting the player he want to move, make sure that 
       the last one that got selected , change it's color to normal */
        if (LastSelected != null)
        {
            // if last selected is exist, than change to his matriel to normal. Otherwise, don't do anything
            LastSelected.GetComponent<Renderer>().material = NormalColor;
        }

        // on selecting a player, change the color.
        if (PlayerTurn == p.PlayerType)
        {
            p.GetComponent<Renderer>().material = newMaterialSelected;
            LastSelected = p.gameObject;
        }
    }

    // check if in this index on the board we have a null stack / same Type players / diff Type player (1 player)
    // If so, can move. Else, not.
    // If we have in the potential index traingle one Player Type diff, than also pass this diff player to array that respersent out of board (Eaten).
    // If we have player out of board,In our turn, check if player can get in to board.
    //If you have only one player out of board,have another move according to the second dice. Pass the turn if you don't have where to enter the player
    public int CheckCanPutThere(int x, string OppisteType){
        // if index isn't out of board
        if (x <= BOARD_TRIANGLES && x > 0){
            // there is anything in stack
            if (BoardGame[x - 1].Count != 0){
                if (BoardGame[x - 1].Peek().gameObject.GetComponent<Player>().PlayerType == OppisteType){
                    if (BoardGame[x - 1].Count != 1){
                        // if there is more than one player that has oppoise Type to the current selected player
                        x = -1;
                    }
                }
            }
            return x;
        }
        // return -1 when it's out of index board
        return -1;
    }
    // show the player where can he jump to.
    public void ShowWhereCanJumpTo(Player p){
        if (PlayerTurn == p.PlayerType){
            if (PlayerTurn == "Black"){
                // Before move, check if we have something in "OnBlack" array 
                if (onPlayerBlack.Count == 0){
                    // if it's my turn, Black and onPlayerBlack is empty, save where the optional index triangle is.
                    IndexPutAccordingToDice[0] = p.indexTriangle + dices[0].diceCount;
                    IndexPutAccordingToDice[1] = p.indexTriangle + dices[1].diceCount;
                    CheckIfDidMoveByDices("White");
                }
            }else{
                if (onPlayerWhite.Count == 0){
                    IndexPutAccordingToDice[0] = p.indexTriangle - dices[0].diceCount;
                    IndexPutAccordingToDice[1] = p.indexTriangle - dices[1].diceCount;
                    CheckIfDidMoveByDices("Black");
                }
            }
        }
        OnSelected.OnChosingMove -= ChangeColorToCurrentPlayer;
        OnSelected.OnChosingMove -= ShowWhereCanJumpTo;
        OnSelected.OnChosingMove -= ShowTriangleMovement;
    }

    public void CheckIfDidMoveByDices(string TypeOppoistePlayer)
    {
        if (SumMovements.IsDouble){
            for (int i = 2; i <= 3; i++)
                IndexPutAccordingToDice[i] = IndexPutAccordingToDice[0];

            for (int i = 0; i < 4; i++){
                if (DoneMove[i])
                    IndexPutAccordingToDice[i] = -1;
                else
                    IndexPutAccordingToDice[i] = CheckCanPutThere(IndexPutAccordingToDice[i], TypeOppoistePlayer);
            }
        }
        else{
            // not double - no need in IndexPutAccordingToDice[2] + IndexPutAccordingToDice[3]
            for (int i = 2; i <= 3; i++)
                IndexPutAccordingToDice[i] = 0;

            for (int i = 0; i < 2; i++){
                if (DoneMove[i])
                    IndexPutAccordingToDice[i] = -1;
                else
                    IndexPutAccordingToDice[i] = CheckCanPutThere(IndexPutAccordingToDice[i], TypeOppoistePlayer);
            }
        }
    }

    public void ShowTriangleMovement(Player p){
        // remove the triangles that got selected , so in the next time, the participent will click on specific player, it will remove the the triangles that were already appear (if exist)
        HideAllTriangles();
        ShowAvaliableTriangleIndex();
    }
    public void ShowAvaliableTriangleIndex(){
        for (int i = 0; i < IndexPutAccordingToDice.Length; i++)
            if (IndexPutAccordingToDice[i] != -1 && IndexPutAccordingToDice[i] != 0){
                foreach (Triangle triangle in triangles){
                    if (triangle.TriangleIndex == IndexPutAccordingToDice[i])
                        triangle.gameObject.SetActive(true);
                }
            }
    }
    public void HideAllTriangles(){
        foreach (Triangle triangle in triangles)
            triangle.gameObject.SetActive(false);
    }

    //pass turn to the next player
    public void PassTurn(){
        // change color to normal
        if(LastSelected != null)
            LastSelected.GetComponent<Renderer>().material = NormalColor;
        // change turn
        if (PlayerTurn == "Black")
            PlayerTurn = "White";
        else
            PlayerTurn = "Black";

        canRoll = true; //  be able to roll when the other participent finish his turn.
        DiceManager.NotCheckForThisTurn = true;
        //make this so it won't be able to click on the other Type player and add OnSelected before dices rolled again.
        if (IsBothDicesLandAndRoll())
        {
            foreach (Dice d in dices) { 
                d.diceCount = 0;
            }
        }
        print("do something");

        foreach (Stack<Player> p in BoardGame)
        {
            if (p.Count > 0)
            {
                if (p.Peek().GetComponent<OnSelected>())
                    Destroy(p.Peek().GetComponent<OnSelected>());
            }
        }
        // remove Onselected from all stones
        foreach(Player p in onPlayerBlack){
            if (p.GetComponent<OnSelected>())
                Destroy(p.GetComponent<OnSelected>());
        }
        foreach (Player p in onPlayerWhite)
        {
            if (p.GetComponent<OnSelected>())
                Destroy(p.GetComponent<OnSelected>());
        }
    }

    public void updateAfterFinishTurn()
    {
        indexCountMove++;
        // if the particpent move according to first/ second dice
        HideAllTriangles();
        DisableChosingPlayer();
    }

    // add OnSelected Script to players that only are in the last index in their stack + are in the color of PlayerTurn
    public void EnableChosingPlayer(Stack<Player> p)
    {
        if (p.Count >= 1){
            if (p.Peek().gameObject.GetComponent<Player>().PlayerType == PlayerTurn){
                Player FirstToPopInStack = p.Peek();
                if (!FirstToPopInStack.GetComponent<OnSelected>())
                    FirstToPopInStack.gameObject.AddComponent<OnSelected>();
            }
        }
    }

    public void DisableChosingPlayer()
    {
        foreach (Stack<Player> p in BoardGame)
        {
            if (p.Count >= 1){
                if (p.Peek().gameObject.GetComponent<Player>().PlayerType == PlayerTurn)
                {
                    Player FirstToPopInStack = p.Peek();
                    Destroy(FirstToPopInStack.gameObject.GetComponent<OnSelected>());
                }
            }
        }
        PassTurn();
    }

    public void changeLocationsToTrappedStones(string PlayerTrapped)
    {
        int canPutInDice1, canPutInDice2;
        string oppoiste;

        oppoiste = PlayerTrapped == "Black" ? "White" : "Black";

        if (PlayerTrapped == "Black")
        {
            if (DoneMove[0])
                canPutInDice1 = -1;
            else canPutInDice1 = CheckCanPutThere(dices[0].diceCount, oppoiste);
            if (DoneMove[1])
                canPutInDice2 = -1;
            else
                canPutInDice2 = CheckCanPutThere(dices[1].diceCount, oppoiste);
        }
        else
        {
            if (DoneMove[0])
                canPutInDice1 = -1;
            else canPutInDice1 = CheckCanPutThere(BOARD_TRIANGLES - dices[0].diceCount + 1, oppoiste);
            if (DoneMove[1])
                canPutInDice2 = -1;
            else canPutInDice2 = CheckCanPutThere(BOARD_TRIANGLES - dices[1].diceCount + 1, oppoiste);
        }
        if (canPutInDice1 != -1 && canPutInDice2 != -1)
        {
            triangles[canPutInDice1 - 1].gameObject.SetActive(true);
            triangles[canPutInDice2 - 1].gameObject.SetActive(true);
        }
        else if (canPutInDice1 == -1 && canPutInDice2 != -1)
            triangles[canPutInDice2 - 1].gameObject.SetActive(true);

        else if (canPutInDice1 != -1 && canPutInDice2 == -1)
            triangles[canPutInDice1 - 1].gameObject.SetActive(true);
        else
        {
            panelTurnpass.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "אין לך היכן להניח את האבנים הכלואות ולכן התור עובר ליריב";
            panelTurnpass.gameObject.SetActive(true);
            // show a message on display to tell the player that the turn pass
            PassTurn();

        }
    }
    // search on TrappedList , if a stone is found. If so, return true. else, return false.
    public bool IsPlayerFoundOnTrapped(List<Player> TrappedList, Player player)
    {
        foreach (Player p in TrappedList)
        {
            if (p == player)
                return true;
        }
        return false;
    }

    //This function return true if there is an optional move on all board from TypeTurn, false if not.
    public bool ThereIsOptionalMove(){
        bool ThereIsOption = false;
        for (int indexBuffer = 0; indexBuffer < dicesCount.Length; indexBuffer++){
            if (dicesCount[indexBuffer] != 0){
                ThereIsOption =  ThereIsOptionalMoveByDice(indexBuffer);
                // if there at least one optional - return true
                if (ThereIsOption)
                    return true;
            }
        }
        return ThereIsOption;
    }

    public bool ThereIsOptionalMoveByDice(int indexBuffer)
    {
        if (!DoneMove[indexBuffer])
        {
            foreach (Stack<Player> s in BoardGame){
                if (s.Count > 0){
                    if (s.Peek().PlayerType == PlayerTurn)
                    {
                        int index = PlayerTurn == "Black" ? s.Peek().indexTriangle + dicesCount[indexBuffer] : s.Peek().indexTriangle - dicesCount[indexBuffer];
                        string opposite = PlayerTurn == "Black" ? "White" : "Black";
                        if (CheckCanPutThere(index, opposite) != -1)
                            return true;
                    }
                }
            }
        }
    return false;
    }

    public int CountPossibleOptionsToDoHighestMove()
    {
        countPossibleForHighestDice = 0;
        int highestDice = GetHighDice();
        foreach (Stack<Player> s in BoardGame){
            if (s.Count > 0){
                if (s.Peek().PlayerType == PlayerTurn){
                    if (PlayerTurn == "Black"){
                        if (CheckCanPutThere(s.Peek().indexTriangle + highestDice, "White") != -1){
                            currentObjectPossibleToMovement = s.Peek();
                            countPossibleForHighestDice++;
                        }
                    }else{
                        if (CheckCanPutThere(s.Peek().indexTriangle - highestDice, "Black") != -1){
                            currentObjectPossibleToMovement = s.Peek();
                            countPossibleForHighestDice++;
                        }
                    }
                }
            }
        }
        if (countPossibleForHighestDice != 1)
            currentObjectPossibleToMovement = null; // save player when there is only option for movemnt for the higher dice. for checking if the stone the participent click on is the possible one.
        return countPossibleForHighestDice;
    }

    //This function return the higher dice count from 2 dices
    public int GetHighDice(){
       return Mathf.Max(dices[0].diceCount, dices[1].diceCount);

    }

    // This function return true if all stones from PlayerTurn can be removed , false if not.
    public bool isAllPlayersCanRemoved(List<Stack<Player>> playerList, string PlayerType)
    {
        if (PlayerType == "White"){
            foreach (Stack<Player> s in playerList){
                if (s.Count > 0){
                    if (s.Peek().indexTriangle > 6 && s.Peek().PlayerType == PlayerType)
                        return false;
                }
            }
            return true;
        }else{
            foreach (Stack<Player> s in playerList){
                if (s.Count > 0){
                    if (s.Peek().indexTriangle < 19 && s.Peek().PlayerType == PlayerType)
                        return false;
                }
            }
            return true;
        }
    }

    public void UpdateBufferOfDices()
    {
        for (int i = 0; i < dices.Length; i++)
            dicesCount[i] = dices[i].diceCount;
            for(int i = 2; i < 4; i++) {
            if (SumMovements.IsDouble)
                dicesCount[i] = dicesCount[0];
            else
                dicesCount[i] = 0;
        }
    }
}
