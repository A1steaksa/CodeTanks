using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorLogic : MonoBehaviour {
	
	//Buttons
    public Button stepButton;
	public Button execButton;

	//The text area containing code
	public InputField codeArea;

    //A reference to the device we're currently editing code for
    public Controllable currentDevice;

	//Tracks if the exec button should act as a stop button
	private bool execButtonIsStop = false;

    // Start is called before the first frame update
    void Start() {

		//Set up event handlers
		stepButton.onClick.AddListener( StepButtonHandler );
		execButton.onClick.AddListener( ExecButtonHandler );

		//Disable the step button
		stepButton.interactable = false;

	}

	//Saves the code from the editor to the device
	public void SaveCode() {
		currentDevice.SetCode( codeArea.text );
	}

    public void StepButtonHandler() {
		//Step the device
		currentDevice.Step();
	}

	//Prepares the editor to edit code
	public void SwitchToEditMode() {
		//Disable the step button
		stepButton.interactable = false;

		//Enable editing code
		codeArea.interactable = true;

		//Switch to being an exec button again
		execButtonIsStop = false;

		//Change button text
		execButton.GetComponentInChildren<Text>().text = "EXEC";
	}

	//Prepares the editor to execute code
	public void SwitchToExecutionMode() {
		//Save the code to the device
		SaveCode();

		//Disable editing code
		codeArea.interactable = false;

		//Tell the device to get ready to execute
		currentDevice.GetReadyToExecute();

		//Enable the step button
		stepButton.interactable = true;

		//Switch to a stop execution button
		execButtonIsStop = true;

		//Change button text
		execButton.GetComponentInChildren<Text>().text = "STOP";
	}

	public void ExecButtonHandler() {
		//If we're currently a stop button, stop execution and switch to edit mode
		if( execButtonIsStop ) {
			SwitchToEditMode();
		} else {
			//Otherwise switch to execution mode
			SwitchToExecutionMode();
		}
	}
}
