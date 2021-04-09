using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITextTypeWriter : MonoBehaviour
{
	TextMeshProUGUI textforWriting;
	string story;
	int index = 0;

	[SerializeField]
	string AddStoryPlayer2;

	[SerializeField]
	string OnTieStr;

public GameObject diceRoll; // dice for roll one dice - Who start the game 
	void Awake()
	{
		textforWriting = GetComponent<TextMeshProUGUI>();
		story = textforWriting.text;
		textforWriting.text = "";

		StartCoroutine("PlayText", story);
	}

	 IEnumerator PlayText(string str)
	{
		foreach (char c in str)
		{
			textforWriting.text += c;
			yield return new WaitForSeconds(0.055f);
		}
		
	if (diceRoll.GetComponent<DiceWhoStartGame>() == null){
		//add the ability to click on the dice to roll a dice.
		diceRoll.AddComponent<DiceWhoStartGame>();
	}
	else{
		Destroy(diceRoll.GetComponent<DiceWhoStartGame>());
	}
}
		

	public void OnPlayerRoll(int Letters)
	{
		index = 0;
		
		// remove the ability to roll the dice when the letters didnt update yet.
		Destroy(diceRoll.GetComponent<DiceWhoStartGame>());

		// when the first player roll, change the text and tell the second player to roll the cubes
		StartCoroutine(RemoveLetters(Letters));

	}
	IEnumerator RemoveLetters(int indexForRemove)
	{
		// remove the last sentence, and update that the second player need to roll the cubes now.
		while (index < indexForRemove)
		{
			index++;
			textforWriting.text = textforWriting.text.Substring(0, textforWriting.text.Length - 1);
			yield return new WaitForSeconds(0.05f);
		}

		switch (indexForRemove)
		{
			case 20:
			case 34:
				StartCoroutine("PlayText", AddStoryPlayer2);
				break;
			case 40:
				StartCoroutine("PlayText", OnTieStr);
				break;
			case 79:
				StartCoroutine("PlayText", OnTieStr);
				break;
		}
		
	}
}
