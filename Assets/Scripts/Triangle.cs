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



        for(int j = 1; j <=5; j++)
        {
            for (int z = 0; z <= 5 - j; z++)
            {
                if (z == 0 && j == 1)
                    location[0] = firstLocation;
                else
                {
                    if (TriangleIndex < 13)
                        location[indexStart + z] = location[0] + (new Vector3(0, 0, 0.5f))*(j-1) +(new Vector3(0, 0, 1.25f) * (z)) + ((j - 1) * (new Vector3(0, 0.2f, 0)));
                    else
                        location[indexStart + z ] = location[0]- (new Vector3(0, 0, 0.5f)) * (j-1) - (new Vector3(0, 0, 1.25f) * (z)) + ((j - 1) * (new Vector3(0, 0.2f, 0)));

                }
            }
            indexStart+= 6 - j;
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



    public void OnMouseDown(){
    if (DidClickOnTriangle()){
        // if we don't have Trapped stones
        if (NoTrappedTypePlayerStones()){
            if (!NeedToPopMessageRules())
                UpdateMoventDiceNormalSituation();
            else{
                // show message must to do the highest dice
                gameManager.WarnPanel.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "לפי חוק שש-בש, אם ניתן להזיז שחקן לפי הקובייה הגדולה במקום אחד, אזי חובה לשחק את הקוביה הגדולה מבין השניים";
                gameManager.WarnPanel.gameObject.SetActive(true);
            }
        }else{ // there is something in OnPlayerBlack / onPlayerWhite array (Trapped stones)
            JumpOnOppositeStone(); // check if the participent jump on oppoise stone on the board

            List<Player> currentList = gameManager.GetCurrentListAccordingToTurn(); // get current trapped stones Array according to turn's current player
            OnSelected.SelectedPlayer.indexTriangle = TriangleIndex;
            UpdateOnBoardRemoveFromTrapped(currentList);
            // after remove the current trapped stone - check if there is more trapped stones from the current TypePlayer
            if (currentList.Count == 0)UpdateMoventOnlyOneTrapped();
            else UpdateMovementMoreThanOneTrapped();
        }
    }
}
       
    public bool NeedToPopMessageRules()
    {
        bool MessageNeedToPop = false;
        if (!gameManager.SumMovements.IsDouble){
            // if there is one possible option to do highest dice
            if (gameManager.CountPossibleOptionsToDoHighestMove() == 1){
                // save the  object HighDice
                Dice HighDice = gameManager.GetHighDice() == gameManager.dices[0].diceCount ? gameManager.dices[0] : gameManager.dices[1];
                //if highest dice hasn't been done yet.
                if (!gameManager.DoneMove[HighDice.indexDice]){
                    // if player chose to select the player that has the possible option to move according to the highest dice.
                    if (OnSelected.SelectedPlayer.indexTriangle == gameManager.currentObjectPossibleToMovement.indexTriangle){
                        // if player chose to move according the other dice
                        if (gameManager.GetHighDice() != Math.Abs(OnSelected.SelectedPlayer.indexTriangle - TriangleIndex)){
                            // did the other dice and can't do the second dice (the higher) after , than MessageNeedToPop = true;
                            if (GameManger.PlayerTurn == "Black"){
                                if (gameManager.CheckCanPutThere(OnSelected.SelectedPlayer.indexTriangle + Math.Abs(OnSelected.SelectedPlayer.indexTriangle - TriangleIndex) + gameManager.GetHighDice(), "White") == -1) {
                                    if (gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Count == 1){
                                            MessageNeedToPop = true;
                                    }
                                }
                            }
                            else{
                                if (gameManager.CheckCanPutThere(OnSelected.SelectedPlayer.indexTriangle - Math.Abs(OnSelected.SelectedPlayer.indexTriangle - TriangleIndex) - gameManager.GetHighDice(), "Black") == -1){
                                    if (gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Count == 1){
                                            MessageNeedToPop = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return MessageNeedToPop;
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
    
    //This function should happen when we have one trapped stone. After Replacing it, at need present a message that we can't move accoding to the other dice.
    public void UpdateMoventOnlyOneTrapped()
    {
        gameManager.CanMoveStonesOnBoard = true;
        UpdateRollingOnRelocateTrappedStones();
        if (!gameManager.SumMovements.IsPlayerDidAllSteps()){
            if (!gameManager.ThereIsOptionalMove())
                gameManager.ShowErrorPassTurn("אין ביכולתך להזיז אף אבן ולכן התור עובר ליריב");
        }
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
            OnSelected.SelectedPlayer.ChangeBackToNormal(); // change color of stone to be normal
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

        if (!gameManager.SumMovements.IsPlayerDidAllSteps()){ // if player didn't finish his turn beacuse did all steps.
            if (gameManager.isAllPlayersCanRemoved(gameManager.BoardGame, GameManger.PlayerTurn)){
                if (gameManager.CantMove()){
                    gameManager.ShowMessagePassTurn = true; //reset
                    gameManager.ShowMessagePassTurn = gameManager.NeedPassTurnMsg(); // this function check if there is an optional to take out stones by last stack/ stack according to dices

                    if (gameManager.ShowMessagePassTurn)
                        gameManager.ShowErrorPassTurn("אין ביכולתך להזיז אף אבן ולכן התור עובר לי");
                }
                OnSelected.SelectedPlayer.PlayerRemoveStones();
            }
            else{
                if(gameManager.CantMove())
                    gameManager.ShowErrorPassTurn("אין ביכולתך להזיז אבנים לפי הקוביות הנתונות ולכן התור עובר ליריב"); // if we can't get stones out & can't move at all - show msg
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
        if (gameManager.BoardGame[TriangleIndex - 1].Count > 0){
            if (gameManager.BoardGame[TriangleIndex - 1].Peek().GetComponent<OnSelected>())
                Destroy(gameManager.BoardGame[TriangleIndex - 1].Peek().GetComponent<OnSelected>());
        }
        gameManager.BoardGame[TriangleIndex - 1].Push(OnSelected.SelectedPlayer);
        OnSelected.SelectedPlayer.transform.localPosition = location[gameManager.BoardGame[TriangleIndex - 1].Count - 1];
    }

    public void UpdateOnBoardRemoveFromTrapped(List <Player> currentList){
        SearchCurrentTrappedStone(currentList); // remove from Trapped stones array

        if (gameManager.BoardGame[TriangleIndex - 1].Count > 0)
        {
            if (gameManager.BoardGame[TriangleIndex - 1].Peek().GetComponent<OnSelected>())
                Destroy(gameManager.BoardGame[TriangleIndex - 1].Peek().GetComponent<OnSelected>());
        }
        // black player put on 0-5 , white player pn 23 - 18 (TriangleIndex)
        gameManager.BoardGame[TriangleIndex - 1].Push(OnSelected.SelectedPlayer); // add to board game
        OnSelected.SelectedPlayer.transform.localPosition = location[gameManager.BoardGame[TriangleIndex - 1].Count - 1]; // update on board - visual
        OnSelected.SelectedPlayer.gameObject.layer = 9;
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
                    else{
                        // if player locate a stone that was out of board
                        gameManager.indexCountMove++;
                        gameManager.HideAllTriangles();
                    }
                    gameManager.DoneMove[0] = true;
                    gameManager.IndexPutAccordingToDice[0] = -1;
                }
            }
             if (!gameManager.DoneMove[1]){
                if (gameManager.SumMovements.secondDice == MovementToDecrease){
                    if (gameManager.CanMoveStonesOnBoard)
                        ShowMovementAfterDice(gameManager.DoneMove[0], 0);
                    else{
                        gameManager.indexCountMove++;
                        gameManager.HideAllTriangles();
                    }

                    gameManager.DoneMove[1] = true;
                    gameManager.IndexPutAccordingToDice[1] = -1;
                }
            }

             if(gameManager.DoneMove[0] && gameManager.DoneMove[1])
                gameManager.updateAfterFinishTurn();
        }
        // when the dices show double when indexCountMove is 4 than update that the participent rolled according to 2 dices twice. IndexCountMove respresnt count of moves for this current turn.
        else if (gameManager.SumMovements.IsDouble)
            MovementAccordingIndexCountMove();
    }

    public void MovementAccordingIndexCountMove()
    {
        if (gameManager.indexCountMove <= 2){
            gameManager.DoneMove[gameManager.indexCountMove] = true;
            gameManager.IndexPutAccordingToDice[gameManager.indexCountMove] = -1;
            if (gameManager.CanMoveStonesOnBoard)
                ShowMovementAfterDice(gameManager.DoneMove[gameManager.indexCountMove + 1], 0);
            else{
                gameManager.indexCountMove++;
                gameManager.HideAllTriangles();
            }
            
        }else{
            gameManager.DoneMove[3] = true;
            gameManager.IndexPutAccordingToDice[3] = -1;
            gameManager.indexCountMove++;
            gameManager.updateAfterFinishTurn();
        }
        //switch (gameManager.indexCountMove)
        //{
        //    case 0:
        //        if (gameManager.CanMoveStonesOnBoard)
        //            ShowMovementAfterDice(gameManager.DoneMove[1], 0);
        //        else
        //        {
        //            gameManager.indexCountMove++;
        //            gameManager.HideAllTriangles();
        //        }
        //        gameManager.DoneMove[0] = true;
        //        gameManager.IndexPutAccordingToDice[0] = -1;

        //        break;
        //    case 1:
        //        if (gameManager.CanMoveStonesOnBoard)
        //            ShowMovementAfterDice(gameManager.DoneMove[2], 0);
        //        else
        //        {
        //            gameManager.indexCountMove++;
        //            gameManager.HideAllTriangles();
        //        }
        //        gameManager.DoneMove[1] = true;
        //        gameManager.IndexPutAccordingToDice[1] = -1;

        //        break;
        //    case 2:
        //        if (gameManager.CanMoveStonesOnBoard)
        //            ShowMovementAfterDice(gameManager.DoneMove[3], 0);
        //        else
        //        {
        //            gameManager.indexCountMove++;
        //            gameManager.HideAllTriangles();
        //        }
        //        gameManager.DoneMove[2] = true;
        //        gameManager.IndexPutAccordingToDice[2] = -1;

        //        break;
        //    case 3:
        //        gameManager.DoneMove[3] = true;
        //        gameManager.IndexPutAccordingToDice[3] = -1;
        //        gameManager.indexCountMove++;
        //        gameManager.updateAfterFinishTurn();
        //        break;
        //}
    }
    // function return true if you are in your turn and you don't have stones out of board, else return false
    public bool NoTrappedTypePlayerStones(){
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

        // get currentTrapped List
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
                // remove OnSelected functiontality
                if (removePlayer.Peek().GetComponent<OnSelected>())
                    Destroy(removePlayer.Peek().GetComponent<OnSelected>());
                removePlayer.Peek().gameObject.layer = 0;
                currentTrappedList.Add(removePlayer.Pop());

            }
        }
    }
}
