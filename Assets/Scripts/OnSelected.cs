using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSelected : MonoBehaviour
{
    public delegate void ChosingMove(Player p);
    public static event ChosingMove OnChosingMove;

    public static Player SelectedPlayer;
    public void OnMouseDown()
    {
      if(OnChosingMove != null)
        {
            SelectedPlayer = gameObject.GetComponent<Player>();
            OnChosingMove(SelectedPlayer);
            SelectedPlayer.PlayerRemoveStones();
            print("on chosing move");
        }
    }
    
}
