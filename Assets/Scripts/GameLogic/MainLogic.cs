using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLogic : MonoBehaviour {

	//A list of all controllable items that need to be stepped when appropriate
	public List<Controllable> controllables = new List<Controllable>();

	//This is a flag used to stop execution and not perform the next step
	private bool halt = false;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	//Gets all controllables ready to execute
	public void GetReadyToExecute() {
		//Loop through every controllable
		foreach( Controllable controllable in controllables ) {
			//Get it ready to execture
			controllable.GetReadyToExecute();
		}
	}

	//Beings playing the code for the entire battlefield
	public void Play() {

		while( !halt ) {
			StartCoroutine( PlayRoutine() );
		}

	}

	//The routine called by the play method
	//This is where steps are actually taken
	public IEnumerator PlayRoutine() {

		//Step every controllable
		StepAll();

		yield return new WaitForSeconds( 1 );
	}

	//Loops through all controllables and steps them
	public void StepAll() {
		//Loop through every controllable
		foreach( Controllable controllable in controllables ) {
			//Step it
			bool hadProblem = controllable.Step();

			//If there was a halting problem in the controllable, halt
			if( hadProblem ) {
				halt = true;
				break;
			}
		}
	}

}
