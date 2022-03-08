using System.Collections;
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
        if (Physics.Raycast(ray, out hit)) {
            if (Input.GetMouseButton(0)) {
                if (hit.collider.tag == "Rectangle") {
                    return true;
                }
            }
        }
        return false;
    }

    public void OnMouseDown()
    {
        if (DidClickOnRectangle()) {
            // adding to list of stones that take out and remove from the board.
            // also update visual - localPosition + rotation
            List<Player> currentTakeOutList = gameManager.UpdateTakeOutListPlayerTurn();
            Vector3[] currentTakeOutloc = UpdateTakeOutLocPlayerTurn(GameManger.PlayerTurn);

            UpdateVisualAndOnBoard(currentTakeOutList, currentTakeOutloc);
            UpdateDoneMove();
            ResetSelected();
            gameManager.PrintWinner();

            if(!gameManager.CanvasVictory.gameObject.activeInHierarchy)
                TakeCareOfCases();

        }
    }

    //This function update currentTakeOutList according to Playerturn paramter
    public Vector3[] UpdateTakeOutLocPlayerTurn(string playerTurn) {
        return playerTurn == "White" ? gameManager.Vector3TakeOutWhiteStones : gameManager.Vector3TakeOutBlackStones;
    }

    public void UpdateVisualAndOnBoard(List<Player> currentTakeOutList, Vector3[] currentTakeOutloc) {
        // add stone to TakeOut array and show it visual
        currentTakeOutList.Add(OnSelected.SelectedPlayer);
        gameManager.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Pop(); // remove from current stack

        OnSelected.SelectedPlayer.transform.localPosition = currentTakeOutloc[currentTakeOutList.Count - 1];
        OnSelected.SelectedPlayer.transform.rotation = Quaternion.Euler(new Vector3(270, 0, 0));
        gameObject.SetActive(false); // hide rectangle
        gameManager.ToggleHideShowRectangle(false); // hide triangle selected after moving a stone for there
        gameManager.HideAllTriangles();

        gameManager.indexCountMove++;
    }

    public void ResetSelected() {
        OnSelected.SelectedPlayer.gameObject.GetComponent<Renderer>().material = gameManager.NormalColor; // change color matriel to normal
        OnSelected.SelectedPlayer.indexTriangle = -1; // not on board anymore
        OnSelected.SelectedPlayer = null;
    }

    public void UpdateDoneMove()
    {
        // update DoneMove after movement out of board - remove stone
        if (!gameManager.SumMovements.IsDouble) {
            if (OnSelected.SelectedPlayer.indexDiceToRemove != -1)
                gameManager.DoneMove[OnSelected.SelectedPlayer.indexDiceToRemove] = true;
        } else {
            // if not double - look for DoneMove that isn't true beacue dices have the same val and so we can't do what we did on !IsDouble
            for (int i = 0; i < gameManager.DoneMove.Length; i++) {
                if (!gameManager.DoneMove[i]) {
                    gameManager.DoneMove[i] = true;
                    break; // get out of for
                }
            }
        }
    }

    public void TakeCareOfCases() {
        // on finish turn
        if (gameManager.SumMovements.IsPlayerDidAllSteps())
            gameManager.PassTurn();
        else{
            if (gameManager.CantMove()){// can't moves stones at all
                if (gameManager.isAllPlayersCanRemoved(gameManager.BoardGame, GameManger.PlayerTurn)){
                    gameManager.ShowMessagePassTurn = true; //reset
                    gameManager.ShowMessagePassTurn = gameManager.NeedPassTurnMsg();

                    if (gameManager.ShowMessagePassTurn)
                        gameManager.ShowErrorPassTurn("אין ביכולתך להזיז אף אבן ולכן התור עובר ליריב");
                }
                else
                    gameManager.ShowErrorPassTurn("אין ביכולתך להזיז אבנים לפי הקוביות הנתונות ולכן התור עובר ליריב");
            }
        }
    }
}


