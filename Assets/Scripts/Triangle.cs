using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Triangle : MonoBehaviour
{
    [SerializeField]
    Vector3 firstLocation;
    [SerializeField]

    const int LocationMaxPlayers = 15;

    public int TriangleIndex;
    public Vector3[] location; // array of Vector3 of the locations for players according the TriangleIndex.

    GameManger gameManager;
    Ray ray;
    RaycastHit hit;
    int MovementToDecrease;
    string OppisteType;
    int x;
    string currentPlayer;

    void Start()
    {
        gameManager = FindObjectOfType<GameManger>();
        location = new Vector3[LocationMaxPlayers];
        int indexStart = 0;
        int i;
        for (int J = 1; J <= 3; J++){
            for (i = indexStart; i < LocationMaxPlayers - (3 - J) * 5; i++){
                if (i == 0)
                    location[0] = firstLocation;
                else{
                    if(TriangleIndex < 13)
                        location[i] = location[0] + ((i % 5) * new Vector3(0, 0, 1.25f)) + (J - 1) * (new Vector3(0, 1.25f, 0));
                    else
                        location[i] = location[0] - ((i % 5) * new Vector3(0, 0, 1.25f)) + (J - 1) * (new Vector3(0, 1.25f, 0));
                }
            }
            indexStart = i;
        }
    }
    /* after the movement according to first/second dice, 
       update the next location the paritcipent can jump to, according to the other dice. */
    public void ShowMovementAfterDice(bool moveByOtherDice, int dice){
        gameManager.indexCountMove++;
        gameManager.HideAllTriangles();
        if (!moveByOtherDice)
        {     
            if (OnSelected.SelectedPlayer.PlayerType == "Black") {
                OppisteType = "White";
                gameManager.IndexPutAccordingToDice[dice] = TriangleIndex + gameManager.dices[dice].diceCount;
            } else {
                OppisteType = "Black";
                gameManager.IndexPutAccordingToDice[dice] = TriangleIndex - gameManager.dices[dice].diceCount;
            }
            // according the current TypePlayer, show according to your current location after movent, if you can relocate the same player according the second dice.
            int canJumpToOtherDice = gameManager.CheckCanPutThere(gameManager.IndexPutAccordingToDice[dice], OppisteType);

            if (canJumpToOtherDice != -1){
                // show the specific triangles that the participent can jump to
                gameManager.triangles[canJumpToOtherDice - 1].gameObject.SetActive(true);

            }
        }
    }

    public void updateAfterFinishTurn()
    {
        gameManager.indexCountMove++;
        // if the particpent move according to first/ second dice
        gameManager.HideAllTriangles();
        gameManager.DisableChosingPlayer();

        //make this so it won't be able to click on the other Type player and add OnSelected before dices rolled again.
        gameManager.dices[0].diceCount = 0;
        gameManager.dices[1].diceCount = 0;

        foreach (Stack<Player> p in gameManager.BoardGame){
            if (p.Count > 0){
                if (p.Peek().GetComponent<OnSelected>())
                    Destroy(p.Peek().GetComponent<OnSelected>());
            }
        }
    }

    public void OnMouseDown(){
        bool MessageNeedToPop = false;
        if (DidClickOnTriangle()) {
            // check if we have Trapped stones
            if (CheckTrappedTypePlayerStones()){
                if (!gameManager.SumMovements.IsDouble) {
                    if (gameManager.CountPossibleOptionsToDoHighestMove() == 1){
                        Dice HighDice = gameManager.GetHighDice() == gameManager.dices[0].diceCount ? gameManager.dices[0] : gameManager.dices[1];
                        //if highest dice hasn't been done yet.
                        if (!gameManager.DoneMove[HighDice.indexDice])
{                           // if player chose to select the player that has the possible option to move according to the highest dice.
                            if (TriangleIndex == gameManager.currentObjectPossibleToMovement.indexTriangle){
                                // if player chose to move according the other dice
                                if (gameManager.GetHighDice() != Math.Abs(OnSelected.SelectedPlayer.indexTriangle - TriangleIndex)){
                                    // did the other dice and can't do the second dice (the higher) after , than MessageNeedToPop = true;
                                    if (GameManger.PlayerTurn == "Black"){
                                        if (gameManager.CheckCanPutThere(OnSelected.SelectedPlayer.indexTriangle + Math.Abs(OnSelected.SelectedPlayer.indexTriangle - TriangleIndex) + gameManager.GetHighDice(), "Black") == -1)
                                            MessageNeedToPop = true;
                                    }else{
                                        if (gameManager.CheckCanPutThere(OnSelected.SelectedPlayer.indexTriangle - Math.Abs(OnSelected.SelectedPlayer.indexTriangle - TriangleIndex) - gameManager.GetHighDice() + 1, "White") == -1)
                                            MessageNeedToPop = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!MessageNeedToPop)
                    UpdateMoventDiceNormalSituation();
                else
                {
                    // show message must to do the highest dice
                    gameManager.panelTurnpass.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "לפי חוק שש-בש, אם ניתן להזיז שחקן לפי הקובייה הגדולה במקום אחד, אזי חובה לשחק את הקוביה הגדולה מבין השניים";
                    gameManager.panelTurnpass.gameObject.SetActive(true);
                    // show a message on display to tell the player that the turn pass
                }
            }
            else{ // there is something in OnPlayerBlack / onPlayerWhite array (Trapped stones)
                JumpOnOppositeStone(); // check if the participent jump on oppoise stone on the board

                List<Player> currentList = GetCurrentListAccordingToTurn(); // get current trapped stones Array according to turn's current player
                OnSelected.SelectedPlayer.indexTriangle = TriangleIndex;
                UpdateOnBoardRemoveFromTrapped(currentList);
                // after remove the current trapped stone - check if there is more trapped stones from the current TypePlayer
                if (currentList.Count == 0)
                    UpdateMoventOnlyOneTrapped();
                else
                    UpdateMovementMoreThanOneTrapped();
            }
           
        }
    }
            
        
    
    /*the function return true if the participent click on triangle for moving his current stone , else didn't click and return false*/
    public bool DidClickOnTriangle() {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            if (Input.GetMouseButton(0)){
                if (hit.collider.tag == "Triangle") {
                    // if the particpent click on a triangle, move the player to this location
                    if (OnSelected.SelectedPlayer != null)
                        return true;
                }
            }
        }
        return false;
    }
    
    public void UpdateMoventOnlyOneTrapped()
    {
        gameManager.CanMoveStonesOnBoard = true;
        UpdateRollingOnRelocateTrappedStones();
    }

    public void UpdateMovementMoreThanOneTrapped()
    {
        // there is more trapped stones
        /*We don't want to allow selecting stones in the board, only TrappedStones*/
        if (TriangleIndex == gameManager.dices[0].diceCount ||
            TriangleIndex == gameManager.dices[1].diceCount ||
            GameManger.BOARD_TRIANGLES - TriangleIndex + 1 == gameManager.dices[0].diceCount ||
            GameManger.BOARD_TRIANGLES - TriangleIndex + 1 == gameManager.dices[1].diceCount){
            // remove the option to move the selected object beacuse we have to enter all trapped stones before starting moving the stones that already on the board
            if (OnSelected.SelectedPlayer.GetComponent<OnSelected>())
                Destroy(OnSelected.SelectedPlayer.GetComponent<OnSelected>());

            gameManager.CanMoveStonesOnBoard = false;
            UpdateRollingOnRelocateTrappedStones(); // canMoveStonesOnBoard prevent from showing triangles and will only show the participent the option to located the trapped stones 
            gameManager.HideAllTriangles();
        }
    }

    /*Normal situation - we dont have trapped stones, update the movement of the stone*/
    public void UpdateMoventDiceNormalSituation()
    {
        // using Math.Abs for not checking if the current player is white or black.
        MovementToDecrease = Math.Abs(OnSelected.SelectedPlayer.indexTriangle - TriangleIndex);

        gameManager.CanMoveStonesOnBoard = true;
        UpdateRolling(MovementToDecrease);
       
        JumpOnOppositeStone(); // check if the participent jump on oppoise stone on the board

        gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Pop(); // remove from current stack

        UpdateOnSelectedOptionOnStone();
        UpdatePropertiesAfterMovement();
        if (gameManager.dices[0].diceCount != 0 && gameManager.dices[1].diceCount != 0)
        { // if player didn't finish his turn beacuse did all steps.
            if (gameManager.isAllPlayersCanRemoved(gameManager.BoardGame, GameManger.PlayerTurn))
            {
                OnSelected.SelectedPlayer.PlayerRemoveStones();
            }
            else
            {
                if (!gameManager.ThereIsOptionalMove())
                {
                    gameManager.panelTurnpass.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "אין ביכולתך להזיז אף אבן ולכן התור עובר ליריב";
                    gameManager.panelTurnpass.SetActive(true);
                    gameManager.PassTurn();
                }
            }
        }
    }

    /* if there is a player in the gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1] now after removing ,than addComponent OnSelected */
    public void UpdateOnSelectedOptionOnStone(){
        if (gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Count >= 1){
            if (!gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Peek().gameObject.GetComponent<OnSelected>())
                gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Peek().gameObject.AddComponent<OnSelected>();
        }
    }

    public void UpdateRollingOnRelocateTrappedStones()
    {
        if (GameManger.PlayerTurn == "Black")
            UpdateRolling(TriangleIndex);
        else
            UpdateRolling(GameManger.BOARD_TRIANGLES - TriangleIndex + 1);
    }

    // update new Properties - triangleIndex, at BoardGame array and visual on Unity
    public void UpdatePropertiesAfterMovement()
    {
        OnSelected.SelectedPlayer.indexTriangle = TriangleIndex;
        gameManager.BoardGame[TriangleIndex - 1].Push(OnSelected.SelectedPlayer);
        OnSelected.SelectedPlayer.transform.localPosition = location[gameManager.BoardGame[TriangleIndex - 1].Count - 1];
    }

    public void UpdateOnBoardRemoveFromTrapped(List <Player> currentList){
        SearchCurrentTrappedStone(currentList); // remove from Trapped stones array

        // black player put on 0-5 , white player pn 23 - 18 (TriangleIndex)
            gameManager.BoardGame[TriangleIndex - 1].Push(OnSelected.SelectedPlayer); // add to board game
            OnSelected.SelectedPlayer.transform.localPosition = location[gameManager.BoardGame[TriangleIndex - 1].Count - 1]; // update on board - visual
        
    }
    // this func return the currentListTrapped stones according to currrent TypePlayer
    public List<Player> GetCurrentListAccordingToTurn(){
        List<Player> currentList;
        if (GameManger.PlayerTurn == "Black")
            currentList = gameManager.onPlayerBlack;
        else
            currentList = gameManager.onPlayerWhite;
        return currentList;
    }

    // update location for putting stone after doing a step.
    public void UpdateRolling(int MovementToDecrease)
    {
        if (!gameManager.SumMovements.IsDouble){
            if (!gameManager.DoneMove[0]) { 
                // if the player chose to move according to first dice
                if (gameManager.SumMovements.firstDice == MovementToDecrease){
                    if(gameManager.CanMoveStonesOnBoard)
                        ShowMovementAfterDice(gameManager.DoneMove[1], 1);
                    gameManager.DoneMove[0] = true;
                    gameManager.IndexPutAccordingToDice[0] = -1;
                }
            }
             if (!gameManager.DoneMove[1]){
                if (gameManager.SumMovements.secondDice == MovementToDecrease){
                    if (gameManager.CanMoveStonesOnBoard)
                        ShowMovementAfterDice(gameManager.DoneMove[0], 0);

                    gameManager.DoneMove[1] = true;
                    gameManager.IndexPutAccordingToDice[1] = -1;
                }
            }

             if(gameManager.DoneMove[0] && gameManager.DoneMove[1])
                updateAfterFinishTurn();
        }
        // when the dices show double when indexCountMove is 4 than update that the participent rolled according to 2 dices twice. IndexCountMove respresnt count of moves for this current turn.
        else if (gameManager.SumMovements.IsDouble)
        {
            switch (gameManager.indexCountMove)
            {
                case 0:
                    if (gameManager.CanMoveStonesOnBoard)
                        ShowMovementAfterDice(gameManager.DoneMove[1], 0);
                    gameManager.DoneMove[0] = true;
                    gameManager.IndexPutAccordingToDice[0] = -1;
                    break;
                case 1:
                    if (gameManager.CanMoveStonesOnBoard)
                        ShowMovementAfterDice(gameManager.DoneMove[2], 0);
                    gameManager.DoneMove[1] = true;
                    gameManager.IndexPutAccordingToDice[1] = -1;
                    break;
                case 2:
                    if (gameManager.CanMoveStonesOnBoard)
                        ShowMovementAfterDice(gameManager.DoneMove[3], 0);
                    gameManager.DoneMove[2] = true;
                    gameManager.IndexPutAccordingToDice[2] = -1;
                    break;
                case 3:
                    gameManager.DoneMove[3] = true;
                    gameManager.IndexPutAccordingToDice[3] = -1;
                    gameManager.indexCountMove++;
                    updateAfterFinishTurn();
                    break;
            }
        }
    }
    // function return true if you are in your turn and you don't have stones out of board, else return false
    public bool CheckTrappedTypePlayerStones(){
        return (GameManger.PlayerTurn == "Black" && gameManager.onPlayerBlack.Count == 0) || (GameManger.PlayerTurn == "White" && gameManager.onPlayerWhite.Count == 0);
    }

    // remove the current player selected from onPlayerBlack / onPlayerWhite - trapped stones
    public void SearchCurrentTrappedStone(List<Player> listTrapped){
        foreach(Player p in listTrapped){
            if (p == OnSelected.SelectedPlayer) {
                // search for the player in list that got clicked
                listTrapped.Remove(p);
                break;
            }

        }
    }

    /*if you jump on one player that oppoise to your PlayerType - eat it and remove from board, add to OnPlayerBlack / onPlayerWhite array */
    public void JumpOnOppositeStone(){
        bool FoundThere = false;
        Stack<Player> removePlayer = null;
        List<Player> currentTrappedList = null;
        if (gameManager.BoardGame[TriangleIndex - 1].Count == 1){
            if (OnSelected.SelectedPlayer.PlayerType == "Black" && gameManager.BoardGame[TriangleIndex - 1].Peek().PlayerType == "White") 
                currentTrappedList = gameManager.onPlayerWhite;
            else if (OnSelected.SelectedPlayer.PlayerType == "White" && gameManager.BoardGame[TriangleIndex - 1].Peek().PlayerType == "Black")
                currentTrappedList = gameManager.onPlayerBlack;        

            if(currentTrappedList != null){
                // save on variable the current stone that will get removed from board
                removePlayer = gameManager.BoardGame[TriangleIndex - 1];
                Vector3 [] currentVector3LocationForTrapped = (currentTrappedList == gameManager.onPlayerBlack) ? gameManager.Vector3ArrayTrappedBlackStones : gameManager.Vector3ArrayTrappedWhiteStones;
                for(int j=0;j<currentVector3LocationForTrapped.Length;j++){
                    FoundThere = false;
                    for (int i = 0; i < currentTrappedList.Count; i++){
                        // if already exist an object eith this location on TrappedList 
                        if (currentTrappedList[i].transform.localPosition == currentVector3LocationForTrapped[j]){
                            FoundThere = true;
                            break;
                        }
                    }
                    if (!FoundThere){
                        removePlayer.Peek().gameObject.transform.localPosition = currentVector3LocationForTrapped[j];
                        break;
                    }
                }

                removePlayer.Peek().indexTriangle = 0; // reset indexTriangle
                currentTrappedList.Add(removePlayer.Pop());
            }
        }
    }
}
