using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

public abstract class Controllable : MonoBehaviour {

	//General registers that will be generated
	protected Dictionary<string, float> generalRegisters = new Dictionary<string, float>();

	//Special registers used in most/all devices
	protected Dictionary<string, float> specialRegisters = new Dictionary<string, float>() {
        { "PC", 0 },
    };

	//Registers specific to whichever device is extending this class
	protected Dictionary<string, float> deviceRegisters = new Dictionary<string, float>();

	//Holds a list of all register names along with their line number
	protected Dictionary<string, int> labels = new Dictionary<string, int>();

	//This device's code
	protected string code = "";

    //The number of general registers to generate
    protected int generalRegisterCount = 4;

	//Tracks whether or not we ran into a problem
	private bool halt = false;

    // Start is called before the first frame update
    public void Start() {

		//Add ourselves to main logic's controllables list
		GameObject.Find( "GameLogic" ).GetComponent<MainLogic>().controllables.Add( this );

        //Set up general registers
        for( int i = 0; i < generalRegisterCount; i++ ) {
            generalRegisters.Add( "R" + i, 0f );
        }

		//Reset all registers
		ResetAllRegisters();

		//If this has an AI script on it, load our code from that
		if( this.GetComponent<AI>() ) {
			SetCode( this.GetComponent<AI>().AICode );
		}

	}

	//Resets values and prepares to execute
	public void GetReadyToExecute() {

		//Reset all registers
		ResetAllRegisters();

		//Find the first executable line if the first one isn't executable
		if( !IsLineExecutable( (int) GetRegisterValue( "PC" ) ) ) {
			IncrementProgramCounter();
		}

		//Clear the labels
		labels.Clear();

		//Loop through the code and find all the labels
		for( int i = 0; i < GetLineCount(); i++ ) {

			//Get the next line
			string line = GetLine( i );

			//Check if it's a label
			//Labels are a line that ends in ":", doesn't have any spaces, and has at least 1 non-number character
			
			//If it ends with ":"
			if( line.EndsWith( ":" ) ) {

				//If it doesn't contain spaces
				if( !Regex.IsMatch( line, "\\s" ) ) {

					//If it has at least 1 non-number character
					if( Regex.IsMatch( line, "\\D" ) ) {
						//Then it's probably a label

						//Remove the :
						line = line.Substring( 0, line.Length - 1 );

						//Labels are not going to be case sensitive to uppercase it
						line = line.ToUpper();

						//Save the label in the labels dictionary
						labels.Add( line, i );

						Debug.Log( "Added label " + line + " at line " + i  );

					} else {

						//If it ends with a ":" but is only numbers, it's probably a very poorly made label
						Error( "Malformed label detected!  Labels must contain non-number characters!" );

					}

				} else {

					//If it ends with a ":", it must want to be a label
					//If it fails here then it probably is malformed
					Error( "Malformed label detected!  Labels must not contain spaces!" );
				
				}
			}
		}
	}

    //Handles an error internally
    public void Error( string str ) {
		
		//Every error is a halting error
		Halt();

		//Display the error
		//Eventually this should not be an exception but output to some "actual" console
		throw new System.Exception( "Line #" + GetRegisterValue( "PC" ) + ": " + str );
    }

	//Resets all registers for this device
	public void ResetAllRegisters() {
		ResetRegisters( generalRegisters );
		ResetRegisters( specialRegisters );
		ResetRegisters( deviceRegisters );
	}

	//Resets all registers in a given register set
	public void ResetRegisters( Dictionary<string, float> registers ) {
		//Reset all keys to value 0
		for( int i = 0; i < registers.Count; i++ ) {
			registers[registers.ElementAt( i ).Key] = 0;
		}
	}

    //Returns this device's code
    public string GetCode() {
        return code;
    }

    //Sets this device's code
    public void SetCode( string newCode ) {

		//Remove bad new line characters
		newCode = newCode.Replace( "\r", "" );

		code = newCode;
    }

	//Stops execution
	public void Halt() {
		//Mark down that we ran into a halting error
		halt = true;
	}

	//Returns the number of lines in the code
	public int GetLineCount() {

		//Trim the code
		string trimCode = code.Trim();

		//Return the line count
		return trimCode.Split( '\n' ).Length;
	}

	//Returns a given line out of this device's code
	string GetLine( int lineNumber ) {

		if( lineNumber >= GetLineCount() || lineNumber < 0 ) {
			Error( "Invalid line #" + lineNumber + " referenced!" );
			return "";
		}

		string[] lines = code.Split( '\n' );

		string line = lines[lineNumber];
		line = line.Trim();

		return line;
	}

	//Checks a line and returns if it should be executed
	public bool IsLineExecutable( int lineNumber ) {

		//If the line is longer than the code is, that's immediately no
		if( lineNumber >= GetLineCount() ) {
			return false;
		}

		//Get the line we're checking
		string line = GetLine( lineNumber );

		//Check for comments
		if( line.StartsWith( "#" ) ) {
			return false;
		}

		//Check for labels
		if( line.EndsWith( ":" ) ) {

			//If it has empty space we don't want it
			if( Regex.IsMatch( line, "\\s" ) ) {
				Error( "Labels cannot contain spaces!" );
			}

			return false;

		}

		//Check for empty lines
		if( line.Length == 0 ) {
			return false;
		}

		//Otherwise, yes it should execute
		return true;

	}

	//Moves to the next valid line to be executed
	public void IncrementProgramCounter() {

		//Move one line forward
		SetRegisterValue( "PC", GetRegisterValue( "PC" ) + 1 );

		//If this line is not executable, move on until we find an executable one
		while( !IsLineExecutable( (int) GetRegisterValue( "PC" ) ) ) {

			//If we have run out of lines, stop running
			if( GetRegisterValue( "PC" ) >= GetLineCount() ) {
				//Stop execution
				Halt();
				break;
			}

			//Move one line forward
			SetRegisterValue( "PC", GetRegisterValue( "PC" ) + 1 );
		}
		
	}

	//Gets the appropriate float value for an argument
	public float GetArgumentValue( string arg ) {

		//Check if this was a register
		if( GetRegisterIsValid( arg ) ) {

			//If it was, return the register's value
			return GetRegisterValue( arg );
		} else {

			//If it isn't a register then it should be a float literal
			try {
				return float.Parse( arg );
			} catch( System.Exception e ) {

				//The console error was bothering me so I'm doing this to keep it quiet
				e.GetType();

				Error( "Argument was neither a register nor a float!" );
				return 0f;
			}
		}
	}

	//Gets the line number of a label
	public int GetLabelLineNumber( string labelName ) {

		//Label names are uppercase so uppercase it
		labelName = labelName.ToUpper();

		//Check if the label is valid
		if( !labels.ContainsKey( labelName ) ) {
			//Then this cannot possibly be a real label
			Error( "A reference was made to an invalid label!" );
			return -1;
		}

		//Otherwise it's a real label

		//Return the line number
		return labels[labelName];

	}

	//Handles execution of basic operations that all devices have
	public bool HandleBasicOperations( string[] args ) {
	
		//C# case switches don't like variables so we have to declare them up here I guess
		//I blame returning inside of cases
		float valueA = 0;
		float valueB = 0;
		float result = 0;

		switch( args[0] ) {

			//Print <A>
			//Prints A to the console
			case "print":

				//Check argument count
				if( args.Length != 2 ) {
					Error( "Wrong number of arguments!" );
				}

				//Get the value of the arguments
				valueA = GetArgumentValue( args[1] );

				//Print the result
				Debug.Log( valueA );

				return true;

			//Copy <A> <B>
			//Copies A to register B
			case "copy":

				//Check argument count
				if( args.Length != 3 ) {
					Error( "Wrong number of arguments!" );
				}

				//Get the value of argument A
				valueA = GetArgumentValue( args[1] );

				//Copy the value from argument A to register B
				SetRegisterValue( args[2], valueA );

				return true;

			//Add <A> <B> <C>
			//Adds A and B together and stores the result in register C
			case "add":

				//Check argument count
				if( args.Length != 4 ) {
					Error( "Wrong number of arguments!" );
				}

				//Get the value of the arguments
				valueA = GetArgumentValue( args[1] );
				valueB = GetArgumentValue( args[2] );

				//Add A and B together
				result = valueA + valueB;

				//Save the result in register C
				SetRegisterValue( args[3], result );

				return true;

			//Sub <A> <B> <C>
			//Subtracts B from A and stores the result in register C
			case "sub":

				//Check argument count
				if( args.Length != 4 ) {
					Error( "Wrong number of arguments!" );
				}

				//Get the value of the arguments
				valueA = GetArgumentValue( args[1] );
				valueB = GetArgumentValue( args[2] );

				//Subtract B from A
				result = valueA - valueB;

				//Save the result in register C
				SetRegisterValue( args[3], result );

				return true;

			//Mult <A> <B> <C>
			//Multiplies A and B and stores the result in register C
			case "mult":

				//Check argument count
				if( args.Length != 4 ) {
					Error( "Wrong number of arguments!" );
				}

				//Get the value of the arguments
				valueA = GetArgumentValue( args[1] );
				valueB = GetArgumentValue( args[2] );

				//Multiply A and B
				result = valueA * valueB;

				//Save the result in register C
				SetRegisterValue( args[3], result );

				return true;

			//Div <A> <B> <C>
			//Divides A by B and stores the result in register C
			case "div":

				//Check argument count
				if( args.Length != 4 ) {
					Error( "Wrong number of arguments!" );
				}

				//Get the value of the arguments
				valueA = GetArgumentValue( args[1] );
				valueB = GetArgumentValue( args[2] );

				//Divide A by B
				result = valueA / valueB;

				//Save the result in register C
				SetRegisterValue( args[3], result );

				return true;

			//Pow <A> <B> <C>
			//Raises A to the power of B and stores the result in register C
			case "pow":

				//Check argument count
				if( args.Length != 4 ) {
					Error( "Wrong number of arguments!" );
				}

				//Get the value of the arguments
				valueA = GetArgumentValue( args[1] );
				valueB = GetArgumentValue( args[2] );

				//Raise A to the power of B
				result = Mathf.Pow( valueA, valueB );

				//Save the result in register C
				SetRegisterValue( args[3], result );

				return true;

			//Mod <A> <B> <C>
			//Performs A % B and stores the result in register C
			case "mod":

				//Check argument count
				if( args.Length != 4 ) {
					Error( "Wrong number of arguments!" );
				}

				//Get the value of the arguments
				valueA = GetArgumentValue( args[1] );
				valueB = GetArgumentValue( args[2] );

				//Perform A % B
				result = valueA % valueB;

				//Save the result in register C
				SetRegisterValue( args[3], result );

				return true;

			//Br <A>
			//Branches to label A
			case "br":

				//Check argument count
				if( args.Length != 2 ) {
					Error( "Wrong number of arguments!" );
				}

				//Get the line number of label A
				valueA = GetLabelLineNumber( args[1] );

				Debug.Log( "Jumping to line " + valueA );

				//Move the program counter to the label's line number
				SetRegisterValue( "PC", valueA );

				return true;

		}

		return false;

	}

	//Handles device specific operations
	public abstract bool HandleDeviceOperations( string[] args );

	//Executes a given line
	public void ExecuteLine( int lineNumber ) {
		
		//Get the line	
		string line = GetLine( lineNumber );
		line = line.ToLower();
		line = line.Trim();

		//Split the line into an array
		string[] splitLine = line.Split( ' ' );

		//Track whether we found a method willing to execute this line's opcode
		bool executed = false;

		//Pass the line through the baseline operations that every device has
		executed = HandleBasicOperations( splitLine );

		//Then pass the line through the device specific operations if it wasn't a basic operation
		if( !executed ) {
			executed = HandleDeviceOperations( splitLine );
		}

		//If we couldn't execute the line, it must have been an invalid opcode
		if( !executed ) {
			Error( "Invalid opcode found!" );
		}
	}

	//Executes the current line and steps forward
    public void Step() {

		//Execute the current line
		ExecuteLine( (int) GetRegisterValue( "PC" ) );

		//Move to the next line
		IncrementProgramCounter();

		//Return whether or not we had a halting issue somewhere in stepping
		return halt;
	}

	//Returns whether or not a register exists
	public bool GetRegisterIsValid( string name ) {

		//Make sure the register name is upper case
		name = name.ToUpper();

		//First, check device registers
		if( deviceRegisters.ContainsKey( name ) ) {
			return true;
		}

		//Second, check special registers
		if( specialRegisters.ContainsKey( name ) ) {
			return true;
		}

		//Last, check general registers
		if( generalRegisters.ContainsKey( name ) ) {
			return true;
		}

		//Otherwise, return false
		return false;
	}

	//Gets and returns the float value of a register and throws a halting error if it could not be found
    public float GetRegisterValue( string name ) {

		//Make sure the register name is upper case
		name = name.ToUpper();

        //First, check device registers
        if( deviceRegisters.ContainsKey( name ) ) {
            return deviceRegisters[name];
        }

        //Second, check special registers
        if( specialRegisters.ContainsKey( name ) ) {
            return specialRegisters[name];
        }

        //Last, check general registers
        if( generalRegisters.ContainsKey( name ) ) {
            return generalRegisters[name];
        }

		Error( "A reference to a non-existant register was made!" );

		return 0f;

    }

	//Sets the float value of a register and throws a halting error if it could not be found
	public void SetRegisterValue( string name, float value ) {

		//Make sure that the register name we have is upper case
		name = name.ToUpper();

		//First, check device registers
		if( deviceRegisters.ContainsKey( name ) ) {
			deviceRegisters[name] = value;
			
			//Stop checking once we find it
			return;
		}

		//Second, check special registers
		if( specialRegisters.ContainsKey( name ) ) {
			specialRegisters[name] = value;

			//Stop checking once we find it
			return;
		}

		//Last, check general registers
		if( generalRegisters.ContainsKey( name ) ) {
			generalRegisters[name] = value;

			//Stop checking once we find it
			return;
		}

		Error( "A reference to a non-existant register was made!" );

	}

}
