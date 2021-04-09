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

    public DiceManager SumMovements = new DiceManager(0,0);

    public static bool canRoll = true;

    public static bool moveByFirstDice = false;
    public static bool moveBySecondDice = false;
    public int indexCountMove = 0;

    public bool ReplaceOneByOne;

    private void Start()
    {
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
        for (int row = 1; row <= 3; row++)
        {
            for (int OnTop = 1; OnTop <= 5; OnTop++)
            {
                print(OnTop + (row - 1) * 5 - 1);
                stones[OnTop + (5 * (row - 1)) - 1] = stones[0] + new Vector3(0, 0, -1.5f) * (OnTop - 1) + (row - 1) * new Vector3(0, 0.65f, 0);
            }
        }
        return stones;
    }

    void PushStacksToBoard(){
        for(int boardIndex = 0; boardIndex < BOARD_TRIANGLES; boardIndex++){
            BoardGame.Add(new Stack<Player>());
        }
        print(BoardGame.Count);
        AddDefaultBoard();
    }

    void AddDefaultBoard(){
        /* pass on all 24 traingles at board and add into the BoardGame[traingleIndex] the players 
        that exist at the begining of the game on the board.
        (players are organize in a stack (Last in, first out - LIFO). */
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
        if (PlayerTurn == p.PlayerType){
            if (PlayerTurn == "Black")
            {
                // Before move, check if we have something in "OnBlack" array 
                if (onPlayerBlack.Count == 0)
                {
                    // if it's my turn, Black and onPlayerBlack is empty, save where the optional index triangle is.
                    canPut = p.indexTriangle + dices[0].diceCount;
                    canPut2 = p.indexTriangle + dices[1].diceCount;

                    if (moveByFirstDice)
                        canPut = -1;
                    else canPut = CheckCanPutThere(canPut, "White");
                    if (moveBySecondDice)
                        canPut2 = -1;
                    else canPut2 = CheckCanPutThere(canPut2, "White");

                }
            }
            else{
                if (onPlayerWhite.Count == 0){
                    canPut = p.indexTriangle - dices[0].diceCount;
                    canPut2 = p.indexTriangle - dices[1].diceCount;
                    if (moveByFirstDice)
                        // play this code on 2 options - at indexCountMove 4 + in double and not in double
                        canPut = -1;
                    else canPut = CheckCanPutThere(canPut, "Black");

                    if (moveBySecondDice)
                        canPut2 = -1;
                    else canPut2 = CheckCanPutThere(canPut2, "Black");
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
            if (canPut != -1 || canPut2 != -1){
                if (triangle.TriangleIndex == canPut || triangle.TriangleIndex == canPut2){
                    triangle.gameObject.SetActive(true);
                }
            }
        }
    }

    public void HideAllTriangles()
    {
        foreach (Triangle triangle in triangles) {
            triangle.gameObject.SetActive(false);
        }
    }

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

    public void EnableChosingPlayer()
    {
        // add OnSelected Script to players that only are in the last index in their stack + are in the color of PlayerTurn
        foreach (Stack<Player> p in BoardGame)
        {
            if (p.Count >= 1)
            {
                if (p.Peek().gameObject.GetComponent<Player>().PlayerType == PlayerTurn)
                {
                    Player FirstToPopInStack = p.Peek();
                    print("1");
                    FirstToPopInStack.gameObject.AddComponent<OnSelected>();
                }
            }
        }
    }

    public void DisableChosingPlayer()
    {
        foreach (Stack<Player> p in BoardGame)
        {
            if (p.Count >= 1)
            {
                if (p.Peek().gameObject.GetComponent<Player>().PlayerType == PlayerTurn)
                {
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
        foreach (Player trappedStone in Trapped) {
            if (trappedStone.PlayerType == "Black")
                oppoiste = "White";
            else
                oppoiste = "Black";

            canPutInDice1 = CheckCanPutThere(dices[0].diceCount, oppoiste);
            canPutInDice2 = CheckCanPutThere(dices[1].diceCount, oppoiste);
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
                PassTurn();
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
}
