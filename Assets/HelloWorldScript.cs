using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HelloWorldScript : MonoBehaviour
{
    public string myName = "Kromium Kromiumsen";
    public int myAge = 0;

    private TextMeshProUGUI textMeshPro;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hello from the other side!");
        textMeshPro = GetComponent<TextMeshProUGUI>();
        textMeshPro.text = $"We are {myName}";
    }

    // Update is called once per frame
    void Update()
    {
        // textMeshPro.transform.position = new Vector3(0, 0, 0);
    }
}
