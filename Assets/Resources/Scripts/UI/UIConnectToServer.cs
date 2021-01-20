using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIConnectToServer : MonoBehaviour
{
    public static UIConnectToServer instance;
   [SerializeField] private TMP_InputField usernameInputField;

   
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public string GetUsername(){
        return usernameInputField.text;
    }
}
