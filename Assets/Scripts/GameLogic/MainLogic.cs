using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLogic : MonoBehaviour {

	//A list of all controllable items that need to be stepped when appropriate
	public List<Controllable> controllables = new List<Controllable>();

	//The editor logic
	public EditorLogic editorLogic;

	//This is a flag used to stop execution and not perform the next step
	private bool halt = false;

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

		StartCoroutine( PlayRoutine() );

		

		//Switch to edit mode
		editorLogic.SwitchToEditMode();

	}

	//The routine called by the play method
	//This is where steps are actually taken
	public IEnumerator PlayRoutine() {

		int loopCount = 0;

		while( !halt || loopCount >= 100 ) {
			//Step every controllable
			StepAll();

			loopCount++;

			yield return new WaitForSeconds( 0.1f );

		}
		
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
