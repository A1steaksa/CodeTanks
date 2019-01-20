using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorStyle : MonoBehaviour {

    private InputField inputField;

    // Start is called before the first frame update
    void Start() {
        inputField = GetComponent<InputField>();

        inputField.caretWidth = 40;
    }

    // Update is called once per frame
    void Update() {

    }
}
