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
            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.gameObject == gameObject) {
                    if ((GameManger.PlayerTurn == "Black" && gameManager.onPlayerBlack.Count == 0) ||
                    (GameManger.PlayerTurn == "White" && gameManager.onPlayerWhite.Count == 0)) {
                        if (PlayerType == GameManger.PlayerTurn) {
                            if (!gameManager.SumMovements.IsPlayerDidAllSteps())
                            {
                                if (GameManger.LastSelected != gameObject)
                                {
                                    gameManager.EnableChosingPlayer(gameManager.BoardGame[indexTriangle - 1]);
                                    OnSelected.OnChosingMove += gameManager.ChangeColorToCurrentPlayer;
                                    OnSelected.OnChosingMove += gameManager.ShowWhereCanJumpTo;
                                    OnSelected.OnChosingMove += gameManager.ShowTriangleMovement;
                                    PlayerRemoveStones();
                                }
                            }
                            else
                            {
                                ChangeBackToNormal();
                                GameManger.LastSelected = null;
                                // hide the triangles , so after deselect, we won't see the triangles movements.
                                gameManager.HideAllTriangles();
                            }
                                
                        }
                    }
                    else {
                        gameManager.SumMovements = gameManager.UpdateCurrentDiceManager();
                        if (GameManger.LastSelected != gameObject)
                            OnSelected.OnChosingMove += gameManager.ChangeColorToCurrentPlayer;
                        List<Player> currentList;
                        currentList = (PlayerType == "Black") ? gameManager.onPlayerBlack : gameManager.onPlayerWhite;
                        if (currentList != null) {
                            if (gameManager.IsPlayerFoundOnTrapped(currentList, this)) {
                                foreach (Player p in currentList) {
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
    public void ChangeBackToNormal()
    {
        Renderer renderSelected = GameManger.LastSelected.GetComponent<Renderer>();
        Material[] materials = renderSelected.materials;

        Material normal = gameManager.NormalColor;
        Material[] mat = new Material[] { normal, materials[1] };
        renderSelected.materials = mat;
    }

    public void PlayerRemoveStones()
    {
        if (gameManager.isAllPlayersCanRemoved(gameManager.BoardGame, GameManger.PlayerTurn)){
            foreach(Dice dice in gameManager.dices){
                if (gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Count > 0){
                    // if you have at least one stone on the stack of countDice
                    if (OnSelected.SelectedPlayer.indexTriangle == dice.diceCount || GameManger.BOARD_TRIANGLES - OnSelected.SelectedPlayer.indexTriangle == dice.diceCount)
                        ToggleHideShowRectangle(true);
                    else
                        ToggleHideShowRectangle(false);
                }
                else
                    ToggleHideShowRectangle(false);
            }
        }
    }
    public void ToggleHideShowRectangle(bool IsShown)
    {
        // if you have at least one stone on the stack of countDice
        if (GameManger.PlayerTurn == "Black")
        {
            gameManager.RectanglesShowTakeOut[0].SetActive(IsShown);
        }
        else
        {
            gameManager.RectanglesShowTakeOut[1].SetActive(IsShown);
        }
    }

}
