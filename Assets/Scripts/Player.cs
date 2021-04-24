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
    int locStart;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManger>();
    }
    private void OnMouseDown()
    {
        // only if both dices land , than I can select a player.
        if (gameManager.IsBothDicesLandAndRoll() && gameManager.RollFirstTime) {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)){
                if (hit.collider.gameObject == gameObject){
                    if (!gameManager.SumMovements.IsPlayerDidAllSteps()){
                        if ((GameManger.PlayerTurn == "Black" && gameManager.onPlayerBlack.Count == 0) ||
                    (GameManger.PlayerTurn == "White" && gameManager.onPlayerWhite.Count == 0))
                        {
                            if (PlayerType == GameManger.PlayerTurn){
                                if (GameManger.LastSelected != gameObject){
                                    if (gameManager.IsBothDicesLandAndRoll()){
                                        // if one of dices or both are 0 mean finishTurn (Did all steps/ pass turn by some reason)
                                        if (gameManager.dices[0].diceCount != 0 && gameManager.dices[1].diceCount != 0){
                                            gameManager.EnableChosingPlayer(gameManager.BoardGame[indexTriangle - 1]);

                                            OnSelected.OnChosingMove -= gameManager.ChangeColorToCurrentPlayer;
                                            OnSelected.OnChosingMove -= gameManager.ShowWhereCanJumpTo;
                                            OnSelected.OnChosingMove -= gameManager.ShowTriangleMovement;

                                            OnSelected.OnChosingMove += gameManager.ChangeColorToCurrentPlayer;
                                            OnSelected.OnChosingMove += gameManager.ShowWhereCanJumpTo;
                                            OnSelected.OnChosingMove += gameManager.ShowTriangleMovement;
                                        }
                                    }
                                }else{
                                    ToggleHideShowRectangle(false);
                                    OnSelected.OnChosingMove -= gameManager.ChangeColorToCurrentPlayer;
                                    OnSelected.OnChosingMove -= gameManager.ShowWhereCanJumpTo;
                                    OnSelected.OnChosingMove -= gameManager.ShowTriangleMovement;
                                    ChangeBackToNormal();
                                    GameManger.LastSelected = null;
                                    // hide the triangles , so after deselect, we won't see the triangles movements.
                                    gameManager.HideAllTriangles();
                                }
                            }
                        }
                        else
                        {
                            if (gameManager.IsBothDicesLandAndRoll())
                            {
                                gameManager.SumMovements = gameManager.UpdateCurrentDiceManager();
                                if (GameManger.LastSelected != gameObject){
                                    OnSelected.OnChosingMove -= gameManager.ChangeColorToCurrentPlayer;
                                    OnSelected.OnChosingMove += gameManager.ChangeColorToCurrentPlayer;
                                    currentList = (PlayerType == "Black") ? gameManager.onPlayerBlack : gameManager.onPlayerWhite;
                                    if (currentList != null)
                                    {
                                        if (gameManager.IsPlayerFoundOnTrapped(currentList, this))
                                        {
                                            foreach (Player p in currentList)
                                            {
                                                if (!p.GetComponent<OnSelected>())
                                                    p.transform.gameObject.AddComponent<OnSelected>();
                                            }
                                            gameManager.changeLocationsToTrappedStones(PlayerType);
                                        }
                                    }
                                }else{

                                    OnSelected.OnChosingMove -= gameManager.ChangeColorToCurrentPlayer;
                                    ChangeBackToNormal();
                                    GameManger.LastSelected = null;
                                    // hide the triangles , so after deselect, we won't see the triangles movements.
                                    gameManager.HideAllTriangles();
                                }
                                
                            }
                        }
                    }
                }
            }
        }
    }
    public void ChangeBackToNormal()
    {
        print(GameManger.LastSelected);
        Renderer renderSelected = GameManger.LastSelected.GetComponent<Renderer>();
        Material[] materials = renderSelected.materials;

        Material normal = gameManager.NormalColor;
        Material[] mat = new Material[] { normal, materials[1] };
        renderSelected.materials = mat;
        GameManger.LastSelected.GetComponent<Renderer>().materials = renderSelected.materials;
    }

    public void PlayerRemoveStones()
    {
        currentList = (PlayerType == "Black") ? gameManager.onPlayerBlack : gameManager.onPlayerWhite;
        int IndexCheck = 0;
        if (PlayerType == GameManger.PlayerTurn){
            // check if stones be removed from board
            if (gameManager.isAllPlayersCanRemoved(gameManager.BoardGame, GameManger.PlayerTurn) && currentList.Count == 0)
            {
                // can't use foreach , beacuse break doesn't work there.
                for (int i = 0; i < gameManager.dices.Length; i++) {
                    switch (PlayerType)
                    {
                        case "White":
                            IndexCheck = gameManager.dices[i].diceCount - 1;
                            break;
                        case "Black":
                            IndexCheck = GameManger.BOARD_TRIANGLES - gameManager.dices[i].diceCount;
                            break;

                    }
                    if (gameManager.BoardGame[IndexCheck].Count > 0) {
                        if (gameManager.BoardGame[IndexCheck].Peek().PlayerType == PlayerType)
                        {
                            // if you have at least one stone on the stack of countDice from playerType
                            if (indexTriangle == gameManager.dices[i].diceCount || GameManger.BOARD_TRIANGLES - indexTriangle + 1 == gameManager.dices[i].diceCount)
                            {
                                if ((!gameManager.DoneMove[gameManager.dices[i].indexDice] && !gameManager.SumMovements.IsDouble) || gameManager.SumMovements.IsDouble)
                                {
                                    indexDiceToRemove = gameManager.dices[i].indexDice;
                                    ToggleHideShowRectangle(true);
                                    break;
                                }
                            }
                            else
                            {
                                indexDiceToRemove = -1;
                                ToggleHideShowRectangle(false);
                            }
                        }
                    }
                    // if we don't have anything from 6 to diceIndex than have to take from the last stack that exist
                    else {
                        string opposite = PlayerType == "Black" ? "White" : "Black";
                        int loc = PlayerType == "Black" ? OnSelected.SelectedPlayer.indexTriangle + gameManager.dices[i].diceCount : OnSelected.SelectedPlayer.indexTriangle - gameManager.dices[i].diceCount;

                        switch (PlayerType)
                        {
                            case "White":
                                locStart = 6;
                                break;
                            case "Black":
                                locStart = 19;
                                break;

                        }
                            if (locStart == 6 || locStart == 19)
                            {
                                if (IsStacksEmptyUntilLocation(gameManager.dices[i].diceCount, locStart))
                                {
                                    if (OnSelected.SelectedPlayer.indexTriangle == GetLastStackFull(locStart))
                                    {
                                        indexDiceToRemove = gameManager.dices[i].indexDice;
                                        ToggleHideShowRectangle(true);
                                    }
                                    else
                                    {
                                        indexDiceToRemove = -1;
                                        ToggleHideShowRectangle(false);
                                    }
                                    
                                } else
                                {
                                // check if there is optional to do move by this dice on all board.
                                // if so - ok. If not, need to check the other dice.
                                // if no dices can be moved - show message pass turn
                                if(!gameManager.ThereIsOptionalMove()){
                                    gameManager.panelTurnpass.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "אין לך היכן להניח את האבנים הכלואות ולכן התור עובר ליריב";
                                    gameManager.panelTurnpass.gameObject.SetActive(true);
                                    // show a message on display to tell the player that the turn pass
                                    gameManager.PassTurn();
                                }
                                    indexDiceToRemove = -1;
                                    ToggleHideShowRectangle(false);
                                }
                            }
                        }
                    
                    
                }
            }
        }
    }
    // Toggle - show/hide Triangle for helping player where to click in order to remove stones from board
    public void ToggleHideShowRectangle(bool IsShown)
    {
        // if you have at least one stone on the stack of countDice
        if (GameManger.PlayerTurn == "Black")
            gameManager.RectanglesShowTakeOut[0].SetActive(IsShown);
        else
            gameManager.RectanglesShowTakeOut[1].SetActive(IsShown);
    }
    // This function return true if in stack of stones 
    public bool IsStacksEmptyUntilLocation(int locationDice, int locationStart)
    {
        if (locationStart == 6)
        {
            for (int i = locationStart; i >= locationDice; i--)
            {
                if (gameManager.BoardGame[locationStart - 1].Count > 0)
                {
                    if(gameManager.BoardGame[locationStart - 1].Peek().PlayerType == PlayerType)
                        return false;
                }
            }
        }
        else
        {
            //locationStart = 19
            for (int i = locationStart; i <= GameManger.BOARD_TRIANGLES - locationDice; i++)
            {
                if (gameManager.BoardGame[locationStart - 1].Count > 0)
                    return false;
            }
        }
        return true;
    }
    public int GetLastStackFull(int locationStart)
    {
        if (locationStart == 6)
        {
            for (int i = locationStart; i > locationStart - 6; i--)
            {
                if (gameManager.BoardGame[i - 1].Count > 0)
                {
                    if(gameManager.BoardGame[i-1].Peek().PlayerType == PlayerType)
                        return i;
                }
            }
        }
        else
        {
            //locationStart = 19
            for (int i = locationStart; i < GameManger.BOARD_TRIANGLES ; i++)
            {
                if (gameManager.BoardGame[i - 1].Count > 0){
                    if (gameManager.BoardGame[i - 1].Peek().PlayerType == PlayerType)
                        return i;
                }
            }
        }
        return -1; // all stones are out
    }
}
