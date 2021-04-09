using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class Player : MonoBehaviour
{
    public string PlayerType;
    public int indexTriangle;// index Traingle will help to understand where the player circle is stand 
                             // index 0 - right bottom on the board.
    Ray ray;
    RaycastHit hit;
    [SerializeField] GameManger gameManager;

    private void OnMouseDown()
    {
        // only if both dices land , than I can select a player.
        if (gameManager.IsBothDicesLandAndRoll()){
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit)){
                    if (hit.collider.gameObject == gameObject){

                    if ((GameManger.PlayerTurn == "Black" && gameManager.onPlayerBlack.Count == 0 ) ||
                    (GameManger.PlayerTurn == "White" && gameManager.onPlayerWhite.Count == 0)){
                        if (PlayerType == GameManger.PlayerTurn){
                            if (GameManger.LastSelected != gameObject){
                                if (!gameObject.GetComponent<OnSelected>())
                                    gameManager.EnableChosingPlayer();
                                gameManager.SumMovements = gameManager.UpdateCurrentDiceManager();
                                OnSelected.OnChosingMove += gameManager.ChangeColorToCurrentPlayer;
                                OnSelected.OnChosingMove += gameManager.ShowWhereCanJumpTo;
                                OnSelected.OnChosingMove += gameManager.ShowTriangleMovement;
                            }
                            else
                            {
                                GameManger.LastSelected.GetComponent<Renderer>().material = gameManager.NormalColor;
                                // remove all listeners 
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

                        if (PlayerType == "Black"){
                            if (gameManager.IsPlayerFoundOnTrapped(gameManager.onPlayerBlack, this)){
                                foreach (Player p in gameManager.onPlayerBlack){
                                    if (!p.GetComponent<OnSelected>())
                                        p.transform.gameObject.AddComponent<OnSelected>();
                                }
                                gameManager.changeLocationsToTrappedStones(gameManager.onPlayerBlack);
                            }
                        } else{
                            if (gameManager.IsPlayerFoundOnTrapped(gameManager.onPlayerWhite, this)){
                                foreach (Player p in gameManager.onPlayerWhite){
                                    if (!p.GetComponent<OnSelected>())
                                        p.transform.gameObject.AddComponent<OnSelected>();
                                }
                                gameManager.changeLocationsToTrappedStones(gameManager.onPlayerWhite);
                            }
                        }

                    }
                }
            }
        }
    }

    
}
