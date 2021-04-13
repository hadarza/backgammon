using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : MonoBehaviour
{
    [SerializeField]
    Vector3 firstLocation;
    [SerializeField]
    GameManger gameManger;
    const int LocationMaxPlayers = 15;
    public int TriangleIndex;
    public Vector3[] location; // array of Vector3 of the locations for players according the TriangleIndex.
    Ray ray;
    RaycastHit hit;

    int MovementToDecrease;
    string OppisteType;
    int x;
    void Start()
    {
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
        gameManger.indexCountMove++;
        gameManger.HideAllTriangles();
        if (!moveByOtherDice)
        {     
            if (OnSelected.SelectedPlayer.PlayerType == "Black") {
                OppisteType = "White";
                x = TriangleIndex + gameManger.dices[dice].diceCount;
            } else {
                OppisteType = "Black";
                x = TriangleIndex - gameManger.dices[dice].diceCount;
            }
            // according the current TypePlayer, show according to your current location after movent, if you can relocate the same player according the second dice.
            int canJumpToOtherDice = gameManger.CheckCanPutThere(x, OppisteType);

            if (canJumpToOtherDice != -1)
            {
                // show the specific triangles that the participent can jump to
                gameManger.triangles[canJumpToOtherDice - 1].gameObject.SetActive(true);

            }
        }
    }

    public void updateAfterFinishTurn()
    {
        gameManger.indexCountMove++;
        // if the particpent move according to first/ second dice
        gameManger.HideAllTriangles();
        gameManger.DisableChosingPlayer();

        //make this so it won't be able to click on the other Type player and add OnSelected before dices rolled again.
        gameManger.dices[0].diceCount = 0;
        gameManger.dices[1].diceCount = 0;

        foreach (Stack<Player> p in gameManger.BoardGame){
            if (p.Count > 0){
                if (p.Peek().GetComponent<OnSelected>())
                    Destroy(p.Peek().GetComponent<OnSelected>());
            }
        }
    }
    public void OnMouseDown(){
        if (DidClickOnTriangle()) { 
            // check if we have Trapped stones
            if (CheckTrappedTypePlayerStones()){
                UpdateMoventDiceNormalSituation();
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
        gameManger.CanMoveStonesOnBoard = true;
        UpdateRollingOnRelocateTrappedStones();
        gameManger.indexCountMove++;
    }

    public void UpdateMovementMoreThanOneTrapped()
    {
        // there is more trapped stones
        /*We don't want to allow selecting stones in the board, only TrappedStones*/
        if (TriangleIndex == gameManger.dices[0].diceCount ||
            TriangleIndex == gameManger.dices[1].diceCount ||
            GameManger.BOARD_TRIANGLES - TriangleIndex + 1 == gameManger.dices[0].diceCount ||
            GameManger.BOARD_TRIANGLES - TriangleIndex + 1 == gameManger.dices[1].diceCount){
            // remove the option to move the selected object beacuse we have to enter all trapped stones before starting moving the stones that already on the board
            if (OnSelected.SelectedPlayer.GetComponent<OnSelected>())
                Destroy(OnSelected.SelectedPlayer.GetComponent<OnSelected>());

            gameManger.CanMoveStonesOnBoard = false;
            UpdateRollingOnRelocateTrappedStones(); // canMoveStonesOnBoard prevent from showing triangles and will only show the participent the option to located the trapped stones 
            gameManger.indexCountMove++;
            gameManger.HideAllTriangles();
        }
    }

    /*Normal situation - we dont have trapped stones, update the movement of the stone*/
    public void UpdateMoventDiceNormalSituation()
    {
        // using Math.Abs for not checking if the current player is white or black.
        MovementToDecrease = Math.Abs(OnSelected.SelectedPlayer.indexTriangle - TriangleIndex);

        gameManger.CanMoveStonesOnBoard = true;
        UpdateRolling(MovementToDecrease);
        JumpOnOppositeStone(); // check if the participent jump on oppoise stone on the board

        gameManger.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Pop(); // remove from current stack

        UpdateOnSelectedOptionOnStone();
        UpdatePropertiesAfterMovement();
    }

    /* if there is a player in the gameManger.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1] now after removing ,than addComponent OnSelected */
    public void UpdateOnSelectedOptionOnStone()
    {
        if (gameManger.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Count >= 1){
            if (!gameManger.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Peek().gameObject.GetComponent<OnSelected>())
                gameManger.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Peek().gameObject.AddComponent<OnSelected>();
        }
    }

    public void UpdateRollingOnRelocateTrappedStones()
    {
        if (GameManger.PlayerTurn == "Black")
            UpdateRolling(TriangleIndex);
        else
            UpdateRolling(GameManger.BOARD_TRIANGLES - TriangleIndex + 1);
    }

    public void UpdatePropertiesAfterMovement()
    {
        // update new Properties - triangleIndex, at BoardGame array and visual on Unity
        OnSelected.SelectedPlayer.indexTriangle = TriangleIndex;
        gameManger.BoardGame[TriangleIndex - 1].Push(OnSelected.SelectedPlayer);
        OnSelected.SelectedPlayer.transform.localPosition = location[gameManger.BoardGame[TriangleIndex - 1].Count - 1];
    }

    public void UpdateOnBoardRemoveFromTrapped(List <Player> currentList){
        SearchCurrentTrappedStone(currentList); // remove from Trapped stones array

        // black player put on 0-5 , white player pn 23 - 18 (TriangleIndex)
            gameManger.BoardGame[TriangleIndex - 1].Push(OnSelected.SelectedPlayer); // add to board game
            OnSelected.SelectedPlayer.transform.localPosition = location[gameManger.BoardGame[TriangleIndex - 1].Count - 1]; // update on board - visual
        
    }

    public List<Player> GetCurrentListAccordingToTurn(){
        // this func return the currentListTrapped stones according to currrent TypePlayer
        List<Player> currentList;
        if (GameManger.PlayerTurn == "Black")
            currentList = gameManger.onPlayerBlack;
        else
            currentList = gameManger.onPlayerWhite;
        return currentList;
    }

    public void UpdateRolling(int MovementToDecrease)
    {
        if (!gameManger.SumMovements.IsDouble && gameManger.CanMoveStonesOnBoard)
        {
            if (!GameManger.moveByFirstDice)
            {
                // if the player chose to move according to first dice
                if (gameManger.SumMovements.firstDice == MovementToDecrease)
                {
                    ShowMovementAfterDice(GameManger.moveBySecondDice, 1);
                    GameManger.moveByFirstDice = true;
                    gameManger.canPut = -1;

                }
            }
             if (!GameManger.moveBySecondDice)
            {
                if (gameManger.SumMovements.secondDice == MovementToDecrease)
                {
                    ShowMovementAfterDice(GameManger.moveByFirstDice, 0);
                    GameManger.moveBySecondDice = true;
                    gameManger.canPut2 = -1;
                }
            }

             if(GameManger.moveByFirstDice && GameManger.moveBySecondDice)
                updateAfterFinishTurn();
        }
        // when the dices show double when indexCountMove is 4 than update that the participent rolled according to 2 dices twice. IndexCountMove respresnt count of moves for this current turn.
        else if (gameManger.SumMovements.IsDouble && gameManger.CanMoveStonesOnBoard)
        {
            switch (gameManger.indexCountMove)
            {
                case 0:
                    ShowMovementAfterDice(GameManger.moveBySecondDice, 0);
                    GameManger.moveByFirstDice = true;
                    gameManger.canPut = -1;
                    break;
                case 1:
                    ShowMovementAfterDice(GameManger.moveByThirdDice, 0);
                    GameManger.moveBySecondDice = true;
                    gameManger.canPut2 = -1;
                    break;
                case 2:
                    ShowMovementAfterDice(GameManger.moveByFourthDice, 0);
                    GameManger.moveByThirdDice = true;
                    gameManger.canPut3 = -1;
                    break;
                case 3:
                    GameManger.moveByFourthDice = true;
                    gameManger.canPut4 = -1;
                    updateAfterFinishTurn();
                    break;
            }
        }
    }

    public bool CheckTrappedTypePlayerStones(){
        return (GameManger.PlayerTurn == "Black" && gameManger.onPlayerBlack.Count == 0) || (GameManger.PlayerTurn == "White" && gameManger.onPlayerWhite.Count == 0);
    }

    // remove the current player selected forom onPlayerBlack / onPlayerWhite - trapped stones
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
        Stack<Player> removePlayer = null;
        List<Player> currentTrappedList = null;
        if (gameManger.BoardGame[TriangleIndex - 1].Count == 1){
            if (OnSelected.SelectedPlayer.PlayerType == "Black" && gameManger.BoardGame[TriangleIndex - 1].Peek().PlayerType == "White") 
                currentTrappedList = gameManger.onPlayerWhite;
            else if (OnSelected.SelectedPlayer.PlayerType == "White" && gameManger.BoardGame[TriangleIndex - 1].Peek().PlayerType == "Black")
                currentTrappedList = gameManger.onPlayerBlack;        

            if(currentTrappedList != null){
                // save on variable the current stone that will get removed from board
                removePlayer = gameManger.BoardGame[TriangleIndex - 1];

                Vector3 [] currentVector3LocationForTrapped = currentTrappedList == gameManger.onPlayerBlack ? gameManger.Vector3ArrayTrappedBlackStones : gameManger.Vector3ArrayTrappedWhiteStones;
                removePlayer.Peek().gameObject.transform.localPosition = currentVector3LocationForTrapped[currentTrappedList.Count];
                removePlayer.Peek().indexTriangle = 0; // reset indexTriangle
                currentTrappedList.Add(removePlayer.Pop());
            }
        }
    }
}
