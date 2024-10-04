using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddButtons : MonoBehaviour
{
    [SerializeField] private Transform puzzleField;
    [SerializeField] private GameObject btn;
    private int NumberOfButtons = 12;
   
    void Awake()
    {//creating the buttons in runtime
        for (int i=0; i < NumberOfButtons; i++)
        {
            GameObject MyButton = Instantiate(btn);
            MyButton.name = "" + i; 
            MyButton.transform.SetParent(puzzleField, false);
        }
    }
    
}