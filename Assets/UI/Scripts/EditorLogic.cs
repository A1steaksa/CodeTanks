﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorLogic : MonoBehaviour {
	
	//Buttons
    public Button stepButton;
	public Button execButton;
	public Button playButton;

	//The text area containing code
	public TMP_InputField codeArea;

    //A reference to the device we're currently editing code for
    public Controllable currentDevice;

	//Tracks if the exec button should act as a stop button
	private bool execButtonIsStop = false;

	//Main game logic
	public MainLogic mainLogic;

    // Start is called before the first frame update
    void Start() {

		//Set up event handlers
		stepButton.onClick.AddListener( StepButtonHandler );
		execButton.onClick.AddListener( ExecButtonHandler );
		playButton.onClick.AddListener( PlayButtonHandler );

		//Disable the step and play buttons
		stepButton.interactable = false;
		playButton.interactable = false;
	}

	//Saves the code from the editor to the device
	public void SaveCode() {
		currentDevice.SetCode( codeArea.text );
	}

	//Prepares the editor to edit code
	public void SwitchToEditMode() {

		//Disable the running code
		stepButton.interactable = false;
		playButton.interactable = false;

		//Enable editing code
		codeArea.interactable = true;

		//Switch to being an exec button again
		execButtonIsStop = false;

		//Change button text
		execButton.GetComponentInChildren<Text>().text = "Execute";
	}

	//Prepares the editor to execute code
	public void SwitchToExecutionMode() {
		//Save the code to the device
		SaveCode();

		//Disable editing code
		codeArea.interactable = false;

		//Tell main logic to get ready to execute
		mainLogic.GetReadyToExecute();

		//Enable executing code
		stepButton.interactable = true;
		playButton.interactable = true;

		//Switch to a stop execution button
		execButtonIsStop = true;

		//Change button text
		execButton.GetComponentInChildren<Text>().text = "Stop";
	}

	public void StepButtonHandler() {
		//Ask the main logic to step everybody
		mainLogic.StepAll();
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

	public void PlayButtonHandler() {

		//Switch to execution mode
		SwitchToExecutionMode();

		//Being playing the code on main game logic
		mainLogic.Play();

	}

}
