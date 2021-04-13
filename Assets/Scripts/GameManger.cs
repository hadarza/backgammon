using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public const int BOARD_TRIANGLES = 24;

    public static string PlayerTurn;
    public static Material newMaterialSelected;

    public Dice[] dices;

    public Triangle[] triangles;

    public List<Player> onPlayerBlack;
    public List<Player> onPlayerWhite;

    public static GameObject LastSelected = null;
    public Material NormalColor;
    public int canPut;
    public int canPut2;
    public int canPut3;
    public int canPut4;
    Player player;


    public bool CanMoveStonesOnBoard = true;

    public DiceManager SumMovements = new DiceManager(0,0);

    public static bool canRoll = true;

    public static bool moveByFirstDice = false;
    public static bool moveBySecondDice = false;
    public static bool moveByThirdDice = false;
    public static bool moveByFourthDice = false;
    public int indexCountMove = 0;

    public bool ReplaceOneByOne;
    int countMove;
    int countMove2;

    public enum Move { PlayTwoDice, PlayOneDice, NoPlayTurnPass}
    public Move move;

    private void Start()
    {
        move = Move.PlayTwoDice;
        canRoll = true;
        newMaterialSelected = new Material(source);
        BoardGame = new List<Stack<Player>>();
        print("start");
        PushStacksToBoard(); // push into an array a null stack. every stack will indicate a traingle in the board

        triangles = new Triangle[BOARD_TRIANGLES];
        Transform TriangleFatherTransform = GameObject.Find("Traingles").transform;
        for (int i = 0; i < TriangleFatherTransform.childCount; i++){
            triangles[i] = TriangleFatherTransform.GetChild(i).gameObject.GetComponent<Triangle>();

            if (triangles[i].gameObject.name.Length > 9) // for triangle12 for example - get 12 and not only 2
                triangles[i].TriangleIndex = int.Parse(triangles[i].gameObject.name.Substring(triangles[i].gameObject.name.Length - 2, 2));
            else
                triangles[i].TriangleIndex = int.Parse(triangles[i].gameObject.name.Substring(triangles[i].gameObject.name.Length - 1));
        }
        Vector3ArrayTrappedBlackStones = UpdateVector3ArrayTrappedStones(Vector3ArrayTrappedBlackStones);
        Vector3ArrayTrappedWhiteStones = UpdateVector3ArrayTrappedStones(Vector3ArrayTrappedWhiteStones);
    }

    public Vector3 []  UpdateVector3ArrayTrappedStones(Vector3 [] stones)
    {
        if(stones == Vector3ArrayTrappedBlackStones){
        for (int row = 1; row <= 3; row++){
            for (int OnTop = 1; OnTop <= 5; OnTop++)
                stones[OnTop + (5 * (row - 1)) - 1] = stones[0] + new Vector3(0, 0, -1.5f) * (OnTop - 1) + (row - 1) * new Vector3(0, 0.65f, 0);
        }
            return stones;
        }else{
            // Vector3ArrayTrappedWhiteStones
             for (int row = 1; row <= 3; row++){
                for (int OnTop = 1; OnTop <= 5; OnTop++)
                    stones[OnTop + (5 * (row - 1)) - 1] = stones[0] + new Vector3(0, 0, 1.5f) * (OnTop - 1) + (row - 1) * new Vector3(0, 0.65f, 0);
            }
            return stones;
        }
    }

    void PushStacksToBoard(){
        for(int boardIndex = 0; boardIndex < BOARD_TRIANGLES; boardIndex++){
            BoardGame.Add(new Stack<Player>());
        }
        print(BoardGame.Count);
        AddDefaultBoard();
    }
    /* pass on all 24 traingles at board and add into the BoardGame[traingleIndex] the players 
   that exist at the begining of the game on the board.
   (players are organize in a stack (Last in, first out - LIFO). */
    void AddDefaultBoard(){
        for (int boardIndex = 0; boardIndex < BOARD_TRIANGLES; boardIndex++){
            for (int playerIndex = 0; playerIndex < playerDefault.Length; playerIndex++) {
                /* if player index from playerDefault array is equal to board traingle index,
                than add to the stack the player*/
                if (playerDefault[playerIndex].indexTriangle - 1 == boardIndex){
                    BoardGame[boardIndex].Push(playerDefault[playerIndex]);
                }
            }
        }
    }

    public void OnStartGame()
    {
        // after finding who is the player that start the game , call this function
        foreach (GameObject objectToHide in objectIntro) { objectToHide.SetActive(false); }
        foreach (GameObject objectToShow in objectsForGame) { objectToShow.SetActive(true); }
    }
    public bool IsBothDicesLandAndRoll()
    {
      
        foreach(Dice d in dices) {
            if (!d.isDiceLand || d.diceCount == 0)
                return false;
        }
        return true;
    }
    public DiceManager UpdateCurrentDiceManager()
    {
        return new DiceManager(dices[0].diceCount, dices[1].diceCount);
    }
    public void ChangeColorToCurrentPlayer(Player p)
    {
        /* if the player wan to change his choise on selecting the player he want to move, make sure that 
       the last one that got selected , change it's color to normal */
        if (LastSelected != null){
            // if last selected is exist, than change to his matriel to normal. Otherwise, don't do anything
            LastSelected.GetComponent<Renderer>().material = NormalColor;
        }

        // on selecting a player, change the color.
        if (PlayerTurn == p.PlayerType){
            p.GetComponent<Renderer>().material = newMaterialSelected;
            LastSelected = p.gameObject;
        }
    }

    public int CheckCanPutThere(int x, string OppisteType)
    {
        // check if in this index on the board we have a null stack / same Type players / diff Type player (1 player)
        // If so, can move. Else, not.
        // If we have in the potential index traingle one Player Type diff, than also pass this diff player to array that respersent out of board (Eaten).
        // If we have player out of board,In our turn, check if player can get in to board.
        //If you have only one player out of board,have another move according to the second dice. Pass the turn if you don't have where to enter the player
        if (x <= BOARD_TRIANGLES && x > 0) {
            if (BoardGame[x - 1].Count != 0){
                // there is anything in stack
                if (BoardGame[x - 1].Peek().gameObject.GetComponent<Player>().PlayerType == OppisteType){
                    if (BoardGame[x - 1].Count != 1) {
                        // if there is more than one player that has oppoise Type to the current selected player
                        print("cant put in " + x);
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
    public void ShowWhereCanJumpTo(Player p)
    {
            if (PlayerTurn == p.PlayerType)
            {
                if (PlayerTurn == "Black")
                {
                    // Before move, check if we have something in "OnBlack" array 
                    if (onPlayerBlack.Count == 0)
                    {
                        // if it's my turn, Black and onPlayerBlack is empty, save where the optional index triangle is.
                        canPut = p.indexTriangle + dices[0].diceCount;
                        canPut2 = p.indexTriangle + dices[1].diceCount;

                        if(SumMovements.IsDouble){
                            canPut3 = canPut;
                            canPut4 = canPut;

                        if (moveByFirstDice)
                            canPut = -1;
                        else canPut = CheckCanPutThere(canPut, "White");
                        if (moveBySecondDice)
                            canPut2 = -1;
                        else canPut2 = CheckCanPutThere(canPut2, "White");
                         if (moveByThirdDice)
                            canPut3 = -1;
                        else canPut3 = CheckCanPutThere(canPut3, "White");
                        if (moveByFourthDice)
                            canPut4 = -1;
                        else canPut4 = CheckCanPutThere(canPut4, "White");
                    }
                    else{
                        canPut3 = 0;
                        canPut4 = 0;
                        if (moveByFirstDice)
                            canPut = -1;
                        else canPut = CheckCanPutThere(canPut, "White");
                        if (moveBySecondDice)
                            canPut2 = -1;
                        else canPut2 = CheckCanPutThere(canPut2, "White");
                    }

                    }
                }
                else
                {
                if (onPlayerWhite.Count == 0)
                {
                    canPut = p.indexTriangle - dices[0].diceCount;
                    canPut2 = p.indexTriangle - dices[1].diceCount;

                    if (SumMovements.IsDouble)
                    {
                        canPut3 = canPut;
                        canPut4 = canPut;
                        if (moveByFirstDice)
                            canPut = -1;
                        else canPut = CheckCanPutThere(canPut, "Black");
                        if (moveBySecondDice)
                            canPut2 = -1;
                        else canPut2 = CheckCanPutThere(canPut2, "Black");
                        if (moveByThirdDice)
                            canPut3 = -1;
                        else canPut3 = CheckCanPutThere(canPut3, "Black");
                        if (moveByFourthDice)
                            canPut4 = -1;
                        else canPut4 = CheckCanPutThere(canPut4, "Black");
                    }
                    else
                    {
                        canPut3 = 0;
                        canPut4 = 0;
                        if (moveByFirstDice)
                            canPut = -1;
                        else canPut = CheckCanPutThere(canPut, "Black");

                        if (moveBySecondDice)
                            canPut2 = -1;
                        else canPut2 = CheckCanPutThere(canPut2, "Black");
                    }
                }
            }
        }
        OnSelected.OnChosingMove -= ChangeColorToCurrentPlayer;
        OnSelected.OnChosingMove -= ShowWhereCanJumpTo;
        OnSelected.OnChosingMove -= ShowTriangleMovement;
    }
    public void ShowTriangleMovement(Player p)
    {
        // remove the triangles that got selected , so in the next time, the participent will click on specific player, it will remove the the triangles that were already appear (if exist)
        HideAllTriangles();
        foreach (Triangle triangle in triangles){
            if (canPut != -1 || canPut2 != -1 || canPut3 != -1 || canPut4 != -1){
                if (triangle.TriangleIndex == canPut || triangle.TriangleIndex == canPut2 || triangle.TriangleIndex == canPut3 || triangle.TriangleIndex == canPut4)
                    triangle.gameObject.SetActive(true);
            }
        }
    }

    public void HideAllTriangles()
    {
        foreach (Triangle triangle in triangles) {
            triangle.gameObject.SetActive(false);
        }
    }

    //pass turn to the next player
    public void PassTurn()
    {
        // change color to normal
        LastSelected.GetComponent<Renderer>().material = NormalColor;
        // change turn
        if (PlayerTurn == "Black")
            PlayerTurn = "White";
        else
            PlayerTurn = "Black";

        canRoll = true; //  be able to roll when the other participent finish his turn.
    }

    public void EnableChosingPlayer(Stack<Player> p)
    {
        // add OnSelected Script to players that only are in the last index in their stack + are in the color of PlayerTurn
      
            if (p.Count >= 1) {
            if (p.Peek().gameObject.GetComponent<Player>().PlayerType == PlayerTurn){
                Player FirstToPopInStack = p.Peek();
                if(!FirstToPopInStack.GetComponent<OnSelected>())
                    FirstToPopInStack.gameObject.AddComponent<OnSelected>();
                }
            }
        
    }

    public void DisableChosingPlayer()
    {
        foreach (Stack<Player> p in BoardGame){
            if (p.Count >= 1) {
                if (p.Peek().gameObject.GetComponent<Player>().PlayerType == PlayerTurn){
                    Player FirstToPopInStack = p.Peek();
                    Destroy(FirstToPopInStack.gameObject.GetComponent<OnSelected>());
                }
            }
        }
        PassTurn();
    }

    public void changeLocationsToTrappedStones(List <Player> Trapped)
    {
        int canPutInDice1, canPutInDice2;
        string oppoiste;
        foreach (Player trappedStone in Trapped)
        {
            if (trappedStone.PlayerType == "Black")
                oppoiste = "White";
            else
                oppoiste = "Black";
            if (trappedStone.PlayerType == "Black")
            {
                if (moveByFirstDice)
                    canPutInDice1 = -1;
                else canPutInDice1 = CheckCanPutThere(dices[0].diceCount, oppoiste);
                if (moveBySecondDice)
                    canPutInDice2 = -1;
                else
                    canPutInDice2 = CheckCanPutThere(dices[1].diceCount, oppoiste);
                
            }
            else
            {
                if (moveByFirstDice)
                    canPutInDice1 = -1;
                else canPutInDice1 = CheckCanPutThere(BOARD_TRIANGLES - dices[0].diceCount + 1, oppoiste);
                if (moveBySecondDice)
                    canPutInDice2 = -1;
                else canPutInDice2 = CheckCanPutThere(BOARD_TRIANGLES - dices[1].diceCount + 1, oppoiste);
            }
            if (canPutInDice1 != -1 && canPutInDice2 != -1){
                    triangles[canPutInDice1 - 1].gameObject.SetActive(true);
                    triangles[canPutInDice2 - 1].gameObject.SetActive(true);
            }
            else if (canPutInDice1 == -1 && canPutInDice2 != -1)
                    triangles[canPutInDice2 - 1].gameObject.SetActive(true);
            
            else if (canPutInDice1 != -1 && canPutInDice2 == -1) 
                    triangles[canPutInDice1 - 1].gameObject.SetActive(true);
            else{
                panelTurnpass.gameObject.SetActive(true);
                // show a message on display to tell the player that the turn pass
                PassTurn();
            }
        }
    }

    public bool IsPlayerFoundOnTrapped(List<Player> TrappedList,Player player)
    {
        foreach (Player p in TrappedList)
        {
            if (p == player)
                return true;
        }
        return false;
    }

     public bool ThereIsOptionalMove(){
        foreach (Stack<Player> p in BoardGame){
            if (p.Count > 0){
                if (CheckCanPutThere(PlayerTurn == "Black" ? (p.Peek().indexTriangle + dices[0].diceCount) : (p.Peek().indexTriangle - dices[0].diceCount), PlayerTurn == "Black" ? "White" : "Black") != -1
                || CheckCanPutThere(PlayerTurn == "Black" ? (p.Peek().indexTriangle + dices[1].diceCount) : (p.Peek().indexTriangle - dices[1].diceCount), PlayerTurn == "Black" ? "White" : "Black") != -1){
                    return true;
                }
            }
        }
        // can't do any moves - pass turn
        return false;
    }

    //public int CountPossibleOptionsToDoHighestMove()
    //{
    //    foreach(Stack<Player> s in BoardGame)
    //    {
    //        if (PlayerTurn == "Black"){
    //            if (CheckCanPutThere(s.Peek().indexTriangle + highestDice, "White") != -1)
    //                countPossibleForHighestDice++;
    //        }
    //        else{
    //            if (CheckCanPutThere(s.Peek().indexTriangle - highestDice, "Black") != -1)
    //                countPossibleForHighestDice++;
    //        }
    //    }
    //    return countPossibleForHighestDice;
    //}

    public bool isAllPlayersCanRemoved(List <Stack<Player>> playerList, string PlayerType)
    {
        if (PlayerType == "Black"){
            foreach (Stack<Player> s in playerList) {
                foreach (Player p in s){
                    if (p.indexTriangle > 7 && p.PlayerType == PlayerType)
                        return false;
                }
            }
            return true;
        }
        else{
            foreach (Stack<Player> s in playerList){
                foreach (Player p in s){
                    if (p.indexTriangle < 19 && p.PlayerType == PlayerType)
                        return false;
                }
            }
            return true;
        }
    }
}
