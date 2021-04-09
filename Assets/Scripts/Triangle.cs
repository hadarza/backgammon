using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : MonoBehaviour
{
    public int TriangleIndex;
    [SerializeField]
    Vector3 firstLocation;
    const int LocationMaxPlayers = 15;
    public Vector3[] location; // array of Vector3 of the locations for players according the TriangleIndex.
    Ray ray;
    RaycastHit hit;

    [SerializeField]
    GameManger gameManger;


    int MovementToDecarse;
    string OppisteType;
    int x;
    void Start()
    {
        location = new Vector3[LocationMaxPlayers];
        int indexStart = 0;
        int i;
        for (int J = 1; J <= 3; J++)
        {
            for (i = indexStart; i < LocationMaxPlayers - (3 - J) * 5; i++)
            {
                if (i == 0)
                {
                    location[0] = firstLocation;
                }
                else
                {
                    print(i + ", " + J);
                    // fix that 
                    location[i] = location[0] + ((i % 5) * new Vector3(0, 0, 1.25f)) + (J - 1) * (new Vector3(0, 1.25f, 0));
                }
            }
            indexStart = i;
        }
    }

    public void ShowMovementAfterDice(bool moveByOtherDice, int dice)
    {
        /* after the movement according to first/second dice, 
        update the next location the paritcipent can jump to, according to the other dice. */
        gameManger.indexCountMove++;
        gameManger.HideAllTriangles();
        if (!moveByOtherDice)
        {     // if the player didn't move the player according to second dice
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

    public void OnClickEnterFromTrappedArray()
    {

    }

    public void DoubleLastStep()
    {
        GameManger.moveByFirstDice = true;
        GameManger.moveBySecondDice = true;
        gameManger.canPut = -1;
        gameManger.canPut2 = -1;
    }
    public void OnMouseDown()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButton(0))
            {
                if (hit.collider.tag == "Triangle")
                {
                    // if the particpent click on a triangle, move the player to this location
                    if (OnSelected.SelectedPlayer != null)
                    {
                        if ((GameManger.PlayerTurn == "Black" && gameManger.onPlayerBlack.Count == 0) ||
                            (GameManger.PlayerTurn == "White" && gameManger.onPlayerWhite.Count == 0))
                        {
                            // using Math.Abs for not checking if the current player is white or black.
                            MovementToDecarse = Math.Abs(OnSelected.SelectedPlayer.indexTriangle - TriangleIndex);
                            // if the particpent didn't move the players at his turn at all
                            if (!GameManger.moveByFirstDice && !GameManger.moveBySecondDice)
                            {
                                if (gameManger.SumMovements.firstDice == MovementToDecarse)
                                {
                                    if ((gameManger.SumMovements.IsDouble && gameManger.indexCountMove < 4) || !gameManger.SumMovements.IsDouble)
                                        ShowMovementAfterDice(GameManger.moveBySecondDice, 1);
                                    if (!gameManger.SumMovements.IsDouble)
                                    {
                                        GameManger.moveByFirstDice = true;
                                        gameManger.canPut = -1;
                                    }
                                    else if (gameManger.SumMovements.IsDouble && gameManger.indexCountMove == 4)
                                    {
                                        DoubleLastStep();
                                        updateAfterFinishTurn();
                                    }
                                }
                                else if (gameManger.SumMovements.secondDice == MovementToDecarse)
                                {
                                    if ((gameManger.SumMovements.IsDouble && gameManger.indexCountMove < 4) || !gameManger.SumMovements.IsDouble)
                                        ShowMovementAfterDice(GameManger.moveByFirstDice, 0);
                                    if (!gameManger.SumMovements.IsDouble)
                                    {
                                        GameManger.moveBySecondDice = true;
                                        gameManger.canPut2 = -1;
                                    }
                                    else if (gameManger.SumMovements.IsDouble && gameManger.indexCountMove == 4)
                                    {
                                        DoubleLastStep();
                                        updateAfterFinishTurn();
                                    }
                                }
                            }
                            else if (gameManger.SumMovements.secondDice == MovementToDecarse && GameManger.moveByFirstDice && !GameManger.moveBySecondDice)
                            {
                                if (!gameManger.SumMovements.IsDouble)
                                {
                                    updateAfterFinishTurn();
                                    gameManger.canPut2 = -1;
                                    GameManger.moveBySecondDice = true;
                                }
                            }
                            else if (gameManger.SumMovements.firstDice == MovementToDecarse && GameManger.moveBySecondDice && !GameManger.moveByFirstDice)
                            {
                                if (!gameManger.SumMovements.IsDouble)
                                {
                                    updateAfterFinishTurn();
                                    GameManger.moveByFirstDice = true;
                                    gameManger.canPut = -1;
                                }
                            }

                            JumpOnOppositeStone();
                            gameManger.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Pop(); // remove from current stack
                            /* if there is a player in the gameManger.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1],
                             than addComponent OnSelected */
                            if (gameManger.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Count >= 1)
                                if (!gameManger.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Peek().gameObject.GetComponent<OnSelected>())
                                    gameManger.BoardGame[OnSelected.SelectedPlayer.indexTriangle - 1].Peek().gameObject.AddComponent<OnSelected>();
                            OnSelected.SelectedPlayer.indexTriangle = TriangleIndex; // get the new Triangle that got selected
                            gameManger.BoardGame[TriangleIndex - 1].Push(OnSelected.SelectedPlayer); // update in the new stack
                            OnSelected.SelectedPlayer.transform.localPosition = location[gameManger.BoardGame[TriangleIndex - 1].Count - 1]; // update on board - visual
                        }
                        else
                        {
                            OnSelected.OnChosingMove -= gameManger.ChangeColorToCurrentPlayer;
                            // there is something in OnPlayerBlack / onPlayerWhite

                            JumpOnOppositeStone();

                            List<Player> currentList;
                            if (GameManger.PlayerTurn == "Black")
                                currentList = gameManger.onPlayerBlack;
                            else
                                currentList = gameManger.onPlayerWhite;

                            OnSelected.SelectedPlayer.indexTriangle = TriangleIndex;
                            SearchCurrentTrappedStone(currentList); // remove from Trapped stones array
                            gameManger.BoardGame[TriangleIndex - 1].Push(OnSelected.SelectedPlayer); // add to board game
                            OnSelected.SelectedPlayer.transform.localPosition = location[gameManger.BoardGame[TriangleIndex - 1].Count - 1]; // update on board - visual

                            if ((gameManger.SumMovements.IsDouble && gameManger.indexCountMove < 4) || !gameManger.SumMovements.IsDouble){
                                if (TriangleIndex == gameManger.dices[0].diceCount)
                                    ShowMovementAfterDice(GameManger.moveByFirstDice, 0);
                                else
                                    ShowMovementAfterDice(GameManger.moveBySecondDice, 1);
                            }
                            if (!gameManger.SumMovements.IsDouble)
                            {
                                if (TriangleIndex == gameManger.dices[0].diceCount){
                                    GameManger.moveByFirstDice = true;
                                    gameManger.canPut = -1;
                                }else{
                                    GameManger.moveBySecondDice = true;
                                    gameManger.canPut2 = -1;
                                }
                            }
                            else if (gameManger.SumMovements.IsDouble && gameManger.indexCountMove == 4)
                            {
                                DoubleLastStep();
                                updateAfterFinishTurn();
                            }
                        }
                    }
                }
            }
        }
    }
    public void SearchCurrentTrappedStone(List<Player> listTrapped)
    {
        // remove the current player selected forom onPlayerBlack / onPlayerWhite - trapped stones
        foreach(Player p in listTrapped){
            if (p == OnSelected.SelectedPlayer) {
                // search for the player in list that got clicked
                listTrapped.Remove(p);
                break;
            }

        }
    }

    public void JumpOnOppositeStone()
    {
        /*if you jump on one player that oppoise to your PlayerType - eat it and remove from board, add to OnPlayerBlack / onPlayerWhite array */
                            if (gameManger.BoardGame[TriangleIndex - 1].Count == 1)
        {
            if (OnSelected.SelectedPlayer.PlayerType == "Black" && gameManger.BoardGame[TriangleIndex - 1].Peek().PlayerType == "White")
            {
                print(gameManger.Vector3ArrayTrappedBlackStones[gameManger.onPlayerBlack.Count]);
                    gameManger.BoardGame[TriangleIndex - 1].Peek().gameObject.transform.localPosition = gameManger.Vector3ArrayTrappedBlackStones[gameManger.onPlayerWhite.Count];
                    gameManger.onPlayerWhite.Add(gameManger.BoardGame[TriangleIndex - 1].Pop());
            }
            else if (OnSelected.SelectedPlayer.PlayerType == "White" && gameManger.BoardGame[TriangleIndex - 1].Peek().PlayerType == "Black")
            {
                print(gameManger.Vector3ArrayTrappedWhiteStones[gameManger.onPlayerWhite.Count]);
                    gameManger.BoardGame[TriangleIndex - 1].Peek().gameObject.transform.localPosition = gameManger.Vector3ArrayTrappedWhiteStones[gameManger.onPlayerBlack.Count - 1];
                    gameManger.onPlayerBlack.Add(gameManger.BoardGame[TriangleIndex - 1].Pop());
                
            }
        }
    }
}
