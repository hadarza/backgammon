﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rectangleTakeOutStones : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;
    GameManger gameManager;

    public void Start()
    {
        gameManager = FindObjectOfType<GameManger>();
    }


    /*the function return true if the participent click on triangle for moving his current stone , else didn't click and return false*/
    public bool DidClickOnRectangle()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)){
            if (Input.GetMouseButton(0)){
                if (hit.collider.tag == "Rectangle"){
                        return true;
                }
            }
        }
        return false;
    }

    public void OnMouseDown()
    {
        if (DidClickOnRectangle()){
            // adding to list of stones that take out and remove from the board.
            // also update visual - localPosition + rotation
            if (GameManger.PlayerTurn == "White"){
                gameManager.WhiteStonesTakeOut.Add(OnSelected.SelectedPlayer);
                OnSelected.SelectedPlayer.transform.localPosition = gameManager.Vector3TakeOutWhiteStones[gameManager.WhiteStonesTakeOut.Count - 1];
            }else{
                gameManager.BlackStonesTakeOut.Add(OnSelected.SelectedPlayer);
                OnSelected.SelectedPlayer.transform.localPosition = gameManager.Vector3TakeOutBlackStones[gameManager.BlackStonesTakeOut.Count - 1];
            }
            OnSelected.SelectedPlayer.transform.rotation = Quaternion.Euler(new Vector3(270, 0, 0));
            gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Pop(); // remove from current stack
            gameManager.HideAllTriangles();
            gameManager.indexCountMove++;
            OnSelected.SelectedPlayer.ToggleHideShowRectangle(false); // hide triangle selected after moving a stone for there
            gameObject.SetActive(false); // hide rectangle
            // update DoneMove after movement out of board - remove stone
                if (!gameManager.SumMovements.IsDouble){
                    if(OnSelected.SelectedPlayer.indexDiceToRemove != -1)
                    gameManager.DoneMove[OnSelected.SelectedPlayer.indexDiceToRemove] = true;
                }else{
                    // if not double - look for DoneMove that isn't true beacue dices have the same val and so we can't do what we did on !IsDouble
                    for (int i = 0; i <gameManager.DoneMove.Length;i++){
                        if (!gameManager.DoneMove[i]){
                            gameManager.DoneMove[i] = true;
                            break; // get out of for
                        }
                    }
                }
            
                OnSelected.SelectedPlayer.indexTriangle = -1; // not on board anymore
                OnSelected.SelectedPlayer = null;
            if (gameManager.SumMovements.IsPlayerDidAllSteps()){
                gameManager.PassTurn();
            }
        }
    }
}
