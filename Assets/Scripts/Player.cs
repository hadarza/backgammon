using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string PlayerType;
    public int indexTriangle;// index Traingle will help to understand where the player circle is stand 
                             // index 0 - right bottom on the board.
    Ray ray;
    RaycastHit hit;
    GameManger gameManager;
    

    private void Start()
    {
        gameManager = FindObjectOfType<GameManger>();
    }
    private void OnMouseDown()
    {
        // only if both dices land , than I can select a player.
        if (gameManager.IsBothDicesLandAndRoll() && gameManager.RollFirstTime) {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (!gameManager.SumMovements.IsPlayerDidAllSteps())
                    {
                        if ((GameManger.PlayerTurn == "Black" && gameManager.onPlayerBlack.Count == 0) ||
                    (GameManger.PlayerTurn == "White" && gameManager.onPlayerWhite.Count == 0))
                        {
                            if (PlayerType == GameManger.PlayerTurn)
                            {
                                if (GameManger.LastSelected != gameObject)
                                {
                                    gameManager.EnableChosingPlayer(gameManager.BoardGame[indexTriangle - 1]);
                                    OnSelected.OnChosingMove += gameManager.ChangeColorToCurrentPlayer;
                                    OnSelected.OnChosingMove += gameManager.ShowWhereCanJumpTo;
                                    OnSelected.OnChosingMove += gameManager.ShowTriangleMovement;
                                }else{
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
                            gameManager.SumMovements = gameManager.UpdateCurrentDiceManager();
                            if (GameManger.LastSelected != gameObject)
                                OnSelected.OnChosingMove += gameManager.ChangeColorToCurrentPlayer;
                            List<Player> currentList;
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
                                    gameManager.changeLocationsToTrappedStones(currentList);
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
    }

    public void PlayerRemoveStones()
    {
        int locStart;
        if (PlayerType == GameManger.PlayerTurn){
            if (gameManager.isAllPlayersCanRemoved(gameManager.BoardGame, GameManger.PlayerTurn)){
                // can't use foreach , beacuse break doesn't work there.
                for (int i = 0; i < gameManager.dices.Length; i++){
                    if (gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Count > 0) {
                        // if you have at least one stone on the stack of countDice
                        if (indexTriangle == gameManager.dices[i].diceCount || GameManger.BOARD_TRIANGLES - indexTriangle + 1 == gameManager.dices[i].diceCount) {
                            if ((!gameManager.DoneMove[gameManager.dices[i].indexDice] && !gameManager.SumMovements.IsDouble) || gameManager.SumMovements.IsDouble) {
                                ToggleHideShowRectangle(true);
                                break;
                            }
                        }
                        else
                            ToggleHideShowRectangle(false);
                    }
                    // if we don't have anything from 6 to diceIndex than have to take from the last stack that exist
                    else{
                        switch (PlayerType)
                        {
                            case "White":
                                locStart = 6;
                                if (IsStacksEmptyUntilLocation(gameManager.dices[i].diceCount, locStart))
                                {
                                    if (OnSelected.SelectedPlayer.indexTriangle == GetLastStackFull(locStart))
                                        ToggleHideShowRectangle(true);
                                    else ToggleHideShowRectangle(false);
                                }
                                break;
                            case "Black":
                                locStart = 24;
                                if (IsStacksEmptyUntilLocation(gameManager.dices[i].diceCount, locStart))
                                {
                                    if (OnSelected.SelectedPlayer.indexTriangle == GetLastStackFull(locStart))
                                        ToggleHideShowRectangle(true);
                                    else ToggleHideShowRectangle(false);
                                }
                                break;
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
        for(int i = locationStart; i >= locationDice; i--)
        {
            if(gameManager.BoardGame[locationStart - 1].Count == 0)
                return false;
        }
        return true;
    }
    public int GetLastStackFull(int locationStart)
    {
        for (int i = locationStart; i >= locationStart - 6; i--){
            if (gameManager.BoardGame[i].Count > 0)
                return i;
        }
        return -1; // all stones are out
    }
}
