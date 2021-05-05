using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public string PlayerType;
    public int indexTriangle;// index Traingle will help to understand where the player circle is stand 
                             // index 0 - right bottom on the board.
    Ray ray;
    RaycastHit hit;
    GameManger gameManager;
    List<Player> currentList;
    public int indexDiceToRemove;
    int index;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManger>();
    }
    private void OnMouseDown()
    {
    if (CanClickOnStone()){
        if (hasTrappedStones()){
            if (ChooseToSelect()) UpdateListenersOnSelect();
            else UpdateListenersOnDeselect(); // Deselect
        }else{ // Trapped stones
            EnableSelectedTrappedStone();
        }
    }
}

    public bool CanClickOnStone()
    {
        if (IsParticipentClickOnStone()){
            if (IsThisMyTurn()){
                if (NotDoneYetAllDices())
                    return true;
            }
        }
        return false;
    }

    // This function return true if it's my turn, else if not
    public bool IsThisMyTurn(){
        return PlayerType == GameManger.PlayerTurn;
    }

    // This function return true if participent choose to select twice same stone (mean deselect)
    public bool ChooseToSelect(){
        return GameManger.LastSelected != gameObject;
    }
    
    public bool NotDoneYetAllDices(){
        if (gameManager.IsBothDicesLandAndRoll()){
            if (gameManager.dices[0].diceCount != 0 && gameManager.dices[1].diceCount != 0)
                return true;
        }
        return false;
    }

    public void EnableSelectedTrappedStone(){
        gameManager.SumMovements = gameManager.UpdateCurrentDiceManager();
        if (GameManger.LastSelected != gameObject)
            OnSelectTrappedStone();
        else
            OnDeselectTrappedStone();
    }

    // this function get called on select trapped stones
    public void OnSelectTrappedStone(){
        OnSelected.OnChosingMove -= gameManager.ChangeColorToCurrentPlayer;
        OnSelected.OnChosingMove += gameManager.ChangeColorToCurrentPlayer;

        currentList = (PlayerType == "Black") ? gameManager.onPlayerBlack : gameManager.onPlayerWhite;
        if (currentList != null){
            // check if player is fount on trapped array accordint to TypePlayer
            if (gameManager.IsPlayerFoundOnTrapped(currentList, this)){
                // pass on the array and add OnSelected Component, if not have one.
                foreach (Player p in currentList){
                    if (!p.GetComponent<OnSelected>())
                        p.transform.gameObject.AddComponent<OnSelected>();
                }
                // show options for movement
                gameManager.changeLocationsToTrappedStones(PlayerType);
            }
        }
    }

    // this function get called on deslect trapped stones
    public void OnDeselectTrappedStone()
    {
        OnSelected.OnChosingMove -= gameManager.ChangeColorToCurrentPlayer;
        ChangeBackToNormal();
        GameManger.LastSelected = null;
        // hide the triangles , so after deselect, we won't see the triangles movements.
        gameManager.HideAllTriangles();
    }


    public void UpdateListenersOnSelect()
    {
        // if one of dices or both are 0 mean finishTurn (Did all steps/ pass turn by some reason)
        if (gameManager.dices[0].diceCount != 0 && gameManager.dices[1].diceCount != 0){
            gameManager.EnableChosingPlayer(gameManager.BoardGame[indexTriangle - 1]);
            gameManager.RemoveListeners();
            gameManager.addListeners();
        }
    }

    public void UpdateListenersOnDeselect(){
        //Hide triangle of stones for removing from the board & hide the triangles , so after deselect, we won't see the triangles movements.
        gameManager.ToggleHideShowRectangle(false);
        gameManager.HideAllTriangles();

        //remove Onselected Listeners
        gameManager.RemoveListeners();

        // change color of stone to normal & remove from LastSelect 
        ChangeBackToNormal();
        GameManger.LastSelected = null;
    }

    //This function return true if the participent click on stone
    public bool IsParticipentClickOnStone()
    {
        if (gameManager.IsBothDicesLandAndRoll() && gameManager.RollFirstTime){
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)){
                if (hit.collider.gameObject == gameObject){
                    if (!gameManager.SumMovements.IsPlayerDidAllSteps())
                        return true;
                }
            }
        }
        return false;
    }

    // This function return true if there is no trappedStones from our current TypePlayer
    public bool hasTrappedStones(){
        return (GameManger.PlayerTurn == "Black" && gameManager.onPlayerBlack.Count == 0) ||(GameManger.PlayerTurn == "White" && gameManager.onPlayerWhite.Count == 0);
    }
    // This function responsible to change the color of stone on deselect it.
    public void ChangeBackToNormal(){
        Renderer renderSelected = GameManger.LastSelected.GetComponent<Renderer>();
        Material[] materials = renderSelected.materials;

        Material normal = gameManager.NormalColor;
        Material[] mat = new Material[] { normal, materials[1] };
        renderSelected.materials = mat;
        GameManger.LastSelected.GetComponent<Renderer>().materials = renderSelected.materials;
    }

    //This function is responsible for removing stones 
    public void PlayerRemoveStones(){
        currentList = (PlayerType == "Black") ? gameManager.onPlayerBlack : gameManager.onPlayerWhite;
        int IndexCheck = 0;

        if (PlayerType == GameManger.PlayerTurn){
            // check if stones be removed from board
            if (gameManager.isAllPlayersCanRemoved(gameManager.BoardGame, GameManger.PlayerTurn) && currentList.Count == 0)
            {
                // can't use foreach , beacuse break doesn't work there.
                for (int i = 0; i < gameManager.dicesCount.Length; i++) {
                    if (gameManager.dicesCount[i] >= 1){ // if pass turn happen - then all dices will reset and we will get an error whie trying to do  gameManager.dices[i].diceCount - 1;
                        // if you didn't did this dice yet
                        if (!gameManager.DoneMove[i]) {
                            IndexCheck = gameManager.GetIndexCountOnRemovingStones(GameManger.PlayerTurn, i); // update IndexCheck of triangle for checking an optional move to remove stone at this location
                            // if the stack according dice has one stone at least. if so, if TriangleIndex selected is equal to dice or 24-dice+ 1 then show option for removing
                        if (gameManager.BoardGame[IndexCheck].Count > 0){
                                if (gameManager.BoardGame[IndexCheck].Peek().PlayerType == PlayerType){
                                    // if you have at least one stone on the stack of countDice from playerType
                                    if (indexTriangle == gameManager.dicesCount[i] || GameManger.BOARD_TRIANGLES - indexTriangle + 1 == gameManager.dicesCount[i]){
                                        if ((!gameManager.DoneMove[i] && !gameManager.SumMovements.IsDouble) || gameManager.SumMovements.IsDouble){
                                            indexDiceToRemove = i;
                                            if (gameManager.DoneMove[i]){
                                                if (i == 1)
                                                    indexDiceToRemove = 0;
                                                else
                                                    indexDiceToRemove = 1;
                                            }
                                            gameManager.ToggleHideShowRectangle(true);
                                            break;
                                        }
                                    }else{
                                        indexDiceToRemove = -1;
                                        gameManager.ToggleHideShowRectangle(false);
                                    }
                                }else{
                                    gameManager.TakeCareOnNotOnStackAsDice(i,PlayerType,this);
                                    if (gameManager.RectanglesShowTakeOut[0].activeInHierarchy || gameManager.RectanglesShowTakeOut[1].activeInHierarchy)
                                        break;
                                }
                        }
                            // if we don't have anything from 6 to diceIndex than have to take from the last stack that exist
                            else{
                                gameManager.TakeCareOnNotOnStackAsDice(i,PlayerType,this);
                                if (gameManager.RectanglesShowTakeOut[0].activeInHierarchy || gameManager.RectanglesShowTakeOut[1].activeInHierarchy)
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
