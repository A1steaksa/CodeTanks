using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : Controllable {

	public GameObject turret;

	//How far each forward move moves the tank
	private float forwardSpeed = 1f;

	//How far each backward move moves the tank
	private float backwardSpeed = 0.75f;

	//How far each left turn rotates the tank, in degrees
	private float leftTurnSpeed = 15f;

	//How far each right turn rotates the tank, in degrees
	private float rightTurnSpeed = 15f;

	//Tank opcodes
	public override bool HandleDeviceOperations( string[] args ) {

		switch( args[0] ) {
			
			//RotateTurret <A>
			//Rotates the turret to A degrees
			case "rotateturret":

				//Check argument count
				if( args.Length != 2 ) {
					Error( "Wrong number of arguments!" );
				}

				//Get the new angle for the turret
				float newAngle = GetArgumentValue( args[1] );

				//Rotate the turret
				turret.gameObject.GetComponent<Transform>().rotation = Quaternion.Euler( 0, newAngle, 0 );

				return true;

			//Forward
			//Drives the tank forward
			case "forward":

				//Check argument count
				if( args.Length != 1 ) {
					Error( "Wrong number of arguments!" );
				}

				//Move the tank forward
				this.transform.position += this.transform.forward * forwardSpeed;

				return true;

			//Backward
			//Drives the tank backwards
			case "backward":

				//Check argument count
				if( args.Length != 1 ) {
					Error( "Wrong number of arguments!" );
				}

				//Move the tank backward
				this.transform.position -= this.transform.forward * backwardSpeed;

				return true;

			//TurnLeft
			//Turns the tank left
			case "turnleft":

				//Check argument count
				if( args.Length != 1 ) {
					Error( "Wrong number of arguments!" );
				}

				//Move the tank backward
				this.transform.Rotate( Vector3.up, -leftTurnSpeed );

				return true;

			//TurnRight
			//Turns the tank right
			case "turnright":

				//Check argument count
				if( args.Length != 1 ) {
					Error( "Wrong number of arguments!" );
				}

				//Move the tank backward
				this.transform.Rotate( Vector3.up, rightTurnSpeed );

				return true;
		}
		
		return false;
	}

	// Start is called before the first frame update
	new void Start() {
		base.Start();

		//Set up device registers
		deviceRegisters = new Dictionary<string, float>(){
			{ "TurretAngle", 0f },
		};

	}
}
