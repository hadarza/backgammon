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
            OnSelected.SelectedPlayer.indexTriangle = -1; // not on board anymore
        }
    }
}
