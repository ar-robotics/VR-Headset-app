using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// This script is used to display a simple "Hello World" message on the screen for testing purposes.
/// </summary>
public class HelloWorldScript : MonoBehaviour
{
    public string myName = "Kromium Kromiumsen";

    /// <summary>
    /// The TextMeshPro component used to display the message.
    /// </summary>
    private TextMeshProUGUI textMeshPro;

    /// <summary>
    /// Start is called before the first frame update and is used to initialize the script.
    /// It sets the text of the TextMeshPro component to a simple "Hello World" message.
    /// It also logs a message to the console.
    /// </summary>
    void Start()
    {
        Debug.Log("Hello from the other side!");
        textMeshPro = GetComponent<TextMeshProUGUI>();
        textMeshPro.text = $"We are {myName}";
    }

}
