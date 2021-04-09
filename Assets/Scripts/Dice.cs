using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dice : MonoBehaviour
{
	internal Vector3 initPos;

	#region Public Variables
	// Orignal Dice
	public GameObject[] orignalDice;
	//dice resultant number..
	public int diceCount;
	//dice play view camera...
	public Camera dicePlayCam;
	//Can Throw Dice
	public bool isDiceLand = true;
	public Transform diceCarrom;
	#endregion
	#region Private Variables
	Ray ray;
	RaycastHit hit;
	#endregion
	#region SerializeField Variables
	[SerializeField]
	GameManger gameManager;
	#endregion

	void Start()
	{
		GetComponent<Rigidbody>().solverIterations = 250;
		initPos = transform.position;
	}
	private void Update()
	{
		if (isDiceLand && GameManger.canRoll) {
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				if (hit.collider.name == "dice" || hit.collider.name == "dice2") {
					initPos = Input.mousePosition;
					gameManager.indexCountMove = 0; //resart countMovemnts 
					Vector3 currentPos = Input.mousePosition;
					currentPos.z = 25f;
					Vector3 newPos = dicePlayCam.ScreenToWorldPoint(new Vector3(currentPos.x, currentPos.y, Mathf.Clamp(currentPos.y / 10, 5, 70)));
					newPos.y = Mathf.Clamp(newPos.y, -114.5f, 100);
					newPos = dicePlayCam.ScreenToWorldPoint(currentPos);

					// restart at rolling the dices again.
					gameManager.canPut = 0;
					gameManager.canPut2 = 0;
					GameManger.moveByFirstDice = false;
					GameManger.moveBySecondDice = false;
					
					if (Input.GetMouseButtonUp(0)) {
						initPos = dicePlayCam.ScreenToWorldPoint(initPos);

						enableTheDice();
						addForce(newPos);
						isDiceLand = false;
						GameManger.canRoll = false;

						//	StartCoroutine(getDiceCount());
					}
				}
			}
		}
	}

	void addForce(Vector3 lastPos)
	{
		for (int i = 0; i < orignalDice.Length; i++) {
			orignalDice[i].gameObject.transform.position = diceCarrom.transform.position - new Vector3(i * 4, 0, 0);
			// Cross the vectors to get a perpendicular vector, then normalize it.
			orignalDice[i].GetComponent<Rigidbody>().AddTorque(Vector3.Cross(lastPos, initPos) * 1000, ForceMode.Impulse);
			lastPos.z -= 30;
			orignalDice[i].GetComponent<Rigidbody>().AddForce(((lastPos - initPos).normalized) * (Vector3.Distance(lastPos, initPos)) * 5 * orignalDice[i].GetComponent<Rigidbody>().mass);
		}
	}

	void enableTheDice()
	{
		for (int i = 0; i < orignalDice.Length; i++) {
			orignalDice[i].transform.rotation = Quaternion.Euler(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)); ;
		}
	}

	void OnCollisionStay(Collision collision)
	{
		// if dice collide with the boardGame
		if(collision.gameObject.tag == "Board"){
			isDiceLand = true;
			diceCount = 0;
			// get dice count according to the angle of the vectors
			diceCount = GetDiceCount();
			for (int i = 0; i < orignalDice.Length; i++)
			{
				orignalDice[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
			}
		}
	}

	void OnEnable(){
		initPos = transform.position;
	}
	public int GetDiceCount(){
		// find dice count according the angle between 2 vectors 
		if (Vector3.Dot(transform.forward, Vector3.up) > 0.6f)
			diceCount = 2;
		else if (Vector3.Dot(-transform.forward, Vector3.up) > 0.6f)
			diceCount = 4;
		else if (Vector3.Dot(transform.up, Vector3.up) > 0.6f)
			diceCount = 3;
		else if (Vector3.Dot(-transform.up, Vector3.up) > 0.6f)
			diceCount = 1;
		else if (Vector3.Dot(transform.right, Vector3.up) > 0.6f)
			diceCount = 5;
		else if (Vector3.Dot(-transform.right, Vector3.up) > 0.6f)
			diceCount = 6;
		return diceCount;
	}

}
